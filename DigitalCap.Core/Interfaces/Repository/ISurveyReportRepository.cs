using DigitalCap.Core.Models.Survey;
using DigitalCap.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Repository
{
    public interface ISurveyReportRepository
    {
        Task<SurveyStatus> MapSurveyStatus(string classNumber, int imo);
        Task<Report> MapStatuatoryCertificates(string classNumber, Report report);
        Task<List<ReportTemplateSection>> GetTemplateSections(int templateId);
        Task<List<ReportTemplate>> GetReportTemplateList();
        Task<List<int>> GetExistingReportPartsSectionIds(int projectId);
        Task<List<ProjectSections>> GetProjectSectionsId(int projectId);
        Task<ReportTemplateUI> GetTemplateTitle(int templateId);
        Task<List<CurrentCondition>> GetCurrentCondition();
        Task<List<ProjectReportMapping>> GetProjectReportTemplate(int projectId, int templateId);
        Task<List<ProjectGradingMapping>> GetProjectGrading(int projectId, int templateId, Guid sectionId);
        Task<List<ProjectGradingConditionMapping>> GetProjectGradingConditionMapping(int projectId, int templateId, Guid sectionId, int gradingId);
        Task<List<GradingUI>> GetTemplatGradings(int templateId, int projectId);
        Task<List<ImageCardUI>> GetImageCard(int templateId);
        Task<List<GradingConditionUI>> GetGradingCondition(int templateId, int projectid);
        Task<List<ProjectCardMapping>> GetProjectImageCard(int projectId, int templateId, Guid sectionId);
        Task<HandIReport> GetHIReport(int projectId);
        Task<ProjectReportMapping> GetProjectReportTemplate(int projectId, int templateId, Guid sectionId);
        Task<List<ReportTemplateSection>> GetTemplatSections(int templateId);
        Task<List<UpskillImageData>> GetBulkUploadUnplacedImages();
    }
}
