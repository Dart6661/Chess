namespace Chess.Core
{
    public class Rook : Figure
    {
        public Rook(int a, int b, Player owner) : base(a, b, owner)
        {
            Title = FigureType.Rook;
        }

        public override bool CheckMove(int x, int y)
        {
            bool move = A == x && B != y || B == y && A != x;
            return move;
        }

        public override bool CheckTake(int x, int y)
        {
            bool take = A == x && B != y || B == y && A != x;
            return take;
        }

        internal override MoveAction? CheckMovement(int x, int y, Field field)
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