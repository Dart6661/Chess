namespace Chess.Server
{
    internal class MessageHandler(GameManager manager)
    {
        private readonly RequestHandler requestHandler = new(manager);

        internal async Task RunAsync(ConnectedUser user, CancellationToken token)
        {
            user.Disconnected += HandleUserDisconnectionAsync;
            while (await user.Incoming.Reader.WaitToReadAsync(token))
            {
                while (user.Incoming.Reader.TryRead(out string? request))
                {
                    _ = Task.Run(() => HandleRequest(request, user), token);
                }
            }
        }

        internal static async Task SendResponse(List<ResponseElement> responseElements)
        {
            foreach ((ResponseDto responseDto, ConnectedUser connectedUser) in responseElements)
            {
                Console.WriteLine($"user {connectedUser.Id} response: {responseDto.Type}");
                await connectedUser.Outgoing.Writer.WriteAsync(responseDto);
            }
        }

        private async Task HandleRequest(string? request, ConnectedUser user)
        {
            try
            {
                if (request == null) throw new EmptyRequestException("the request is empty");
                RequestDto requestDto = JsonHandler.Deserialize<RequestDto>(request);
                Console.WriteLine($"user {user.Id} requested: {requestDto.Type}");
                List<ResponseElement> responseElements = requestHandler.ProcessRequest(requestDto, user);
                await SendResponse(responseElements);
                Console.WriteLine();
            }
            catch (Exception ex) when (ex is EmptyRequestException)
            {
                List<ResponseElement> responseElements = RequestHandler.CreateErrorResponse(null, user, ex.Message);
                await SendResponse(responseElements);
                Console.WriteLine();
            }
        }

        private async void HandleUserDisconnectionAsync(ConnectedUser user)
        {
            Console.WriteLine($"user {user.Id} disconnected\n");
            List<ResponseElement> responseElements = requestHandler.DisconnectUser(user);
            await SendResponse(responseElements);
            Console.WriteLine();
        }
    }
}
