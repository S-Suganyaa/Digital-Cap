using DigitalCap.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Service
{
    public interface IGradingService
    {
        Task<ServiceResult<bool>> PopulateGrading(string vesselType, int projectId);


    }
}
