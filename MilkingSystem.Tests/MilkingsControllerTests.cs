using Microsoft.AspNetCore.Mvc;
using MilkingSystem.Api.Controllers;
using MilkingSystem.Api.Models;
using MilkingSystem.Core.Results;
using MilkingSystem.Core.Services;
using Moq;

namespace MilkingSystem.Tests;

public class MilkingsControllerTests
{
    [Fact]
    public async Task RecordMilking_Success_Returns201Created()
    {
        var service = ServiceReturning(new MilkingServiceResult { Status = MilkingServiceStatus.Success, EventId = 42 });
        var controller = Build(service);

        var result = await controller.RecordMilking(ValidRequest());

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, created.StatusCode);
    }

    [Fact]
    public async Task RecordMilking_Success_CreatedAtActionPointsToGetForAnimal()
    {
        var service = ServiceReturning(new MilkingServiceResult { Status = MilkingServiceStatus.Success, EventId = 42 });
        var controller = Build(service);

        var result = await controller.RecordMilking(ValidRequest(animalId: 7));

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(MilkingsController.GetForAnimal), created.ActionName);

        // Route values must include the animalId so the Location header resolves correctly
        var routeValues = created.RouteValues;
        Assert.NotNull(routeValues);
        Assert.Equal(7, routeValues!["animalId"]);
    }

    [Fact]
    public async Task RecordMilking_Success_ResponseBodyContainsEventId()
    {
        var service = ServiceReturning(new MilkingServiceResult { Status = MilkingServiceStatus.Success, EventId = 99 });
        var controller = Build(service);

        var result = await controller.RecordMilking(ValidRequest());

        var created = Assert.IsType<CreatedAtActionResult>(result);

        // The body must expose the generated ID so clients can follow up with a GET
        var body = created.Value!;
        var idProperty = body.GetType().GetProperty("id");
        Assert.NotNull(idProperty);
        Assert.Equal(99, idProperty!.GetValue(body));
    }

    [Fact]
    public async Task RecordMilking_AnimalNotFound_Returns404()
    {
        var service = ServiceReturning(new MilkingServiceResult { Status = MilkingServiceStatus.AnimalNotFound });
        var controller = Build(service);

        var result = await controller.RecordMilking(ValidRequest());

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task RecordMilking_RobotNotFound_Returns404()
    {
        var service = ServiceReturning(new MilkingServiceResult { Status = MilkingServiceStatus.RobotNotFound });
        var controller = Build(service);

        var result = await controller.RecordMilking(ValidRequest());

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task RecordMilking_RobotNotActive_Returns422()
    {
        var service = ServiceReturning(new MilkingServiceResult { Status = MilkingServiceStatus.RobotNotActive });
        var controller = Build(service);

        var result = await controller.RecordMilking(ValidRequest());

        Assert.IsType<UnprocessableEntityObjectResult>(result);
    }

    [Fact]
    public async Task RecordMilking_MilkAmountIsIncorrect_Returns400()
    {
        var service = ServiceReturning(new MilkingServiceResult { Status = MilkingServiceStatus.MilkAmountIsIncorrect });
        var controller = Build(service);

        var result = await controller.RecordMilking(ValidRequest());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task RecordMilking_RecentlyMilked_Returns409Conflict()
    {
        var lastMilkedAt = DateTime.UtcNow.AddHours(-1);
        var nextAllowedAt = lastMilkedAt.AddHours(6);

        var service = ServiceReturning(new MilkingServiceResult
        {
            Status = MilkingServiceStatus.RecentlyMilked,
            LastMilkedAt = lastMilkedAt,
            NextAllowedAt = nextAllowedAt
        });
        var controller = Build(service);

        var result = await controller.RecordMilking(ValidRequest());

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task RecordMilking_RecentlyMilked_ResponseBodyContainsTimestamps()
    {
        // Robots need the timestamps to display a meaningful rejection message.
        var lastMilkedAt = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        var nextAllowedAt = lastMilkedAt.AddHours(6);

        var service = ServiceReturning(new MilkingServiceResult
        {
            Status = MilkingServiceStatus.RecentlyMilked,
            LastMilkedAt = lastMilkedAt,
            NextAllowedAt = nextAllowedAt
        });
        var controller = Build(service);

        var result = await controller.RecordMilking(ValidRequest());

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        var body = conflict.Value!;
        var bodyType = body.GetType();

        Assert.Equal(lastMilkedAt, bodyType.GetProperty("lastMilkedAt")!.GetValue(body));
        Assert.Equal(nextAllowedAt, bodyType.GetProperty("nextAllowedAt")!.GetValue(body));
    }

    [Fact]
    public async Task GetLastForAnimal_WhenEventExists_Returns200()
    {
        var service = new Mock<IMilkingService>();
        service.Setup(s => s.GetLastMilkingForAnimal(1))
            .ReturnsAsync(new Core.Models.MilkingEvent { Id = 1, AnimalId = 1, RobotId = 1, MilkYieldLiters = 25m, Timestamp = DateTime.UtcNow });

        var controller = Build(service);

        var result = await controller.GetLastForAnimal(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetLastForAnimal_WhenNoEvent_Returns404()
    {
        var service = new Mock<IMilkingService>();
        service.Setup(s => s.GetLastMilkingForAnimal(1))
            .ReturnsAsync((Core.Models.MilkingEvent?)null);

        var controller = Build(service);

        var result = await controller.GetLastForAnimal(1);

        Assert.IsType<NotFoundResult>(result);
    }

    private static MilkingsController Build(Mock<IMilkingService> service) =>
        new(service.Object);

    private static RecordMilkingRequest ValidRequest(int animalId = 1, int robotId = 1) =>
        new() { AnimalId = animalId, RobotId = robotId, MilkYieldLiters = 25.5m };

    private static Mock<IMilkingService> ServiceReturning(MilkingServiceResult result)
    {
        var service = new Mock<IMilkingService>();
        service
            .Setup(s => s.RecordMilking(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<int?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(result);
        return service;
    }
}
