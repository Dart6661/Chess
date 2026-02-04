namespace Chess.Core
{
    public class CheckMateException(string message) : Exception(message);

    public class StaleMateException(string message) : Exception(message);
}