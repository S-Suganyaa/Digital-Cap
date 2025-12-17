using DigitalCap.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace DigitalCap.Core.Interfaces.Service
{
    public interface IProjectService
    {
        Task<ServiceResult<int>> CreateProject(Project model);
        Task<ServiceResult<bool>> CheckProjectExistsByImoNumber(int imoNumber);
        Task<ServiceResult<SelectListItem>> GetProjectListByIMO(int? imoNumber = null);
        Task<ServiceResult<Project>> GetProject(int id);
        Task UpdatePercentComplete(int projectId, byte percentComplete);
    }
}
