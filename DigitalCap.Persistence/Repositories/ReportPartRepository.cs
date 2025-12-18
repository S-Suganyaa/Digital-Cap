using Dapper;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models.ReportConfig;
using DigitalCap.Core.Models.Tank;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Transactions;

namespace DigitalCap.Persistence.Repositories
{
    public class ReportPartRepository : IReportPartRepository
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected IDbConnection Connection => _unitOfWork?.Connection!;
        protected IDbTransaction Transaction => _unitOfWork?.Transaction!;
        public async Task<bool> CreateProjectReportTemplate(
     int vesselTypeId,
     string userName,
     int projectId,
     bool imoExists = false,
     int copyprojectId = 0)
        {
            try
            {
                var result = await Connection.ExecuteAsync(
                      sql: "[Config].[CreateProject_Templates]",
                 commandType: CommandType.StoredProcedure,
                     param: new
                     {
                         VesselTypeId = vesselTypeId,
                         ProjectId = projectId,
                         UserName = userName,
                         IMOExists = imoExists,
                         CopyProjectId = copyprojectId
                     },
                 transaction: Transaction);

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

    }
}
