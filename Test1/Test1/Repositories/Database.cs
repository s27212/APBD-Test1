using Microsoft.Data.SqlClient;
using Test1.DTO;
using Test1.Models;

namespace Test1.Repositories;

public class Database : IDatabase
{
    private readonly IConfiguration _configuration;

    public Database(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private SqlConnection GetConnection()
    {
        return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
    }

    public async Task<TeamMember?> GetTeamMember(int id)
    {
        var con = GetConnection();
        await con.OpenAsync();
        await using var com = con.CreateCommand();
        com.CommandText = "SELECT * FROM TeamMember WHERE IdTeamMember = @IdTeamMember";
        com.Parameters.AddWithValue("@IdTeamMember", id);
        
        await using var reader = await com.ExecuteReaderAsync();
        if (!reader.HasRows)
        {
            return null;
        }

        reader.Read();
        var teamMember = new TeamMember()
        {
            IdTeamMember = (int)reader["IdTeamMember"],
            FirstName = (string)reader["FirstName"],
            LastName = (string)reader["LastName"],
            Email = (string)reader["Email"]
        };
        
        return teamMember;
    }

    public async Task<IEnumerable<TaskDTO>> GetTasksByMember(int id)
    {
        var con = GetConnection();
        await con.OpenAsync();
        await using var com = con.CreateCommand();
        com.CommandText = "SELECT t.Name, t.Description, t.Deadline, p.Name as ProjectName, tt.Name as TaskType " +
                          "FROM [Task] t " +
                          "JOIN TaskType tt on tt.IdTaskType = t.IdTaskType " +
                          "JOIN Project p on p.IdProject = t.IdProject "+
                          "WHERE t.IdAssignedTO = @IdTeamMember OR t.IdCreator = @IdTeamMember " +
                          "ORDER BY t.Deadline DESC";
        com.Parameters.AddWithValue("@IdTeamMember", id);
        var tasks = new List<TaskDTO>();
        await using var reader = await com.ExecuteReaderAsync();
        while (reader.Read())
        {
            var taskDto = new TaskDTO()
            {
                Name = (string)reader["Name"],
                Description = (string)reader["Description"],
                Deadline = (DateTime)reader["Deadline"],
                ProjectName = (string)reader["ProjectName"],
                Type = (string)reader["TaskType"]
            };
            tasks.Add(taskDto);
        }

        return tasks;
    }

    public async Task<int> DeleteProjectById(int id)
    {
        var con = GetConnection();
        await con.OpenAsync();
        var tran = con.BeginTransaction();
        await using var com = con.CreateCommand();
        com.Transaction = (SqlTransaction)tran;
        com.CommandText = "DELETE FROM Task WHERE IdProject = @IdProject";
        com.Parameters.AddWithValue("@IdProject", id);
        await com.ExecuteNonQueryAsync();

        com.CommandText = "DELETE FROM Project WHERE IdProject = @IdProject";
        var rows = await com.ExecuteNonQueryAsync();
        
        await tran.CommitAsync();

        return rows;
    }

    public async Task<bool> ProjectExists(int id)
    {
        var con = GetConnection();
        await con.OpenAsync();
        await using var com = con.CreateCommand();
        com.CommandText = "SELECT 1 FROM Project WHERE IdProject = @IdProject";
        com.Parameters.AddWithValue("@IdProject", id);
        await using var reader = await com.ExecuteReaderAsync();
        return reader.HasRows;
    }
}