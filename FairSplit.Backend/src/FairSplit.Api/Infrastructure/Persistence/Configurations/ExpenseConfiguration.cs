using FairSplit.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FairSplit.Api.Infrastructure.Persistence.Configurations;

public sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");

        builder.HasKey(expense => expense.Id);

        builder.Property(expense => expense.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(expense => expense.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(expense => expense.GroupId);
        builder.HasIndex(expense => expense.PayerMemberId);
        builder.HasIndex(expense => expense.CreatedAtUtc);

        builder.HasOne<Group>()
            .WithMany()
            .HasForeignKey(expense => expense.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(expense => expense.PayerMemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
