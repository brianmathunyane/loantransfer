using LoanProcessor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanProcessor.Data.Configurations;

public sealed class LoanOwnershipPeriodConfiguration : IEntityTypeConfiguration<LoanOwnershipPeriodEntity>
{
    public void Configure(EntityTypeBuilder<LoanOwnershipPeriodEntity> builder)
    {
        builder.ToTable("LoanOwnershipPeriods");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.LoanId).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Owner).HasMaxLength(100).IsRequired();
        builder.Property(e => e.StartDate).HasColumnType("date");
        builder.Property(e => e.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

        // Prevents the same ownership window from being persisted twice if a batch is reprocessed.
        builder.HasIndex(e => new { e.LoanId, e.Owner, e.StartDate }).IsUnique();

        builder.HasMany(e => e.PeriodBalances)
            .WithOne(b => b.LoanOwnershipPeriod)
            .HasForeignKey(b => b.LoanOwnershipPeriodId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
