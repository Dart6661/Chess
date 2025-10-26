namespace Chess.Server
{
    internal class GameSession
    {
        internal string Id { get; init; }
        internal ConnectedUser WhitePlayer { get; init; }
        internal ConnectedUser BlackPlayer { get; init; }
        internal GameHandler GameHandler { get; init; }
        private TaskCompletionSource<Type>? replacementTcs;

        internal GameSession(string id, ConnectedUser whitePlayer, ConnectedUser blackPlayer, GameHandler gameHandler)
        {
            Id = id;
            WhitePlayer = whitePlayer;
            BlackPlayer = blackPlayer;
            GameHandler = gameHandler;
            gameHandler.SetFigureSelection(UserSelectionOfReplacementAsync);
        }

        internal ConnectedUser GetMovingUser() => (GameHandler.GetMovingPlayer().Color == Color.White) ? WhitePlayer : BlackPlayer;

        internal ConnectedUser GetDefendingUser() => (GameHandler.GetMovingPlayer().Color == Color.White) ? BlackPlayer : WhitePlayer;

        internal ConnectedUser GetOtherUser(ConnectedUser user) => (user == WhitePlayer) ? BlackPlayer : WhitePlayer;

        internal Player GetPlayer(ConnectedUser user) => (user == GetMovingUser()) ? GameHandler.GetMovingPlayer() : GameHandler.GetDefendingPlayer();
        
        internal void SetReplacement(string selectedFigure)
        {
            Type newFigureType = Type.GetType(selectedFigure) ?? throw new FigureTypeException("figure type is incorrect");
            replacementTcs?.SetResult(newFigureType);
            replacementTcs = null;
        }

        internal void CancelReplacement()
        {
            replacementTcs?.TrySetCanceled();
        }

        private async Task<Type> UserSelectionOfReplacementAsync()
        {
            if (replacementTcs != null && !replacementTcs.Task.IsCompleted) throw new InvalidRequestException("replacement already in progress");

            replacementTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
            ConnectedUser user = GetMovingUser();
            List<ResponseElement> responseElements = RequestHandler.DefineFigure(user);
            await MessageHandler.SendResponse(responseElements);
            return await replacementTcs.Task;
        }
    }
}
