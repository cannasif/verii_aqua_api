
namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Repositories
{
    public interface INetOperationRepository
    {
        Task<NetOperation?> GetForPost(long id);
    }
}
