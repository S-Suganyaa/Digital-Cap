using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DigitalCap.Core.Models.ImageDescription;
using DigitalCap.Core.ViewModels;
using DigitalCap.Core.Interfaces.Service;

namespace DigitalCap.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GradingController : ControllerBase
    {
        private readonly IGradingService _gradingService;
        public GradingController(IGradingService gradingService)
        {
            _gradingService = gradingService;
            
        }

        [HttpGet("GetAllGrading")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _gradingService.GetAllGradingsAsync();

            //if (!result.Success)
            //    return BadRequest(result.Message);

            return Ok(result);
        }

        
        [HttpPost("Create")]
        public async Task<IActionResult> CreateGrading([FromBody] GradingListViewModel model)
        {
            var result = await _gradingService.CreateGradingAsync(model);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok("Grading created successfully");
        }

       
        [HttpPut("UpdateGrading/{id}")]
        public async Task<IActionResult> UpdateGrading(int id, [FromBody] GradingListViewModel model)
        {
            model.GradingId = id;

            var result = await _gradingService.UpdateGradingAsync(model);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok("Grading updated successfully");
        }

        [HttpDelete("DeleteGrading/{id}")]
        public async Task<IActionResult> DeleteGrading(int id, [FromQuery] int tankId = 0)
        {
            var result = await _gradingService.DeleteGradingAsync(id, tankId);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok("Grading deleted successfully");
        }
    }
}
