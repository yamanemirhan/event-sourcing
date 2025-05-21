using EventSourcing.Domain.Events;
using EventSourcing.Domain.Projections;
using EventSourcing.Infrastructure.Repositories;

namespace EventSourcing.Application.Services
{
    public interface IProjectionUpdater
    {
        Task HandleAsync(Event @event);
    }
    public class ProjectionUpdater(IBankAccountProjectionRepository _projectionRepository) : IProjectionUpdater
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
                        IsActive = true,
                        Version = e.Version
                    };
                    await _projectionRepository.UpsertAsync(newProjection);
                    break;

                case MoneyDeposited e:
                    var depositProjection = await _projectionRepository.GetAsync(e.AccountId);
                    if (depositProjection != null && e.Version > depositProjection.Version)
                    {
                        depositProjection.Balance += e.Amount;
                        depositProjection.Version = e.Version;
                        await _projectionRepository.UpsertAsync(depositProjection);
                    }
                    break;

                case MoneyWithdrawn e:
                    var withdrawProjection = await _projectionRepository.GetAsync(e.AccountId);
                    if (withdrawProjection != null && e.Version > withdrawProjection.Version)
                    {
                        withdrawProjection.Balance -= e.Amount;
                        withdrawProjection.Version = e.Version;
                        await _projectionRepository.UpsertAsync(withdrawProjection);
                    }
                    break;

                case AccountClosed e:
                    var closeProjection = await _projectionRepository.GetAsync(e.AccountId);
                    if (closeProjection != null && e.Version > closeProjection.Version)
                    {
                        closeProjection.IsActive = false;
                        closeProjection.Version = e.Version;
                        await _projectionRepository.UpsertAsync(closeProjection);
                    }
                    break;
            }
        }
    }
}
