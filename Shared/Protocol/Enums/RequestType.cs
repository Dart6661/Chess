namespace Chess.Shared
{
    public enum RequestType
    {
        GetUserData,
        Play,
        CreateSession,
        ConnectToSession,
        CancelWaiting,
        MakeMove,
        AbortSession,
        EndSession,
        DisconnectFromGame,
    }
}