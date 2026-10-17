using Fcg.Notification.Function.Application.Common.Exceptions;
using Fcg.Notification.Function.Domain.Entities;
using Fcg.Notification.Function.Infrastructure.Persistence;
using Fcg.Notification.Function.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Fcg.Notification.Function.Infrastructure.Tests.Idempotency
{
    /// <summary>
    /// SQLite em memória para exercitar a PK de verdade. Cada contexto simula uma entrega
    /// (um escopo de Function); todos compartilham a mesma conexão, logo o mesmo banco.
    /// </summary>
    public class ProcessedMessageIdempotencyTests : IDisposable
    {
        private readonly CancellationToken _cancellationToken = CancellationToken.None;
        private readonly SqliteConnection _connection;
        private readonly string _key = $"SendPaymentApprovedEmailCommand:{Guid.NewGuid()}";

        public ProcessedMessageIdempotencyTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            using var context = CreateContext();
            context.Database.EnsureCreated();
        }

        public void Dispose() => _connection.Dispose();

        private NotificationDbContext CreateContext() =>
            new(new DbContextOptionsBuilder<NotificationDbContext>().UseSqlite(_connection).Options);

        private static ProcessedMessageIdempotencyService CreateService(NotificationDbContext context, int retentionInDays = 7) =>
            new(context, Options.Create(new IdempotencySettings { RetentionInDays = retentionInDays }));

        [Fact]
        public async Task TryProcessAsync_ShouldReturnTrueAndPersistOnCommit_WhenKeyIsNew()
        {
            //Arrange
            using var context = CreateContext();

            //Act
            var canProcess = await CreateService(context).TryProcessAsync(_key, _cancellationToken);
            await new UnitOfWork(context).CommitAsync();

            //Assert
            canProcess.Should().BeTrue();
            using var verify = CreateContext();
            (await verify.ProcessedMessages.AnyAsync(p => p.Key == _key)).Should().BeTrue();
        }

        [Fact]
        public async Task TryProcessAsync_ShouldReturnFalse_WhenKeyAlreadyProcessed()
        {
            //Arrange
            using (var first = CreateContext())
            {
                await CreateService(first).TryProcessAsync(_key, _cancellationToken);
                await new UnitOfWork(first).CommitAsync();
            }

            using var second = CreateContext();

            //Act
            var canProcess = await CreateService(second).TryProcessAsync(_key, _cancellationToken);

            //Assert
            canProcess.Should().BeFalse();
            second.ChangeTracker.Entries<ProcessedMessage>().Should().BeEmpty();
        }

        [Fact]
        public async Task CommitAsync_ShouldThrowDuplicateMessage_WhenConcurrentDeliveryCommittedFirst()
        {
            //Arrange: as duas entregas passam pela verificação antes de qualquer commit
            using var first = CreateContext();
            using var second = CreateContext();
            await CreateService(first).TryProcessAsync(_key, _cancellationToken);
            await CreateService(second).TryProcessAsync(_key, _cancellationToken);
            await new UnitOfWork(first).CommitAsync();

            //Act
            var act = () => new UnitOfWork(second).CommitAsync();

            //Assert
            await act.Should().ThrowAsync<DuplicateMessageException>();
            second.ChangeTracker.Entries().Should().BeEmpty();
        }

        [Fact]
        public async Task PurgeExpiredAsync_ShouldRemoveOnlyKeysOlderThanRetention()
        {
            //Arrange
            var oldKey = $"Old:{Guid.NewGuid()}";
            using (var seed = CreateContext())
            {
                seed.ProcessedMessages.Add(new ProcessedMessage(oldKey));
                seed.ProcessedMessages.Add(new ProcessedMessage(_key));
                await seed.SaveChangesAsync();
                await seed.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE ProcessedMessages SET CreatedAt = {DateTime.UtcNow.AddDays(-8)} WHERE Key = {oldKey}");
            }

            using var context = CreateContext();

            //Act
            var removed = await CreateService(context, retentionInDays: 7).PurgeExpiredAsync(_cancellationToken);

            //Assert
            removed.Should().Be(1);
            (await context.ProcessedMessages.Select(p => p.Key).ToListAsync()).Should().Equal(_key);
        }
    }
}
