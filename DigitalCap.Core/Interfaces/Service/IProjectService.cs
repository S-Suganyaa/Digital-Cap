using DigitalCap.Core.Enumerations;
using DigitalCap.Core.Models;
using DigitalCap.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace DigitalCap.Core.Interfaces.Service
{
    public interface IProjectService
    {
        Task<ServiceResult<int>> CreateProject(Project model, string Name);

        Task<ServiceResult<Project>> UpdateProject(int id, string currentUser);

        Task<ServiceResult<int>> Update_Project(Project model, string currentUser);

        Task<ServiceResult<int>> UpdateProjectStatus(int id, string currentUserId, int ProjectStatus);

        Task<ServiceResult<int>> DeleteProject(int id, string currentUserId);

        Task UpdatePriority(int projectId, int? priority);
        Task CancelProject(int projectId, string userId);
        Task<ServiceResult<UpdateProjectPriorityViewModel>> GetSummary(int id);
        Task<ServiceResult<object>> GetCompletionStatus(int id, ProjectStatusType statusType);

        Task<ServiceResult<object>> GetList(string currentUser, bool isElectron);

        Task<ServiceResult<List<AllProjectsGrid>>> GetAllProjectsGridData(bool isElectron, string userName);

        Task<ServiceResult<byte>> UpdateProjectPercentComplete(int projectId);

        Task<ServiceResult<List<Client>>> GetAllClients(bool wcnList);

        Task<ServiceResult<object>> GetAllUsers();
        Task<ServiceResult<List<string>>> GetAllSurveyors(bool wcnList);
        Task<ServiceResult<WorkflowViewModel>> Workflow(int? id);

        Task<ServiceResult<ProjectLandingViewModel>> Landing(int? id, string currentUser);

        Task<ServiceResult<ProjectLandingViewModel>> ClientLanding(int? id);

        Task<ServiceResult<bool>> SaveReportDetails(ReportDetailsViewModel reportDetails);

        Task<ServiceResult<string>> RecentActivityForProject(int projectId);
        Task<ServiceResult<string>> GetProjectName(int id);
        Task<ServiceResult<IEnumerable<ProjectDetailItem>>> GetDetails(int id);
    }

}
