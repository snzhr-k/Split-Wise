using FairSplit.Api.Domain.Entities;
using FairSplit.Api.Infrastructure.Persistence;
using FairSplit.Api.Repositories.Implementations;
using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit.Sdk;

namespace FairSplit.LayerIsolationTests.Repositories;

public sealed class ExpenseRepositoryTests
{
    [SkippableFact]
    public async Task AddAndGetAll_UsesDatabaseOnly()
    {
        PostgreSqlContainer? postgres = null;

        try
        {
            postgres = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("fairsplit_test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await postgres.StartAsync();
        }
        catch (DockerUnavailableException)
        {
            Skip.If(true, "Docker is unavailable; skipping PostgreSQL repository isolation test.");
        }

        Assert.NotNull(postgres);

        try
        {
            var options = new DbContextOptionsBuilder<FairSplitDbContext>()
                .UseNpgsql(postgres!.GetConnectionString())
                .Options;

            await using (var setupContext = new FairSplitDbContext(options))
            {
                await setupContext.Database.EnsureDeletedAsync();
                await setupContext.Database.EnsureCreatedAsync();
            }

            var expenseId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var payerId = Guid.NewGuid();

            await using (var writeContext = new FairSplitDbContext(options))
            {
                var repository = new ExpenseRepository(writeContext);

                await repository.AddAsync(new Expense
                {
                    Id = expenseId,
                    GroupId = groupId,
                    PayerMemberId = payerId,
                    Amount = 42.25m,
                    CreatedAtUtc = DateTimeOffset.UtcNow
                }, CancellationToken.None);

                await writeContext.SaveChangesAsync();
            }

            await using (var readContext = new FairSplitDbContext(options))
            {
                var repository = new ExpenseRepository(readContext);
                var expenses = await repository.GetAllAsync(CancellationToken.None);

                var stored = Assert.Single(expenses);
                Assert.Equal(expenseId, stored.Id);
                Assert.Equal(groupId, stored.GroupId);
                Assert.Equal(42.25m, stored.Amount);
            }
        }
        finally
        {
            await postgres.DisposeAsync();
        }
    }
}
