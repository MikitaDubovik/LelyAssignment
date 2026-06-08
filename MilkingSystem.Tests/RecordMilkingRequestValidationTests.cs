using System.ComponentModel.DataAnnotations;
using MilkingSystem.Api.Models;

namespace MilkingSystem.Tests;

public class RecordMilkingRequestValidationTests
{
    [Fact]
    public void ValidRequest_PassesValidation()
    {
        var request = new RecordMilkingRequest
        {
            AnimalId = 1,
            RobotId = 1,
            MilkYieldLiters = 25.5m
        };

        Assert.Empty(Validate(request));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public void AnimalId_ZeroOrNegative_FailsValidation(int animalId)
    {
        var request = new RecordMilkingRequest { AnimalId = animalId, RobotId = 1, MilkYieldLiters = 25.5m };

        Assert.True(HasErrorFor(Validate(request), nameof(RecordMilkingRequest.AnimalId)));
    }

    [Fact]
    public void AnimalId_One_PassesValidation()
    {
        var request = new RecordMilkingRequest { AnimalId = 1, RobotId = 1, MilkYieldLiters = 25.5m };

        Assert.False(HasErrorFor(Validate(request), nameof(RecordMilkingRequest.AnimalId)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RobotId_ZeroOrNegative_FailsValidation(int robotId)
    {
        var request = new RecordMilkingRequest { AnimalId = 1, RobotId = robotId, MilkYieldLiters = 25.5m };

        Assert.True(HasErrorFor(Validate(request), nameof(RecordMilkingRequest.RobotId)));
    }

    [Fact]
    public void RobotId_One_PassesValidation()
    {
        var request = new RecordMilkingRequest { AnimalId = 1, RobotId = 1, MilkYieldLiters = 25.5m };

        Assert.False(HasErrorFor(Validate(request), nameof(RecordMilkingRequest.RobotId)));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("0.0009")] // below the 0.001 minimum
    public void MilkYieldLiters_BelowMinimum_FailsValidation(string yieldStr)
    {
        var request = new RecordMilkingRequest
        {
            AnimalId = 1,
            RobotId = 1,
            MilkYieldLiters = decimal.Parse(yieldStr, System.Globalization.CultureInfo.InvariantCulture)
        };

        Assert.True(HasErrorFor(Validate(request), nameof(RecordMilkingRequest.MilkYieldLiters)));
    }

    [Theory]
    [InlineData("0.001")]  // minimum
    [InlineData("25.5")]   // typical
    [InlineData("1000000")] // maximum
    public void MilkYieldLiters_WithinRange_PassesValidation(string yieldStr)
    {
        var request = new RecordMilkingRequest
        {
            AnimalId = 1,
            RobotId = 1,
            MilkYieldLiters = decimal.Parse(yieldStr, System.Globalization.CultureInfo.InvariantCulture)
        };

        Assert.False(HasErrorFor(Validate(request), nameof(RecordMilkingRequest.MilkYieldLiters)));
    }

    [Fact]
    public void Duration_Null_PassesValidation()
    {
        // Null is always valid for optional fields.
        var request = new RecordMilkingRequest { AnimalId = 1, RobotId = 1, MilkYieldLiters = 25.5m, Duration = null };

        Assert.False(HasErrorFor(Validate(request), nameof(RecordMilkingRequest.Duration)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Duration_ZeroOrNegative_FailsValidation(int duration)
    {
        var request = new RecordMilkingRequest { AnimalId = 1, RobotId = 1, MilkYieldLiters = 25.5m, Duration = duration };

        Assert.True(HasErrorFor(Validate(request), nameof(RecordMilkingRequest.Duration)));
    }

    [Fact]
    public void Duration_Positive_PassesValidation()
    {
        var request = new RecordMilkingRequest { AnimalId = 1, RobotId = 1, MilkYieldLiters = 25.5m, Duration = 420 };

        Assert.False(HasErrorFor(Validate(request), nameof(RecordMilkingRequest.Duration)));
    }

    
    private static IList<ValidationResult> Validate(RecordMilkingRequest request)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);
        return results;
    }

    private static bool HasErrorFor(IList<ValidationResult> results, string propertyName) =>
        results.Any(r => r.MemberNames.Contains(propertyName));
}
