using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.VesselModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Infrastructure.Service
{
    public class VesselService
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

        public async Task<string> CreateVesselMainData(
            string classNumber,
            Project project)
        {
            Vessel vessel = null;

            if (!string.IsNullOrWhiteSpace(classNumber))
            {
                vessel = await _freedomAPIRepository.GetVessel(classNumber);
            }

            await _vesselRepository.CreateVesselMainDataAsync(
                classNumber,
                project,
                vessel);

            return classNumber;
        }
    }


}
