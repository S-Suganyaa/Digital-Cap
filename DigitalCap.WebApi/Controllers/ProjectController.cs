using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Infrastructure.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalCap.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromBody] Project model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var projectId = await _projectService.CreateProject(model);
            return Ok(new { ProjectId = projectId });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetById(int id)
        {
            var project = await _projectService.GetProject(id);

            if (project == null)
                return NotFound();

            return Ok(project);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Update(int id, [FromBody] Project model)
        {
            if (id != model.ID)
                return BadRequest("Id mismatch");

            await _projectService.Update(model);
            return NoContent();
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            await _projectService.DeleteProject(id, userId);
            return NoContent();
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAll()
        {
            var projects = await _projectService.GetAllProjects();
            return Ok(projects);
        }

    }
}
