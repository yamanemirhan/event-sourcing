using EventSourcing.Domain.Projections;
using EventSourcing.Infrastructure.Mongo;
using MongoDB.Driver;

namespace EventSourcing.Infrastructure.Repositories
{
    public interface IBankAccountProjectionRepository
    {
        Task<BankAccountProjection> GetAsync(string id);
        Task UpsertAsync(BankAccountProjection projection);
    }

    public class BankAccountProjectionRepository : IBankAccountProjectionRepository
    {
        private readonly IMongoCollection<BankAccountProjection> _collection;

        public BankAccountProjectionRepository(MongoContext context)
        {
            _collection = context.GetCollection<BankAccountProjection>("bankaccounts");
        }

        public async Task<BankAccountProjection> GetAsync(string id) =>
            await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();

        public async Task UpsertAsync(BankAccountProjection projection) =>
            await _collection.ReplaceOneAsync(
                p => p.Id == projection.Id,
                projection,
                new ReplaceOptions { IsUpsert = true });
    }
}
