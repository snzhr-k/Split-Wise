using FairSplit.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FairSplit.Api.Infrastructure.Persistence.Configurations;

public sealed class ExpenseParticipantConfiguration : IEntityTypeConfiguration<ExpenseParticipant>
{
    public void Configure(EntityTypeBuilder<ExpenseParticipant> builder)
    {
        builder.ToTable("ExpenseParticipants");

        builder.HasKey(expenseParticipant => expenseParticipant.Id);

        builder.Property(expenseParticipant => expenseParticipant.ShareAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasIndex(expenseParticipant => expenseParticipant.ExpenseId);
        builder.HasIndex(expenseParticipant => expenseParticipant.MemberId);

        builder.HasIndex(expenseParticipant => new { expenseParticipant.ExpenseId, expenseParticipant.MemberId })
            .IsUnique();

        builder.HasOne<Expense>()
            .WithMany()
            .HasForeignKey(expenseParticipant => expenseParticipant.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(expenseParticipant => expenseParticipant.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
