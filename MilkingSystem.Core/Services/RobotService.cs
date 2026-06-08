using MilkingSystem.Core.Models;
using MilkingSystem.Core.Repositories;

namespace MilkingSystem.Core.Services;

public class RobotService(IRobotRepository robotRepository) : IRobotService
{
    private readonly IRobotRepository _robotRepository = robotRepository;

    public Task<List<Robot>> GetAllRobots()
        => _robotRepository.GetAllRobots();

    public Task<Robot?> GetRobotById(int id)
        => _robotRepository.GetRobotById(id);

    public Task<List<Robot>> GetActiveRobots()
        => _robotRepository.GetActiveRobots();
}
