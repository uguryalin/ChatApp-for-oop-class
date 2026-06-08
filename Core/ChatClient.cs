using System;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleChatApp.Models;

namespace ConsoleChatApp.Core
{
    // The Chat Client manages connection to the server, sends messages/commands,
    // and processes incoming network streams in a background task (multithreading).
    public class ChatClient
    {
        private readonly string _serverIp;
        private readonly int _port;
        private readonly string _nickname;
        private TcpClient? _client;
        private StreamReader? _reader;
        private StreamWriter? _writer;
        private bool _isConnected;

        private readonly TextReader _inputReader;

        public ChatClient(string nickname, string serverIp = "127.0.0.1", int port = 8080, TextReader? inputReader = null)
        {
            _nickname = nickname;
            _serverIp = serverIp;
            _port = port;
            _inputReader = inputReader ?? Console.In;
        }

        // Connects to the server, registers the nickname, and starts communication loops.
        public async Task StartAsync()
        {
            try
            {
                _client = new TcpClient();
                Console.WriteLine($"[CLIENT] Connecting to {_serverIp}:{_port}...");
                await _client.ConnectAsync(_serverIp, _port);

                var stream = _client.GetStream();
                _reader = new StreamReader(stream);
                _writer = new StreamWriter(stream) { AutoFlush = true };

                // 1. Send the chosen nickname as the first plain text line
                await _writer.WriteLineAsync(_nickname);

                // 2. Await validation from the server
                string? response = await _reader.ReadLineAsync();
                if (response != "SUCCESS")
                {
                    Console.WriteLine($"[CLIENT ERROR] Registration failed: {response ?? "No response"}");
                    Disconnect();
                    return;
                }

                _isConnected = true;
                Console.WriteLine("[CLIENT] Connected successfully!");
                PrintHelp();

                // 3. Spawn a background task to handle incoming network payloads asynchronously (Multithreading)
                var receiveTask = ReceiveMessagesAsync();

                // 4. Main client thread input reading loop
                while (_isConnected)
                {
                    string? input = await _inputReader.ReadLineAsync();
                    if (input == null) break; // End of reader stream
                    if (string.IsNullOrWhiteSpace(input)) continue;

                    if (input.Equals("/quit", StringComparison.OrdinalIgnoreCase) || 
                        input.Equals("/exit", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    if (input.Equals("/help", StringComparison.OrdinalIgnoreCase))
                    {
                        PrintHelp();
                        continue;
                    }

                    await SendInputAsync(input);
                }

                Disconnect();
                await receiveTask; // Await receiver loop termination
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT ERROR] Connection failed: {ex.Message}");
                Disconnect();
            }
        }

        private void PrintHelp()
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("Chat Commands:");
            Console.WriteLine("  /list                    - List all active online users");
            Console.WriteLine("  /msg <username> <text>   - Send a private message to a user");
            Console.WriteLine("  /help                    - Show command help guide");
            Console.WriteLine("  /quit                    - Disconnect and shut down client");
            Console.WriteLine("==================================================");
        }

        // Parses client command input and transforms it into polymorphic Message objects.
        private async Task SendInputAsync(string input)
        {
            if (input.StartsWith("/msg ", StringComparison.OrdinalIgnoreCase))
            {
                var parts = input.Split(' ', 3);
                if (parts.Length < 3)
                {
                    Console.WriteLine("[SYSTEM] Invalid usage. Syntax: /msg <username> <message>");
                    return;
                }

                var recipient = parts[1];
                var content = parts[2];

                // Create a PrivateMessage object
                var privateMsg = new PrivateMessage
                {
                    Sender = _nickname,
                    Recipient = recipient,
                    Content = content
                };

                await SendJsonAsync(privateMsg);
            }
            else
            {
                // Create a standard TextMessage object
                var textMsg = new TextMessage
                {
                    Sender = _nickname,
                    Content = input
                };

                await SendJsonAsync(textMsg);
            }
        }

        // Serializes a Message polymorphically into JSON and sends it over the TCP network stream.
        private async Task SendJsonAsync(Message message)
        {
            if (_writer != null && _isConnected)
            {
                var json = JsonSerializer.Serialize<Message>(message);
                await _writer.WriteLineAsync(json);
            }
        }

        // Background receiver thread loop.
        private async Task ReceiveMessagesAsync()
        {
            try
            {
                while (_isConnected && _reader != null)
                {
                    string? line = await _reader.ReadLineAsync();
                    if (line == null)
                    {
                        Console.WriteLine("[CLIENT] Connection lost with the server.");
                        _isConnected = false;
                        break;
                    }

                    try
                    {
                        // Deserialize JSON polymorphically back into the abstract Message type.
                        var message = JsonSerializer.Deserialize<Message>(line);
                        if (message != null)
                        {
                            // Polymorphic call: Format() executes differently based on concrete type.
                            Console.WriteLine(message.Format());
                        }
                    }
                    catch (JsonException)
                    {
                        // Ignore malformed JSON packets.
                    }
                }
            }
            catch (Exception ex)
            {
                if (_isConnected)
                {
                    Console.WriteLine($"[CLIENT ERROR] Error reading server stream: {ex.Message}");
                }
            }
            finally
            {
                _isConnected = false;
            }
        }

        private void Disconnect()
        {
            _isConnected = false;
            try
            {
                _reader?.Dispose();
                _writer?.Dispose();
                _client?.Close();
            }
            catch
            {
                // Ignore cleanup exceptions
            }
        }
    }
}
