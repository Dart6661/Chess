using System.Net;

namespace Chess.Client.Cli
{
    internal class AppFactory
    {
        internal static async Task<Client> CreateAsync(IPAddress ip, int port)
        {
            var connection = new Connection(ip, port);
            var responseHandler = new ResponseHandler(connection);

            var serverApi = new ServerApi(connection, responseHandler);
            var receivingCts = new CancellationTokenSource();
            await serverApi.ConnectToServer(receivingCts.Token);

            var userData = await serverApi.GetUserDataAsync();
            var playerState = new PlayerState(userData.UserId);

            var uiHandler = new UIHandler();
            var serverEventHandler = new ServerEventHandler(playerState, responseHandler);

            var context = new Context(playerState, uiHandler, serverApi, serverEventHandler, receivingCts);
            var startMenu = new MainMenu(context);

            return new Client(startMenu, context, receivingCts);
        }
    }
}
