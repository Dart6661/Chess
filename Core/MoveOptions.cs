namespace Chess.Core
{
    public abstract class MoveOption
    {
    }


    public class ReplacementOption(Type selectedFigure) : MoveOption
    {
        public Type SelectedFigure { get; init; } = selectedFigure;
    }
}