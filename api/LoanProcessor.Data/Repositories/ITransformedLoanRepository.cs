namespace LoanProcessor.Data.Repositories;

public interface ITransformedLoanRepository
{
    Task<IReadOnlyList<TransformedLoanRecord>> GetAllAsync(CancellationToken cancellationToken);
}
