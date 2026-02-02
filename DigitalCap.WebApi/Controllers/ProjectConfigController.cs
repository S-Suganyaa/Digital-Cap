using DigitalCap.Core.DTO;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models.Grading;
using DigitalCap.Core.Models.View.Admin;
using DigitalCap.Core.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalCap.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectConfigController : ControllerBase
    {
        private readonly IGradingService _gradingService;
        private readonly ITankService _tankService;

        public ProjectConfigController(IGradingService gradingService, ITankService tankService)
        {
            _gradingService = gradingService;
            _tankService = tankService;
        }
        #region ManageGrading

        [HttpGet("[action]")]
        public async Task<IActionResult> ManageGradingAbsTanks(bool isActive = false, int gradingRestoreFilter = 0, int searchGradingRestoreFilter = 0)
        {
            var username = User.Identity?.Name;

            var result = await _gradingService.ManageGradingAsync(username, isActive, gradingRestoreFilter, searchGradingRestoreFilter);

            if (!result.IsSuccess)
            {
                if (result.Message == "AccessDenied")
                    return Forbid();

                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ManageGradingAbsTanksRedirect([FromBody] ManageGradingFilterRequest request)
        {
            var result = await _gradingService.SetManageGradingFiltersAsync(request);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(new { redirectUrl = "/manage-grading" });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> AddGrading()
        {
            var username = User.Identity?.Name ?? "";

            var result = await _gradingService.GetGradingForAddAsync(username);

            if (!result.IsSuccess)
            {
                if (result.Message == "AccessDenied")
                    return Forbid();

                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> EditGradingById(int gradingId, Guid sectionId, int tankTypeId)
        {
            var username = User.Identity?.Name ?? "";

            var result = await _gradingService.GetGradingForEditAsync(gradingId, sectionId, tankTypeId, username);

            if (!result.IsSuccess)
            {
                if (result.Message == "AccessDenied")
                    return Forbid();

                if (result.Message == "Grading not found")
                    return NotFound(result.Message);

                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddNewGrading([FromBody] Grading model)
        {
            var result = await _gradingService.CreateGradingAsync(model);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(new { message = "Grading created successfully", filter = 1, searchValue = 1 });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> EditGrading([FromBody] Grading model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _gradingService.UpdateGradingAsync(model);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(new { message = "Grading updated successfully", filter = 1, searchValue = 1 });
        }

       
        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteGrading([FromBody] DeleteGradingRequest request)
        {
            try
            {
                var result = await _gradingService.DeleteGradingAsync(
                    request.GradingId,
                    request.TankId
                );

                if (!result.IsSuccess)
                    return BadRequest(result.Message);

                return Ok(result.Data);
            }
            catch
            {
                return BadRequest();
            }
        }


        #endregion

        #region Manage Tank

        [HttpGet("[action]")]
        public async Task<IActionResult> ManageTank(bool isActive = false, string imo = null, string projectId = null, int tankRestoreFilter = 0, int searchRestoreFilter = 0)
        {
            var username = User.Identity?.Name;

            var result = await _tankService.ManageTankAsync(username, isActive, imo, projectId, tankRestoreFilter, searchRestoreFilter);

            if (!result.IsSuccess)
            {
                if (result.Message == "AccessDenied")
                    return Forbid();

                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ManageTankRedirect([FromBody] ManageTankFilterRequest request)
        {
            var result = await _tankService.SetManageTankFiltersAsync(request);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(new { redirectUrl = "/manage-tank", imo = request.IMO, projectId = request.ProjectId });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetDataByIMO(string imo)
        {
            var result = await _tankService.GetDataByIMOAsync(imo);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> AddTank(string imo = null, string projectId = null, bool projectTanktype = false)
        {
            var result = await _tankService.GetTankForAddAsync(imo, projectId, projectTanktype);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> EditTankById(Guid tankId, int projectId)
        {
            var username = User.Identity?.Name ?? "";

            var result = await _tankService.GetTankForEditAsync(tankId, projectId, username);

            if (!result.IsSuccess)
            {
                if (result.Message == "AccessDenied")
                    return Forbid();

                if (result.Message == "Tank not found")
                    return NotFound(result.Message);

                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddNewTank([FromBody] CreateTankRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var username = User.Identity.Name;

            var result = await _tankService.CreateTankAsync(model, username);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(new { message = "Tank created successfully" });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> EditTank([FromBody] CreateTankRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var username = User.Identity?.Name ?? "";

            var result = await _tankService.UpdateTankAsync(model, username);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            //if (!string.IsNullOrEmpty(imo) && !string.IsNullOrEmpty(projectId))
            //{
            //    return Ok(new { message = "Tank updated successfully", filter = 1, searchValue = 1, imo = imo, projectId = projectId });
            //}

            return Ok(new { message = "Tank updated successfully", filter = 1, searchValue = 1 });
        }

        #endregion
    }
}
