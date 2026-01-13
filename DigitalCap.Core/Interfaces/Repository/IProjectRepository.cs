using DigitalCap.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Repository
{
    public interface IProjectRepository : IRepositoryBase<Project, int>
    {
        Task<int> CreateProject(Project project);
        Task<bool> CheckProjectExistsByImoNumber(int imoNumber);
        Task<List<Models.SelectListItem>> GetProjectListByIMO(int? imoNumber = null);
        Task<Project> GetProject(int id);
        Task<int> UpdatePercentComplete(int projectId, byte percentComplete);
        Task<bool> CheckProjectExistsByProjectName(string projectName, int imoNumber, int vesseltypeid);
        Task Update_Project(Project project);
        Task<string> GetProjectName(int id);
        Task UpdateProjectStatus(int id, string currentUserId, int projectStatus);
        Task DeleteProject(int id, string currentUserId);
        // Task<IDictionary<int, int>> GetProjectIdByIMONumberLookup();
        Task<string> GetIMONumberByProjectId(int projectId);
        Task<IEnumerable<ProjectDetailItem>> GetDetails(int id);
        Task<ProjectSummary> GetSummary(int id);
        Task UpdatePriority(int id, int? priority);
        Task<IEnumerable<AllProjectsGrid>> GetAllProjectsGridData();
        Task<List<CAPCoordinator>> GetCAPCoordinator(string region, bool includeManager);
        Task<string> GetProjectVesselType(int id);
    }
}
