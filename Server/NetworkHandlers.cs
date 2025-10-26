namespace Chess.Server
{
    internal class NetworkReader
    {
        internal async Task RunAsync(ConnectedUser user, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                string? message = await user.Connection.ReceiveAsync();
                if (message == null) break;
                await user.Incoming.Writer.WriteAsync(message, token);
            }
        }
    }


    internal class NetworkWriter
    {
        internal async Task RunAsync(ConnectedUser user, CancellationToken token)
        {
            while (await user.Outgoing.Reader.WaitToReadAsync(token))
            {
                while (user.Outgoing.Reader.TryRead(out ResponseDto? responseDto))
                {
                    string response = JsonHandler.Serialize(responseDto);
                    await user.Connection.SendAsync(response);
                }
            }
        }
    }
}
