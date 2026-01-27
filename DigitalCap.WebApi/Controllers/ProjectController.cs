using DigitalCap.Core.Enumerations;
using DigitalCap.Core.Helpers.Constants;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.Permissions;
using DigitalCap.Core.Security;
using DigitalCap.Core.ViewModels;
using DigitalCap.Infrastructure.Service;
using DigitalCap.WebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using System.Security.Claims;
using System.Text.Json;

namespace DigitalCap.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IConfiguration _configData;

        public ProjectController(IProjectService projectService, IConfiguration configData)
        {
            _projectService = projectService;
            _configData = configData;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Create([FromBody] Project model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUser = User.Identity?.Name;

            var projectId = await _projectService.CreateProject(model, currentUser);
            return Ok(new { ProjectId = projectId });
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> Update(int id)
        {
            var currentUser = User.Identity?.Name;

            var result = await _projectService.UpdateProject(id, currentUser);
            if (!result.IsSuccess)
                return BadRequest(new { result.ErrorMessage, result.ErrorCode });

            return Ok(result.Data);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Update_Project(Project model)
        {
            //if (id != model.ID)
            //    return BadRequest("Project id mismatch");

            var currentUser = User.Identity?.Name;

            var result = await _projectService.Update_Project(model, currentUser);
            if (!result.IsSuccess)
                return BadRequest(new { result.ErrorMessage, result.ErrorCode });

            return Ok(result.Data);
        }

        [HttpPost("[action]")]
        public async void CloseProject(int id)
        {
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            await _projectService.UpdateProjectStatus(id, currentUserId, (int)ProjectStatus.Closed);

            return;
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var result = await _projectService.DeleteProject(id, currentUserId);

            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            return Ok(result.Data);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Details(int id)
        {
            var result = await _projectService.GetDetails(id);

            if (!result.IsSuccess)
            {
                return StatusCode(500, result.ErrorMessage);
            }
            return Ok(result.Data);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> GetSummary(int id)
        {
            var result = _projectService.GetSummary(id);

            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task UpdatePriority(int id, int? priority)
        {
            await _projectService.UpdatePriority(id, priority);
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> CompletionStatus(int id, ProjectStatusType statusType)
        {
            if (statusType != ProjectStatusType.Invoicing
             && statusType != ProjectStatusType.Technical)
                return BadRequest("");


            var result = _projectService.GetCompletionStatus(id, statusType);

            return Ok(result);

        }
        [HttpGet("[action]")]
        public LoginData LoginData()
        {
            LoginData Logininfo = new LoginData()
            {
                Id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value,
                FirstName = null,
                LastName = null,
                ProviderKey = null,
                EmailId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value
            };
            //if (ApplicationUserData.LoginData != null)
            //{
            //	if (ApplicationUserData.LoginData.ProviderKey != null)
            //	{
            return Logininfo;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _projectService.GetAllUsers();
            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CancelProject(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _projectService.UpdateProjectStatus(id, userId, (int)ProjectStatus.Cancelled);
            return NoContent();
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> List()
        {
            var isElectron = Convert.ToBoolean(_configData["IsElectron"]);
            var currentUser = User.Identity?.Name;

            var result = _projectService.GetList(currentUser, isElectron);

            return Ok(result);
        }

        //public async Task<IActionResult> EmptyProject()
        //{
        //    return View();
        //}
        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllProjectsGridData([FromBody] DataSourceRequest request)
        {
            var Iselectron = Convert.ToBoolean(_configData["IsElectron"]);
            var currentUser = User.Identity.Name;
            var result = _projectService.GetAllProjectsGridData(Iselectron, currentUser);
            return Ok(result);
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> UpdateProjectPercentComplete(int projectId)
        {
            var result = await _projectService.UpdateProjectPercentComplete(projectId);

            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessages);

            return Ok(result.Data);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllClients(bool wcnList = false)
        {
            var clients = _projectService.GetAllClients(wcnList);
            return Ok(clients);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllSurveyors(bool wcnList = false)
        {
            var users = _projectService.GetAllSurveyors(wcnList);
            return Ok(users);
        }

        [HttpGet("[action]")]
        [PermissionAuthorize(DigitalCap.Core.Security.Permission.ViewWorkflowAndTasks)]
        public async Task<IActionResult> Workflow(int? id)
        {

            if (!id.HasValue)
                return BadRequest("Parameter(s) null or empty.");

            var model = _projectService.Workflow(id);

            return Ok(model);
        }



        // [PermissionAuthorize(false, Core.Security.Permission.ViewAbsProjectLandingPage, Core.Security.Permission.ViewClientProjectLandingPage)]
        [HttpGet("[action]")]
        public async Task<IActionResult> Landing(int? id)
        {
            if (!id.HasValue)
            {
                return BadRequest("Parameter(s) null or empty.");
            }

            var currentuser = User.Identity.Name;

            var result = _projectService.Landing(id, currentuser);

            return Ok(result);
        }

        //  [PermissionAuthorize(false, Permission.ViewClientProjectLandingPage)]
        [HttpGet("[action]")]
        public async Task<IActionResult> ClientLanding(int? id)
        {
            if (!id.HasValue)
            {
                return BadRequest("Parameter(s) null or empty.");
            }

            var result = _projectService.ClientLanding(id);
            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SaveReportDetails([FromBody] ReportDetailsViewModel reportDetails)
        {
            if (reportDetails == null)
                return BadRequest("Invalid request");

            var result = await _projectService.SaveReportDetails(reportDetails);

            return Ok(true);
        }


        // [PermissionAuthorize(false, Permission.ViewRecentActivityAllProjectsAllRegions, Permission.ViewRecentActivityAllProjectsOwnRegion)]
        //public IActionResult RecentActivity()
        //{
        //    ViewData["ActionName"] = "GetRecentActivities2";
        //    return View();
        //}

        [HttpGet("[action]")]
        public async Task<IActionResult> RecentActivityForProject(int projectId)
        {
            // ViewData["ActionName"] = "GetAllActivities2ByProjectId";
            //ViewData["ProjectId"] = projectId;
            //ViewData["ProjectName"] = await _projectService.GetProjectName(projectId);

            var result = _projectService.RecentActivityForProject(projectId);

            return Ok(result);
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> GetProjectName(int id)
        {
            var result = await _projectService.GetProjectName(id);

            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessages);

            return Ok(result.Data);
        }


    }
}
