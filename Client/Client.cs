using System.Globalization;
using System.Reflection;

namespace Chess.Client.Cli
{
    internal class Client
    {
        private Menu? menu;
        private readonly Context context;
        private readonly CancellationTokenSource receivingCts;

        internal Client(Menu menu, Context context, CancellationTokenSource receivingCts)
        {
            this.context = context;
            this.menu = menu;
            this.receivingCts = receivingCts;
        }

        internal async Task Run()
        {
            while (menu != null)
            {
                try
                {
                    SetChangerMenu();
                    menu = await menu.Run();
                }
                catch (DisconnectedException ex) { context.UIHandler.DisplayMessage(ex.Message); }
                catch (IntegerInputException ex) { context.UIHandler.DisplayMessage(ex.Message); }
                catch (ItemNotFoundException ex) { context.UIHandler.DisplayMessage(ex.Message); }
                catch (DataTypeException ex) { context.UIHandler.DisplayMessage(ex.Message); }
                catch (InvalidResponseException ex) { context.UIHandler.DisplayMessage(ex.Message); }
                catch (ChangingMenuException ex) { context.UIHandler.DisplayMessage(ex.Message); }
                catch (CoordinatesInputException ex) { context.UIHandler.DisplayMessage(ex.Message); }
                catch (InputException ex) { context.UIHandler.DisplayMessage(ex.Message); }
                catch (InvalidOperationException ex) { context.UIHandler.DisplayMessage(ex.Message); }
                catch (OperationCanceledException) { }
                catch (IOException ex) { context.UIHandler.DisplayMessage(ex.Message); }
                catch (Exception ex) { context.UIHandler.DisplayMessage($"{ex.Message}\n{ex.StackTrace}"); }
            }
            receivingCts.Cancel();
            context.ServerApi.Disconnect();
        }

        private void SetChangerMenu()
        {
            menu!.ChangeMenuAction += menuType =>
            {
                menu = Activator.CreateInstance(
                    menuType,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    binder: null,
                    args: [menu.Context],
                    culture: CultureInfo.InvariantCulture
                ) as Menu ?? throw new ChangingMenuException();
            };
        }
    }
}