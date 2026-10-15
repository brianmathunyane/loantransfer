namespace LoanProcessor.Data.Entities;

/// <summary>
/// One row per ownership window: a loan, owned by one party, from <see cref="StartDate"/>
/// until the next window's start date (open-ended for the current owner).
/// </summary>
public sealed class LoanOwnershipPeriodEntity
{
    public Guid Id { get; set; }
    public required string LoanId { get; set; }
    public required string Owner { get; set; }
    public DateOnly StartDate { get; set; }
    public Guid BatchId { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public List<LoanPeriodBalanceEntity> PeriodBalances { get; set; } = [];
}
