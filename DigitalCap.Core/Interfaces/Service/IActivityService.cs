using DigitalCap.Core.Models;
using DigitalCap.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace DigitalCap.Core.Interfaces.Service
{
    public interface IActivityService
    {
        Task<List<RecentActivityViewModel>> GetRecentActivities();
        Task<List<RecentActivityViewModel>> GetRecentActivities2();
        Task<List<RecentActivityViewModel>> GetRecentActivitiesByProjectId(int projectId);
        Task<List<RecentActivityViewModel>> GetAllActivitiesByProjectId(int projectId);

    }
}
