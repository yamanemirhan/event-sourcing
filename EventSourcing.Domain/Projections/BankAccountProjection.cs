using MongoDB.Bson.Serialization.Attributes;

namespace EventSourcing.Domain.Projections
{
    public class BankAccountProjection
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }  // AccountId

        public string AccountHolder { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
        public bool IsActive { get; set; }
    }
}
