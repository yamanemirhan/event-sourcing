using MongoDB.Bson.Serialization.Attributes;

namespace EventSourcing.Domain.Events
{
    [BsonDiscriminator("AccountClosed")]
    public record AccountClosed(
        string AccountId,
        string Reason
    ) : Event(AccountId);
}
