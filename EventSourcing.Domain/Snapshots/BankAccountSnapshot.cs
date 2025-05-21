using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventSourcing.Domain.Snapshots
{
    public class BankAccountSnapshot
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } // AccountId

        public string AccountHolder { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
        public bool IsActive { get; set; }

        public int Version { get; set; } // kacinci evente kadar snapshot alindi, kalan eventler replay edilir
    }
}
