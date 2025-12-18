using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Infrastructure.Service
{
    public class GradingService : IGradingService
    {

        private readonly IGradingRepository _gradingRepository ;
        private readonly ITankRepository _tankRepository ;

        public GradingService(IGradingRepository gradingRepository, ITankRepository tankRepository)
        {
            _gradingRepository = gradingRepository;
            _tankRepository = tankRepository;
        }
        public async Task<ServiceResult<bool>> PopulateGradingAsync(string vesselType, int projectId)
        {
            try
            {
                // 1. Create section grading
                await _gradingRepository.CreateProjectSectionGrading(projectId, vesselType);

                // 2. Get tank grading template
                //var tankGradings =
                //    await _tankRepository.GetVesselGradingByVesselType(vesselType);

                // 3. Populate grading
                //foreach (var grading in tankGradings)
                //{
                //    grading.ProjectId = projectId;
                //    grading.CreatedDttm = DateTime.Now;
                //    grading.UpdateDttm = DateTime.Now;

                //    await _gradingRepository.CreateVesselGradingAsync(grading);
                //}

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure(ex.Message);
            }
        }

    }
}
