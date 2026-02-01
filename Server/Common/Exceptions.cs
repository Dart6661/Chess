namespace Chess.Server
{
    public class EmptyRequestException(string message) : Exception(message);

    public class InvalidRequestException(string message) : Exception(message);

    public class RequestTypeException(string message) : Exception(message);

    public class FigureTypeException(string message) : Exception(message);
}
