namespace aqua_api.Shared.Common.Exceptions;

public sealed class PagedQueryValidationException : Exception
{
    private static readonly AsyncLocal<string?> CurrentValidationMessage = new();

    public PagedQueryValidationException(string message) : base(message)
    {
        CurrentValidationMessage.Value = message;
    }

    internal static bool TryConsume(string? exceptionMessage, out string validationMessage)
    {
        validationMessage = CurrentValidationMessage.Value ?? string.Empty;
        if (validationMessage.Length == 0
            || exceptionMessage?.Contains(validationMessage, StringComparison.Ordinal) != true)
        {
            validationMessage = string.Empty;
            return false;
        }

        CurrentValidationMessage.Value = null;
        return true;
    }
}
