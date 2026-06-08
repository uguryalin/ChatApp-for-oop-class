using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleChatApp.Models;

namespace ConsoleChatApp.Core
{
    // The main Chat Server managing client sockets, routing messages,
    // and maintaining thread-safe states using encapsulation.
    public class ChatServer
    {
        private readonly int _port;
        private TcpListener? _listener;
        private bool _isRunning;
        
        // Encapsulation: The participant collection is private, preventing outside manipulation.
        private readonly ConcurrentDictionary<string, IChatParticipant> _participants = new();

        public ChatServer(int port = 8080)
        {
            _port = port;
        }

        // Asynchronously listens for incoming client connections.
        public async Task StartAsync()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _isRunning = true;
            Console.WriteLine($"[SERVER] Chat server started on port {_port}. Waiting for clients...");

            try
            {
                while (_isRunning)
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    // Handle client asynchronously in a separate task
                    _ = HandleClientAsync(client);
                }
            }
            catch (ObjectDisposedException) when (!_isRunning)
            {
                // Graceful shutdown of the listener
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVER ERROR] {ex.Message}");
            }
        }

        // Stops the server and disconnects all active clients.
        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();

            foreach (var participant in _participants.Values)
            {
                participant.Disconnect();
            }
            _participants.Clear();
            Console.WriteLine("[SERVER] Server stopped.");
        }

        // Handles communications for a specific client connection.
        private async Task HandleClientAsync(TcpClient tcpClient)
        {
            using var client = tcpClient;
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream) { AutoFlush = true };

            string? nickname = null;
            try
            {
                // The client must submit a valid nickname on connection.
                nickname = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(nickname))
                {
                    await writer.WriteLineAsync("ERROR: Nickname cannot be empty.");
                    return;
                }

                nickname = nickname.Trim();

                if (_participants.ContainsKey(nickname))
                {
                    await writer.WriteLineAsync("ERROR: Nickname already in use.");
                    return;
                }

                // Acknowledge successful registration.
                await writer.WriteLineAsync("SUCCESS");
                
                // Nested private class instantiation (Encapsulation).
                var participant = new ConnectedClient(nickname, tcpClient, writer);
                if (_participants.TryAdd(nickname, participant))
                {
                    Console.WriteLine($"[SERVER] User '{nickname}' connected from {tcpClient.Client.RemoteEndPoint}.");
                    
                    // Broadcast entry notification.
                    await BroadcastMessageAsync(new SystemMessage
                    {
                        Content = $"{nickname} has joined the chat room."
                    });

                    // Main socket message receiving loop.
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        try
                        {
                            var message = JsonSerializer.Deserialize<Message>(line);
                            if (message != null)
                            {
                                // Set sender securely server-side.
                                message.Sender = nickname;
                                await RouteMessageAsync(nickname, message);
                            }
                        }
                        catch (JsonException)
                        {
                            // Skip invalid data payloads.
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is SocketException || ex is IOException || ex.InnerException is SocketException)
                {
                    Console.WriteLine($"[SERVER] User '{nickname ?? "Unknown"}' connection closed.");
                }
                else
                {
                    Console.WriteLine($"[SERVER ERROR] Exception for client '{nickname ?? "Unknown"}': {ex.Message}");
                }
            }
            finally
            {
                // Handle socket closing, remove from participants, and clean up.
                if (nickname != null && _participants.TryRemove(nickname, out var participant))
                {
                    participant.Disconnect();
                    Console.WriteLine($"[SERVER] User '{nickname}' disconnected.");
                    
                    await BroadcastMessageAsync(new SystemMessage
                    {
                        Content = $"{nickname} has left the chat room."
                    });
                }
            }
        }

        // Send a message to all connected users.
        private async Task BroadcastMessageAsync(Message message)
        {
            var tasks = _participants.Values.Select(p => p.SendMessageAsync(message));
            await Task.WhenAll(tasks);
        }

        // Route messages based on their types (Polymorphism and routing).
        private async Task RouteMessageAsync(string sender, Message message)
        {
            if (message is PrivateMessage pm)
            {
                // Direct private message route.
                if (_participants.TryGetValue(pm.Recipient, out var recipientNode))
                {
                    await recipientNode.SendMessageAsync(pm);
                    // Echo private message back to sender so they see it in their thread.
                    if (_participants.TryGetValue(sender, out var senderNode))
                    {
                        await senderNode.SendMessageAsync(pm);
                    }
                }
                else
                {
                    // Notify sender that target user is offline.
                    if (_participants.TryGetValue(sender, out var senderNode))
                    {
                        await senderNode.SendMessageAsync(new SystemMessage
                        {
                            Content = $"User '{pm.Recipient}' is not online."
                        });
                    }
                }
            }
            else if (message is TextMessage tm)
            {
                // Client-side can send commands via text. E.g. /list.
                if (tm.Content.Trim() == "/list")
                {
                    if (_participants.TryGetValue(sender, out var senderNode))
                    {
                        var listContent = "Active Users: " + string.Join(", ", _participants.Keys);
                        await senderNode.SendMessageAsync(new SystemMessage { Content = listContent });
                    }
                }
                else
                {
                    // Standard text message broadcast.
                    await BroadcastMessageAsync(tm);
                }
            }
        }

        // Private nested class representing a client connection, demonstrating Encapsulation.
        // It implements IChatParticipant, demonstrating Abstraction.
        private class ConnectedClient : IChatParticipant
        {
            private readonly TcpClient _tcpClient;
            private readonly StreamWriter _writer;
            private readonly object _lock = new();

            public string Name { get; }

            public ConnectedClient(string name, TcpClient tcpClient, StreamWriter writer)
            {
                Name = name;
                _tcpClient = tcpClient;
                _writer = writer;
            }

            public async Task SendMessageAsync(Message message)
            {
                try
                {
                    var json = JsonSerializer.Serialize<Message>(message);
                    // Sync lock to guarantee thread safety over the network stream.
                    lock (_lock)
                    {
                        _writer.WriteLine(json);
                    }
                    await Task.CompletedTask;
                }
                catch
                {
                    // Handled gracefully by read-loop drop.
                }
            }

            public void Disconnect()
            {
                try
                {
                    _tcpClient.Close();
                }
                catch
                {
                    // Ignore already closed socket.
                }
            }
        }
    }
}
