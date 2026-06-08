using MilkingSystem.Core.Models;
using MilkingSystem.Core.Services;
using Moq;
using Xunit;

namespace MilkingSystem.Tests;

/// <summary>
/// Example unit tests demonstrating proper unit testing patterns.
/// Candidates can use these as a reference for writing their own tests.
/// </summary>
public class ExampleUnitTests
{
    [Fact]
    public void Animal_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var animal = new Animal();

        // Assert
        Assert.Equal(0, animal.Id);
        Assert.Equal(string.Empty, animal.IdentificationNumber);
        Assert.Null(animal.Name);
        Assert.Null(animal.BirthDate);
    }

    [Fact]
    public void MilkingEvent_CanBeCreated()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;

        // Act
        var milkingEvent = new MilkingEvent
        {
            Id = 1,
            AnimalId = 1,
            RobotId = 1,
            Timestamp = timestamp,
            MilkYieldLiters = 25.5m,
            Duration = 360
        };

        // Assert
        Assert.Equal(1, milkingEvent.Id);
        Assert.Equal(1, milkingEvent.AnimalId);
        Assert.Equal(1, milkingEvent.RobotId);
        Assert.Equal(timestamp, milkingEvent.Timestamp);
        Assert.Equal(25.5m, milkingEvent.MilkYieldLiters);
        Assert.Equal(360, milkingEvent.Duration);
    }

    [Fact]
    public void WeightMeasurement_CanBeCreated()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;

        // Act
        var measurement = new WeightMeasurement
        {
            Id = 1,
            AnimalId = 1,
            RobotId = 1,
            Timestamp = timestamp,
            WeightKg = 650.5m
        };

        // Assert
        Assert.Equal(1, measurement.Id);
        Assert.Equal(650.5m, measurement.WeightKg);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100)]
    public void MilkYield_AcceptsVariousValues(decimal milkYield)
    {
        // Arrange & Act
        var milkingEvent = new MilkingEvent { MilkYieldLiters = milkYield };

        // Assert
        Assert.Equal(milkYield, milkingEvent.MilkYieldLiters);
    }

    [Fact]
    public void Robot_DefaultIsActiveIsFalse()
    {
        // Arrange & Act
        var robot = new Robot();

        // Assert
        Assert.False(robot.IsActive);
    }
}
