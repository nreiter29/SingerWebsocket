using System.Net.Sockets;
using System.Text;

namespace SingerWebsocket
{
    /// <summary>
    /// Represents a chat client that can connect to a server and send/receive messages.
    /// </summary>
    public class ChatClient
    {
        private readonly string server;
        private readonly int port;
        private bool skipWait = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatClient"/> class.
        /// </summary>
        /// <param name="server">The server to connect to.</param>
        /// <param name="port">The port to connect on.</param>
        public ChatClient(string server, int port)
        {
            this.server = server;
            this.port = port;
        }

        /// <summary>
        /// Starts the chat client, connecting to the server and beginning to send/receive messages.
        /// </summary>
        public async Task StartAsync()
        {
            try
            {
                using (var client = new TcpClient(server, port))
                {
                    Console.WriteLine("Connected to server.");

                    using (var stream = client.GetStream())
                    using (var reader = new StreamReader(stream, Encoding.ASCII))
                    using (var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true })
                    {
                        Task listenTask = ListenForMessagesAsync(reader);
                        Task<string> inputTask = Task.Run(() => Console.ReadLine());

                        while (true)
                        {
                            var completedTask = await Task.WhenAny(listenTask, inputTask);

                            if (completedTask == listenTask)
                            {
                                listenTask = ListenForMessagesAsync(reader);
                            }
                            else if (completedTask == inputTask)
                            {
                                var input = inputTask.Result;

                                if (string.IsNullOrEmpty(input)) continue;
                                if (input.Contains("exit"))
                                {
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine($"Sending message: {input}");
                                    if (input.ToUpper().Contains("BROADCAST") || input.ToUpper().Contains("SEND"))
                                    {
                                        skipWait = true;
                                        Console.WriteLine("Enter message: ");
                                    }
                                    else
                                    {
                                        skipWait = false;
                                    }

                                    await writer.WriteLineAsync(input.Replace(" ", "\n") + "\n.");
                                }

                                inputTask = Task.Run(() => Console.ReadLine());
                            }
                        }

                        if (listenTask != null && !listenTask.IsCompleted)
                        {
                            await listenTask;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Listens for messages from the server.
        /// </summary>
        /// <param name="reader">The StreamReader to read messages from.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task<bool> ListenForMessagesAsync(StreamReader reader)
        {
            string response;
            while ((response = await reader.ReadLineAsync()) != null)
            {
                Console.WriteLine(response);
                if (!skipWait && response.Trim() == ".")
                {
                    Console.WriteLine("Enter command: ");
                    return false;
                }
                else if (skipWait)
                {
                    Console.WriteLine("Enter command: ");
                    skipWait = false;
                    return false;
                }
            }

            return true;
        }
    }
}
