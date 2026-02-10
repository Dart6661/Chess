namespace Chess.Core
{
    public class Pawn : Figure
    {
        public Pawn(int a, int b, Player owner) : base(a, b, owner)
        {
            Title = FigureType.Pawn;
        }

        public override bool CheckMove(int x, int y)
        {
            bool moveDirection = Color == Color.White && y - B == 1 || Color == Color.Black && B - y == 1;
            bool move = moveDirection && Math.Abs(A - x) == 0;
            return move;
        }

        public override bool CheckTake(int x, int y)
        {
            bool takeDirection = (Color == Color.White && y - B == 1) || (Color == Color.Black && B - y == 1);
            bool take = takeDirection && Math.Abs(A - x) == 1;
            return take;
        }

        internal bool CheckMoveThroughCage(int x, int y)
        {
            bool moveThroughCageDirection = (Color == Color.White && y - B == 2 || Color == Color.Black && B - y == 2);
            bool moveThroughCage = moveThroughCageDirection && Math.Abs(A - x) == 0;
            return moveThroughCage;
        }

        internal override MoveAction? GetMoveAction(int x, int y, Field field)
        {
            Figure? f = field.GetCell(x, y);
            List<(int x, int y)> cells = PathAlgorithm.GetPath(A, B, x, y);
            Figure king = field.GetKing(Color);
            bool moveThroughCage = f == null && CheckMoveThroughCage(x, y) && AmountMovesOfFigure == 0 
                && MoveValidator.PathIsClear(cells, field) && MoveValidator.CellIsSafeAfterMove(king.A, king.B, this, x, y, field);
            bool move = f == null && CheckMove(x, y) && MoveValidator.CellIsSafeAfterMove(king.A, king.B, this, x, y, field);
            bool take = f != null && Color != f.Color && CheckTake(x, y) && MoveValidator.CellIsSafeAfterMove(king.A, king.B, this, x, y, field);
            bool replacement = (move || take) && (Color == Color.White && y == 7 || Color == Color.Black && y == 0);
            bool takeOnPassage = f == null && CheckTake(x, y) && (Color == Color.White && B == 4 || Color == Color.Black && B == 3) 
                && MoveValidator.IsValidTakeOnPassage(this, x, field) && MoveValidator.CellIsSafeAfterMove(king.A, king.B, this, x, y, field);
            if (replacement) return new ReplacementMoveAction(this, x, y, field);
            if (takeOnPassage) return new TakeOnPassageMoveAction(this, x, y, field);
            if (move || take || moveThroughCage) return new RegularMoveAction(this, x, y, field);
            return null;
        }
    }
}