using System;
using System.IO;
using System.Threading.Tasks;
using ConsoleChatApp.Core;

namespace ConsoleChatApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // If "--test" argument is passed, execute a headless automated integration test
            if (args.Length > 0 && args[0].Equals("--test", StringComparison.OrdinalIgnoreCase))
            {
                await RunProgrammaticTestAsync();
                return;
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==================================================");
            Console.WriteLine("       Welcome to Console Chat Application!       ");
            Console.WriteLine("        (OOP Assignment Console Chat App)         ");
            Console.WriteLine("==================================================");
            Console.ResetColor();

            while (true)
            {
                Console.WriteLine("\nSelect Application Mode:");
                Console.WriteLine("1. Run as Server");
                Console.WriteLine("2. Run as Client");
                Console.WriteLine("3. Exit");
                Console.Write("Enter your choice (1-3): ");

                var choice = Console.ReadLine()?.Trim();

                if (choice == "1")
                {
                    await RunServerAsync();
                    break;
                }
                else if (choice == "2")
                {
                    await RunClientAsync();
                    break;
                }
                else if (choice == "3")
                {
                    Console.WriteLine("Goodbye!");
                    break;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid selection. Please enter 1, 2, or 3.");
                    Console.ResetColor();
                }
            }
        }

        private static async Task RunServerAsync()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=== Start Chat Server ===");
            Console.ResetColor();

            Console.Write("Enter Port (Default 8080): ");
            var portInput = Console.ReadLine()?.Trim();
            if (!int.TryParse(portInput, out int port))
            {
                port = 8080;
            }

            var server = new ChatServer(port);
            var serverTask = server.StartAsync();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Server is active. Press 'Q' at any time to shut down the server.");
            Console.ResetColor();

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Q)
                    {
                        server.Stop();
                        break;
                    }
                }
                await Task.Delay(100);
            }

            await serverTask;
            Console.WriteLine("Server execution completed.");
        }

        private static async Task RunClientAsync()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("=== Start Chat Client ===");
            Console.ResetColor();

            string nickname = "";
            while (string.IsNullOrWhiteSpace(nickname))
            {
                Console.Write("Enter your Nickname: ");
                nickname = Console.ReadLine()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(nickname))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Nickname cannot be empty.");
                    Console.ResetColor();
                }
            }

            Console.Write("Enter Server IP (Default 127.0.0.1): ");
            var ipInput = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(ipInput))
            {
                ipInput = "127.0.0.1";
            }

            Console.Write("Enter Server Port (Default 8080): ");
            var portInput = Console.ReadLine()?.Trim();
            if (!int.TryParse(portInput, out int port))
            {
                port = 8080;
            }

            var client = new ChatClient(nickname, ipInput, port);
            await client.StartAsync();

            Console.WriteLine("\nClient shut down. Press any key to return to menu...");
            Console.ReadKey();
            Console.Clear();
        }

        // Simulates automated test conversation without blocking on Console.ReadLine.
        private static async Task RunProgrammaticTestAsync()
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("         STARTING PROGRAMMATIC SYSTEM TEST        ");
            Console.WriteLine("==================================================");

            int testPort = 9099;
            var server = new ChatServer(testPort);
            var serverTask = server.StartAsync();

            // Allow the TCP listener to bind
            await Task.Delay(500);

            // Alice scripts
            var aliceInputs = new StringReader(
                "Hello everyone!\n" +
                "/msg Bob Hi Bob, this is a secret!\n" +
                "/list\n" +
                "/quit\n"
            );
            var aliceClient = new ChatClient("Alice", "127.0.0.1", testPort, aliceInputs);

            // Bob scripts
            var bobInputs = new StringReader(
                "Hey Alice!\n" +
                "Glad to be here.\n" +
                "/quit\n"
            );
            var bobClient = new ChatClient("Bob", "127.0.0.1", testPort, bobInputs);

            Console.WriteLine("[TEST] Launching Alice...");
            var aliceTask = aliceClient.StartAsync();
            await Task.Delay(500); // Wait for Alice connection

            Console.WriteLine("[TEST] Launching Bob...");
            var bobTask = bobClient.StartAsync();

            // Wait for both clients to process their stream lines
            await Task.WhenAll(aliceTask, bobTask);
            Console.WriteLine("[TEST] Both clients disconnected gracefully.");

            // Stop server
            Console.WriteLine("[TEST] Shutting down Server...");
            server.Stop();
            await serverTask;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("==================================================");
            Console.WriteLine("        SYSTEM TEST COMPLETED SUCCESSFULLY!       ");
            Console.WriteLine("==================================================");
            Console.ResetColor();
        }
    }
}
