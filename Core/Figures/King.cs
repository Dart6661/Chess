namespace Chess.Core
{
    public class King : Figure
    {
        public King(int a, int b, Player owner) : base(a, b, owner)
        {
            Title = FigureType.King;
        }

        public override bool CheckMove(int x, int y)
        {
            bool move = Math.Abs(A - x) == 0 && Math.Abs(B - y) == 1 || Math.Abs(A - x) == 1 && Math.Abs(B - y) == 0 || Math.Abs(A - x) == 1 && Math.Abs(B - y) == 1;
            return move;
        }

        public override bool CheckTake(int x, int y)
        {
            bool take = Math.Abs(A - x) == 0 && Math.Abs(B - y) == 1 || Math.Abs(A - x) == 1 && Math.Abs(B - y) == 0 || Math.Abs(A - x) == 1 && Math.Abs(B - y) == 1;
            return take;
        }

        internal bool CheckCastling(int x, int y)
        {
            bool castling = Math.Abs(A - x) == 2 && B == y;
            return castling;
        }

        internal override MoveAction? CheckMovement(int x, int y, Field field)
        {
            Figure? f = field.GetCell(x, y);
            List<(int x, int y)> cells = PathAlgorithm.GetPath(A, B, x, y);
            bool castling = f == null && AmountMovesOfFigure == 0 && CheckCastling(x, y) && MoveValidator.CellIsSafe(x, y, Color, field) &&
                MoveValidator.PathIsClear(cells, field) && MoveValidator.PathIsSafe(cells, Color, field) && MoveValidator.IsValidCastling(this, x, field);
            bool move = f == null && CheckMove(x, y) && MoveValidator.FigureIsSafeAfterMove(x, y, this, field);
            bool take = f != null && Color != f.Color && CheckTake(x, y) && MoveValidator.FigureIsSafeAfterMove(x, y, this, field);
            if (castling) return new CastlingMoveAction(this, x, y, field);
            if (move || take) return new RegularMoveAction(this, x, y, field);
            return null;
        }
    }
}