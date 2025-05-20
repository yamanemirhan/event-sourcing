using EventSourcing.Domain.Events;
using EventSourcing.Infrastructure.Mongo;
using MongoDB.Driver;

namespace EventSourcing.Infrastructure.Repositories
{
    public interface IEventStoreRepository
    {
        Task SaveEventsAsync(string streamId, IEnumerable<Event> events);
        Task<List<Event>> GetEventsAsync(string streamId);
    }

    public class EventStoreRepository : IEventStoreRepository
    {
        private readonly IMongoCollection<Event> _eventsCollection;

        public EventStoreRepository(MongoContext context)
        {
            _eventsCollection = context.GetCollection<Event>("events");
        }

        public async Task SaveEventsAsync(string streamId, IEnumerable<Event> events)
        {
            // eventleri insert et
            await _eventsCollection.InsertManyAsync(events);
        }

        public async Task<List<Event>> GetEventsAsync(string streamId)
        {
            // events by streamId
            var filter = Builders<Event>.Filter.Eq(e => e.StreamId, streamId);
            return await _eventsCollection.Find(filter).ToListAsync();
        }
    }
}
