using Dapper;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalCap.Persistence.Repositories
{
    public class ActivityRepository
        : RepositoryBase<Project, int>, IActivityRepository
    {
        private readonly ILogger<ActivityRepository> _logger;

        public ActivityRepository(
            IUnitOfWork unitOfWork,
            ILogger<ActivityRepository> logger
        ) : base(unitOfWork, logger)
        {
            _logger = logger;
        }

        public async Task<List<RecentActivity>> GetRecentActivities(int page, int size)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Page", page);
            parameters.Add("@Size", size);

            return (await Connection.QueryAsync<RecentActivity>(
                sql: "[CAP].[Read_Activities]",
                param: parameters,
                commandType: CommandType.StoredProcedure,
                transaction: Transaction
            )).ToList();
        }

        public async Task<List<RecentActivity>> GetRecentActivitiesByProjectId(int projectId, int size)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ProjectId", projectId);
            parameters.Add("@Take", size);

            return (await Connection.QueryAsync<RecentActivity>(
                sql: "[CAP].[Read_Activities_ByProjectId]",
                param: parameters,
                commandType: CommandType.StoredProcedure,
                transaction: Transaction
            )).ToList();
        }

        public async Task<List<RecentActivity>> GetAllActivitiesByProjectId(int projectId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ProjectId", projectId);

            return (await Connection.QueryAsync<RecentActivity>(
                sql: "[CAP].[Read_All_Activities_ByProjectId]",
                param: parameters,
                commandType: CommandType.StoredProcedure,
                transaction: Transaction
            )).ToList();
        }
    }
}
