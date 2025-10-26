namespace Chess.Core
{
    public abstract class Figure(int a, int b, Player owner)
    {
        public int A { get; protected set; } = a;
        public int B { get; protected set; } = b;
        public Player Owner { get; protected set; } = owner;
        public int AmountMovesOfFigure { get; protected set; } = 0;
        public FigureType Title { get; protected set; }
        public Color Color { get; protected set; } = owner.Color;

        public static List<Type> GetTypeOfAllFigures() => [typeof(King), typeof(Queen), typeof(Rook), typeof(Bishop), typeof(Knight), typeof(Pawn)];

        public static List<Type> GetTypeOfReplacementFigures() => [typeof(Queen), typeof(Rook), typeof(Bishop), typeof(Knight)];

        public abstract bool CheckMove(int x, int y);

        public abstract bool CheckTake(int x, int y);

        internal abstract MoveAction? CheckMovement(int x, int y, Field field);

        internal void RemoveFromPlayer() => Owner.RemoveFigure(this);

        internal void AddToPlayer() => Owner.AddFigure(this);

        internal void Shift(int x, int y, bool isReplay = false)
        {
            A = x;
            B = y;
            if (!isReplay) AmountMovesOfFigure++;
        }
    }


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
            List<(int x, int y)> cells = ChessAlgorithm.GetPath(A, B, x, y);
            bool castling = f == null && AmountMovesOfFigure == 0 && CheckCastling(x, y) && MoveValidator.CellIsSafe(x, y, Color, field) &&
                MoveValidator.PathIsClear(cells, field) && MoveValidator.PathIsSafe(cells, Color, field) && MoveValidator.IsValidCastling(this, x, field);
            bool move = f == null && CheckMove(x, y) && MoveValidator.FigureIsSafeAfterMove(x, y, this, field);
            bool take = f != null && Color != f.Color && CheckTake(x, y) && MoveValidator.FigureIsSafeAfterMove(x, y, this, field);
            if (castling) return new CastlingMoveAction(this, x, y, field);
            if (move || take) return new RegularMoveAction(this, x, y, field);
            return null;
        }
    }


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

        internal override MoveAction? CheckMovement(int x, int y, Field field)
        {
            Figure? f = field.GetCell(x, y);
            List<(int x, int y)> cells = ChessAlgorithm.GetPath(A, B, x, y);
            Figure king = field.GetKing(Color);
            bool path = MoveValidator.PathIsClear(cells, field);
            bool move = f == null && CheckMove(x, y) && path && MoveValidator.CellIsSafeAfterMove(king.A, king.B, this, x, y, field);
            bool take = f != null && Color != f.Color && CheckTake(x, y) && path && MoveValidator.CellIsSafeAfterMove(king.A, king.B, this, x, y, field);
            if (move || take) 
                return new RegularMoveAction(this, x, y, field);
            return null;
        }
    }


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
            List<(int x, int y)> cells = ChessAlgorithm.GetPath(A, B, x, y);
            Figure king = field.GetKing(Color);
            bool path = MoveValidator.PathIsClear(cells, field);
            bool move = f == null && CheckMove(x, y) && path && MoveValidator.CellIsSafeAfterMove(king.A, king.B, this, x, y, field);
            bool take = f != null && Color != f.Color && CheckTake(x, y) && path && MoveValidator.CellIsSafeAfterMove(king.A, king.B, this, x, y, field);
            if (move || take)
                return new RegularMoveAction(this, x, y, field);
            return null;
        }
    }


    public class Bishop : Figure
    {
        public Bishop(int a, int b, Player owner) : base(a, b, owner)
        {
            Title = FigureType.Bishop;
        }

        public override bool CheckMove(int x, int y)
        {
            bool move = Math.Abs(A - x) == Math.Abs(B - y) && A - x != 0;
            return move;
        }

        public override bool CheckTake(int x, int y)
        {
            bool take = Math.Abs(A - x) == Math.Abs(B - y) && A - x != 0;
            return take;
        }

        internal override MoveAction? CheckMovement(int x, int y, Field field)
        {
            Figure? f = field.GetCell(x, y);
            List<(int x, int y)> cells = ChessAlgorithm.GetPath(A, B, x, y);
            Figure king = field.GetKing(Color);
            bool path = MoveValidator.PathIsClear(cells, field);
            bool move = f == null && CheckMove(x, y) && path && MoveValidator.CellIsSafeAfterMove(king.A, king.B, this, x, y, field);
            bool take = f != null && Color != f.Color && CheckTake(x, y) && path && MoveValidator.CellIsSafeAfterMove(king.A, king.B, this, x, y, field);
            if (move || take)
                return new RegularMoveAction(this, x, y, field);
            return null;
        }
    }


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

        internal override MoveAction? CheckMovement(int x, int y, Field field)
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

        internal override MoveAction? CheckMovement(int x, int y, Field field)
        {
            Figure? f = field.GetCell(x, y);
            List<(int x, int y)> cells = ChessAlgorithm.GetPath(A, B, x, y);
            Figure king = field.GetKing(Color);
            bool moveThroughCage = f == null && CheckMoveThroughCage(x, y) && AmountMovesOfFigure == 0 
                && MoveValidator.PathIsClear(cells, field) && MoveValidator.CellIsSafeAfterMove(king.A, king.B, this, x, y, field);
            bool move = f == null && CheckMove(x, y) && MoveValidator.CellIsSafeAfterMove(king.A, king.B, this, x, y, field);
            bool take = f != null && Color != f.Color && CheckTake(x, y) && MoveValidator.CellIsSafeAfterMove(king.A, king.B, this, x, y, field);
            bool replacement = (move || take) && (Color == Color.White && y == 7 || Color == Color.Black && y == 0) && field.SelectionMethodIsSet();
            bool takeOnPassage = f == null && CheckTake(x, y) && (Color == Color.White && B == 4 || Color == Color.Black && B == 3) 
                && MoveValidator.IsValidTakeOnPassage(this, x, field) && MoveValidator.CellIsSafeAfterMove(king.A, king.B, this, x, y, field);
            if (replacement) return new ReplacementMoveAction(this, x, y, field);
            if (takeOnPassage) return new TakeOnPassageMoveAction(this, x, y, field);
            if (move || take || moveThroughCage) return new RegularMoveAction(this, x, y, field);
            return null;
        }
    }
}