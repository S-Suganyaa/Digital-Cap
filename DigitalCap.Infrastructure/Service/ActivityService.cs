using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.ViewModels;
using DigitalCap.Core.Models.Survey;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Web.Mvc;

namespace DigitalCap.Infrastructure.Service
{
    public class ActivityService : IActivityService
    {
        private readonly IActivityRepository _activityRepository;


        public ActivityService(IActivityRepository activityRepository)
        {
            _activityRepository = activityRepository;

        }

        public async Task<List<RecentActivityViewModel>> GetRecentActivities()
        {
            var activities = await _activityRepository.GetRecentActivities(0,10);
            return activities.Select(a => new RecentActivityViewModel(a)).ToList();
        }

        public async Task<List<RecentActivityViewModel>> GetRecentActivities2()
        {
            var activities = await _activityRepository.GetRecentActivities(0,200);
            return activities.Select(a => new RecentActivityViewModel(a)).ToList();
        }

        public async Task<List<RecentActivityViewModel>> GetRecentActivitiesByProjectId(int projectId)
        {
            var activities = await _activityRepository.GetRecentActivitiesByProjectId(10, projectId);
            return activities.Select(a => new RecentActivityViewModel(a)).ToList();
        }

        public async Task<List<RecentActivityViewModel>> GetAllActivitiesByProjectId(int projectId)
        {
            var activities = await _activityRepository.GetRecentActivitiesByProjectId(200, projectId);
            return activities.Select(a => new RecentActivityViewModel(a)).ToList();
        }



    }
}
