using DigitalCap.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;

namespace DigitalCap.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SurveyReportController : ControllerBase
    {
        private readonly ISurveyReportService _surveyReportService;

        public SurveyReportController(ISurveyReportService surveyReportService, IConfiguration configData)
        {
            _surveyReportService = surveyReportService;
        }
    }
}
