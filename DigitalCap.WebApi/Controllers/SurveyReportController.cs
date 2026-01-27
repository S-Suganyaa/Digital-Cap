using DigitalCap.Core.Enumerations;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.ReportConfig;
using DigitalCap.Core.Models.Skylight;
using DigitalCap.Core.Models.Survey;
using DigitalCap.Core.Models.Tank;
using DigitalCap.Core.ViewModels;
using DigitalCap.Infrastructure.Service;
using DigitalCap.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;

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
        [HttpGet("[action]")]
        public async Task<IActionResult> SurveyReportIndex(int projectId, int? templateSectionId = null)
        {
            var currentUserName = User.Identity?.Name;

            var result = _surveyReportService.GetIndexAsync(projectId, currentUserName, templateSectionId);

            return Ok(result);
        }

        //public IActionResult ConfigSurveyReport(SurveyReportViewModel model)
        //{
        //    return View(model);

        //}
        [HttpGet("[action]")]
        public async Task<IActionResult> IndexPartial(int projectId, int? templateSectionId = null)
        {
            var result = _surveyReportService.IndexPartial(projectId, templateSectionId);

            return Ok(result);
        }

        //public async Task<IActionResult> HelpPartial()
        //{
        //    return PartialView("_VesselEvaluationMethodAndGradingSystem");
        //}
        [HttpGet("[action]")]
        public async Task<IActionResult> GetBulkUploadImagesList()
        {
            var result = await _surveyReportService.GetBulkUploadImagesList();
            return Ok(result);
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> GetBulkUploadSectionDropdown([FromBody] DataSourceRequest request, int projectId)
        {
            var result = await _surveyReportService.GetBulkUploadSectionDropdown(request, projectId);

            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateReport(int projectId, int templateSectionId)
        {
            var result = await _surveyReportService.CreateReport(projectId, templateSectionId);

            return Ok(result);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteReport(int projectId, int templateSectionId)
        {
            var result = await _surveyReportService.DeleteReport(projectId, templateSectionId);

            return Ok(result);
        }

        [HttpPost("[action]")]
        public IActionResult SurveyReportPDF(int projectId, int? templateId, string sectionId, bool allTemplate)
        {
            if (templateId == 0)
                sectionId = Guid.NewGuid().ToString();

            return Ok();
        }

        [HttpPost("[action]")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = 268435456)]
        public async Task<ActionResult> SaveReportFile(string contentType, string base64, bool allTemplate, string id, string sectionId, int totalTemplates, int totalSections, int templateId, int projectId)
        {
            try
            {
                var fileContents = Convert.FromBase64String(base64);
                // Validate the id parameter
                if (id.Contains("..") || id.Contains("/") || id.Contains("\\"))
                {
                    return BadRequest("Invalid path");
                }

                var result = _surveyReportService.SaveReportFile(contentType, base64, allTemplate, id, sectionId, totalTemplates, totalSections, templateId, projectId, fileContents);

                return Content(JsonConvert.SerializeObject(result));
            }
            catch (Exception ex)
            {
                return Content(JsonConvert.SerializeObject(null));
            }
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> Export(int projectId, bool allTemplate, int templateId)
        {
            var result = _surveyReportService.Export(projectId, allTemplate, templateId);

            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> MergeReportPDF(string id, bool allTemplate, int projectId, int templateId)
        {
            var result = _surveyReportService.MergeReportPDF(id, allTemplate, projectId, templateId);


            return Content(JsonConvert.SerializeObject(result));
        }
        [HttpGet("[action]")]
        public int GetGradingCount(List<GradingUI> gradings, Guid sectionId)
        {
            try
            {
                var count = gradings.Where(x => x.SectionId == sectionId).ToList().Count;
                return count;
            }
            catch { }
            return 0;
        }
        [HttpGet("[action]")]
        public async Task<int> GetUploadedImageCount(int projectId, int templateId, Guid sectionId)
        {
            var result = await _surveyReportService.GetUploadedImageCount(projectId, templateId, sectionId);

            return result;
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> GetPhotoPlacementDropdownPartial(int projectId, int templateId, Guid sectionId)
        {
            //var sectionIds = GetSectionIdsByProjectId(projectId, templateId).Result.ReportPartGrid.Where(x => x.TemplateId == templateId).FirstOrDefault().SectionIds;
            //SurveyPhotoPlacementDropdownsViewModel surveyPhotoPlacementDropdownViewModel = _surveyReportRepository.GetBulkUploadDropdownValues(projectId, templateId, sectionId, sectionIds);
            //return PartialView("_SurveyPhotoPlacementDropdownsPartial", surveyPhotoPlacementDropdownViewModel);

            var result = _surveyReportService.GetPhotoPlacementDropdownPartial(projectId, templateId, sectionId);

            return Ok(result);

        }
        //[HttpGet("[action]")]
        //public PhotoPlacementSequence GetPhotoCardsInList(List<Template> allCards, List<UpSkillCard> sequenceCards, List<string> populatedCardIds, string type)
        //{

        //    var placementSequence = _surveyReportService.GetPhotoCardsInList(allCards, sequenceCards, populatedCardIds, type);

        //    return placementSequence;
        //}
        [HttpPost("[action]")]
        public async Task<IActionResult> UpdatePhotoUploadPartial(int projectId, string assignmentId)
        {
            var count = await _surveyReportService.UpdatePhotoUploadPartial(projectId, assignmentId);

            return Ok(count);
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> GetSurveyPhotoPlacementPartial(int projectId, int templateSectionId, string assignmentId, string appIds)
        {
            var result = await _surveyReportService.GetSurveyPhotoPlacementPartial(projectId, templateSectionId, assignmentId, appIds);

            return Ok(result.Data);
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> DownloadImageForPlacement(int imageId)
        {
            var result = _surveyReportService.DownloadImageForPlacement(imageId);
            return Ok(result);
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> GetUserApplications()
        {
            var final = await _surveyReportService.GetUserApplications();
            return Ok(final);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RefreshSurveyStatus(int projectId)
        {
            var data = _surveyReportService.RefreshCertificate(projectId).Result;

            return Ok(data);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> RefreshCertificates(int projectId)
        {
            var data = _surveyReportService.RefreshCertificate(projectId).Result;
            return Ok(data);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> GetReport(int projectId, int templateSectionId)
        {
            try
            {
                var sectionResult = await _surveyReportService.GetSectionIdsByProjectId(projectId, templateSectionId);

                if (!sectionResult.IsSuccess || sectionResult.Data == null)
                    return BadRequest("Failed to get section ids");

                var reportPart = sectionResult.Data.ReportPartGrid.FirstOrDefault(x => x.TemplateId == templateSectionId);

                if (reportPart == null)
                    return NotFound("Template section not found");

                var sectionIds = reportPart.SectionIds.Select(x => x.ToString()).ToList();

                var reportResult = await _surveyReportService.GetReport(projectId, templateSectionId, sectionIds);

                return Ok(reportResult);
            }
            catch (Exception ex)
            {

                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> ValidateReport(int projectId, int templateSectionId)
        {
            var result = _surveyReportService.ValidateReport(projectId, templateSectionId);

            return Ok(result);
        }

        // [HttpPost("[action]")]
        //public ActionResult ValidatePreviousTabReport(int projectId)
        //{
        //    try
        //    {
        //        var resultsession = "";
        //        #region project id checking
        //        if (HttpContext.Session.GetInt32("projectid") != null)
        //        {
        //            int projId = (int)HttpContext.Session.GetInt32("projectid");
        //            #region Session
        //            if (projId == projectId)
        //            {
        //                if (HttpContext.Session.GetString("reportsession") != null)
        //                {
        //                    resultsession = HttpContext.Session.GetString("reportsession");
        //                }
        //            }

        //            #endregion session

        //        }
        //        return Json(resultsession);
        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(ex.Message);
        //    }

        //}

        [HttpPost("[action]")]
        public async Task<IActionResult> ValidateGrading(int projectId, int templateSectionId)
        {
            var result = _surveyReportService.ValidateGrading(projectId, templateSectionId);

            return Ok(result);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> ExportAllValidation(int projectId)
        {
            var result = _surveyReportService.ExportAllValidation(projectId);
            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ExportValidationPartial(int projectId, int sectionid)
        {
            var result = _surveyReportService.ExportValidationPartial(projectId, sectionid);
            return Ok(result);
        }

        // [AllowAnonymous]
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return Ok(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}

        [NonAction]
        public IActionResult Error()
        {
            return Problem("An error occurred");
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> DeleteImage(string assignmentId, string fileId)
        {
            var ret = await _surveyReportService.DeleteImage(assignmentId, fileId);
            return Ok(ret);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> DeleteCard(int projectId, string cardId, string sequenceId, string assignmentId, string applicationId)
        {
            var ret = await _surveyReportService.DeleteCard(projectId, cardId, sequenceId, assignmentId, applicationId);
            return Ok(ret);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UploadPhoto(IFormFile file, [FromForm] int projectId, string cardId, string sequenceId, string assignmentId, string applicationId, string label, string appName, string fileId, bool replaceImage = false)
        {
            var result = await _surveyReportService.UploadPhoto(file, projectId, cardId, sequenceId, assignmentId, applicationId, label, appName, fileId, replaceImage);

            return Ok(result.Data);
        }

        [HttpPost("[action]")]
        public IActionResult Delete(int? fileId)
        {
            if (!fileId.HasValue && fileId != 0)
                return NotFound();

            var result = _surveyReportService.DeleteBulkUploadImage(fileId.Value);
            return Ok(result);
        }

         [HttpPost("[action]")]
        public async Task<ActionResult> UploadUnPlacedPhoto(IFormFile uploadedFile, FileDataType dataType, int projectId, int taskId, string assignmentId)
        {
            string newfileId = Guid.NewGuid().ToString();
            var result = _surveyReportService.UploadUnPlacedPhoto(uploadedFile, dataType, projectId, taskId, assignmentId);

            return Ok(result);
        }


         [HttpPost("[action]")]
        public async Task<ActionResult> UpdateGenericImageDescriptionDropdownCard(int templateId, int projectId, Guid sectionId, int cardId, int value, string cardName)
        {
            var result = _surveyReportService.UpdateGenericImageDescriptionDropdownCard(templateId, projectId, sectionId, cardId, value, cardName);

            return Ok(result);
        }

         [HttpPost("[action]")]
        public async Task<ActionResult> UpdateGenericAdditionalDescription(int templateId, int projectId, Guid sectionId, int cardId, string value, string cardName)
        {
            var result = _surveyReportService.UpdateGenericAdditionalDescription(templateId, projectId, sectionId, cardId, value, cardName);
            return Ok(result);
        }

         [HttpPost("[action]")]
        public async Task<ActionResult> UpdateGenericCurrentCondition(int templateId, int projectId, Guid sectionId, int cardId, int value, string cardName)
        {
            var result = _surveyReportService.UpdateGenericCurrentCondition(templateId, projectId, sectionId, cardId, value, cardName);
            return Ok(result);
        }

         [HttpPost("[action]")]
        public async Task<ActionResult> UploadGenericImage(IFormFile file, int templateId, int projectId, Guid sectionId, int cardId, int imageId, bool replaceImage = false, string cardName = null)
        {
            var result = _surveyReportService.UploadGenericImage(file, templateId, projectId, sectionId, cardId, imageId, replaceImage, cardName);

            //  return Json(new { FileId = newfileId, Image = resizedImage }, new JsonSerializerOptions { PropertyNamingPolicy = null });
            return Ok(result);
        }

         [HttpPost("[action]")]
        public async Task<ActionResult> DeleteGenericImage(int templateId, int projectId, Guid sectionId, int cardId, int imageId)
        {
            var result = await _surveyReportService.DeleteGenericImage(templateId, projectId, sectionId, cardId, imageId);

            return Ok(result);
        }


    }
}
