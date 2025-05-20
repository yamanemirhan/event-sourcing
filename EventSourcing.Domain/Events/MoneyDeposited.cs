using MongoDB.Bson.Serialization.Attributes;

namespace EventSourcing.Domain.Events
{
    [BsonDiscriminator("MoneyDeposited")]
    public record MoneyDeposited(
        string AccountId,
        decimal Amount
    ) : Event(AccountId);
}
