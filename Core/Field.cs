using System.Collections.ObjectModel;

namespace Chess.Core
{
    public class Field
    {
        public int AmountMovesOnField { get; internal set; } = 0;
        internal Dictionary<int, ChessMove> moves = [];
        private readonly Figure?[,] field = new Figure?[8, 8];

        internal UserSelectionOfReplacement SelectFigure = null!;

        internal Field(Player whitePlayer, Player blackPlayer)
        {
            List<(int, int, Player)> initialPositions = [
                (0, 1,  whitePlayer),
                (7, 6, blackPlayer)
            ];

            foreach (var (figurePosition, pawnPosition, player) in initialPositions)
            {
                InitializeFigure(typeof(Rook), 0, figurePosition, player);
                InitializeFigure(typeof(Knight), 1, figurePosition, player);
                InitializeFigure(typeof(Bishop), 2, figurePosition, player);
                InitializeFigure(typeof(Queen), 3, figurePosition, player);
                InitializeFigure(typeof(King), 4, figurePosition, player);
                InitializeFigure(typeof(Bishop), 5, figurePosition, player);
                InitializeFigure(typeof(Knight), 6, figurePosition, player);
                InitializeFigure(typeof(Rook), 7, figurePosition, player);

                for (int pawnX = 0; pawnX < 8; pawnX++)
                    InitializeFigure(typeof(Pawn), pawnX, pawnPosition, player);
            }
        }

        public int GetCurrentMoveNumber()
        {
            if (AmountMovesOnField == 0 || moves[AmountMovesOnField].blackMoveAction != null) return AmountMovesOnField + 1;
            return AmountMovesOnField;
        }

        public Figure?[,] GetCopyOfField()
        {
            Player whitePlayer = new(Color.White);
            Player blackPlayer = new(Color.Black);
            Figure?[,] fieldCopy = new Figure?[8, 8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Figure? figure = field[i, j];
                    if (figure != null)
                    {
                        Player currentPlayer = (figure.Color == Color.White) ? whitePlayer : blackPlayer;
                        InitializeFigure(figure.GetType(), i, j, currentPlayer, fieldCopy);
                    }
                    else fieldCopy[i, j] = null;
                }
            }

            return fieldCopy;
        }
        
        public Figure?[,] GetCopyOfField(int numberMove, Color playerColor)
        {
            if (numberMove < 1 || numberMove > AmountMovesOnField)
                throw new InputException("invalid move number");

            int currentMove = AmountMovesOnField;
            for (; currentMove > numberMove; currentMove--)
            {
                moves[currentMove].blackMoveAction?.UndoMove(true);
                moves[currentMove].whiteMoveAction?.UndoMove(true);
            }

            if (playerColor == Color.White) 
                moves[currentMove].blackMoveAction?.UndoMove(true);

            Figure?[,] fieldCopy = GetCopyOfField();

            if (playerColor == Color.White)
                moves[currentMove].blackMoveAction?.ExecuteMove(true);

            currentMove++;
            for (; currentMove <= AmountMovesOnField; currentMove++)
            {
                moves[currentMove]?.whiteMoveAction?.ExecuteMove(true);
                moves[currentMove]?.blackMoveAction?.ExecuteMove(true);
            }

            return fieldCopy;
        }
        
        public IReadOnlyDictionary<int, ChessMove> GetMoves() => new ReadOnlyDictionary<int, ChessMove>(moves);

        public Figure? GetCell(int x, int y)
        {
            if (!MoveValidator.IsValidCoordinates(x, y))
                throw new InputException("invalid coordinates");

            return field[x, y];
        }

        public Figure GetKing(Color color)
        {
            Figure? figure = null;
            foreach (Figure? f in field)
            {
                if (f != null && f is King && f.Color == color)
                {
                    figure = GetCell(f.A, f.B);
                    break;
                }
            }
            return figure!;
        }

        internal Figure?[,] GetField()
        {
            Figure?[,] fieldCopy = new Figure?[8, 8];
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    fieldCopy[i, j] = field[i, j];

            return fieldCopy;
        }

        internal void Reposition(int a, int b, int x, int y, bool isReplay = false)
        {
            Figure f = GetCell(a, b)!;
            f.Shift(x, y, isReplay);
            ChangeCell(x, y, f);
            ChangeCell(a, b, null);
        }

        internal void ChangeCell(int x, int y, Figure? f) => field[x, y] = f;

        internal void AddMove(List<(int, int, int, int)> coordinates, MoveAction moveAction)
        {
            if (moves.Count == 0 || moves[AmountMovesOnField].ColorOfMovingPlayer() == Color.White)
            {
                AmountMovesOnField++;
                moves[AmountMovesOnField] = new ChessMove();
                moves[AmountMovesOnField].SetMove(coordinates, moveAction, Color.White);
            }
            else moves[AmountMovesOnField].SetMove(coordinates, moveAction, Color.Black);
        }

        internal bool SelectionMethodIsSet()
        {
            if (SelectFigure == null) 
                throw new ReplacementException("selection method not assigned");
            return true;
        }

        private void InitializeFigure(Type figureType, int x, int y, Player player)
        {
            if (!typeof(Figure).IsAssignableFrom(figureType))
                throw new ArgumentException("entered type must be a type of Figure or its subclass");

            field[x, y] = Activator.CreateInstance(figureType, x, y, player) as Figure;
            player.AddFigure(field[x, y]!);
        }

        private static void InitializeFigure(Type figureType, int x, int y, Player player, Figure?[,] localField)
        {
            if (!typeof(Figure).IsAssignableFrom(figureType))
                throw new ArgumentException("entered type must be a type of Figure or its subclass");

            localField[x, y] = Activator.CreateInstance(figureType, x, y, player) as Figure;
            player.AddFigure(localField[x, y]!);
        }
    }
}