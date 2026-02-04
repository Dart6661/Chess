namespace Chess.Core
{
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

        internal override void ExecuteMove(bool isReplay = false, params MoveOption[] moveOptions)
        {
            if (!isReplay)
            {
                ReplacementOption? replacementOption = moveOptions?.OfType<ReplacementOption>()?.FirstOrDefault() ?? throw new OptionException("replacement option not provided");
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

            if (!isReplay) field.AddMove([(pawnX, pawnY, x, y)], this);
        }

        internal override void UndoMove(bool isReplay = false, params MoveOption[] moveOptions)
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