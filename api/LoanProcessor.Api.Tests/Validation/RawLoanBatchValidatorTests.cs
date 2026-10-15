using LoanProcessor.Api.Contracts;
using LoanProcessor.Api.Validation;

namespace LoanProcessor.Api.Tests.Validation;

public class RawLoanBatchValidatorTests
{
    private readonly RawLoanBatchValidator _sut = new();

    private static RawLoanRecordRequest ValidRecord() => new()
    {
        LoanId = "A123456",
        StartDate = new DateOnly(2019, 8, 1),
        Owner = "Originator A",
        RpbByPeriod = new Dictionary<string, decimal?> { ["f201908"] = 4500m },
    };

    [Fact]
    public void Validate_WithWellFormedBatch_ReturnsValid()
    {
        var batch = new RawLoanBatchRequest { Records = [ValidRecord()] };

        var result = _sut.Validate(batch);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithNoRecords_ReturnsInvalid()
    {
        var batch = new RawLoanBatchRequest { Records = [] };

        var result = _sut.Validate(batch);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("At least one loan record"));
    }

    [Fact]
    public void Validate_WithMissingLoanId_ReturnsInvalid()
    {
        var record = ValidRecord() with { LoanId = " " };
        var batch = new RawLoanBatchRequest { Records = [record] };

        var result = _sut.Validate(batch);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("loanId is required"));
    }

    [Fact]
    public void Validate_WithAllNullRpbValues_ReturnsInvalid()
    {
        var record = ValidRecord() with
        {
            RpbByPeriod = new Dictionary<string, decimal?> { ["f201908"] = null },
        };
        var batch = new RawLoanBatchRequest { Records = [record] };

        var result = _sut.Validate(batch);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("non-null value"));
    }

    [Fact]
    public void Validate_WithMalformedPeriodKey_ReturnsInvalid()
    {
        var record = ValidRecord() with
        {
            RpbByPeriod = new Dictionary<string, decimal?> { ["2019-08"] = 4500m },
        };
        var batch = new RawLoanBatchRequest { Records = [record] };

        var result = _sut.Validate(batch);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("not a valid period key"));
    }
}
