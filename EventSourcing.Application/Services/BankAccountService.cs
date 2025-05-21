using EventSourcing.Domain.Aggregates;
using EventSourcing.Infrastructure.Repositories;

namespace EventSourcing.Application.Services
{
    public interface IBankAccountService
    {
        Task<string> OpenAccountAsync(string accountHolder, decimal initialDeposit);
        Task DepositAsync(string accountId, decimal amount);
        Task WithdrawAsync(string accountId, decimal amount);
        Task<(decimal Balance, bool IsActive)> GetBalanceAsync(string accountId);
    }

    public class BankAccountService : IBankAccountService
    {
        private readonly IEventStoreRepository _eventStore;
        private readonly IBankAccountProjectionRepository _projectionRepository;
        private readonly IBankAccountSnapshotRepository _snapshotRepository;
        private readonly IProjectionUpdater _projectionUpdater;

        public BankAccountService(
            IEventStoreRepository eventStore,
            IBankAccountProjectionRepository projectionRepository,
            IBankAccountSnapshotRepository snapshotRepository,
            IProjectionUpdater projectionUpdater)
        {
            _eventStore = eventStore;
            _projectionRepository = projectionRepository;
            _snapshotRepository = snapshotRepository;
            _projectionUpdater = projectionUpdater;
        }

        public async Task<string> OpenAccountAsync(string accountHolder, decimal initialDeposit)
        {
            var account = BankAccount.Open(accountHolder, initialDeposit);
            await _eventStore.SaveEventsAsync(account.Id, account.Events);

            var projection = new Domain.Projections.BankAccountProjection
            {
                Id = account.Id,
                AccountHolder = account.AccountHolder,
                Balance = account.Balance,
                Currency = account.Currency,
                IsActive = account.IsActive,
                Version = 1
            };

            await _projectionRepository.UpsertAsync(projection);
            return account.Id;
        }

        public async Task DepositAsync(string accountId, decimal amount)
        {
            var account = await LoadAggregateAsync(accountId);
            account.Deposit(amount);

            await _eventStore.SaveEventsAsync(accountId, account.Events);

            foreach (var ev in account.Events)
            {
                await _projectionUpdater.HandleAsync(ev);
            }
                
            if (account.Version % 5 == 0)
            {
                var snapshot = account.ToSnapshot();
                await _snapshotRepository.SaveSnapshotAsync(snapshot);
            }
        }

        public async Task WithdrawAsync(string accountId, decimal amount)
        {
            var account = await LoadAggregateAsync(accountId);
            account.Withdraw(amount);

            await _eventStore.SaveEventsAsync(accountId, account.Events);

            foreach (var ev in account.Events)
                await _projectionUpdater.HandleAsync(ev);

            if (account.Version % 5 == 0)
            {
                var snapshot = account.ToSnapshot();
                await _snapshotRepository.SaveSnapshotAsync(snapshot);
            }
        }

        public async Task<(decimal Balance, bool IsActive)> GetBalanceAsync(string accountId)
        {
            var projection = await _projectionRepository.GetAsync(accountId);
            if (projection == null)
                throw new Exception("Account not found");

            return (projection.Balance, projection.IsActive);
        }

        private async Task<BankAccount> LoadAggregateAsync(string accountId)
        {
            var snapshot = await _snapshotRepository.GetSnapshotAsync(accountId);

            if (snapshot != null)
            {
                var events = await _eventStore.GetEventsAsync(accountId, snapshot.Version);
                return BankAccount.FromSnapshot(snapshot, events);
            }
            else
            {
                var events = await _eventStore.GetEventsAsync(accountId, 0);
                return BankAccount.ReplayEvents(events);
            }
        }
    }
}
