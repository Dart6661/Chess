namespace Chess.Client.Cli
{
    internal delegate Task<Menu?> MenuItem();
    internal delegate ServerEventData ServerEventItem(ResponseDto responseDto);
    internal delegate void ChangeMenu(Type menuType);


    internal static class MoveHandler
    {
        internal static (int, int, int, int) ProcessMove(string? input)
        {
            if (input == null) throw new InputException("empty input");
            char[] separator = [' '];
            string[] coordinates = input.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (coordinates.Length != 2)
                throw new InputException("invalid coordinates input");

            (int A, int B) = CoordinateHandler.ConvertCoordinate(coordinates[0]);
            (int X, int Y) = CoordinateHandler.ConvertCoordinate(coordinates[1]);
            return (A, B, X, Y);
        }
    }


    internal static class CoordinateHandler
    {
        internal static (int, int) ConvertCoordinate(string coordinate)
        {
            if (!IsValidCoordinate(coordinate))
                throw new CoordinatesInputException("invalid coordinates");

            int x = coordinate[0] - 'a';
            int y = coordinate[1] - '1';
            return (x, y);
        }

        internal static string ConvertCoordinate(int x, int y)
        {
            if (!MoveValidator.IsValidCoordinates(x, y))
                throw new CoordinatesInputException("invalid coordinates");

            string coordinate = "";
            coordinate += (char)(x + 'a');
            coordinate += (char)(y + '1');
            return coordinate;
        }

        internal static bool IsValidCoordinate(string? coordinate)
        {
            return (coordinate != null && coordinate.Length == 2 &&
                coordinate[0] >= 'a' && coordinate[0] <= 'h' &&
                coordinate[1] >= '1' && coordinate[1] <= '8');
        }
    }
}
