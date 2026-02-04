namespace Chess.Core
{
    public static class PathAlgorithm
    {
        public static List<(int, int)> GetPath(int a, int b, int x, int y)
        {
            if (!MoveValidator.IsValidCoordinates(x, y) || !MoveValidator.IsValidCoordinates(a, b))
                throw new InputException("invalid coordinates");

            List<(int, int)> cells = [];
            if (a == x || b == y)
            {
                int direction = 1;
                int size = Math.Abs(a + b - x - y);
                if (a - x != 0)
                {
                    if (a - x > 0) direction = -1;
                    for (int i = 1; i < size; i++) cells.Add((a + i * direction, b));
                }
                else
                {
                    if (b - y > 0) direction = -1;
                    for (int i = 1; i < size; i++) cells.Add((a, b + i * direction));
                }
            }
            else if (Math.Abs(a - x) == Math.Abs(b - y))
            {
                int direction = 1;
                int size = Math.Abs(a - x);
                if (a - x == b - y)
                {
                    if (a - x > 0) direction = -1;
                    for (int i = 1; i < size; i++) cells.Add((a + i * direction, b + i * direction));
                }
                else
                {
                    if (a - x > 0) direction = -1;
                    for (int i = 1; i < size; i++) cells.Add((a + i * direction, b - i * direction));
                }
            }
            return cells;
        }
    }
}