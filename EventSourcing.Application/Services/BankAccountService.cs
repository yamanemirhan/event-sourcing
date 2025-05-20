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

    public class BankAccountService(IEventStoreRepository _eventStoreRepository) : IBankAccountService
    {
        public async Task<string> OpenAccountAsync(string accountHolder, decimal initialDeposit)
        {
            var account = BankAccount.Open(accountHolder, initialDeposit);
            await _eventStoreRepository.SaveEventsAsync(account.Id, account.Events);
            return account.Id;
        }

        public async Task<(decimal Balance, bool IsActive)> GetBalanceAsync(string accountId)
        {
            var events = await _eventStoreRepository.GetEventsAsync(accountId);
            var account = BankAccount.ReplayEvents(events);
            return (account.Balance, account.IsActive);
        }

        public async Task DepositAsync(string accountId, decimal amount)
        {
            var events = await _eventStoreRepository.GetEventsAsync(accountId);
            var account = BankAccount.ReplayEvents(events);
            account.Deposit(amount);
            await _eventStoreRepository.SaveEventsAsync(account.Id, account.Events);
        }

        public async Task WithdrawAsync(string accountId, decimal amount)
        {
            var events = await _eventStoreRepository.GetEventsAsync(accountId);
            var account = BankAccount.ReplayEvents(events);
            account.Withdraw(amount);
            await _eventStoreRepository.SaveEventsAsync(account.Id, account.Events);
        }
    }
}
