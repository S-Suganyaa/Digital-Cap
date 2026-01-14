using DigitalCap.Core.Enumerations;
using DigitalCap.Core.Helpers.Constants;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.ImageDescription;
using DigitalCap.Core.Models.Permissions;
using DigitalCap.Core.Security;
using DigitalCap.Core.ViewModels;
using DigitalCap.Infrastructure.Service;
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
    public class ProjectConfigController : ControllerBase
    {
        private readonly IGradingService _gradingService;
        private readonly IDescriptionService _descriptionService;
        public ProjectConfigController(IGradingService gradingService, IDescriptionService descriptionService)
        {
            _gradingService = gradingService;
            _descriptionService = descriptionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _gradingService.GetAllGradingsAsync();

            //if (!result.Success)
            //    return BadRequest(result.Message);

            return Ok(result);
        }

        // POST api/projectconfig
        [HttpPost]
        public async Task<IActionResult> CreateGrading([FromBody] GradingListViewModel model)
        {
            var result = await _gradingService.CreateGradingAsync(model);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok("Grading created successfully");
        }

        // PUT api/projectconfig/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGrading(int id, [FromBody] GradingListViewModel model)
        {
            model.GradingId = id;

            var result = await _gradingService.UpdateGradingAsync(model);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok("Grading updated successfully");
        }

        // DELETE api/projectconfig/{id}?tankId=0
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGrading(int id, [FromQuery] int tankId = 0)
        {
            var result = await _gradingService.DeleteGradingAsync(id, tankId);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok("Grading deleted successfully");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDescription()
        {
            var result = await _descriptionService.GetAllAsync();
            return Ok(result);
        }

        // GET api/descriptions/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDescriptionById(int id)
        {
            var result = await _descriptionService.GetByIdAsync(id);
            if (result == null)
                return NotFound("Description not found");

            return Ok(result);
        }

    
        [HttpPost]
        public async Task<IActionResult> AddDescription(ImageDescriptions model)
        {
            var result = await _descriptionService.CreateAsync(model);
            return result.IsSuccess ? Ok("Created") : BadRequest(result.Message);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDescription(int id, ImageDescriptions model)
        {
            model.Id = id;
            var result = await _descriptionService.UpdateAsync(model);
            return result.IsSuccess ? Ok("Updated") : BadRequest(result.Message);
        }
    }

}

