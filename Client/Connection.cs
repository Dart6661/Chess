using System.Net;
using System.Net.Sockets;

namespace Chess.Client.Cli
{
    internal class Connection
    {
        internal IPAddress IP { get; init; }
        internal int Port { get; init; }
        private readonly TcpClient client;
        private StreamReader? reader;
        private StreamWriter? writer;

        internal Connection(IPAddress ip, int port)
        {
            IP = ip;
            Port = port;
            client = new();
        }

        internal async Task Connect()
        {
            await client.ConnectAsync(IP, Port);
            NetworkStream stream = client.GetStream();
            reader = new(stream);
            writer = new(stream) { AutoFlush = true };
        }

        internal async Task SendAsync(string message)
        {
            if (writer == null)
                throw new InvalidOperationException("not connected yet");

            await writer.WriteLineAsync(message);
        }

        internal async Task<string?> ReceiveAsync()
        {
            if (reader == null)
                throw new InvalidOperationException("not connected yet");

            return await reader.ReadLineAsync();
        }

        internal void Close()
        {
            reader?.Close();
            writer?.Close();
            client.Close();
        }
    }
}
