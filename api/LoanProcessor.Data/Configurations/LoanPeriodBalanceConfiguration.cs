using LoanProcessor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanProcessor.Data.Configurations;

public sealed class LoanPeriodBalanceConfiguration : IEntityTypeConfiguration<LoanPeriodBalanceEntity>
{
    public void Configure(EntityTypeBuilder<LoanPeriodBalanceEntity> builder)
    {
        builder.ToTable("LoanPeriodBalances");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.PeriodDate).HasColumnType("date");
        builder.Property(e => e.Rpb).HasColumnType("decimal(18,2)");

        builder.HasIndex(e => new { e.LoanOwnershipPeriodId, e.PeriodDate }).IsUnique();
    }
}
