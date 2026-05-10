using FairSplit.Api.Infrastructure.Persistence;
using FairSplit.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FairSplit.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = $"fairsplit-integration-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                descriptor => descriptor.ServiceType == typeof(DbContextOptions<FairSplitDbContext>));

            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<FairSplitDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
            });

            var transactionManagerDescriptor = services.SingleOrDefault(
                descriptor => descriptor.ServiceType == typeof(ITransactionManager));

            if (transactionManagerDescriptor is not null)
            {
                services.Remove(transactionManagerDescriptor);
            }

            services.AddScoped<ITransactionManager, TestTransactionManager>();

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FairSplitDbContext>();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        });
    }
}
