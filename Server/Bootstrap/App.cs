namespace Chess.Server
{
    internal class App
    {
        private readonly IPAddress ip = IPAddress.Any;
        private readonly int port = 5000;
        private readonly Server server; 
        private readonly GameManager gameManager;

        internal App()
        {
            server = new(ip, port);
            gameManager = new();
        }

        internal async Task Run()
        {
            await server.Run(gameManager);
        }
    }
}
