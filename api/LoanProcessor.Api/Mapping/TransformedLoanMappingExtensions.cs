using LoanProcessor.Api.Contracts;
using LoanProcessor.Data.Repositories;

namespace LoanProcessor.Api.Mapping;

public static class TransformedLoanMappingExtensions
{
    public static TransformedLoanDto ToDto(this TransformedLoanRecord record) =>
        new(record.LoanId, record.Owner, record.StartDate, record.RpbByPeriod);
}
