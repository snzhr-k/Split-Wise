using FairSplit.Api.Repositories.Implementations;
using FairSplit.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FairSplit.Api.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FairSplitDb")
            ?? throw new InvalidOperationException("Connection string 'FairSplitDb' was not found.");

        services.AddDbContext<FairSplitDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IExpenseParticipantRepository, ExpenseParticipantRepository>();
        services.AddScoped<IBalanceRepository, BalanceRepository>();
        services.AddScoped<ISettlementRepository, SettlementRepository>();
        services.AddScoped<ITransactionManager, TransactionManager>();

        return services;
    }
}
