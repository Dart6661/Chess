namespace Chess.Server
{
    internal class RequestController
    {
        private readonly GameManager gameManager;
        private readonly Dictionary<RequestType, RequestProcessor> messageHandlers;

        internal RequestController(GameManager manager)
        {
            gameManager = manager;
            messageHandlers = InitializeMessageHandlers();
        }

        internal List<ResponseElement> ProcessRequest(RequestDto requestDto, ConnectedUser user)
        {
            try
            {
                if (!messageHandlers.TryGetValue(requestDto.Type, out RequestProcessor? messageHandler))
                    throw new RequestTypeException("the type of action is not recognized");
                return messageHandler(requestDto, user);
            }
            catch (Exception ex) when (
                ex is JsonException ||
                ex is RequestTypeException ||
                ex is InvalidRequestException)
            {
                return CreateErrorResponse(requestDto.Id, user, ex.Message);
            }
        }

        internal List<ResponseElement> DisconnectUser(ConnectedUser connectedUser)
        {
            List<ResponseElement> responseElements = [];
            GameSession? session = connectedUser.Session;
            if (session != null)
            {
                ConnectedUser otherUser = session.GetOtherUser(connectedUser);
                ResponseDto response = new()
                {
                    Status = Status.OK,
                    Type = ResponseType.UserDisconnected,
                    SessionId = session.Id,
                    Message = $"user {connectedUser.Id} disconnected"
                };
                responseElements.Add(new ResponseElement(response, otherUser));
                gameManager.RemoveUser(otherUser, UserType.PlayingUser);
                gameManager.RemoveSession(session);
                otherUser.Session = null;
            }
            gameManager.RemoveUser(connectedUser, UserType.ConnectedUser);
            return responseElements;
        }

        internal static List<ResponseElement> CreateErrorResponse(string? responsetId, ConnectedUser user, string? message)
        {
            ResponseDto responseDto = new()
            {
                Id = responsetId,
                Status = Status.ERROR,
                Type = ResponseType.Info,
                Message = message
            };
            return [new ResponseElement(responseDto, user)];
        }

        private List<ResponseElement> GetUserData(RequestDto request, ConnectedUser connectedUser)
        {
            RequestValidator.CheckRequestType(request, RequestType.GetUserData);
            List<ResponseElement> responseElements = [];
            GameSession? session = connectedUser.Session;
            Color? color = (session != null) ? (session.WhitePlayer == connectedUser) ? Color.White : Color.Black : null;
            ConnectedUser? otherUser = session?.GetOtherUser(connectedUser);
            UserDataDto UserDataDto = new(connectedUser.Id, session?.Id, color, otherUser?.Id);
            ResponseDto response = new()
            {
                Id = request.Id,
                Status = Status.OK,
                Type = ResponseType.UserData,
                Message = "user data",
                Data = JsonHandler.Serialize(UserDataDto)
            };
            responseElements.Add(new ResponseElement(response, connectedUser));
            return responseElements;
        }

        private List<ResponseElement> Play(RequestDto request, ConnectedUser connectedUser)
        {
            RequestValidator.CheckRequestType(request, RequestType.Play);
            List<ResponseElement> responseElements = [];
            if (gameManager.UsersListEmpty(UserType.WaitingRandomUser))
            {
                gameManager.AddUser(connectedUser, UserType.WaitingRandomUser);
                ResponseDto response = new()
                {
                    Id = request.Id,
                    Status = Status.OK,
                    Type = ResponseType.WaitingForOpponent,
                    Message = "waiting for opponent"
                };
                responseElements.Add(new ResponseElement(response, connectedUser));
            }
            else
            {
                ConnectedUser otherUser = gameManager.GetAnyUser(UserType.WaitingRandomUser)!;
                gameManager.RemoveUser(otherUser, UserType.WaitingRandomUser);
                GameSession session = new(Guid.NewGuid().ToString(), otherUser, connectedUser, new GameHandler());
                connectedUser.Session = session;
                otherUser.Session = session;
                gameManager.AddSession(session);
                gameManager.AddUser(connectedUser, UserType.PlayingUser);
                gameManager.AddUser(otherUser, UserType.PlayingUser);

                ResponseDto responseToConnectedUser = new()
                {
                    Id = request.Id,
                    Status = Status.OK,
                    Type = ResponseType.RandomSessionStarted,
                    SessionId = session.Id,
                    Message = "connection is established",
                    Data = JsonHandler.Serialize(Mapper.GameHandlerToDto(session.GameHandler, session.WhitePlayer.Id, session.BlackPlayer.Id, session.GetPlayer(connectedUser).Color))
                };
                ResponseDto responseToOtherUser = new()
                {
                    Status = Status.OK,
                    Type = ResponseType.RandomSessionStarted,
                    SessionId = session.Id,
                    Message = "connection is established",
                    Data = JsonHandler.Serialize(Mapper.GameHandlerToDto(session.GameHandler, session.WhitePlayer.Id, session.BlackPlayer.Id, session.GetPlayer(otherUser).Color))
                };
                responseElements.Add(new ResponseElement(responseToConnectedUser, connectedUser));
                responseElements.Add(new ResponseElement(responseToOtherUser, otherUser));
            }
            return responseElements;
        }

        private List<ResponseElement> CancelWaiting(RequestDto request, ConnectedUser connectedUser)
        {
            RequestValidator.CheckRequestType(request, RequestType.CancelWaiting);
            List<ResponseElement> responseElements = [];
            gameManager.RemoveUser(connectedUser, UserType.WaitingRandomUser);
            gameManager.AddUser(connectedUser, UserType.ConnectedUser);
            ResponseDto response = new()
            {
                Id = request.Id,
                Status = Status.OK,
                Type = ResponseType.WaitCancelled,
                Message = "the wait has been cancelled",
            };
            responseElements.Add(new ResponseElement(response, connectedUser));
            return responseElements;
        }

        private List<ResponseElement> MakeMove(RequestDto request, ConnectedUser connectedUser)
        {
            RequestValidator.CheckRequestType(request, RequestType.MakeMove);
            GameSession? session = connectedUser.Session ?? throw new InvalidRequestException("the user does not have an active session");
            if (connectedUser != session.GetMovingUser()) throw new InvalidRequestException("it is not your turn");

            List<ResponseElement> responseElements = [];
            ConnectedUser otherUser = session.GetOtherUser(connectedUser);
            ChessMoveDto move = RequestValidator.GetData<ChessMoveDto>(request.Data);
            try
            {
                session.GameHandler.MakeMove(move.A, move.B, move.X, move.Y, [.. move.Options.Select(Mapper.DtoToMoveOption)]);
                ResponseDto responseToMovingPlayer = new()
                {
                    Id = request.Id,
                    Type = ResponseType.MoveApplied,
                    Status = Status.OK,
                    SessionId = session.Id,
                    Message = "the move was made",
                    Data = JsonHandler.Serialize(Mapper.GameHandlerToDto(session.GameHandler, session.WhitePlayer.Id, session.BlackPlayer.Id, session.GetPlayer(connectedUser).Color))
                };
                ResponseDto responseToDefendingPlayer = new()
                {
                    Status = Status.OK,
                    Type = ResponseType.SessionUpdated,
                    SessionId = session.Id,
                    Message = "the session has been updated",
                    Data = JsonHandler.Serialize(Mapper.GameHandlerToDto(session.GameHandler, session.WhitePlayer.Id, session.BlackPlayer.Id, session.GetPlayer(otherUser).Color))
                };
                responseElements.Add(new ResponseElement(responseToMovingPlayer, connectedUser));
                responseElements.Add(new ResponseElement(responseToDefendingPlayer, otherUser));
            }
            catch (OptionException ex)
            {
                ResponseDto response = new()
                {
                    Id = request.Id,
                    Status = Status.OK,
                    Type = ResponseType.OptionsRequired,
                    SessionId = session.Id,
                    Message = ex.Message,
                    Data = JsonHandler.Serialize(Mapper.GameHandlerToDto(session.GameHandler, session.WhitePlayer.Id, session.BlackPlayer.Id, session.GetPlayer(connectedUser).Color))
                };
                responseElements.Add(new ResponseElement(response, connectedUser));
            }
            catch (Exception ex) when (
                ex is InputException ||
                ex is MovementException ||
                ex is ReplacementException)
            {
                ResponseDto response = new()
                {
                    Id = request.Id,
                    Status = Status.ERROR,
                    Type = ResponseType.MoveRejected,
                    SessionId = session.Id,
                    Message = ex.Message,
                    Data = JsonHandler.Serialize(Mapper.GameHandlerToDto(session.GameHandler, session.WhitePlayer.Id, session.BlackPlayer.Id, session.GetPlayer(connectedUser).Color))
                };
                responseElements.Add(new ResponseElement(response, connectedUser));
            }
            catch (Exception ex) when (
                ex is CheckMateException ||
                ex is StaleMateException)
            {
                connectedUser.Session = null;
                otherUser.Session = null;
                gameManager.RemoveSession(session);
                gameManager.RemoveUser(connectedUser, UserType.PlayingUser);
                gameManager.AddUser(connectedUser, UserType.ConnectedUser);
                gameManager.RemoveUser(otherUser, UserType.PlayingUser);
                gameManager.AddUser(otherUser, UserType.ConnectedUser);
                ResponseDto responseToMovingPlayer = new()
                {
                    Id = request.Id,
                    Status = Status.OK,
                    Type = ResponseType.SessionEnded,
                    SessionId = session.Id,
                    Message = ex.Message,
                    Data = JsonHandler.Serialize(Mapper.GameHandlerToDto(session.GameHandler, session.WhitePlayer.Id, session.BlackPlayer.Id, session.GetPlayer(connectedUser).Color))
                };
                ResponseDto responseToDefendingPlayer = new()
                {
                    Status = Status.OK,
                    Type = ResponseType.SessionEnded,
                    SessionId = session.Id,
                    Message = ex.Message,
                    Data = JsonHandler.Serialize(Mapper.GameHandlerToDto(session.GameHandler, session.WhitePlayer.Id, session.BlackPlayer.Id, session.GetPlayer(otherUser).Color))
                };
                responseElements.Add(new ResponseElement(responseToMovingPlayer, connectedUser));
                responseElements.Add(new ResponseElement(responseToDefendingPlayer, otherUser));
            }
            return responseElements;
        }

        private List<ResponseElement> AbortSession(RequestDto request, ConnectedUser connectedUser)
        {
            RequestValidator.CheckRequestType(request, RequestType.AbortSession);
            List<ResponseElement> responseElements = [];
            GameSession session = connectedUser.Session ?? throw new InvalidRequestException("the type of action is incorrect");
            ConnectedUser otherUser = session.GetOtherUser(connectedUser);
            connectedUser.Session = null;
            otherUser.Session = null;
            gameManager.RemoveSession(session);
            gameManager.RemoveUser(connectedUser, UserType.PlayingUser);
            gameManager.AddUser(connectedUser, UserType.ConnectedUser);
            gameManager.RemoveUser(otherUser, UserType.PlayingUser);
            gameManager.AddUser(otherUser, UserType.ConnectedUser);
            ResponseDto responseToConnectedUser = new()
            {
                Id = request.Id,
                Status = Status.OK,
                Type = ResponseType.SessionInterrupted,
                SessionId = session.Id,
                Message = "the session was interrupted"
            };
            ResponseDto responseToOtherUser = new()
            {
                Status = Status.OK,
                Type = ResponseType.SessionInterrupted,
                SessionId = session.Id,
                Message = "the session was interrupted by another player"
            };
            responseElements.Add(new ResponseElement(responseToConnectedUser, connectedUser));
            responseElements.Add(new ResponseElement(responseToOtherUser, otherUser));
            return responseElements;
        }

        private Dictionary<RequestType, RequestProcessor> InitializeMessageHandlers()
        {
            return new Dictionary<RequestType, RequestProcessor>()
            {
                { RequestType.GetUserData, GetUserData},
                { RequestType.Play, Play },
                { RequestType.CancelWaiting, CancelWaiting },
                { RequestType.MakeMove, MakeMove },
                { RequestType.AbortSession, AbortSession }
            };
        }
    }
}
