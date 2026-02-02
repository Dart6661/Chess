namespace Chess.Shared.DtoMapping
{
    public enum RequestType
    {
        GetUserData,
        Play,
        CreateSession,
        ConnectToSession,
        CancelWaiting,
        MakeMove,
        FigureSelection,
        AbortSession,
        EndSession,
        DisconnectFromGame,
    }


    public enum ResponseType
    {
        Info,
        UserData,
        WaitingForOpponent,
        RandomSessionStarted,
        TargetedSessionStarted,
        WaitCancelled,
        MoveApplied,
        MoveRejected,
        OptionsRequired,
        DefineFigure,
        FigureSelected,
        SessionUpdated,
        SessionInterrupted,
        SessionEnded,
        UserDisconnected
    }


    public enum Status
    {
        OK,
        ERROR
    }


    public class RequestDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public RequestType Type { get; set; }
        public string? SessionId { get; set; }
        public string? Data { get; set; }
    }


    public class ResponseDto
    {
        public string? Id { get; set; }
        public Status Status { get; set; }
        public ResponseType Type { get; set; }
        public string? SessionId { get; set; }
        public string? Message { get; set; }
        public string? Data { get; set; }
    }
}
