using Dapper;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models.Grading;
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

        public async Task<bool> CreateImageDescription(ImageDescriptions entity)
        {
            try
            {
                await Connection.ExecuteAsync(
                    sql: "dbo.CreateImageDescription",
                    param: new
                    {
                        entity.IsActive,
                        entity.IsDeleted,
                        entity.Description,
                        entity.SectionId,
                        entity.TankTypeId,
                        CreatedDttm = DateTime.Now,
                        UpdatedDttm = DateTime.Now,
                        entity.ProjectId,
                        entity.CategoryId,
                        entity.VesselType
                    },
                    transaction: Transaction,
                    commandType: CommandType.StoredProcedure);

                return true;
            }
            catch
            {
                return false;
            }
        }


        public async Task<bool> UpdateImageDescription(ImageDescriptions entity)
        {
            try
            {
                await Connection.ExecuteAsync(
                    sql: "dbo.UpdateImageDescription",
                    param: new
                    {
                        entity.Id,
                        entity.IsActive,
                        entity.IsDeleted,
                        entity.Description,
                        entity.SectionId,
                        entity.TankTypeId,
                        UpdatedDttm = DateTime.Now,
                        entity.ProjectId,
                        entity.CategoryId,
                        entity.VesselType
                    },
                    transaction: Transaction,
                    commandType: CommandType.StoredProcedure);

                return true;
            }
            catch
            {
                return false;
            }
        }


        public async Task<List<ImageDescriptions>> CheckImageDescriptionExistsOrNot(string vesselType, string sectionName, string partName, string description)
        {
            var result = await Connection.QueryAsync<ImageDescriptions>(
            sql: "dbo.CheckImageDescriptionExistsOrNot",
            param: new
            {
                VesselType = vesselType,
                SectionName = sectionName,
                PartName = partName,
                Description = description
            },
        transaction: Transaction,
        commandType: CommandType.StoredProcedure);

            return result.ToList();
        }


        public async Task<List<ImageDescriptions>> GetAllDescription()
        {
            var result = await Connection.QueryAsync<ImageDescriptions>(
            sql: "dbo.GetAllDescription",
            transaction: Transaction,
            commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<List<GradingSection>> GetDescriptionSectionNamesByTemplateNameAndVesselType(string templateName, string vesselType)
        {
            var result = await Connection.QueryAsync<GradingSection>(
            sql: "dbo.GetDescriptionSectionsByTemplateNameAndVesselType",
            param: new
            {
                TemplateName = templateName,
                VesselType = vesselType
            },
            transaction: Transaction,
            commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<List<GradingTemplate>> GetGradingTemplates()
        {
            var result = await Connection.QueryAsync<GradingTemplate>(
            sql: "dbo.GetGradingTemplate",
            transaction: Transaction,
            commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<List<GradingSection>> GetGradingSections(int templateId, string vesselType)
        {
            var result = await Connection.QueryAsync<GradingSection>(
            sql: "dbo.GetGradingSections",
            param: new
            {
                TemplateId = templateId,
                VesselType = vesselType
            },
            transaction: Transaction,
            commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<ImageDescriptions> GetImageDescriptionById(int id)
        {
            var result = await Connection.QueryAsync<ImageDescriptions>(
            sql: "dbo.GetImageDescriptionById",   // Stored procedure name
            param: new { Id = id },
            commandType: CommandType.StoredProcedure,
            transaction: Transaction);

            return result.FirstOrDefault();
        }


    }
}
