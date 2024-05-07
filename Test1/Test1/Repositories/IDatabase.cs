using Test1.DTO;
using Test1.Models;

namespace Test1.Repositories;

public interface IDatabase
{
    Task<TeamMember?> GetTeamMember(int id);
    Task<Dictionary<string, IEnumerable<TaskDTO>>> GetTasksByMember(int id);
    Task<int> DeleteProjectById(int id);
    Task<bool> ProjectExists(int id);
}