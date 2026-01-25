using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models.Grading;
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
    public class GradingController : ControllerBase
    {
        private readonly IGradingService _gradingService;
        private readonly ITankService _tankService;
        public GradingController(IGradingService gradingService, ITankService tankService)
        {
            _gradingService = gradingService;
            _tankService = tankService;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> GetVesselType()
        {
            try
            {
                var vesselTypes = await _tankService.GetShipType();
                return Ok(vesselTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving vessel types", error = ex.Message });
            }
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> GetTemplateName([FromQuery] string vesselType = null)
        {
            try
            {
                var templates = await _gradingService.GetTemplateName(vesselType);
                return Ok(templates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving templates", error = ex.Message });
            }
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> GetSectionNameByTemplateNameAndVesselType([FromQuery] string templateName, [FromQuery] string vesselType)
        {
            try
            {
                var sections = await _gradingService.GetSectionNameByTemplateNameAndVesselType(templateName, vesselType);
                return Ok(sections);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving sections", error = ex.Message });
            }
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> GetSectionName(int templateId,string vesselType)
        {
            try
            {
                var sections = await _gradingService.GetGradingSections(templateId, vesselType);
                return Ok(sections);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving sections", error = ex.Message });
            }
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> GetSectionGrading()
        {
            List<SectionGrading> sectionsGrading = new List<SectionGrading>();
            return Ok(sectionsGrading);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _gradingService.GetAllGradingsAsync();

            //if (!result.Success)
            //    return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateGrading([FromBody] GradingListViewModel model)
        {
            var result = await _gradingService.CreateGradingAsync(model);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok("Grading created successfully");
        }


        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateGrading(int id, [FromBody] GradingListViewModel model)
        {
            model.GradingId = id;

            var result = await _gradingService.UpdateGradingAsync(model);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok("Grading updated successfully");
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteGrading(int id, [FromQuery] int tankId = 0)
        {
            var result = await _gradingService.DeleteGradingAsync(id, tankId);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok("Grading deleted successfully");
        }
    }
}
