
namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Repositories
{
    public interface IShipmentRepository
    {
        Task<Shipment?> GetForPost(long id);
    }
}
