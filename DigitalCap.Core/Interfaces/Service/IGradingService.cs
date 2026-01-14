using DigitalCap.Core.Models;
using DigitalCap.Core.ViewModels;
using DigitalCap.Core.Models.Grading;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Service
{
    public interface IGradingService
    {
      
        Task<ServiceResult<bool>> PopulateGrading(string vesselType, int projectId);

        Task<List<Grading>> GetAllGradingsAsync();
        Task<ServiceResult<bool>> CreateGradingAsync(GradingListViewModel model);
        Task<ServiceResult<bool>> UpdateGradingAsync(GradingListViewModel model);
        Task<ServiceResult<bool>> DeleteGradingAsync(int gradingId, int tankId);


    }
}   
