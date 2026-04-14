using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Repositories
{
    public class ShipmentRepository : IShipmentRepository
    {
        private readonly AquaDbContext _db;

        public ShipmentRepository(AquaDbContext db)
        {
            _db = db;
        }

        public Task<Shipment?> GetForPost(long id)
        {
            return _db.Shipments
                .Include(x => x.Lines)
                .ThenInclude(x => x.FishBatch)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }
    }
}
