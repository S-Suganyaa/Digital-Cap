using DigitalCap.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace DigitalCap.Core.Interfaces.Repository
{
    public interface IProjectRepository : IRepositoryBase<Project,int>
    {
        Task<int> CreateProject(Project project);
        Task<bool> CheckProjectExistsByImoNumber(int imoNumber);
        Task<List<SelectListItem>> GetProjectListByIMO(int? imoNumber = null);
        Task<Project> GetProject(int id);
        Task UpdatePercentComplete(int projectId, byte percentComplete);
    }
}
