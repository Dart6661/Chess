namespace Chess.Core
{
    internal abstract class MoveAction(Figure figure, int x, int y, Field field)
    {
        protected Field field = field;
        protected Figure figure = figure;
        protected int x = x;
        protected int y = y;

        internal abstract void ExecuteMove(bool isReplay = false, params MoveOption[] moveOptions);

        internal abstract void UndoMove(bool isReplay = false, params MoveOption[] moveOptions);
    }
}