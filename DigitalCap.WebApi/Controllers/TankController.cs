using DigitalCap.Core.Helpers.Constants;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.ReportConfig;
using DigitalCap.Core.Models.Tank;
using DigitalCap.Core.Models.View.Admin;
using DigitalCap.Core.ViewModels;
using DigitalCap.Infrastructure.Service;
using DigitalCap.Persistence.Repositories;
using DigitalCap.WebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Newtonsoft.Json;
using System.Numerics;
using System.Text.Json;

namespace DigitalCap.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TankController : ControllerBase
    {
        private readonly ITankService _tankService;

        public TankController(ITankService tankService)
        {
            _tankService = tankService;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateTankImageDescriptionDropdownCard(int templateId, int projectId, Guid sectionId, int cardNumber, int value, string cardName)
        {
            var result = _tankService.UpdateTankImageDescriptionDropdownCard(templateId, projectId, sectionId, cardNumber, value, cardName);

            return Ok(result);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateTankCardAdditionalDescription(int templateId, int projectId, Guid sectionId, int cardNumber, string value, string cardName)
        {
            var result = _tankService.UpdateTankCardAdditionalDescription(templateId, projectId, sectionId, cardNumber, value, cardName);

            return Ok(result);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateTankCardCurrentCondition(int templateId, int projectId, Guid sectionId, int cardNumber, int value, string cardName)
        {
            var result = _tankService.UpdateTankCardCurrentCondition(templateId, projectId, sectionId, cardNumber, value, cardName);

            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> UploadTankCardImage(IFormFile tankfile, int templateId, int projectId, Guid sectionId, int cardNumber, int imageId, bool replaceImage = false, string cardName = null)
        {
            var result = _tankService.UploadTankCardImage(tankfile, templateId, projectId, sectionId, cardNumber, imageId, replaceImage, cardName);

            return Ok(result);
            //  return Json(new { FileId = newfileId, Image = resizedImage }, new JsonSerializerOptions { PropertyNamingPolicy = null });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteTankCardImage(int templateId, int projectId, Guid sectionId, int cardNumber, int imageId)
        {
            var result = _tankService.DeleteTankCardImage(templateId, projectId, sectionId, cardNumber, imageId);
            return Ok(result);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> GetTankTypes()
        {
            var result = _tankService.GetTankTypes();
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetMappedTankTypes(int projectId, string vesseltype)
        {
            var result = _tankService.GetMappedTankTypes(projectId, vesseltype);
            return Ok(result);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> GetVesselDetails()
        {
            var result = _tankService.GetVesselDetails();
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetIMONumberByVesselName(string vesselName)
        {
            var result = _tankService.GetIMONumberByVesselName(vesselName);
            return Ok(result);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> UpdateTank(string tankId, string vesseltype, string vesselname, string imonumber, string tanktype, string tankname, bool isActive)
        {
            var result = _tankService.UpdateTank(tankId, vesseltype, vesselname, imonumber, tanktype, tankname, isActive);
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetVesselType()
        {
            var result = _tankService.GetVesselType();
            return Ok(result);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> GetShipType()
        {
            var result = _tankService.GetShipType();
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetVesselTypeList()
        {
            var result = _tankService.GetVesselTypeList();
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> FilterMenuCustomization_Read([FromQuery] DataSourceRequest request)
        {
            try
            {
                var serviceResult = await _tankService.FilterMenuCustomization_Read();

                if (!serviceResult.IsSuccess)
                {
                    return BadRequest(serviceResult.Message);
                }

                var allData = serviceResult.Data;
                var pagedData = allData;

                // Apply pagination
                if (request != null)
                {
                    if (request.Skip.HasValue && request.Take.HasValue)
                    {
                        pagedData = allData.Skip(request.Skip.Value).Take(request.Take.Value).ToList();
                    }
                    else if (request.Page.HasValue && request.PageSize.HasValue)
                    {
                        var skip = (request.Page.Value - 1) * request.PageSize.Value;
                        pagedData = allData.Skip(skip).Take(request.PageSize.Value).ToList();
                    }
                }

                // Return array directly (matching frontend expectation)
                return Ok(pagedData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving tanks", error = ex.Message });
            }
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> ManageTankFilter_TankName(string IMO)
        {
            var result = _tankService.ManageTankFilter_TankName(IMO);
            return Ok(result);
        }



        [HttpGet("[action]")]
        public async Task<IActionResult> ManageTankFilter_VesselType(string IMO)
        {
            var result = _tankService.ManageTankFilter_VesselType(IMO);
            return Ok(result);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> ManageTankFilter_VesselName(string IMO)
        {
            var result = _tankService.ManageTankFilter_VesselName(IMO);
            return Ok(result);
        }



        [HttpGet("[action]")]
        public async Task<IActionResult> ManageTankFilter_IMONumber(string IMO)
        {
            var result = _tankService.ManageTankFilter_IMONumber(IMO);
            return Ok(result);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> ManageTankFilter_TankType(string IMO)
        {
            var result = _tankService.ManageTankFilter_TankType(IMO);
            return Ok(result);
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> ManageTankFilter_Project(string IMO)
        {
            var result = _tankService.ManageTankFilter_VesselName(IMO);
            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ManageTankActiveCheckBox(string data, bool status, string IMO)
        {
            var result = _tankService.ManageTankActiveCheckBox(data, status, IMO);

            return Ok(result);
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> ManageTankInActiveCheckBox()
        {
            var data = new
            {
                status = "Inactive"
            };

            return Ok("Inactive");
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> DeleteTanks(Guid tankId, string IMO, int ProjectId)
        {
            var result = _tankService.DeleteTanks(tankId, IMO, ProjectId);

            return Ok(result);
        }


    }
}
