using EventSourcing.Domain.Events;
using EventSourcing.Domain.Projections;
using EventSourcing.Infrastructure.Repositories;

namespace EventSourcing.Application.Services
{
    public class ProjectionUpdater(IBankAccountProjectionRepository _projectionRepository)
    {
        public async Task HandleAsync(Event @event)
        {
            switch (@event)
            {
                case AccountOpened e:
                    var newProjection = new BankAccountProjection
                    {
                        Id = e.AccountId,
                        AccountHolder = e.AccountHolder,
                        Balance = e.InitialDeposit,
                        Currency = e.Currency,
                        IsActive = true
                    };
                    await _projectionRepository.UpsertAsync(newProjection);
                    break;

                case MoneyDeposited e:
                    var depositProjection = await _projectionRepository.GetAsync(e.AccountId);
                    if (depositProjection != null)
                    {
                        depositProjection.Balance += e.Amount;
                        await _projectionRepository.UpsertAsync(depositProjection);
                    }
                    break;

                case MoneyWithdrawn e:
                    var withdrawProjection = await _projectionRepository.GetAsync(e.AccountId);
                    if (withdrawProjection != null)
                    {
                        withdrawProjection.Balance -= e.Amount;
                        await _projectionRepository.UpsertAsync(withdrawProjection);
                    }
                    break;

                case AccountClosed e:
                    var closeProjection = await _projectionRepository.GetAsync(e.AccountId);
                    if (closeProjection != null)
                    {
                        closeProjection.IsActive = false;
                        await _projectionRepository.UpsertAsync(closeProjection);
                    }
                    break;
            }
        }
    }
}
