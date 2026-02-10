namespace Chess.Core
{
    public class Queen : Figure
    {
        public Queen(int a, int b, Player owner) : base(a, b, owner)
        {
            Title = FigureType.Queen;
        }

        public override bool CheckMove(int x, int y)
        {
            bool move = (Math.Abs(A - x) == Math.Abs(B - y) && A - x != 0) || (A == x && B != y) || (B == y && A != x);
            return move;
        }

        public override bool CheckTake(int x, int y)
        {
            bool take = (Math.Abs(A - x) == Math.Abs(B - y) && A - x != 0) || (A == x && B != y) || (B == y && A != x);
            return take;
        }

        internal override MoveAction? GetMoveAction(int x, int y, Field field)
        {
            Figure? f = field.GetCell(x, y);
            List<(int x, int y)> cells = PathAlgorithm.GetPath(A, B, x, y);
            Figure king = field.GetKing(Color);
            bool path = MoveValidator.PathIsClear(cells, field);
            bool move = f == null && CheckMove(x, y) && path && MoveValidator.CellIsSafeAfterMove(king.A, king.B, this, x, y, field);
            bool take = f != null && Color != f.Color && CheckTake(x, y) && path && MoveValidator.CellIsSafeAfterMove(king.A, king.B, this, x, y, field);
            if (move || take) 
                return new RegularMoveAction(this, x, y, field);
            return null;
        }
    }
}