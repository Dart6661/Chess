namespace Chess.Client.Cli
{
    internal class GameMenu : Menu
    {
        internal GameMenu(Context context) : base(context)
        {
            menuItems = InitializeMenuItems();
        }

        private async Task<Menu?> MakeMove()
        {
            GameHandlerDto gameHandlerDto;
            bool optionsRequired;
            bool sessionEnded;
            string? input = await Context.UIHandler.ReadMoveInput(readingTcs.Token);
            (int A, int B, int X, int Y) = MoveHandler.ProcessMove(input);
            (gameHandlerDto, optionsRequired, sessionEnded) = await Context.ServerApi.MakeMoveAsync(Context.PlayerState.Session!.Id, (A, B), (X, Y));
            if (optionsRequired)
            {
                Dictionary<int, (string title, string type)> replacementFigures = GetReplacementFiguresDictionary();
                int figureNumber = await Context.UIHandler.ReadReplacementFigureSelection(replacementFigures, readingTcs.Token);
                if (!replacementFigures.TryGetValue(figureNumber, out (string title, string type) _))
                    throw new ItemNotFoundException();

                string figureType = replacementFigures[figureNumber].type;
                (gameHandlerDto, optionsRequired, sessionEnded) = await Context.ServerApi.MakeMoveAsync(Context.PlayerState.Session!.Id, (A, B), (X, Y), figureType);
            }
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
            bool interruptionSuccess = await Context.ServerApi.AbortSessionAsync(Context.PlayerState.Session!.Id);
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

        private static Dictionary<int, (string, string)> GetReplacementFiguresDictionary()
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
}