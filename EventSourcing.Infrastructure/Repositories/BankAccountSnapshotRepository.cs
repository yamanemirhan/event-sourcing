using EventSourcing.Domain.Snapshots;
using EventSourcing.Infrastructure.Mongo;
using MongoDB.Driver;

namespace EventSourcing.Infrastructure.Repositories
{
    public interface IBankAccountSnapshotRepository
    {
        Task<BankAccountSnapshot?> GetSnapshotAsync(string accountId);
        Task SaveSnapshotAsync(BankAccountSnapshot snapshot);
    }

    public class BankAccountSnapshotRepository : IBankAccountSnapshotRepository
    {
        private readonly IMongoCollection<BankAccountSnapshot> _collection;

        public BankAccountSnapshotRepository(MongoContext context)
        {
            _collection = context.GetCollection<BankAccountSnapshot>("bankaccount_snapshots");
        }

        public async Task<BankAccountSnapshot?> GetSnapshotAsync(string accountId)
        {
            return await _collection.Find(s => s.Id == accountId).FirstOrDefaultAsync();
        }

        public async Task SaveSnapshotAsync(BankAccountSnapshot snapshot)
        {
            await _collection.ReplaceOneAsync(
                s => s.Id == snapshot.Id,
                snapshot,
                new ReplaceOptions { IsUpsert = true });
        }
    }
}
