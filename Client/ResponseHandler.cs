using System.Collections.Concurrent;

namespace Chess.Client.Cli
{
    internal class ResponseHandler
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource<ResponseDto>> pending;
        private readonly Connection connection;

        internal event Action<ResponseDto>? MessageReceived;

        internal ResponseHandler(Connection connection)
        {
            pending = [];
            this.connection = connection;
        }

        internal void RegisterRequestPending(RequestDto request, TaskCompletionSource<ResponseDto> tcs)
        {
            pending[request.Id] = tcs;
        }

        internal async Task ReceiveAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                string response = await connection.ReceiveAsync() ?? throw new DisconnectedException("disconnected from server");
                ResponseDto responseDto = JsonHandler.Deserialize<ResponseDto>(response);
                if (responseDto.Id != null)
                {
                    if (pending.TryRemove(responseDto.Id, out TaskCompletionSource<ResponseDto>? tcs)) 
                        tcs.TrySetResult(responseDto);
                }
                else
                {
                    MessageReceived?.Invoke(responseDto);
                }
            }
            CancelAllPending();
        }

        internal void CancelAllPending()
        {
            foreach (var elem in pending) elem.Value.TrySetCanceled();
            pending.Clear();
        }
    }
}
