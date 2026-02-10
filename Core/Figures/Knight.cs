namespace Chess.Core
{
    public class Knight : Figure
    {
        public Knight(int a, int b, Player owner) : base(a, b, owner)
        {
            Title = FigureType.Knight;
        }

        public override bool CheckMove(int x, int y)
        {
            bool move = Math.Abs(A - x) == 1 && Math.Abs(B - y) == 2 || Math.Abs(A - x) == 2 && Math.Abs(B - y) == 1;
            return move;
        }

        public override bool CheckTake(int x, int y)
        {
            bool take = Math.Abs(A - x) == 1 && Math.Abs(B - y) == 2 || Math.Abs(A - x) == 2 && Math.Abs(B - y) == 1;
            return take;
        }

        internal override MoveAction? GetMoveAction(int x, int y, Field field)
        {
            Figure? f = field.GetCell(x, y);
            Figure king = field.GetKing(Color);
            bool move = f == null && CheckMove(x, y) && MoveValidator.CellIsSafeAfterMove(king.A, king.B, this, x, y, field);
            bool take = f != null && Color != f.Color && CheckTake(x, y) && MoveValidator.CellIsSafeAfterMove(king.A, king.B, this, x, y, field);
            if (move || take)
                return new RegularMoveAction(this, x, y, field);
            return null;
        }
    }
}