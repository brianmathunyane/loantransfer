using LoanProcessor.Api.Contracts;

namespace LoanProcessor.Api.Validation;

public interface IRawLoanBatchValidator
{
    BatchValidationResult Validate(RawLoanBatchRequest batch);
}
