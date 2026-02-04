namespace Chess.Core
{
    internal class RegularMoveAction : MoveAction
    {
        private readonly int takerX;
        private readonly int takerY;
        private readonly Figure? takenFigure;

        internal RegularMoveAction(Figure figure, int x, int y, Field field) : base(figure, x, y, field)
        {
            takerX = figure.A;
            takerY = figure.B;
            takenFigure = field.GetCell(x, y);
        }

        internal override void ExecuteMove(bool isReplay = false, params MoveOption[] moveOptions)
        {
            takenFigure?.RemoveFromPlayer();
            field.Reposition(takerX, takerY, x, y, isReplay);
            if (!isReplay)
                field.AddMove([(takerX, takerY, x, y)], this);
        }

        internal override void UndoMove(bool isReplay = false, params MoveOption[] moveOptions)
        {
            field.Reposition(x, y, takerX, takerY, isReplay);
            field.ChangeCell(x, y, takenFigure);
            takenFigure?.AddToPlayer();
        }
    }
}