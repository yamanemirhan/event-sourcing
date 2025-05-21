using EventSourcing.Application.Services;
using EventSourcing.Domain.Events;
using EventSourcing.Domain.Projections;
using EventSourcing.Domain.Snapshots;
using EventSourcing.Infrastructure.Repositories;
using Moq;

namespace EventSourcing.Tests
{
    public class ProjectionUpdaterTests
    {
        [Fact]
        public async Task Should_Not_Apply_Same_Event_Twice()
        {
            var accountId = "acc-123";
            var depositEvent = new MoneyDeposited(accountId, 50) { Version = 2 };

            var initialProjection = new BankAccountProjection
            {
                Id = accountId,
                Balance = 100,
                AccountHolder = "Test User",
                Currency = "TL",
                IsActive = true,
                Version = 1
            };

            var mockRepo = new Mock<IBankAccountProjectionRepository>();

            // İlk seferde mevcut projection döner
            mockRepo.Setup(r => r.GetAsync(accountId))
                    .ReturnsAsync(initialProjection);

            // Projection güncellemesi simülasyonu
            mockRepo.Setup(r => r.UpsertAsync(It.IsAny<BankAccountProjection>()))
                    .Callback<BankAccountProjection>(updated =>
                    {
                        initialProjection = updated;
                    })
                    .Returns(Task.CompletedTask);

            var updater = new ProjectionUpdater(mockRepo.Object);

            // Act - aynı event iki kez işleniyor
            await updater.HandleAsync(depositEvent);
            await updater.HandleAsync(depositEvent); // ikinci kez

            // Assert
            Assert.Equal(150, initialProjection.Balance);
            Assert.Equal(2, initialProjection.Version);
        }

        [Fact]
        public async Task Should_Load_From_Snapshot_And_Request_Only_New_Events()
        {
            // Arrange
            var accountId = "some-id";
            var snapshotVersion = 5;

            // Mock snapshot
            var snapshot = new BankAccountSnapshot
            {
                Id = accountId,
                Version = snapshotVersion,
                AccountHolder = "John Doe",
                Balance = 100,
                Currency = "USD",
                IsActive = true
            };

            // Mocks
            var eventStoreMock = new Mock<IEventStoreRepository>();
            var projectionRepoMock = new Mock<IBankAccountProjectionRepository>();
            var snapshotRepoMock = new Mock<IBankAccountSnapshotRepository>();
            var projectionUpdaterMock = new Mock<IProjectionUpdater>();

            snapshotRepoMock
                .Setup(repo => repo.GetSnapshotAsync(accountId))
                .ReturnsAsync(snapshot);
            eventStoreMock
                .Setup(repo => repo.GetEventsAsync(accountId, snapshotVersion))
                .ReturnsAsync(new List<Event>())
                .Verifiable();
            projectionUpdaterMock
                .Setup(pu => pu.HandleAsync(It.IsAny<Event>()))
                .Returns(Task.CompletedTask);

            var service = new BankAccountService(
                eventStoreMock.Object,
                projectionRepoMock.Object,
                snapshotRepoMock.Object,
                projectionUpdaterMock.Object);

            // Act
            await service.DepositAsync(accountId, 50);

            eventStoreMock.Verify(repo => repo.GetEventsAsync(accountId, snapshotVersion), Times.Once);
        }

    }
}
