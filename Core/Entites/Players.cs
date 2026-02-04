namespace Chess.Core
{ 
    public class Player(Color playerColor)
    {
        public int AmountMovesOfPlayer { get; internal set; } = 0;
        public Color Color { get; } = playerColor;
        private readonly List<Figure> figures = [];

        public IReadOnlyList<Figure> GetFigures() => figures.AsReadOnly();

        public int CountFigures() => figures.Count;

        internal List<Figure> GetListOfFigures() => figures;

        internal void AddFigure(Figure figure) => figures.Add(figure);

        internal void RemoveFigure(Figure figure) => figures.Remove(figure);

        internal void ReplaceFigure(Figure figure, int index) => figures[index] = figure;
    }
}
