using Dapper;
using DapperExtensions;
using DigitalCap.Core.Helpers.Constants;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.Tank;
using DigitalCap.Core.ViewModels;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Transactions;

namespace DigitalCap.Persistence.Repositories
{
    public class TaskRepository :  ITaskRepository
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected IDbConnection Connection => _unitOfWork?.Connection!;
        protected IDbTransaction Transaction => _unitOfWork?.Transaction!;
        public async Task<byte> UpdateProjectPercentComplete(int projectId)
        {
            var tasks = await GetCap2Tasks(projectId);
            var projectTasks = tasks.Where(x => x.Category == CategoryNames.Project);
            var taskViewModel = new TasksViewModel(projectId, projectTasks);
            const int STARTING_PROGRESS = 4;
            var percentComplete = (byte)(STARTING_PROGRESS + taskViewModel.Sections.Sum(x => x.Tasks.Where(x => x.PercentageComplete > 0 && x.Status == CapTaskStatus.Completed.ToString()).Sum(x => x.PercentageComplete)));

            return percentComplete;
        }

        public async Task<IEnumerable<SingleTask>> GetCap2Tasks(int projectId)
        {
            var result = await Connection.QueryAsync<SingleTask>(
                sql: "[CAP].[Read_Cap2Tasks_ByProjectId]",
                new { ID = projectId },
                transaction: Transaction,
                commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<List<TaskStatusGroup>> GetStatusGroups(string category)
        {
            var sql = @"
        SELECT *
        FROM TaskStatusGroup
        WHERE Category = @Category
        ORDER BY Id
    ";

            var result = await Connection.QueryAsync<TaskStatusGroup>(
                sql,
                new { Category = category },
                transaction: Transaction
            );

            return result.ToList();
        }
        public async Task<IEnumerable<SingleTask>> GetTasks(int projectId)
        {
            var result = await Connection.QueryAsync<SingleTask>(
     sql: "CAP.Read_Tasks_ByProjectId",
     new { ID = projectId },
     commandType: CommandType.StoredProcedure);

            return result;

        }

    }
}
