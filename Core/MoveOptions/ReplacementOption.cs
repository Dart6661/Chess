namespace Chess.Core
{
    public class ReplacementOption(Type selectedFigure) : MoveOption
    {
        public Type SelectedFigure { get; init; } = selectedFigure;
    }
}