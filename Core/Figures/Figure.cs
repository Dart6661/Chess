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
}