namespace Chess.Shared
{
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
        SessionUpdated,
        SessionInterrupted,
        SessionEnded,
        UserDisconnected
    }
}