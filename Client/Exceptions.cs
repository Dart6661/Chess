namespace Chess.Client.Cli
{
    public class DisconnectedException(string message) : Exception(message);


    public class InvalidResponseException(string? message = null) : Exception(message ?? "response is incorrect");


    public class InputCanceledException(CancellationToken token) : OperationCanceledException(token);


    public class ChangingMenuException(string? message = null) : NullReferenceException(message ?? "menu has not changed");


    public class ItemNotFoundException(string? message = null) : InputException(message ?? "the specified number is not in the list");


    public class IntegerInputException(string? message = null) : InputException(message ?? "input in an invalid format");


    public class CoordinatesInputException(string message) : InputException(message);
}
