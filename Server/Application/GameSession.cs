namespace Chess.Server
{
    internal class GameSession
    {
        internal string Id { get; init; }
        internal ConnectedUser WhitePlayer { get; init; }
        internal ConnectedUser BlackPlayer { get; init; }
        internal GameHandler GameHandler { get; init; }

        internal GameSession(string id, ConnectedUser whitePlayer, ConnectedUser blackPlayer, GameHandler gameHandler)
        {
            Id = id;
            WhitePlayer = whitePlayer;
            BlackPlayer = blackPlayer;
            GameHandler = gameHandler;
        }

        internal ConnectedUser GetMovingUser() => (GameHandler.GetMovingPlayer().Color == Color.White) ? WhitePlayer : BlackPlayer;

        internal ConnectedUser GetDefendingUser() => (GameHandler.GetMovingPlayer().Color == Color.White) ? BlackPlayer : WhitePlayer;

        internal ConnectedUser GetOtherUser(ConnectedUser user) => (user == WhitePlayer) ? BlackPlayer : WhitePlayer;

        internal Player GetPlayer(ConnectedUser user) => (user == GetMovingUser()) ? GameHandler.GetMovingPlayer() : GameHandler.GetDefendingPlayer();
    }
}
