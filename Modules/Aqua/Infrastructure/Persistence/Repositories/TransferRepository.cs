using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Repositories
{
    public class TransferRepository : ITransferRepository
    {
        private readonly AquaDbContext _db;

        public TransferRepository(AquaDbContext db)
        {
            _db = db;
        }

        public Task<Transfer?> GetForPost(long id)
        {
            return _db.Transfers
                .Include(x => x.Lines)
                .ThenInclude(x => x.FishBatch)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }
    }
}
