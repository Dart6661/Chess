namespace Chess.Client.Cli
{
    internal class Context
    {
        internal UIHandler UIHandler { get; init; }
        internal PlayerState PlayerState { get; init; }
        internal ServerApi ServerApi { get; init; }
        internal ServerEventHandler ServerEventHandler { get; init; }
        internal CancellationTokenSource ReceivingCts { get; init; }

        internal Context(PlayerState playerState, UIHandler uiHandler, 
                 ServerApi serverApi, ServerEventHandler serverEventHandler, CancellationTokenSource receivingCts)
        {
            PlayerState = playerState;
            UIHandler = uiHandler;
            ServerApi = serverApi;
            ServerEventHandler = serverEventHandler;
            ReceivingCts = receivingCts;
        }
    }
}
