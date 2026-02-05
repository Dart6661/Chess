namespace Chess.Client.Cli
{
    internal class WaitingMenu : Menu
    {
        internal WaitingMenu(Context context) : base(context)
        {
            menuItems = InitializeMenuItems();
        }

        private async Task<Menu?> CancelWaiting()
        {
            Context.UIHandler.Clear();
            Context.PlayerState.Status = PlayerStatus.Idle;
            bool cancellationSuccess = await Context.ServerApi.CancelWaitingAsync();
            if (cancellationSuccess) return new MainMenu(Context);
            else
            {
                Context.UIHandler.DisplayMessage("the cancellation failed");
                return this;
            }
        }

        private Task<Menu?> Exit()
        {
            return Task.FromResult<Menu?>(null);
        }

        private Type HandleRandomSessionStartedEvent(RandomSessionStartedEventData eventData)
        {
            Context.PlayerState.Status = PlayerStatus.Playing;
            Color otherPlayerColor = (eventData.GameHandlerDto.ColorOfCurrentPlayer == Color.White) ? Color.Black : Color.White;
            string otherPlayerId = (otherPlayerColor == Color.White) ? eventData.GameHandlerDto.WhitePlayerId : eventData.GameHandlerDto.BlackPlayerId;
            Context.PlayerState.Session = new(eventData.SessionId, (Color)eventData.GameHandlerDto.ColorOfCurrentPlayer!, otherPlayerColor, otherPlayerId, eventData.GameHandlerDto);
            Context.UIHandler.Clear();
            Context.UIHandler.DisplayMessage(eventData.Message);
            Context.UIHandler.DisplayField(Context.PlayerState.Session.Figures, Context.PlayerState.Session.OwnColor);
            return typeof(GameMenu);
        }

        protected override void HandleServerMessage(ServerEventData serverEventData)
        {
            Type? menuType = null;
            switch (serverEventData)
            {
                case RandomSessionStartedEventData eventData: menuType = HandleRandomSessionStartedEvent(eventData); break;
                default: Context.UIHandler.DisplayMessage($"unknown event: {serverEventData.GetType().Name}"); break;
            }
            if (menuType != null) ChangeMenu(menuType);
        }

        protected override Dictionary<int, (string, MenuItem)> InitializeMenuItems()
        {
            return new Dictionary<int, (string, MenuItem)>()
            {
                { 1, ("cancel the wait", CancelWaiting) },
                { 2, ("quit the app", Exit) }
            };
        }
    }
}