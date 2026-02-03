using System.Text;
namespace Chess.Client.Cli
{
    internal class UIHandler
    {
        internal UIHandler()
        {
            Console.OutputEncoding = Encoding.UTF8;
        }

        internal void DisplayMessage(string? message) => Console.WriteLine(message);

        internal async Task<int> ReadMenuItemSelection(Dictionary<int, (string description, MenuItem item)> MenuItems, CancellationToken token)
        {
            Console.WriteLine("select one of the actions:");
            foreach (var elem in MenuItems)
                Console.WriteLine($"{elem.Key} {elem.Value.description}");
            Console.WriteLine();

            string? input = await ReadLineAsync(token);
            if (!int.TryParse(input, out int itemNumber)) throw new IntegerInputException();
            return itemNumber;
        }

        internal async Task<string?> ReadMoveInput(CancellationToken token)
        {
            Console.Write("enter the move: ");
            string? input = await ReadLineAsync(token);
            return input;
        }
        
        internal async Task<int> ReadReplacementFigureSelection(Dictionary<int, (string title, string type)> replacementFigures, CancellationToken token)
        {
            Console.WriteLine("select one of the figures:");
            foreach (var elem in replacementFigures)
                Console.WriteLine($"{elem.Key} {elem.Value.title}");
            Console.WriteLine();

            string? input = await ReadLineAsync(token);
            if (!int.TryParse(input, out int itemNumber)) throw new IntegerInputException();
            return itemNumber;
        }

        internal void DisplayField(List<FigureDto> figures, Color color)
        {
            FigureDto?[,] field = BuildMatrix(figures);
            bool isBlackPlayer = (color == Color.Black);
            int indent = 2;
            int startY = (isBlackPlayer) ? 0 : 7;
            int incrementY = (isBlackPlayer) ? 1 : -1;

            int startX = (isBlackPlayer) ? 7 : 0;
            int incrementX = (isBlackPlayer) ? -1 : 1;

            for (int y = startY; (isBlackPlayer) ? y < 8 : y >= 0; y += incrementY)
            {
                for (int x = 0; x < indent; x++) Console.Write(" ");
                Console.Write(y + 1);
                for (int x = startX; (isBlackPlayer) ? x >= 0 : x < 8; x += incrementX)
                {
                    FigureDto? f = field[x, y];
                    if (f != null)
                    {
                        DrawCell(f.Title, f.Color);
                    }
                    else DrawCell();
                }
                Console.WriteLine();
            }
            for (int x = 0; x < indent + 1; x++) Console.Write(" ");
            for (int x = startX; (isBlackPlayer) ? x >= 0 : x < 8; x += incrementX) Console.Write(" " + (char)('a' + x) + " ");
            Console.WriteLine();
        }

        internal void Clear() => Console.Clear();

        private async Task<string?> ReadLineAsync(CancellationToken token)
        {            
            StringBuilder sb = new();

            while (true)
            {
                while (!Console.KeyAvailable)
                {
                    ThrowIfInputCancellationRequested(token);
                    await Task.Delay(50, token);
                }
                ThrowIfInputCancellationRequested(token);
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return sb.ToString();
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                    {
                        sb.Length--;
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    char ch = keyInfo.KeyChar;
                    sb.Append(ch);
                    Console.Write(ch);
                }
            }

            static void ThrowIfInputCancellationRequested(CancellationToken token)
            {
            if (token.IsCancellationRequested)
                throw new InputCanceledException(token);
            }
        }

        private FigureDto?[,] BuildMatrix(List<FigureDto> figures)
        {
            FigureDto?[,] field = new FigureDto[8, 8];
            foreach (var figure in figures) 
                field[figure.A, figure.B] = figure;
            return field;
        }        

        private string GetFigureSymbol(FigureType figure)
        {
            return figure switch
            {
                FigureType.King => "♔",
                FigureType.Queen => "♕",
                FigureType.Rook => "♖",
                FigureType.Bishop => "♗",
                FigureType.Knight => "♘",
                FigureType.Pawn => "♙",
                _ => throw new InputException("invalid figure input"),
            };
        }

        private void DrawCell(FigureType figure, Color color)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("[");

            Console.ForegroundColor = color == Color.White ? ConsoleColor.White : ConsoleColor.Gray;
            Console.Write(GetFigureSymbol(figure));

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("]");
            Console.ResetColor();
        }

        private void DrawCell()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("[ ]");
            Console.ResetColor();
        }
    }
}
