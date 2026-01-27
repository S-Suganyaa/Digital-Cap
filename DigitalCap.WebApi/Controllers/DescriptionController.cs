using DigitalCap.Core.DTO;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models.ImageDescription;
using DigitalCap.Core.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DigitalCap.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DescriptionController : ControllerBase
    {
        private readonly IDescriptionService _descriptionService;
        public DescriptionController(IDescriptionService descriptionService)
        {
            _descriptionService = descriptionService;

        }
                
        [HttpGet("GetAllDescription")]
        public async Task<IActionResult> GetAllDescription()
        {
            var result = await _descriptionService.GetAllAsync();
            return Ok(result);
        }


        [HttpGet("GetDescriptionById/{id}")]
        public async Task<IActionResult> GetDescriptionById(int id)
        {
            var result = await _descriptionService.GetByIdAsync(id);
            if (result == null)
                return NotFound("Description not found");

            return Ok(result);
        }


        [HttpGet("manage")]
        public async Task<IActionResult> ManageDescription(bool isActive = false,int descriptionRestoreFilter = 0,int searchDescriptionRestoreFilter = 0)
        {
            var username = User.Identity?.Name;

            var result = await _descriptionService.ManageDescriptionAsync(username, isActive,descriptionRestoreFilter,searchDescriptionRestoreFilter);

            if (!result.IsSuccess)
            {
                if (result.Message == "AccessDenied")
                    return Forbid();

                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpPost("manage-description-redirect")]
        public async Task<IActionResult> ManageDescriptionRedirect([FromBody] ManageDescriptionFilterRequest request)
        {
            var result = await _descriptionService.SetManageDescriptionFiltersAsync(request);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(new { redirectUrl = "/manage-description" });
        }


        [HttpPost("AddDescription")]
        public async Task<IActionResult> AddDescription(ImageDescriptions model)
        {
            var result = await _descriptionService.CreateAsync(model);
            return result.IsSuccess ? Ok("Created") : BadRequest(result.Message);
        }

        [HttpGet("AddDescription")]
        public async Task<IActionResult> AddDescription()
        {
            var username = User.Identity?.Name ?? "";

            var result = await _descriptionService.GetDescriptionForAddAsync(username);

            if (!result.IsSuccess)
            {
                if (result.Message == "AccessDenied")
                    return Forbid();

                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpPost("AddNewDescription")]
        public async Task<IActionResult> AddNewDescription(ImageDescriptionViewModel model)
        {
            var result = await _descriptionService.AddNewDescriptionAsync(model);

            if (!result.IsSuccess)
                return BadRequest(result.Message ?? result.ErrorMessage);

            return Ok(new { message = "Description created successfully", filter = 1, searchValue = 1 });
        }

        [HttpPut("EditDescription")]
        public async Task<IActionResult> EditDescription([FromBody] ImageDescriptionViewModel model)
        {
            var result = await _descriptionService.EditDescriptionAsync(model);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(true);
        }

        
        [HttpGet("EditDescriptionById/{id}")]
        public async Task<IActionResult> EditDescriptionById(int id)
        {
            var username = User.Identity?.Name ?? "";

            var result = await _descriptionService.EditDescriptionByIdAsync(id, username);

            if (!result.IsSuccess)
            {
                if (result.Message == "AccessDenied")
                    return Forbid();

                if (result.Message == "NotFound")
                    return NotFound();

                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }


        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateDescription(int id, ImageDescriptions model)
        {
            model.Id = id;
            var result = await _descriptionService.UpdateAsync(model);
            return result.IsSuccess ? Ok("Updated") : BadRequest(result.Message);
        }

        [HttpGet("sections")]
        public async Task<IActionResult> GetSectionNameByTemplateNameAndVesselType([FromQuery] string templateName,[FromQuery] string vesselType)
        {
            var result = await _descriptionService.GetSectionNamesByTemplateNameAndVesselTypeAsync(templateName, vesselType);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        
        [HttpGet("DescriptionVesselTypeFilter")]
        public async Task<IActionResult> DescriptionVesselTypeFilter_VesselType()
        {
            var result = await _descriptionService.GetDistinctVesselTypesAsync();

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

    }
}
