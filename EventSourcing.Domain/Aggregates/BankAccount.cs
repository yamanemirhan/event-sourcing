using EventSourcing.Domain.Events;
using MongoDB.Bson.Serialization.Attributes;

namespace EventSourcing.Domain.Aggregates
{
    public class BankAccount
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; private set; }

        public string AccountHolder { get; private set; }
        public decimal Balance { get; private set; }
        public string Currency { get; private set; }
        public bool IsActive { get; private set; }

        private readonly List<Event> _allEvents = new();
        private readonly List<Event> _newEvents = new();

        public IReadOnlyList<Event> Events => _allEvents.AsReadOnly();
        public IReadOnlyList<Event> GetUncommittedEvents() => _newEvents.AsReadOnly();

        private BankAccount() { }

        public static BankAccount Open(string accountHolder, decimal initialDeposit, string currency = "TL")
        {
            if (string.IsNullOrWhiteSpace(accountHolder))
                throw new ArgumentException("Account holder is required.");

            if (initialDeposit <= 0)
                throw new ArgumentException("Initial deposit must be greater than zero.");

            var bankAccount = new BankAccount();

            var @event = new AccountOpened(
                MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                accountHolder,
                initialDeposit,
                currency);

            bankAccount.Apply(@event);
            bankAccount._allEvents.Add(@event);
            bankAccount._newEvents.Add(@event);

            return bankAccount;
        }

        public void Deposit(decimal amount)
        {
            if (!IsActive) throw new InvalidOperationException("Account is closed.");
            if (amount <= 0) throw new ArgumentException("Deposit amount must be positive.");

            var @event = new MoneyDeposited(Id, amount);
            Apply(@event);
            _allEvents.Add(@event);
            _newEvents.Add(@event);
        }

        public void Withdraw(decimal amount)
        {
            if (!IsActive) throw new InvalidOperationException("Account is closed.");
            if (amount <= 0) throw new ArgumentException("Withdrawal amount must be positive.");
            if (amount > Balance) throw new InvalidOperationException("Insufficient funds.");

            var @event = new MoneyWithdrawn(Id, amount);
            Apply(@event);
            _allEvents.Add(@event);
            _newEvents.Add(@event);
        }

        public void Close(string reason)
        {
            if (!IsActive) throw new InvalidOperationException("Account already closed.");

            var @event = new AccountClosed(Id, reason);
            Apply(@event);
            _allEvents.Add(@event);
            _newEvents.Add(@event);
        }

        public void Apply(Event @event)
        {
            switch (@event)
            {
                case AccountOpened e:
                    Id = e.AccountId;
                    AccountHolder = e.AccountHolder;
                    Balance = e.InitialDeposit;
                    Currency = e.Currency;
                    IsActive = true;
                    break;

                case MoneyDeposited e:
                    Balance += e.Amount;
                    break;

                case MoneyWithdrawn e:
                    Balance -= e.Amount;
                    break;

                case AccountClosed _:
                    IsActive = false;
                    break;
            }
        }

        public static BankAccount ReplayEvents(IEnumerable<Event> events)
        {
            var account = new BankAccount();
            foreach (var e in events)
            {
                account.Apply(e);
                account._allEvents.Add(e);
            }
            return account;
        }

        public void ClearUncommittedEvents()
        {
            _newEvents.Clear();
        }
    }
}
