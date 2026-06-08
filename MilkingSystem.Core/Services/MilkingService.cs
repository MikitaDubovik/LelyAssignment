using Microsoft.Extensions.Options;
using MilkingSystem.Core.Configuration;
using MilkingSystem.Core.Notifications;
using MilkingSystem.Core.Results;

namespace MilkingSystem.Core.Services;

public class MilkingService(
    DataService dataService,
    IRobotNotifier notifier,
    IOptions<MilkingSettings> settings) : IMilkingService
{
    private readonly DataService _dataService = dataService;
    private readonly IRobotNotifier _notifier = notifier;
    private readonly int _protectionWindowHours = settings.Value.ProtectionWindowHours;

    public async Task<MilkingServiceResult> RecordMilking(
        int animalId, int robotId, decimal milkYieldLiters, int? duration, DateTime? timestamp)
    {
        if (milkYieldLiters <= 0)
        {
            return new MilkingServiceResult { Status = MilkingServiceStatus.MilkAmountIsIncorrect };
        }

        var animal = _dataService.GetAnimalById(animalId);
        if (animal is null)
        {
            return new MilkingServiceResult { Status = MilkingServiceStatus.AnimalNotFound };
        }

        var robot = _dataService.GetRobotById(robotId);
        if (robot is null)
        {
            return new MilkingServiceResult { Status = MilkingServiceStatus.RobotNotFound };
        }
        if (!robot.IsActive)
        {
            return new MilkingServiceResult { Status = MilkingServiceStatus.RobotNotActive };
        }

        var effectiveTimestamp = timestamp?.ToUniversalTime() ?? DateTime.UtcNow;

        // Fast pre-check using in-memory state - avoids lock acquisition for the common case.
        if (_notifier.WasRecentlyMilked(animalId, _protectionWindowHours))
        {
            return new MilkingServiceResult { Status = MilkingServiceStatus.RecentlyMilked };
        }

        // Per-animal lock: different animals are processed in parallel;
        // the same animal is serialised so the authoritative check-then-save is atomic.
        var animalLock = _dataService.GetAnimalMilkingLock(animalId);
        await animalLock.WaitAsync();
        try
        {
            // Authoritative DB check inside the lock - catches the race where two concurrent
            // requests for the same animal both passed the in-memory pre-check.
            var lastMilking = _dataService.GetLastMilkingForAnimal(animalId);
            if (lastMilking is not null
                && (DateTime.UtcNow - lastMilking.Timestamp).TotalHours < _protectionWindowHours)
            {
                return new MilkingServiceResult
                {
                    Status = MilkingServiceStatus.RecentlyMilked,
                    LastMilkedAt = lastMilking.Timestamp,
                    NextAllowedAt = lastMilking.Timestamp.AddHours(_protectionWindowHours)
                };
            }

            var id = _dataService.SaveMilkingEvent(animalId, robotId, effectiveTimestamp, milkYieldLiters, duration);

            _notifier.NotifyMilkingCompleted(new MilkingNotification
            {
                AnimalId = animalId,
                RobotId = robotId,
                Timestamp = effectiveTimestamp,
                AnimalIdentificationNumber = animal.IdentificationNumber
            });

            return new MilkingServiceResult { Status = MilkingServiceStatus.Success, EventId = id };
        }
        finally
        {
            animalLock.Release();
        }
    }
}
