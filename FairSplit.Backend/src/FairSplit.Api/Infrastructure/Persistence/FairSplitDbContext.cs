using FairSplit.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FairSplit.Api.Infrastructure.Persistence;

public sealed class FairSplitDbContext(DbContextOptions<FairSplitDbContext> options) : DbContext(options)
{
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ExpenseParticipant> ExpenseParticipants => Set<ExpenseParticipant>();
    public DbSet<Balance> Balances => Set<Balance>();
    public DbSet<Settlement> Settlements => Set<Settlement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FairSplitDbContext).Assembly);
    }
}
