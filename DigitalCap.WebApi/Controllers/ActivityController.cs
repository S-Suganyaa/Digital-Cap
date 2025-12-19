using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Infrastructure.Service;
using DigitalCap.Core.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalCap.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityService _activityService;

        public ActivityController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentActivities()
            
        {
            var result = await _activityService.GetRecentActivities();
            return Ok(result);

        }

        [HttpGet("recent/all")]
        public async Task<IActionResult> GetRecentActivities2()
        {
            var data = await _activityService.GetRecentActivities2();
            return Ok(data);
        }

        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetRecentActivities2ByProjectId(
            int projectId,
            [FromQuery] int size = 10)
        {
            var result = await _activityService.GetRecentActivitiesByProjectId(projectId);
            if (result == null || !result.Any())
                return NotFound();
            return Ok(result);
        }

        [HttpGet("project/{projectId}/all")]
        public async Task<IActionResult> GetAllActivities2ByProjectId(int projectId)
        {
            var result = await _activityService.GetAllActivitiesByProjectId(projectId);
            return Ok(result);
        }



    }
}
