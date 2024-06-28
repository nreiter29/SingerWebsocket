namespace SingerWebsocket
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var server = "172.30.83.103";
            var port = 54321;

            var chatClient = new ChatClient(server, port);
            await chatClient.StartAsync();
        }
    }
}
