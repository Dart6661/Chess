namespace Chess.Core
{
    internal class CastlingMoveAction : MoveAction
    {
        private readonly int kingX;
        private readonly int bothY;
        private readonly int originalRookX;
        private readonly int newRookX;

        internal CastlingMoveAction(Figure figure, int x, int y, Field field) : base(figure, x, y, field)
        {
            kingX = figure.A;
            bothY = y;
            newRookX = (kingX < x) ? kingX + 1 : kingX - 1;
            originalRookX = (newRookX == kingX + 1) ? kingX + 3 : kingX - 4;
        }

        internal override void ExecuteMove(bool isReplay = false, params MoveOption[] moveOptions)
        {
            field.Reposition(kingX, bothY, x, y, isReplay);
            field.Reposition(originalRookX, bothY, newRookX, bothY, isReplay);
            if (!isReplay)
                field.AddMove([(kingX, bothY, x, y), (originalRookX, bothY, newRookX, bothY)], this);
        }

        internal override void UndoMove(bool isReplay = false, params MoveOption[] moveOptions)
        {
            field.Reposition(newRookX, bothY, originalRookX, bothY, isReplay);
            field.Reposition(x, y, kingX, bothY, isReplay);
        }
    }
}