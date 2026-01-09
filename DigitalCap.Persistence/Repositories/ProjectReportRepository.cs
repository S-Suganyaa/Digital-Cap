using Dapper;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models.ReportConfig;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DigitalCap.Persistence.Repositories
{
    public class ProjectReportRepository : IProjectReportRepository
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected IDbConnection Connection => _unitOfWork?.Connection!;
        protected IDbTransaction Transaction => _unitOfWork?.Transaction!;

        public async Task<GenericImageCard> GetGenericImagCardByName(int projectId, int templateId, Guid sectionId, int cardNumber)
        {
            try
            {
                var result = await Connection.QueryAsync<GenericImageCard>(
                   sql: "[config].[GetGenericImagecardByName]",
                      new
                      {
                          ProjectId = projectId,
                          TemplateId = templateId,
                          SectionId = sectionId,
                          CardNumber = cardNumber
                      },
                    commandType: CommandType.StoredProcedure);

                return result.ToList().FirstOrDefault();

            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public async Task<bool> UpdateGenericImageCard(GenericImageCard model)
        {
            try
            {
                var parameters = new DynamicParameters();

                foreach (var prop in typeof(GenericImageCard).GetProperties())
                {
                    if (prop.Name == "src" || prop.Name == "IsSync")
                        continue;

                    parameters.Add(
                        name: "@" + prop.Name,
                        value: prop.GetValue(model)
                    );
                }

                await Connection.ExecuteAsync(
                    sql: "[config].[UpdateGenericImageCard]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction
                );

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> CreateGenericImageCard(GenericImageCard model)
        {
            try
            {
                var parameters = new DynamicParameters();

                foreach (var prop in typeof(GenericImageCard).GetProperties())
                {
                    if (prop.Name == "Id" ||
                        prop.Name == "src" ||
                        prop.Name == "IsSync")
                    {
                        continue;
                    }

                    parameters.Add(
                        name: "@" + prop.Name,
                        value: prop.GetValue(model)
                    );
                }

                await Connection.ExecuteAsync(
                    sql: "[config].[CreateGenericImageCard]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction
                );

                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
