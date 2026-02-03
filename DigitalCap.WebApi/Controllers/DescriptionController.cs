using DigitalCap.Core.DTO;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models.ImageDescription;
using DigitalCap.Core.ViewModels;
using DigitalCap.Infrastructure.Service;
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
        private readonly IGradingService _gradingService;
        public DescriptionController(IDescriptionService descriptionService, IGradingService gradingService)
        {
            _descriptionService = descriptionService;
            _gradingService = gradingService;

        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllDescription()
        {
            var result = await _descriptionService.GetAllAsync();
            return Ok(result);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> GetDescriptionById(int id)
        {
            var result = await _descriptionService.GetByIdAsync(id);
            if (result == null)
                return NotFound("Description not found");

            return Ok(result);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> ManageDescription(bool isActive = false, int descriptionRestoreFilter = 0, int searchDescriptionRestoreFilter = 0)
        {
            var username = User.Identity?.Name;

            var result = await _descriptionService.ManageDescriptionAsync(username, isActive, descriptionRestoreFilter, searchDescriptionRestoreFilter);

            if (!result.IsSuccess)
            {
                if (result.Message == "AccessDenied")
                    return Forbid();

                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ManageDescriptionRedirect([FromBody] ManageDescriptionFilterRequest request)
        {
            var result = await _descriptionService.SetManageDescriptionFiltersAsync(request);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(new { redirectUrl = "/manage-description" });
        }


        //[HttpPost("[action]")]
        //public async Task<IActionResult> AddDescription([FromBody] ImageDescriptionDTO model)
        //{
        //    var result = await _descriptionService.CreateAsync(model);
        //    return result.IsSuccess ? Ok("Created") : BadRequest(result.Message);
        //}

        [HttpPost("[action]")]
        public async Task<IActionResult> AddNewDescription([FromBody] ImageDescriptionViewModel model)
        {
            var result = await _descriptionService.AddNewDescriptionAsync(model);

            if (!result.IsSuccess)
                return BadRequest(result.Message ?? result.ErrorMessage);

            return Ok(true);
        }

        //[HttpPost("action")]
        //public async Task<IActionResult> AddDescription()
        //{
        //    var userName = User.Identity?.Name;

        //    var result = await _descriptionService.GetAddDescriptionMetadataAsync(userName);

        //    return result.IsSuccess
        //        ? Ok(result.Data)
        //        : Forbid(result.Message);
        //}

        [HttpPut("[action]")]
        public async Task<IActionResult> EditDescription([FromBody] ImageDescriptionViewModel model)
        {
            var result = await _descriptionService.EditDescriptionAsync(model);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(true);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> EditDescriptionById(int descriptionId, Guid sectionId, int tankTypeId)
        {
            var username = User.Identity?.Name ?? "";

            var result = await _descriptionService.GetDescriptionForEditAsync(descriptionId, sectionId, tankTypeId, username);

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

        //[HttpPut("[action]/{id}")]
        //public async Task<IActionResult> UpdateDescription(int id, [FromBody] UpdateDescriptionRequest model)
        //{
        //    var username = User.Identity?.Name;
        //    var result = await _descriptionService.UpdateDescriptionAsync(model, username);

        //    return result.IsSuccess ? Ok("Updated") : BadRequest(result.Message);
        //}



        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateDescription(int id, [FromBody] UpdateDescriptionRequest model)
        //{
        //    if (id != model.Id)
        //        return BadRequest("Id mismatch");

        //    var result = await _descriptionService.UpdateAsync(ImageDescriptions model);

        //    return result.IsSuccess
        //        ? Ok("Updated")
        //        : BadRequest(result.Message);
        //}


        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateImageDescription([FromBody] ImageDescriptionViewModel model)
        {
            if (model == null)
                return BadRequest("Invalid request");

            var result = await _descriptionService.UpdateAsync(model);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Data); // returns updated model
        }

        //[HttpGet("[action]")]
        //public async Task<IActionResult> EditDescriptionById(int id)
        //{
        //    var username = User.Identity?.Name ?? "";

        //    var result = await _descriptionService.EditDescriptionByIdAsync(id, username);

        //    if (!result.IsSuccess)
        //    {
        //        if (result.Message == "AccessDenied")
        //            return Forbid();

        //        if (result.Message == "NotFound")
        //            return NotFound();

        //        return BadRequest(result.Message);
        //    }

        //    return Ok(result.Data);
        //}


        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateDescription(int id, ImageDescriptionViewModel model)
        {
            model.Id = id;
            var result = await _descriptionService.UpdateAsync(model);
            return result.IsSuccess ? Ok("Updated") : BadRequest(result.Message);
        }



        [HttpGet("[action]")]
        public async Task<IActionResult> GetSectionNameByTemplateNameAndVesselType([FromQuery] string templateName, [FromQuery] string vesselType)
        {
            var result = await _descriptionService.GetSectionNamesByTemplateNameAndVesselTypeAsync(templateName, vesselType);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }


        //[HttpGet("[action]")]
        //public async Task<IActionResult> DescriptionVesselTypeFilter_VesselType()
        //{
        //    var result = await _descriptionService.GetDistinctVesselTypesAsync();

        //    if (!result.IsSuccess)
        //        return BadRequest(result.Message);

        //    return Ok(result.Data);
        //}


    }
}

