using MongoDB.Bson.Serialization.Attributes;

namespace EventSourcing.Domain.Events
{
    [BsonDiscriminator("MoneyWithdrawn")]
    public record MoneyWithdrawn(
        string AccountId,
        decimal Amount
    ) : Event(AccountId);
}
