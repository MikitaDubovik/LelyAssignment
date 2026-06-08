using MilkingSystem.Core.Models;
using MilkingSystem.Core.Repositories;

namespace MilkingSystem.Core.Services;

public class RobotService(IRobotRepository robotRepository) : IRobotService
{
    private readonly IRobotRepository _robotRepository = robotRepository;

    public List<Robot> GetAllRobots()
        => _robotRepository.GetAllRobots();

    public Robot? GetRobotById(int id)
        => _robotRepository.GetRobotById(id);

    public List<Robot> GetActiveRobots()
        => _robotRepository.GetActiveRobots();
}
