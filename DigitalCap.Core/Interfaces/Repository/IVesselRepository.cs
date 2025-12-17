using DigitalCap.Core.Models;
using DigitalCap.Core.Models.VesselModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Repository
{
    public interface IVesselRepository 
    {
        Task CreateVesselMainDataAsync(string classNumber,Project project,Vessel vessel);
    }
}
