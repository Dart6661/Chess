namespace Chess.Core
{
    public class InputException(string message) : Exception(message);

    public class CheckMateException(string message) : Exception(message);

    public class StaleMateException(string message) : Exception(message);

    public class MovementException(string message) : Exception(message);

    public class ReplacementException(string message) : Exception(message);

    public class OptionException(string message) : Exception(message);
}
