using Dapper;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DigitalCap.Persistence.Repositories
{
    public class GradeRepository : IGradeRepository
    {

        protected readonly IUnitOfWork _unitOfWork;
        protected IDbConnection Connection => _unitOfWork?.Connection!;
        protected IDbTransaction Transaction => _unitOfWork?.Transaction!;
        
        public async Task<ProjectGrades> GetProjectGrades(int projectId)
        {

            var result = await Connection.QueryFirstOrDefaultAsync<ProjectGrades>(
                sql: "CAP.Read_Project_Grades",
                param: new { ProjectId = projectId },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction);

            return result;

        }

        public async Task<int> CreateProjectGrades(int projectId)
        {

            var result = await Connection.QueryFirstOrDefaultAsync<int>(
                sql: "CAP.Create_Project_Grades",
                param: new { ProjectId = projectId },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction);

            return result;

        }

        public async Task UpdateProjectGrades(ProjectGrades projectGrades)
        {
            await Connection.ExecuteAsync(
                sql: "CAP.Update_Project_Grades",
                param: new
                {
                    projectGrades.Id,
                    projectGrades.ProjectId,
                    projectGrades.CAPCertificateNumber,
                    projectGrades.CAPCertificateIssuanceDate,
                    projectGrades.ClassReportNumber,
                    projectGrades.ClassReportDate,
                    projectGrades.StructuralReportNumber,
                    projectGrades.StructuralReportDate,
                    projectGrades.FinalGrade,
                    projectGrades.StructuralGrade,
                    projectGrades.FatigueGrade,
                    projectGrades.RenewalGrade,
                    projectGrades.MaterialGrade,
                    projectGrades.GaugingGrade,
                    projectGrades.HullGirderStrength,
                    projectGrades.CAP1CertificateIssuanceDate,
                    projectGrades.CAP1FinalGrade
                },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction
            );
        }

    }
}
