using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Service
{
    public interface IGradingService
    {
        Task<bool> PopulateGrading(string vesseltype, int projectId);
    }
}
