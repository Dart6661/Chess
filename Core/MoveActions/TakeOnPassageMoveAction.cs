namespace Chess.Core
{
    internal class TakeOnPassageMoveAction : MoveAction
    {
        private readonly int takerX;
        private readonly int bothY;
        private readonly Figure takenPawn;
        private readonly int takenX;

        internal TakeOnPassageMoveAction(Figure figure, int x, int y, Field field) : base(figure, x, y, field)
        {
            takerX = figure.A;
            bothY = figure.B;
            takenX = (takerX < x) ? takerX + 1 : takerX - 1;
            takenPawn = field.GetCell(takenX, bothY)!;
        }

        internal override void ExecuteMove(bool isReplay = false, params MoveOption[] moveOptions)
        {
            takenPawn.RemoveFromPlayer();
            field.ChangeCell(takenX, bothY, null);
            field.Reposition(takerX, bothY, x, y, isReplay);
            if (!isReplay) 
                field.AddMove([(takerX, bothY, x, y)], this);
        }

        internal override void UndoMove(bool isReplay = false, params MoveOption[] moveOptions)
        {
            field.Reposition(x, y, takerX, bothY, isReplay);
            field.ChangeCell(takenX, bothY, takenPawn);
            takenPawn.AddToPlayer();
        }
    }
}