namespace Chess.Server
{
    internal class Server
    {
        internal IPAddress Ip { get; init; }
        internal int Port { get; init; }
        internal readonly TcpListener server;

        internal Server(IPAddress ip, int port)
        {
            Ip = ip;
            Port = port;
            server = new(ip, port);
        }

        internal async Task Run(GameManager gameManager) 
        {
            try
            {
                server.Start();
                Console.WriteLine("server is running");
                while (true)
                {
                    try
                    {
                        TcpClient client = await server.AcceptTcpClientAsync();
                        ConnectedUser connectedUser = new(Guid.NewGuid().ToString(), client);
                        connectedUser.Start(gameManager);
                    }
                    catch (IOException ex) { Console.WriteLine(ex.Message); }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                }
            }
            finally
            {
                server.Stop();
            }
        }
    }
}
