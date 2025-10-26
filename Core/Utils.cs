namespace Chess.Core
{
    public enum FigureType
    {
        King,
        Queen,
        Rook,
        Bishop,
        Knight,
        Pawn
    }


    public enum Color
    {
        White,
        Black
    }


    public delegate Task<Type> UserSelectionOfReplacement();


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

    
    public static class ChessAlgorithm
    {
        public static List<(int, int)> GetPath(int a, int b, int x, int y)
        {
            if (!MoveValidator.IsValidCoordinates(x, y) || !MoveValidator.IsValidCoordinates(a, b))
                throw new InputException("invalid coordinates");

            List<(int, int)> cells = [];
            if (a == x || b == y)
            {
                int direction = 1;
                int size = Math.Abs(a + b - x - y);
                if (a - x != 0)
                {
                    if (a - x > 0) direction = -1;
                    for (int i = 1; i < size; i++) cells.Add((a + i * direction, b));
                }
                else
                {
                    if (b - y > 0) direction = -1;
                    for (int i = 1; i < size; i++) cells.Add((a, b + i * direction));
                }
            }
            else if (Math.Abs(a - x) == Math.Abs(b - y))
            {
                int direction = 1;
                int size = Math.Abs(a - x);
                if (a - x == b - y)
                {
                    if (a - x > 0) direction = -1;
                    for (int i = 1; i < size; i++) cells.Add((a + i * direction, b + i * direction));
                }
                else
                {
                    if (a - x > 0) direction = -1;
                    for (int i = 1; i < size; i++) cells.Add((a + i * direction, b - i * direction));
                }
            }
            return cells;
        }
    }
}
