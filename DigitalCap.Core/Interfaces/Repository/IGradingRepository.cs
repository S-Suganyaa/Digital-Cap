using DigitalCap.Core.Models.Tank;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Repository
{
    public interface IGradingRepository
    {
        Task<int> CreateProjectSectionGrading(int projectId, string vesselType);
        Task<bool> CreateVessel_Grading(VesselTankGrading vesselTankGrading);
    }
}
