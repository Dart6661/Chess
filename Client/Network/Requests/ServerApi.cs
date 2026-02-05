namespace Chess.Client.Cli
{
    internal class ServerApi
    {
        private readonly Connection connection;
        private readonly ResponseHandler responseHandler;
        
        internal ServerApi(Connection connection, ResponseHandler responseHandler)
        {
            this.connection = connection;
            this.responseHandler = responseHandler;
        }

        internal async Task ConnectToServerAsync(CancellationToken ct)
        {
            await connection.Connect();
            _ = responseHandler.ReceiveAsync(ct);
        }
        
        internal async Task<UserDataDto> GetUserDataAsync()
        {
            RequestDto requestDto = new() { Type = RequestType.GetUserData };
            ResponseDto responseDto = await CreateTaskAndSend(requestDto).Task;
            if (responseDto.Id != requestDto.Id ||
                responseDto.Status != Status.OK ||
                responseDto.Type != ResponseType.UserData ||
                responseDto.Data == null)
            {
                throw new InvalidResponseException(responseDto.Message);
            }
            UserDataDto userDataDto = JsonHandler.Deserialize<UserDataDto>(responseDto.Data);
            return userDataDto;
        }
        
        internal async Task<(string, Color, Color, string, GameHandlerDto)?> PlayAsync()
        {
            RequestDto requestDto = new() { Type = RequestType.Play };
            ResponseDto responseDto = await CreateTaskAndSend(requestDto).Task;
            if (responseDto.Id != requestDto.Id ||
                responseDto.Status != Status.OK ||
                responseDto.Type != ResponseType.WaitingForOpponent &&
                responseDto.Type != ResponseType.RandomSessionStarted)
            {
                throw new InvalidResponseException(responseDto.Message);
            }
            GameHandlerDto? gameHandlerDto = (responseDto.Data != null && responseDto.SessionId != null) ? JsonHandler.Deserialize<GameHandlerDto>(responseDto.Data) : null;

            if (gameHandlerDto == null) return null;
            if (gameHandlerDto.ColorOfCurrentPlayer == null) throw new InvalidResponseException(responseDto.Message);

            Color otherPlayerColor = (gameHandlerDto.ColorOfCurrentPlayer == Color.White) ? Color.Black : Color.White;
            string otherPlayerId = (otherPlayerColor == Color.White) ? gameHandlerDto.WhitePlayerId : gameHandlerDto.BlackPlayerId;
            return ((string, Color, Color, string, GameHandlerDto))(responseDto.SessionId!, gameHandlerDto.ColorOfCurrentPlayer, otherPlayerColor, otherPlayerId, gameHandlerDto);
        }

        internal async Task<bool> CancelWaitingAsync()
        {
            RequestDto requestDto = new() { Type = RequestType.CancelWaiting };
            ResponseDto responseDto = await CreateTaskAndSend(requestDto).Task;
            if (responseDto.Id != requestDto.Id ||
                responseDto.Status != Status.OK ||
                responseDto.Type != ResponseType.WaitCancelled)
            {
                throw new InvalidResponseException(responseDto.Message);
            }
            return true;
        }

        internal async Task<(GameHandlerDto, bool, bool)> MakeMoveAsync(string sessionId, (int A, int B) oldCoord, (int X, int Y) newCoord, string? figureType = null)
        {
            string data = JsonHandler.Serialize(new ChessMoveDto(oldCoord.A, oldCoord.B, newCoord.X, newCoord.Y, figureType != null ? [new ReplacementOptionDto(figureType)] : []));
            RequestDto requestDto = new() { Type = RequestType.MakeMove, SessionId = sessionId, Data = data };
            ResponseDto responseDto = await CreateTaskAndSend(requestDto).Task;
            if (responseDto.Id != requestDto.Id ||
                responseDto.Status != Status.OK ||
                responseDto.Type != ResponseType.MoveApplied &&
                responseDto.Type != ResponseType.OptionsRequired &&
                responseDto.Type != ResponseType.SessionEnded ||
                responseDto.SessionId != sessionId ||
                responseDto.Data == null)
            {
                throw new InvalidResponseException(responseDto.Message);
            }
            GameHandlerDto gameHandlerDto = JsonHandler.Deserialize<GameHandlerDto>(responseDto.Data);
            if (gameHandlerDto.ColorOfCurrentPlayer == null) throw new InvalidResponseException(responseDto.Message);
            bool optionsRequired = responseDto.Type == ResponseType.OptionsRequired;
            bool sessionEnded = responseDto.Type == ResponseType.SessionEnded;
            return (gameHandlerDto, optionsRequired, sessionEnded);
        }

        internal async Task<bool> AbortSessionAsync(string sessionId)
        {
            RequestDto requestDto = new() { Type = RequestType.AbortSession, SessionId = sessionId };
            ResponseDto responseDto = await CreateTaskAndSend(requestDto).Task;
            if (responseDto.Id != requestDto.Id ||
                responseDto.Status != Status.OK ||
                responseDto.Type != ResponseType.SessionInterrupted || 
                responseDto.SessionId != sessionId)
            {
                throw new InvalidResponseException(responseDto.Message);
            }
            return true;
        }

        internal void Disconnect()
        {
            connection.Close();
            responseHandler.CancelAllPending();
        }

        private TaskCompletionSource<ResponseDto> CreateTaskAndSend(RequestDto requestDto)
        {
            string request = JsonHandler.Serialize(requestDto);
            TaskCompletionSource<ResponseDto> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
            responseHandler.RegisterRequestPending(requestDto, tcs);
            _ = connection.SendAsync(request);
            return tcs;
        }
    }
}
