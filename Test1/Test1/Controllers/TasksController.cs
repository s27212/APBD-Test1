using Microsoft.AspNetCore.Mvc;
using Test1.Repositories;

namespace Test1.Controllers;

[ApiController]
[Route("/api/tasks")]
public class TasksController : ControllerBase
{
    private readonly IDatabase _db;

    public TasksController(IDatabase db)
    {
        _db = db;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTeamMemberAndTasks(int id)
    {
        var teamMember = await _db.GetTeamMember(id);
        if (teamMember == null)
        {
            return BadRequest("Team member with provided id does not exist.");
        }
        var tasks = await _db.GetTasksByMember(id);
        return Ok(new {teamMember, tasks});
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        if (! await _db.ProjectExists(id))
        {
            return BadRequest("Project with provided id does not exist.");
        }
        await _db.DeleteProjectById(id);
        return NoContent();
    }
}