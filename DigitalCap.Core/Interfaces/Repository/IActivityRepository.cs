using DigitalCap.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using DigitalCap.Core.ViewModels;
using DigitalCap.Core.Interfaces.Service;


namespace DigitalCap.Core.Interfaces.Repository
{
    public interface IActivityRepository : IRepositoryBase<Project, int>
    {
        Task<List<RecentActivity>> GetRecentActivities(int page,int size);
        Task<List<RecentActivity>> GetRecentActivitiesByProjectId(int projectId, int size);
        Task<List<RecentActivity>> GetAllActivitiesByProjectId(int projectId);
    }
}
