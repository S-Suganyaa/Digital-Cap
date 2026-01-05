using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.Survey;
using DigitalCap.Core.Models.VesselModel;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DigitalCap.Infrastructure.Service
{
    public class VesselService : IVesselService
    {

        private readonly IFreedomAPIRepository _freedomAPIRepository;
        private readonly IVesselRepository _vesselRepository;

        public VesselService(
            IFreedomAPIRepository freedomAPIRepository,
            IVesselRepository vesselRepository)
        {
            _freedomAPIRepository = freedomAPIRepository;
            _vesselRepository = vesselRepository;
        }

        public Task<ServiceResult<string>> CreateStatutoryCertificate(int projectId, Report report)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<string>> CreateSurveyAudit(SurveyStatus surveyStatus, int projectId)
        {
            throw new NotImplementedException();
        }

        public async Task<string> CreateVesselMainData(string classNumber, Project project)
        {
            var vessel = new Core.Models.VesselModel.Vessel();

            if (!string.IsNullOrWhiteSpace(classNumber))
            {
                vessel = await _freedomAPIRepository.GetVessel(classNumber);
            }

            await _vesselRepository.CreateVesselMainDataAsync(classNumber, project, vessel);

            return classNumber;
        }

        Task<ServiceResult<string>> IVesselService.CreateVesselMainData(string imoNumber, Project project)
        {
            throw new NotImplementedException();
        }
    }


}
