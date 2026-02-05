namespace Chess.Client.Cli
{
    internal class MainMenu : Menu
    {
        internal MainMenu(Context context) : base(context)
        {
            menuItems = InitializeMenuItems();
        }
        
        private async Task<Menu?> Play()
        {
            Context.UIHandler.Clear();
            (string sessionId, Color ownColor, Color otherPlayerColor, string otherPlayerId, GameHandlerDto gameHandlerDto)? sessionData = await Context.ServerApi.PlayAsync();
            if (sessionData == null)
            {
                Context.PlayerState.Status = PlayerStatus.WaitingRandomSession;
                Context.UIHandler.DisplayMessage("waiting for opponent");
                return new WaitingMenu(Context);
            }
            else
            {
                Context.PlayerState.Status = PlayerStatus.Playing;
                Context.PlayerState.Session = new Session(sessionData.Value.sessionId, sessionData.Value.ownColor, sessionData.Value.otherPlayerColor, 
                                                          sessionData.Value.otherPlayerId, sessionData.Value.gameHandlerDto);
                Context.UIHandler.DisplayMessage("game started");
                Context.UIHandler.DisplayField(Context.PlayerState.Session.Figures, Context.PlayerState.Session.OwnColor);
                return new GameMenu(Context);
            }
        }

        private Task<Menu?> ShowData()
        {
            Context.UIHandler.Clear();
            Context.UIHandler.DisplayMessage(Context.PlayerState.Id.ToString());
            Context.UIHandler.DisplayMessage(Context.PlayerState.Status.ToString());
            return Task.FromResult<Menu?>(this);
        }

        private Task<Menu?> Exit()
        {
            return Task.FromResult<Menu?>(null);
        }

        protected override void HandleServerMessage(ServerEventData serverEventData)
        {
            Type? menuType = null;
            switch (serverEventData)
            {
                default: Context.UIHandler.DisplayMessage($"unknown event: {serverEventData.GetType().Name}"); break;
            }
            if (menuType != null) ChangeMenu(menuType);
        }

        protected override Dictionary<int, (string, MenuItem)> InitializeMenuItems()
        {
            return new Dictionary<int, (string, MenuItem)>()
            {
                { 1, ("play with a random opponent", Play) },
                { 2, ("show data", ShowData) },
                { 3, ("quit the app", Exit) }
            };
        }
    }
}