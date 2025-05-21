using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventSourcing.Domain.Events
{
    [BsonDiscriminator("Event")]
    [BsonKnownTypes(typeof(AccountOpened), typeof(MoneyDeposited), typeof(MoneyWithdrawn), typeof(AccountClosed))]
    public abstract record Event(string StreamId)
    {
        [BsonIgnoreIfDefault]
        [BsonId]
        public ObjectId Id { get; init; } // Mongo otomatik veriyor, biz kullanmıyoruz

        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public int Version { get; set; }
    }
}
