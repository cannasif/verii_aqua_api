namespace aqua_api.Modules.Integrations.Application.Services;

public sealed class NetsisIntegrationValidationException : InvalidOperationException
{
    public NetsisIntegrationValidationException(string message)
        : base(message)
    {
    }
}
