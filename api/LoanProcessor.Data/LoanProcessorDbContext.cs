using LoanProcessor.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanProcessor.Data;

public sealed class LoanProcessorDbContext(DbContextOptions<LoanProcessorDbContext> options) : DbContext(options)
{
    public DbSet<LoanOwnershipPeriodEntity> OwnershipPeriods => Set<LoanOwnershipPeriodEntity>();
    public DbSet<LoanPeriodBalanceEntity> PeriodBalances => Set<LoanPeriodBalanceEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LoanProcessorDbContext).Assembly);
    }
}
