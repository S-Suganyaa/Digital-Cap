using DigitalCap.Core.Enumerations;
using DigitalCap.Core.Models.Tank;
using System;
using System.Collections.Generic;
using System.Text;
using ShipType = DigitalCap.Core.Models.Tank.ShipType;

namespace DigitalCap.Core.Interfaces.Repository
{
    public interface ITankRepository : IRepositoryBase<VesselTank, Guid>
    {
        Task<List<VesselTank>> GetTanks_VesselByIMO(string imonumber);
        Task<bool> PopulateTemplate(string imonumber = null, string vesseltype = null, string copyingimonumber = null);
        Task<bool> PopulateTemplate(string imonumber = null, string vesseltype = null, string copyingimonumber = null, int projectid = 0, int copyingProjectId = 0);
        Task<List<VesselTank>> GetTanks_VesselByVesselType(string vesseltype);
        Task<List<VesselTank>> GetTanks_VesselByDetails(string vesseltype, string imonumber);
        Task<bool> CreateTank(VesselTank vesselTank);
        Task<List<ShipType>> GetShipType();
        Task<List<VesselTank>> GetTanks_VesselByProject(string imonumber, int projectId);
        Task<List<VesselTankGrading>> GetVessel_GradingByVesselType(string vesseltype);
        Task<bool> CreateVessel_Grading(VesselTankGrading vesselTankGrading);
        Task<List<TankUI>> GetTemplateTanks(int templateId, string imonumber, string vesseltype, int projectId);
        Task<List<TankCheckBox>> GetTemplateTankGradingCondition(int Id);
        Task<List<TankGradingUI>> GetTemplateTankGrading(int tanktypeId, int projectId, string vesseltype);
        Task<List<TankImageCard>> GetProjectTankImageCard(int projectId, int templateId, Guid sectionId);
    }
}
