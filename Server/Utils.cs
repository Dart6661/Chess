namespace Chess.Server
{
    public enum TurnStatus
    {
        WhiteTurn,
        BlackTurn
    }

    public enum UserType
    {
        ConnectedUser,
        PlayingUser,
        WaitingRandomUser,
        WaitingSpecificUser,
    }


    internal record ResponseElement(ResponseDto ResponseDto, ConnectedUser ConnectedUser);

    internal delegate List<ResponseElement> RequestProcessor(RequestDto requestDto, ConnectedUser connectedUser);

    internal delegate Task ClientConnectionProcessing(TcpClient client);


    public static class RequestValidator
    { 
        public static bool CheckRequestType(RequestDto request, RequestType requiredType)
        {
            if (request.Type != requiredType) throw new InvalidRequestException("the type of action is incorrect");
            return true;
        }
        
        public static T GetData<T>(string? data)
        {
            if (data == null) throw new InvalidRequestException("the data is empty");
            return JsonHandler.Deserialize<T>(data);
        }
    }
}
