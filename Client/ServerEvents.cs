namespace Chess.Client.Cli
{
    internal record ServerEventData(string? Message);

    internal record RandomSessionStartedEventData(string? Message, string SessionId, GameHandlerDto GameHandlerDto) : ServerEventData(Message);

    internal record SessionUpdatedEventData(string? Message, GameHandlerDto GameHandlerDto) : ServerEventData(Message);

    internal record DefineFigureEventData(string? Message) : ServerEventData(Message);

    internal record SessionInterruptedEventData(string? Message) : ServerEventData(Message);

    internal record SessionEndedEventData(string? Message, GameHandlerDto GameHandlerDto) : ServerEventData(Message);

    internal record UserDisconnectedEventData(string? Message) : ServerEventData(Message);
}
