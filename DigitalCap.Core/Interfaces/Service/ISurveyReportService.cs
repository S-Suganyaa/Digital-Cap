using DigitalCap.Core.Enumerations;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.Survey;
using DigitalCap.Core.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Service
{
    public interface ISurveyReportService
    {
        Task<Report> GetReport(int projectId, int templateSectionId, List<string> sectionIds);
        Task<ServiceResult<SurveyReportViewModel>> GetIndexAsync(int projectId, string currentUserName, int? templateSectionId = null);
        Task<bool> DeleteReport(int projectId, int templateSectionId);
        Task<ServiceResult<Report>> IndexPartial(int projectId, int? templateSectionId = null);
        Task<ServiceResult<List<UpskillImageData>>> GetBulkUploadImagesList();
        Task<ServiceResult<List<BulkUploadViewModel>>> GetBulkUploadSectionDropdown(DataSourceRequest request, int projectId);
        Task<ServiceResult<bool>> CreateReport(int projectId, int templateSectionId);
        Task<ServiceResult<Report>> GetReportBySectionId(int projectId, int templateSectionId, Guid SectionId, string imoNumber, List<string> SectionIds);
        Task<ServiceResult<bool>> SaveReportFile(string contentType, string base64, bool allTemplate, string id, string sectionId, int totalTemplates, int totalSections, int templateId, int projectId, byte[] fileContents);
        Task<ServiceResult<SurveyReportViewModel>> Export(int projectId, bool allTemplate, int templateId);
        //Task<ServiceResult<SurveyReportViewModel>> MergeReportPDF(string id, bool allTemplate, int projectId, int templateId);
        Task<int> GetUploadedImageCount(int projectId, int templateId, Guid sectionId);
        Task<ServiceResult<SurveyPhotoPlacementDropdownsViewModel>> GetPhotoPlacementDropdownPartial(int projectId, int templateId, Guid sectionId);
        Task<ServiceResult<MergeReportResultViewModel>> MergeReportPDF(string id, bool allTemplate, int projectId, int templateId);
        PhotoPlacementSequence GetPhotoCardsInList(List<Template> allCards, List<UpSkillCard> sequenceCards, List<string> populatedCardIds, string type);
        Task<int> UpdatePhotoUploadPartial(int projectId, string assignmentId);
        Task<ServiceResult<SurveyPhotoPlacementViewModel>> GetSurveyPhotoPlacementPartial(int projectId, int templateSectionId, string assignmentId, string appIds);
        Task<ServiceResult<DownloadImageResultDto>> DownloadImageForPlacement(int imageId);
        Task<ServiceResult<List<AssignmentsDropdownModel>>> GetUserApplications();
        Task<ServiceResult<Report>> RefreshCertificate(int projectId);
        Task<ServiceResult<SurveyReportViewModel>> GetSectionIdsByProjectId(int projectId, int? templateId);
        Task<ValidateReportResponse> ValidateReport(int projectId, int templateSectionId);
        Task<ServiceResult<string>> ValidateGrading(int projectId, int templateSectionId);
        Task<ServiceResult<string>> ExportAllValidation(int projectId);
        Task<ServiceResult<string>> ExportValidationPartial(int projectId, int sectionid);
        Task<ServiceResult<bool>> DeleteImage(string assignmentId, string fileId);
        Task<ServiceResult<bool>> DeleteCard(int projectId, string cardId, string sequenceId, string assignmentId, string applicationId);
        Task<ServiceResult<UploadPhotoResultDto>> UploadPhoto(IFormFile file, int projectId, string cardId, string sequenceId, string assignmentId, string applicationId, string label, string appName, string fileId, bool replaceImage = false);
        Task<ServiceResult<int>> DeleteBulkUploadImage(int imageId);
        Task<ServiceResult<bool>> UpdateGenericCurrentCondition(int templateId, int projectId, Guid sectionId, int cardId, int value, string cardName);
        Task<ServiceResult<UploadPhotoResultDto>> UploadGenericImage(IFormFile file, int templateId, int projectId, Guid sectionId, int cardId, int imageId, bool replaceImage = false, string cardName = null);
        Task<ServiceResult<int>> UploadUnPlacedPhoto(IFormFile uploadedFile, FileDataType dataType, int projectId, int taskId, string assignmentId);
        Task<ServiceResult<bool>> DeleteGenericImage(int templateId, int projectId, Guid sectionId, int cardId, int imageId);
        Task<ServiceResult<bool>> UpdateGenericAdditionalDescription(int templateId, int projectId, Guid sectionId, int cardId, string value, string cardName);
        Task<ServiceResult<bool>> UpdateGenericImageDescriptionDropdownCard(int templateId, int projectId, Guid sectionId, int cardId, int value, string cardName);
    }
}
