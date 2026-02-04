namespace Chess.Core
{
    public class InputException(string message) : Exception(message);

    public class OptionException(string message) : Exception(message);
}