using Dapper;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models.Grading;
using DigitalCap.Core.Models.ImageDescription;
using DigitalCap.Core.Models.ReportConfig;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
                            commandType: CommandType.StoredProcedure);
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
                transaction: Transaction);

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


        public async Task<List<ImageDescriptions>> CheckImageDescriptionExistsOrNot(string vesselType,string sectionName,string partName,string description)
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

        public async Task<List<GradingSection>> GetDescriptionSectionNamesByTemplateNameAndVesselType(string templateName,string vesselType)
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

        //public async Task<bool> CreateImageDescription(ImageDescriptions entity)
        //{
        //    using var conn = new SqlConnection(_connStr);
        //    using var cmd = new SqlCommand(StoredProcedure.ImageDescription.CreateImageDescription, conn);
        //    cmd.CommandType = CommandType.StoredProcedure;

        //    cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
        //    cmd.Parameters.AddWithValue("@IsDeleted", entity.IsDeleted);
        //    cmd.Parameters.AddWithValue("@Description", entity.Description);
        //    cmd.Parameters.AddWithValue("@SectionId", entity.SectionId);
        //    cmd.Parameters.AddWithValue("@TankTypeId", entity.TankTypeId);
        //    cmd.Parameters.AddWithValue("@CreatedDttm", DateTime.Now);
        //    cmd.Parameters.AddWithValue("@UpdatedDttm", DateTime.Now);
        //    cmd.Parameters.AddWithValue("@ProjectId", entity.ProjectId);
        //    cmd.Parameters.AddWithValue("@CategoryId", entity.CategoryId);
        //    cmd.Parameters.AddWithValue("@VesselType", entity.VesselType);

        //    await conn.OpenAsync();
        //    await cmd.ExecuteNonQueryAsync();
        //    return true;
        //}

        //public async Task<bool> UpdateImageDescription(ImageDescriptions entity)
        //{
        //    using var conn = new SqlConnection(_connStr);
        //    using var cmd = new SqlCommand(StoredProcedure.ImageDescription.UpdateImageDescription, conn);
        //    cmd.CommandType = CommandType.StoredProcedure;

        //    cmd.Parameters.AddWithValue("@Id", entity.Id);
        //    cmd.Parameters.AddWithValue("@IsActive", entity.IsActive);
        //    cmd.Parameters.AddWithValue("@IsDeleted", entity.IsDeleted);
        //    cmd.Parameters.AddWithValue("@Description", entity.Description);
        //    cmd.Parameters.AddWithValue("@SectionId", entity.SectionId);
        //    cmd.Parameters.AddWithValue("@TankTypeId", entity.TankTypeId);
        //    cmd.Parameters.AddWithValue("@UpdatedDttm", DateTime.Now);
        //    cmd.Parameters.AddWithValue("@ProjectId", entity.ProjectId);
        //    cmd.Parameters.AddWithValue("@CategoryId", entity.CategoryId);
        //    cmd.Parameters.AddWithValue("@VesselType", entity.VesselType);

        //    await conn.OpenAsync();
        //    await cmd.ExecuteNonQueryAsync();
        //    return true;
        //}


    }

}

