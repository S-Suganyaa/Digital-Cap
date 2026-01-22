using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models.ImageDescription;
using DigitalCap.Core.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        // GET api/descriptions/{id}
        [HttpGet("GetDescriptionById/{id}")]
        public async Task<IActionResult> GetDescriptionById(int id)
        {
            var result = await _descriptionService.GetByIdAsync(id);
            if (result == null)
                return NotFound("Description not found");

            return Ok(result);
        }


        [HttpPost("Create")]
        public async Task<IActionResult> AddDescription(ImageDescriptions model)
        {
            var result = await _descriptionService.CreateAsync(model);
            return result.IsSuccess ? Ok("Created") : BadRequest(result.Message);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateDescription(int id, ImageDescriptions model)
        {
            model.Id = id;
            var result = await _descriptionService.UpdateAsync(model);
            return result.IsSuccess ? Ok("Updated") : BadRequest(result.Message);
        }
    }
}
