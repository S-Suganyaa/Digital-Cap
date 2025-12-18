using Dapper;
using DigitalCap.Core.Interfaces.Repository;
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

        public async Task<int> CreateProjectSectionGrading(
    int projectId,
    string vesselType)
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

    }
}
