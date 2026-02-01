namespace Chess.Core
{
    internal abstract class MoveAction(Figure figure, int x, int y, Field field)
    {
        protected Field field = field;
        protected Figure figure = figure;
        protected int x = x;
        protected int y = y;

        internal abstract void ExecuteMove(IEnumerable<MoveOption>? moveOptions = null, bool isReplay = false);
        internal abstract void UndoMove(IEnumerable<MoveOption>? moveOptions = null, bool isReplay = false);
    }


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

        internal override void ExecuteMove(IEnumerable<MoveOption>? moveOptions = null, bool isReplay = false)
        {
            takenFigure?.RemoveFromPlayer();
            field.Reposition(takerX, takerY, x, y, isReplay);
            if (!isReplay)
                field.AddMove([(takerX, takerY, x, y)], this);
        }

        internal override void UndoMove(IEnumerable<MoveOption>? moveOptions = null, bool isReplay = false)
        {
            field.Reposition(x, y, takerX, takerY, isReplay);
            field.ChangeCell(x, y, takenFigure);
            takenFigure?.AddToPlayer();
        }
    }


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

        internal override void ExecuteMove(IEnumerable<MoveOption>? moveOptions = null, bool isReplay = false)
        {
            field.Reposition(kingX, bothY, x, y, isReplay);
            field.Reposition(originalRookX, bothY, newRookX, bothY, isReplay);
            if (!isReplay)
                field.AddMove([(kingX, bothY, x, y), (originalRookX, bothY, newRookX, bothY)], this);
        }

        internal override void UndoMove(IEnumerable<MoveOption>? moveOptions = null, bool isReplay = false)
        {
            field.Reposition(newRookX, bothY, originalRookX, bothY, isReplay);
            field.Reposition(x, y, kingX, bothY, isReplay);
        }
    }


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

        internal override void ExecuteMove(IEnumerable<MoveOption>? moveOptions = null, bool isReplay = false)
        {
            takenPawn.RemoveFromPlayer();
            field.ChangeCell(takenX, bothY, null);
            field.Reposition(takerX, bothY, x, y, isReplay);
            if (!isReplay) 
                field.AddMove([(takerX, bothY, x, y)], this);
        }

        internal override void UndoMove(IEnumerable<MoveOption>? moveOptions = null, bool isReplay = false)
        {
            field.Reposition(x, y, takerX, bothY, isReplay);
            field.ChangeCell(takenX, bothY, takenPawn);
            takenPawn.AddToPlayer();
        }
    }


    internal class ReplacementMoveAction : MoveAction
    {
        private readonly int pawnX;
        private readonly int pawnY;
        private readonly Figure? takenFigure;
        private Figure? newFigure;

        internal ReplacementMoveAction(Figure figure, int x, int y, Field field) : base(figure, x, y, field)
        {
            pawnX = figure.A;
            pawnY = figure.B;
            takenFigure = field.GetCell(x, y);
        }

        internal override void ExecuteMove(IEnumerable<MoveOption>? moveOptions = null, bool isReplay = false)
        {
            if (!isReplay)
            {
                ReplacementOption? replacementOption = (moveOptions?.OfType<ReplacementOption>().FirstOrDefault()) ?? throw new ReplacementException("replacement option not provided");
                Type newFigureType = replacementOption.SelectedFigure;
                if (!Figure.GetTypeOfReplacementFigures().Contains(newFigureType)) throw new ReplacementException("the choice is incorrect");
                newFigure = Activator.CreateInstance(newFigureType, x, y, figure.Owner) as Figure ?? throw new ReplacementException("failed replacement");
            }
            
            Player player = figure.Owner;
            for (int i = 0; i < player.CountFigures(); i++)
            {
                if (player.GetFigures()[i] == figure)
                {
                    player.ReplaceFigure(newFigure!, i);
                    break;
                }
            }
            field.ChangeCell(pawnX, pawnY, newFigure);
            field.Reposition(pawnX, pawnY, x, y, isReplay);

            if (!isReplay)
                field.AddMove([(pawnX, pawnY, x, y)], this);
        }

        internal override void UndoMove(IEnumerable<MoveOption>? moveOptions = null, bool isReplay = false)
        {
            field.Reposition(x, y, pawnX, pawnY, isReplay);
            field.ChangeCell(x, y, takenFigure);
            field.ChangeCell(pawnX, pawnY, figure);
            Player player = newFigure!.Owner;
            for (int i = 0; i < player.CountFigures(); i++)
            {
                if (player.GetFigures()[i] == newFigure)
                {
                    player.ReplaceFigure(figure, i);
                    break;
                }
            }
        }
    }
}
