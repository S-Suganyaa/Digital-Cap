using DigitalCap.Core.Models.ExportConfig;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models.ImageDescription;
using DigitalCap.Core.ViewModels;


namespace DigitalCap.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly IExportService _exportService;

        public ExportController(IExportService exportService)
        {
            _exportService = exportService;
        }

        [HttpGet("ManageExport")]
        public async Task<IActionResult> ManageExport()
        {
            var result = await _exportService.GetExportSettingsAsync();

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpGet("parts/{vesselTypeId}")]
        public async Task<IActionResult> GetReportExportPartByVesselType(int vesselTypeId)
        {
            var result = await _exportService.GetExportPartsByVesselTypeAsync(vesselTypeId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result.Data);
        }

        [HttpGet("parts/by-imo/{projectId}")]
        public async Task<IActionResult> GetReportExportPartByImo(int projectId)
        {
            var result = await _exportService.GetExportSettingsByProjectAsync(projectId);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpPost("save-required-in-export")]
        public async Task<IActionResult> SaveRequiredInExport([FromBody] ExportSettingsRequest settings)
        {
            var result = await _exportService.SaveRequiredInExportAsync(settings);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(true);
        }

        [HttpGet("GetProjectList/by-imo")]
        public async Task<IActionResult> GetProjectListByIMO([FromQuery] int? imoNumber)
        {
            var result = await _exportService.GetProjectListByIMOAsync(imoNumber);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

    }
}
