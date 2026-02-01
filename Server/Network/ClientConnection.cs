namespace Chess.Server
{
    internal class ClientConnection
    {
        private readonly TcpClient client;
        private readonly StreamWriter writer;
        private readonly StreamReader reader;

        internal ClientConnection(TcpClient tcpClient)
        {
            client = tcpClient;
            NetworkStream stream = client.GetStream();
            writer = new(stream) { AutoFlush = true };
            reader = new(stream);
        }

        internal async Task SendAsync(string message) => await writer.WriteLineAsync(message);

        internal async Task<string?> ReceiveAsync() => await reader.ReadLineAsync();

        internal void Close()
        {
            writer.Close();
            reader.Close();
            client.Close();
        }
    }
}
