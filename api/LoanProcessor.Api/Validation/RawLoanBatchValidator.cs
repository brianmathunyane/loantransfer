using System.Text.RegularExpressions;
using LoanProcessor.Api.Contracts;

namespace LoanProcessor.Api.Validation;

public sealed partial class RawLoanBatchValidator : IRawLoanBatchValidator
{
    public BatchValidationResult Validate(RawLoanBatchRequest batch)
    {
        var result = new BatchValidationResult();

        if (batch.Records.Count == 0)
        {
            result.AddError("At least one loan record is required.");
            return result;
        }

        for (var i = 0; i < batch.Records.Count; i++)
        {
            ValidateRecord(batch.Records[i], i, result);
        }

        return result;
    }

    private static void ValidateRecord(RawLoanRecordRequest record, int index, BatchValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(record.LoanId))
            result.AddError($"records[{index}]: loanId is required.");

        if (string.IsNullOrWhiteSpace(record.Owner))
            result.AddError($"records[{index}]: owner is required.");

        if (record.StartDate == default)
            result.AddError($"records[{index}]: startDate is required.");

        if (record.RpbByPeriod.Count == 0 || record.RpbByPeriod.Values.All(v => v is null))
            result.AddError($"records[{index}]: rpbByPeriod must contain at least one non-null value.");

        foreach (var period in record.RpbByPeriod.Keys)
        {
            if (!PeriodKeyPattern().IsMatch(period))
                result.AddError($"records[{index}]: '{period}' is not a valid period key (expected format fyyyymm).");
        }
    }

    [GeneratedRegex(@"^f\d{6}$")]
    private static partial Regex PeriodKeyPattern();
}
