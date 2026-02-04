namespace Chess.Core
{
    public class MovementException(string message) : Exception(message);

    public class ReplacementException(string message) : Exception(message);
}