using EventSourcing.Domain.Aggregates;
using EventSourcing.Infrastructure.Repositories;

namespace EventSourcing.Application.Services
{
    public interface IBankAccountService
    {
        Task<string> OpenAccountAsync(string accountHolder, decimal initialDeposit);
        Task<(decimal Balance, bool IsActive)> GetBalanceAsync(string accountId);
        Task DepositAsync(string accountId, decimal amount);
        Task WithdrawAsync(string accountId, decimal amount);
    }

    public class BankAccountService : IBankAccountService
    {
        private readonly IEventStoreRepository _eventStoreRepository;
        private readonly IBankAccountProjectionRepository _projectionRepository;
        private readonly ProjectionUpdater _projectionUpdater;

        public BankAccountService(
            IEventStoreRepository eventStoreRepository,
            IBankAccountProjectionRepository projectionRepository,
            ProjectionUpdater projectionUpdater)
        {
            _eventStoreRepository = eventStoreRepository;
            _projectionRepository = projectionRepository;
            _projectionUpdater = projectionUpdater;
        }

        public async Task<string> OpenAccountAsync(string accountHolder, decimal initialDeposit)
        {
            var account = BankAccount.Open(accountHolder, initialDeposit);
            await _eventStoreRepository.SaveEventsAsync(account.Id, account.GetUncommittedEvents());

            foreach (var @event in account.GetUncommittedEvents())
                await _projectionUpdater.HandleAsync(@event);

            account.ClearUncommittedEvents();

            return account.Id;
        }

        public async Task<(decimal Balance, bool IsActive)> GetBalanceAsync(string accountId)
        {
            var projection = await _projectionRepository.GetAsync(accountId);
            if (projection == null) throw new Exception("Account not found");
            return (projection.Balance, projection.IsActive);
        }

        public async Task DepositAsync(string accountId, decimal amount)
        {
            var events = await _eventStoreRepository.GetEventsAsync(accountId);
            var account = BankAccount.ReplayEvents(events);
            account.Deposit(amount);

            await _eventStoreRepository.SaveEventsAsync(account.Id, account.GetUncommittedEvents());

            foreach (var @event in account.GetUncommittedEvents())
                await _projectionUpdater.HandleAsync(@event);

            account.ClearUncommittedEvents();
        }

        public async Task WithdrawAsync(string accountId, decimal amount)
        {
            var events = await _eventStoreRepository.GetEventsAsync(accountId);
            var account = BankAccount.ReplayEvents(events);
            account.Withdraw(amount);

            await _eventStoreRepository.SaveEventsAsync(account.Id, account.GetUncommittedEvents());

            foreach (var @event in account.GetUncommittedEvents())
                await _projectionUpdater.HandleAsync(@event);

            account.ClearUncommittedEvents();
        }
    }

}
