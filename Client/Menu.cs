namespace Chess.Client.Cli
{
    internal abstract class Menu
    {
        protected internal Context Context { get; init; }
        protected internal CancellationTokenSource readingTcs;
        protected internal Dictionary<int, (string description, MenuItem item)> menuItems = [];

        internal Action<Type>? ChangeMenuAction;

        internal Menu(Context context)
        {
            Context = context;
            readingTcs = new();
        }

        internal async Task<Menu?> Run()
        {
            SubscribeOnServerMessage();
            try 
            {
                readingTcs = new();
                int itemNumber = await Context.UIHandler.ReadMenuItemSelection(menuItems, readingTcs.Token);
                if (!menuItems.TryGetValue(itemNumber, out (string description, MenuItem item) value))
                    throw new ItemNotFoundException();

                Menu? currentMenu = await value.item();
                return currentMenu;
            }
            finally { UnsubscribeOnServerMessage(); }
        }

        protected void ChangeMenu(Type menuType) 
        {
            ChangeMenuAction?.Invoke(menuType);
            readingTcs.Cancel();
        }

        protected abstract void HandleServerMessage(ServerEventData serverEventData);

        protected abstract Dictionary<int, (string, MenuItem)> InitializeMenuItems();

        private void SubscribeOnServerMessage() => Context.ServerEventHandler.MessageProcessed += HandleServerMessage;

        private void UnsubscribeOnServerMessage() => Context.ServerEventHandler.MessageProcessed -= HandleServerMessage;
    }

    
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


    internal class GameMenu : Menu
    {
        internal GameMenu(Context context) : base(context)
        {
            menuItems = InitializeMenuItems();
        }

        private async Task<Menu?> MakeMove()
        {
            string? input = await Context.UIHandler.ReadMoveInput(readingTcs.Token);
            (int A, int B, int X, int Y) = MoveHandler.ProcessMove(input);
            (GameHandlerDto gameHandlerDto, bool sessionEnded) = await Context.ServerApi.MakeMove(Context.PlayerState.Session!.Id, (A, B), (X, Y));
            Context.PlayerState.Session.UpdateSession(gameHandlerDto);
            Context.UIHandler.Clear();
            if (sessionEnded)
            {
                Context.UIHandler.DisplayMessage("the session was ended");
                Context.UIHandler.DisplayField(Context.PlayerState.Session.Figures, Context.PlayerState.Session.OwnColor);
                Context.PlayerState.Status = PlayerStatus.Idle;
                Context.PlayerState.Session = null;
                return new MainMenu(Context);
            }
            else
            {
                Context.UIHandler.Clear();
                Context.UIHandler.DisplayMessage("the move was made");
                Context.UIHandler.DisplayField(Context.PlayerState.Session.Figures, Context.PlayerState.Session.OwnColor);
                return this;
            }
        }

        private async Task<Menu?> Back()
        {
            Context.UIHandler.Clear();
            bool interruptionSuccess = await Context.ServerApi.AbortSession(Context.PlayerState.Session!.Id);
            if (interruptionSuccess)
            {
                Context.PlayerState.Status = PlayerStatus.Idle;
                Context.PlayerState.Session = null;
                Context.UIHandler.DisplayMessage("session interrupted");
                return new MainMenu(Context);
            }
            else
            {
                Context.UIHandler.DisplayMessage("the interruption failed");
                return this;
            }
        }

        private Type HandleSessionUpdatedEvent(SessionUpdatedEventData eventData)
        {
            Context.PlayerState.Session!.UpdateSession(eventData.GameHandlerDto);
            Context.UIHandler.Clear();
            Context.UIHandler.DisplayMessage(eventData.Message);
            Context.UIHandler.DisplayField(Context.PlayerState.Session.Figures, Context.PlayerState.Session.OwnColor);
            return typeof(GameMenu);
        }

        private async Task HandleDefineFigureEven(DefineFigureEventData eventData)
        {
            Dictionary<int, (string title, string type)> replacementFigures = GetReplacementFiguresDictionary();
            int figureNumber = await Context.UIHandler.ReadReplacementFigureSelection(replacementFigures, readingTcs.Token);
            if (!replacementFigures.TryGetValue(figureNumber, out (string title, string type) _))
                throw new ItemNotFoundException();

            string figureType = replacementFigures[figureNumber].type;
            bool figureSelectionSuccess = await Context.ServerApi.FigureSelection(Context.PlayerState.Session!.Id, figureType);
            if (!figureSelectionSuccess) Context.UIHandler.DisplayMessage(eventData.Message);

            static Dictionary<int, (string, string)> GetReplacementFiguresDictionary()
            {
                return Figure.GetTypeOfReplacementFigures()
                    .Select((type, index) => new
                    {
                        Index = index + 1,
                        Name = type.Name.ToLower(),
                        FullName = type.AssemblyQualifiedName!
                    })
                    .ToDictionary(
                        x => x.Index,
                        x => (x.Name, x.FullName)
                    );
            }
        }

        private Type HandleSessionInterrupted(SessionInterruptedEventData eventData)
        {
            Context.PlayerState.Status = PlayerStatus.Idle;
            Context.PlayerState.Session = null;
            Context.UIHandler.Clear();
            Context.UIHandler.DisplayMessage(eventData.Message);
            return typeof(MainMenu);
        }

        private Type HandleSessionEndedEvent(SessionEndedEventData eventData)
        {
            Context.PlayerState.Status = PlayerStatus.Idle;
            Context.PlayerState.Session!.UpdateSession(eventData.GameHandlerDto);
            Context.UIHandler.Clear();
            Context.UIHandler.DisplayMessage(eventData.Message);
            Context.UIHandler.DisplayField(Context.PlayerState.Session.Figures, Context.PlayerState.Session.OwnColor);
            Context.PlayerState.Session = null;
            return typeof(MainMenu);
        }

        private Type HandleUserDisconnected(UserDisconnectedEventData eventData)
        {
            Context.PlayerState.Status = PlayerStatus.Idle;
            Context.PlayerState.Session = null;
            Context.UIHandler.Clear();
            Context.UIHandler.DisplayMessage(eventData.Message);
            return typeof(MainMenu);
        }

        protected override async void HandleServerMessage(ServerEventData serverEventData)
        {
            Type? menuType = null;
            switch (serverEventData) 
            {
                case SessionUpdatedEventData eventData: menuType = HandleSessionUpdatedEvent(eventData); break;
                case DefineFigureEventData eventData: await HandleDefineFigureEven(eventData); break;
                case SessionInterruptedEventData eventData: menuType = HandleSessionInterrupted(eventData); break;
                case SessionEndedEventData eventData: menuType = HandleSessionEndedEvent(eventData); break;
                case UserDisconnectedEventData eventData: menuType = HandleUserDisconnected(eventData); break;
                default: Context.UIHandler.DisplayMessage($"unknown event: {serverEventData.GetType().Name}"); break;
            }
            if (menuType != null) ChangeMenu(menuType);
        }

        protected override Dictionary<int, (string, MenuItem)> InitializeMenuItems()
        {
            return new Dictionary<int, (string, MenuItem)>()
            {
                { 1, ("make a move", MakeMove) },
                { 2, ("leave the game", Back) }
            };
        }
    }


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
            bool cancellationSuccess = await Context.ServerApi.CancelWaiting();
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
