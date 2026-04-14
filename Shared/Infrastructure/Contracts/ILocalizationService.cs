namespace aqua_api.Shared.Infrastructure.Contracts
{
    public interface ILocalizationService
    {
        string GetLocalizedString(string key);
        string GetLocalizedString(string key, params object[] arguments);
    }
}
