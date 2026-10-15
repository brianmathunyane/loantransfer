namespace LoanProcessor.Data.Entities;

/// <summary>
/// The RPB allocated to a single period within one ownership window.
/// </summary>
public sealed class LoanPeriodBalanceEntity
{
    public Guid Id { get; set; }
    public Guid LoanOwnershipPeriodId { get; set; }
    public DateOnly PeriodDate { get; set; }
    public decimal Rpb { get; set; }

    public LoanOwnershipPeriodEntity? LoanOwnershipPeriod { get; set; }
}
