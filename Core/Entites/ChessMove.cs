namespace Chess.Core
{
    public class ChessMove
    {
        internal List<(int, int, int, int)> whiteMoves = [];
        internal List<(int, int, int, int)> blackMoves = [];
        internal MoveAction? whiteMoveAction;
        internal MoveAction? blackMoveAction;

        public Color ColorOfMovingPlayer() => (whiteMoves.Count == 0 || whiteMoves.Count != 0 && blackMoves.Count != 0) ? Color.White : Color.Black;
        
        public IReadOnlyList<(int, int, int, int)> GetWhiteMoves() => whiteMoves.AsReadOnly();

        public IReadOnlyList<(int, int, int, int)> GetBlackMoves() => blackMoves.AsReadOnly();

        internal void SetMove(List<(int, int, int, int)> moves, MoveAction moveAction, Color color)
        {
            if (color == Color.White)
            {
                whiteMoves = moves;
                whiteMoveAction = moveAction;
            }
            else
            {
                blackMoves = moves;
                blackMoveAction = moveAction;
            }
        }
    }
}