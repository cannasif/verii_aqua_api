
namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Repositories
{
    public interface IMortalityRepository
    {
        Task<Mortality?> GetForPost(long id);
    }
}
