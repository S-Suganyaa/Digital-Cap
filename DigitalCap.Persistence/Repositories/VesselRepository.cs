using Dapper;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.VesselModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Transactions;

namespace DigitalCap.Persistence.Repositories
{
    public class VesselRepository :  IVesselRepository
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected IDbConnection Connection => _unitOfWork?.Connection!;
        protected IDbTransaction Transaction => _unitOfWork?.Transaction!;
        public async Task CreateVesselMainDataAsync(
     string classNumber,
     Project project,
     Vessel vessel)
        {
            try
            {
                var parameters = new DynamicParameters();

                parameters.Add("@projectId", project.ID);
                parameters.Add("@classNo", project.AbsClassID);
                parameters.Add("@builtBy", project.Builder);
                parameters.Add("@vesselName", project.VesselName);
                parameters.Add("@length", project.LengthOverall);
                parameters.Add("@yearBuilt", project.YearBuilt);
                parameters.Add("@hullNo", project.HullNumber);
                parameters.Add("@imoNumber", project.IMO);
                parameters.Add("@classed", project.ClassSociety[0]);

                parameters.Add("@firstVisitDate",
                    project.SurveyFirstVisit != null &&
                    project.SurveyFirstVisit != DateTime.MinValue
                        ? project.SurveyFirstVisit.Value.ToString("dd-MM-yyyy")
                        : DateTime.Now.ToString("dd-MM-yyyy"));

                parameters.Add("@lastVisitDate",
                    project.SurveyLastVisit != null &&
                    project.SurveyLastVisit != DateTime.MinValue
                        ? project.SurveyLastVisit.Value.ToString("dd-MM-yyyy")
                        : DateTime.Now.ToString("dd-MM-yyyy"));

                // Optional vessel data
                if (vessel != null)
                {
                    parameters.Add("@vesselID", vessel.ImoNumber);
                    parameters.Add("@vesselType", vessel.VesselType);
                    parameters.Add("@flagName", vessel.FlagName);
                    parameters.Add("@homeport", vessel.PortRegistry);
                    parameters.Add("@officalNumber", vessel.OfficialNumber);
                    parameters.Add("@callSign", vessel.CallSign);
                    parameters.Add("@ownerName", vessel.RegisteredOwner.Name);
                    parameters.Add("@breadth", vessel?.BreadthMolded?.Value);
                    parameters.Add("@depth", vessel?.DepthMolded?.Value);
                    parameters.Add("@deadweight", vessel?.DesignDeadWeight?.Value);
                    parameters.Add("@netTons", vessel?.NetTonnage?.Value);
                    parameters.Add("@classNotation", vessel?.ClassNotation);
                }

                await Connection.ExecuteAsync(
                    sql: "[dbo].[Insert_VesselDetails]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction);
            }
            catch
            {
                throw;
            }
        }

    }
}
