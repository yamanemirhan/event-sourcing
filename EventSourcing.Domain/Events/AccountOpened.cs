using MongoDB.Bson.Serialization.Attributes;

namespace EventSourcing.Domain.Events
{
    [BsonDiscriminator("AccountOpened")]
    public record AccountOpened(
        string AccountId,
        string AccountHolder,
        decimal InitialDeposit,
        string Currency
    ) : Event(AccountId);
}
