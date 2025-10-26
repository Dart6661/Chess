using System.Threading.Channels;

namespace Chess.Server
{
    internal class ConnectedUser(string id, TcpClient tcpClient)
    {
        internal string Id { get; init; } = id;
        internal GameSession? Session { get; set; }
        internal Channel<string?> Incoming { get; } = Channel.CreateUnbounded<string?>();
        internal Channel<ResponseDto> Outgoing { get; } = Channel.CreateUnbounded<ResponseDto>();
        internal ClientConnection Connection { get; init; } = new(tcpClient);
        private readonly CancellationTokenSource cts = new();

        internal event Action<ConnectedUser>? Disconnected;

        internal void Start(GameManager gameManager)
        {
            Task reader = Task.Run(() => new NetworkReader().RunAsync(this, cts.Token));
            Task processor = Task.Run(() => new MessageHandler(gameManager).RunAsync(this, cts.Token));
            Task writer = Task.Run(() => new NetworkWriter().RunAsync(this, cts.Token));

            _ = Task.Run(async () =>
            {
                await Task.WhenAny(reader, processor, writer);
                Stop();
            });
        }

        internal void Stop()
        {
            Disconnected?.Invoke(this);
            cts.Cancel();
            Connection.Close();
        }
    }
}
