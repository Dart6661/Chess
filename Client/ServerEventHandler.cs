namespace Chess.Client.Cli
{
    internal class ServerEventHandler
    {
        private readonly PlayerState playerState;
        private readonly Dictionary<ResponseType, ServerEventItem> serverEventItems;

        internal event Action<ServerEventData>? MessageProcessed;

        internal ServerEventHandler(PlayerState playerState, ResponseHandler responseHandler)
        {
            this.playerState = playerState;
            responseHandler.MessageReceived += HandleServerMessage;
            serverEventItems = InitializeServerEventItems();
        }

        private void HandleServerMessage(ResponseDto responseDto)
        {
            if (!serverEventItems.TryGetValue(responseDto.Type, out ServerEventItem? value))
                throw new ItemNotFoundException();

            ServerEventData serverEventData = value(responseDto);
            MessageProcessed?.Invoke(serverEventData);
        }

        private ServerEventData RandomSessionStarted(ResponseDto responseDto)
        {
            if (responseDto.Data == null || responseDto.SessionId == null) throw new InvalidResponseException(responseDto.Message);
            GameHandlerDto gameHandlerDto = JsonHandler.Deserialize<GameHandlerDto>(responseDto.Data);
            if (gameHandlerDto.ColorOfCurrentPlayer == null) throw new InvalidResponseException(responseDto.Message);
            return new RandomSessionStartedEventData(responseDto.Message, responseDto.SessionId, gameHandlerDto);
        }

        private ServerEventData SessionUpdated(ResponseDto responseDto)
        {
            if (playerState.Session == null ||
                responseDto.SessionId != playerState.Session.Id ||
                responseDto.Status != Status.OK ||
                responseDto.Type != ResponseType.SessionUpdated ||
                responseDto.Data == null)
            {
                throw new InvalidResponseException(responseDto.Message);
            }
            GameHandlerDto gameHandlerDto = JsonHandler.Deserialize<GameHandlerDto>(responseDto.Data);
            if (gameHandlerDto.ColorOfCurrentPlayer == null) throw new InvalidResponseException(responseDto.Message);
            return new SessionUpdatedEventData(responseDto.Message, gameHandlerDto);
        }

        private ServerEventData DefineFigure(ResponseDto responseDto)
        {
            if (responseDto.Status != Status.OK ||
                responseDto.Type != ResponseType.DefineFigure ||
                playerState.Session == null ||
                responseDto.SessionId == null ||
                responseDto.SessionId != playerState.Session.Id)
            {
                throw new InvalidResponseException(responseDto.Message);
            }
            return new DefineFigureEventData(responseDto.Message);
        }

        private ServerEventData SessionInterrupted(ResponseDto responseDto)
        {
            if (responseDto.Status != Status.OK ||
                responseDto.Type != ResponseType.SessionInterrupted ||
                playerState.Session == null ||
                responseDto.SessionId == null || 
                responseDto.SessionId != playerState.Session.Id)
            {
                throw new InvalidResponseException(responseDto.Message);
            }
            return new SessionInterruptedEventData(responseDto.Message);
        }

        private ServerEventData SessionEnded(ResponseDto responseDto)
        {
            if (responseDto.Status != Status.OK ||
                responseDto.Type != ResponseType.SessionEnded ||
                playerState.Session == null ||
                responseDto.SessionId == null ||
                responseDto.SessionId != playerState.Session.Id ||
                responseDto.Data == null)
            {
                throw new InvalidResponseException(responseDto.Message);
            }
            GameHandlerDto gameHandlerDto = JsonHandler.Deserialize<GameHandlerDto>(responseDto.Data);
            if (gameHandlerDto.ColorOfCurrentPlayer == null) throw new InvalidResponseException(responseDto.Message);
            return new SessionEndedEventData(responseDto.Message, gameHandlerDto);
        }

        private ServerEventData UserDisconnected(ResponseDto responseDto)
        {
            if (responseDto.Status != Status.OK ||
                responseDto.Type != ResponseType.UserDisconnected ||
                playerState.Session == null ||
                responseDto.SessionId == null ||
                responseDto.SessionId != playerState.Session.Id)
            {
                throw new InvalidResponseException(responseDto.Message);
            }
            return new UserDisconnectedEventData(responseDto.Message);
        }

        private Dictionary<ResponseType, ServerEventItem> InitializeServerEventItems()
        {
            return new Dictionary<ResponseType, ServerEventItem>()
            {
                { ResponseType.RandomSessionStarted,  RandomSessionStarted},
                { ResponseType.SessionUpdated, SessionUpdated},
                { ResponseType.DefineFigure, DefineFigure},
                { ResponseType.SessionInterrupted,  SessionInterrupted},
                { ResponseType.SessionEnded, SessionEnded},
                { ResponseType.UserDisconnected, UserDisconnected}
            };
        }
    }
}
