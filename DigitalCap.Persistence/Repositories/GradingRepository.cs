using Dapper;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models.Tank;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Transactions;

namespace DigitalCap.Persistence.Repositories
{
    public class GradingRepository : IGradingRepository
    {

        protected readonly IUnitOfWork _unitOfWork;
        protected IDbConnection Connection => _unitOfWork?.Connection!;
        protected IDbTransaction Transaction => _unitOfWork?.Transaction!;

        public async Task<int> CreateProjectSectionGrading(int projectId,string vesselType)
        {
            return await Connection.ExecuteAsync(
                sql: "[dbo].[CreateProjectSectionGrading]",
                param: new
                {
                    ProjectId = projectId,
                    VesselType = vesselType
                },
                transaction: Transaction,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> CreateVessel_Grading(VesselTankGrading vesselTankGrading)
        {
            try
            {
                var parameters = new DynamicParameters();

                foreach (var prop in typeof(VesselTankGrading).GetProperties())
                {
                    if (prop.Name != "Id" &&
                        prop.Name != "TemplateId" &&
                        prop.Name != "VesselName")
                    {
                        parameters.Add("@" + prop.Name, prop.GetValue(vesselTankGrading));
                    }
                }

                await Connection.ExecuteAsync(
                    sql: "dbo.Create_Vessel_Grading",
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                );

                return true;
            }
            catch (Exception ex)
            {
                // optional: log ex
                return false;
            }
        }

    }
}
