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
}
