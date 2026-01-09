using Dapper;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models.ImageDescription;
using DigitalCap.Core.Models.ReportConfig;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Transactions;

namespace DigitalCap.Persistence.Repositories
{
    public class DescriptionRepository : IDescriptionRepository
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected IDbConnection Connection => _unitOfWork?.Connection!;
        protected IDbTransaction Transaction => _unitOfWork?.Transaction!;
        public async Task<bool> CreateProjectImageDescription(int projectId, string vesseltype)
        {
            try
            {
                await Connection.ExecuteAsync(
                            sql: "CreateProjectSectionImageDescriptions",
                             param: new
                             {
                                 ProjectId = projectId,
                                 VesselType = vesseltype
                             },
                            transaction: Transaction,
                            commandType: CommandType.StoredProcedure
                        );
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<ImageDescriptions>> GetImageDescriptionsByProjectId(int projectId)
        {
            var result = await Connection.QueryAsync<ImageDescriptions>(
                sql: "dbo.GetImageDescriptionsByProjectId",
                param: new { ProjectId = projectId },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction
            );

            return result.ToList();
        }

    }
}
