using Dapper;
using DigitalCap.Core.Enumerations;
using DigitalCap.Core.Helpers.Constants;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.Permissions;
using DigitalCap.WebApi.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Numerics;
using System.Security.Claims;
using System.Text;
using System.Transactions;
using System.Web.Mvc;

namespace DigitalCap.Persistence.Repositories
{
    public class ProjectRepository : RepositoryBase<Project, int>, IProjectRepository
    {
        private readonly ILogger<ProjectRepository> _logger;
        private readonly IHttpContextAccessor _httpContext;
        private readonly UserManager<ApplicationDto> _userManager;
        public ProjectRepository(IUnitOfWork unitOfWork, ILogger<ProjectRepository> logger) : base(unitOfWork, logger)
        {
            _logger = logger;
        }

        public async Task<bool> CheckProjectExistsByImoNumber(int imoNumber)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@IMO", imoNumber);

                var result = await Connection.QueryFirstOrDefaultAsync(
                 sql: "[CAP].[Read_ProjectExists_ByImoNumber]",
                 param: parameters,
                 commandType: CommandType.StoredProcedure,
                 transaction: Transaction
             );

                return result != null ? true : false;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<int> CreateProject(Project model)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@User", "");
            parameters.Add("@IsCap2", true);


            await Connection.ExecuteAsync(
                sql: "[CAP].[Create_Project]",
                commandType: CommandType.StoredProcedure,
                param: parameters,
                transaction: Transaction
            );

            return parameters.Get<int>("1");
        }

        public async Task<List<SelectListItem>> GetProjectListByIMO(int? imoNumber)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@ImoNumber", imoNumber);

                return (await Connection.QueryAsync<SelectListItem>(
                    sql: "CAP.GetProjectListByIMO",
                    param: parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction)).ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Project> GetProject(int id)
        {
            try
            {
                var result = await Connection.QueryFirstOrDefaultAsync<Project>(
                    sql: "CAP.Read_Project_ById",
                    param: new { id },
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction
                );

                return result;
            }
            catch
            {
                throw;
            }
        }


        private static T GetValue<T>(IDataReader reader, HashSet<string> columns, string columnName)
        {
            if (!columns.Contains(columnName))
                return default;

            var value = reader[columnName];
            if (value == DBNull.Value)
                return default;

            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            return (T)Convert.ChangeType(value, targetType);
        }

        private static IEnumerable<Project> ReadProjects(IDataReader dataReader)
        {
            if (dataReader == null)
                yield break;

            var columnNames = Enumerable.Range(0, dataReader.FieldCount)
                                        .Select(dataReader.GetName)
                                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

            while (dataReader.Read())
            {
                var project = new Project
                {
                    ID = GetValue<int>(dataReader, columnNames, "ID"),
                    Name = GetValue<string>(dataReader, columnNames, nameof(Project.Name)),
                    PIDNumber = GetValue<int?>(dataReader, columnNames, nameof(Project.PIDNumber)),

                    CapScope = GetValue<string>(dataReader, columnNames, "CapScope"),
                    CapScopeOther = GetValue<string>(dataReader, columnNames, "CapScopeOther"),

                    DrydockLocation = GetValue<int?>(dataReader, columnNames, nameof(Project.DrydockLocation)),
                    DrydockStart = GetValue<DateTime?>(dataReader, columnNames, nameof(Project.DrydockStart)),
                    DrydockEnd = GetValue<DateTime?>(dataReader, columnNames, nameof(Project.DrydockEnd)),

                    SpecialHull = GetValue<bool>(dataReader, columnNames, "SpecialHull"),
                    Fatigue = GetValue<bool>(dataReader, columnNames, "FatigueAnalysis"),

                    ExpenseMarkup = GetValue<int?>(dataReader, columnNames, nameof(Project.ExpenseMarkup)),
                    SurveyDayrateClient = GetValue<decimal?>(dataReader, columnNames, nameof(Project.SurveyDayrateClient)),
                    SurveyDayrateAbs = GetValue<decimal?>(dataReader, columnNames, nameof(Project.SurveyDayrateAbs)),
                    SedRate1 = GetValue<decimal?>(dataReader, columnNames, nameof(Project.SedRate1)),
                    SedRate2 = GetValue<decimal?>(dataReader, columnNames, nameof(Project.SedRate2)),

                    AgreementNumber = GetValue<string>(dataReader, columnNames, "AgreementNumber"),
                    AgreementOwner = GetValue<string>(dataReader, columnNames, "AgreementOwner"),
                    AgreementDate = GetValue<DateTime?>(dataReader, columnNames, "AgreementDate"),
                    AgreementSignedDate = GetValue<DateTime?>(dataReader, columnNames, "AgreementSignedDate"),

                    CertificateGrade = GetValue<string>(dataReader, columnNames, "CertificateGrade"),
                    ProjectComments = GetValue<string>(dataReader, columnNames, "ProjectComments"),

                    CompanyName = GetValue<string>(dataReader, columnNames, "CompanyName"),
                    CompanyAddress = GetValue<string>(dataReader, columnNames, "CompanyAddress"),
                    CompanyBillingEmail = GetValue<string>(dataReader, columnNames, "CompanyBillingEmail"),
                    CompanyBillingSystemUrl = GetValue<string>(dataReader, columnNames, "CompanyBillingSystemUrl"),

                    BillSameAsCapClient = GetValue<bool>(dataReader, columnNames, "BillSameAsCapClient"),
                    BillToCompanyName = GetValue<string>(dataReader, columnNames, "BillToCompanyName"),
                    BillToCompanyAddress = GetValue<string>(dataReader, columnNames, "BillToCompanyAddress"),
                    BillToCompanyEmail = GetValue<string>(dataReader, columnNames, "BillToCompanyEmail"),
                    BillToCompanyBillingUrl = GetValue<string>(dataReader, columnNames, "BillToCompanyBillingUrl"),

                    PocName = GetValue<string>(dataReader, columnNames, "PocName"),
                    PocEmail = GetValue<string>(dataReader, columnNames, "PocEmail"),
                    PocPhone = GetValue<string>(dataReader, columnNames, "PocPhone"),

                    IMO = GetValue<int?>(dataReader, columnNames, nameof(Project.IMO)),
                    VesselName = GetValue<string>(dataReader, columnNames, "VesselName"),

                    ShipType = (ShipType)GetValue<int>(dataReader, columnNames, nameof(Project.ShipType)),
                    CapType = (CapType)GetValue<int>(dataReader, columnNames, nameof(Project.CapType)),

                    WCN = GetValue<string>(dataReader, columnNames, "WCN"),
                    AbsClassID = GetValue<string>(dataReader, columnNames, "AbsClassID"),

                    SisterVessel = GetValue<bool>(dataReader, columnNames, "SisterVessel"),
                    SisterVesselImoNumber = GetValue<string>(dataReader, columnNames, "SisterVesselImoNumber"),
                    SisterVesselName = GetValue<string>(dataReader, columnNames, "SisterVesselName"),

                    Builder = GetValue<string>(dataReader, columnNames, "Builder"),
                    HullNumber = GetValue<string>(dataReader, columnNames, "HullNumber"),

                    MonthBuilt = GetValue<int?>(dataReader, columnNames, nameof(Project.MonthBuilt)),
                    YearBuilt = GetValue<int?>(dataReader, columnNames, nameof(Project.YearBuilt)),
                    LengthOverall = GetValue<decimal?>(dataReader, columnNames, nameof(Project.LengthOverall)),

                    ProjectStatus = GetValue<int>(dataReader, columnNames, "ProjectStatus"),
                    ProjectPriority = GetValue<int?>(dataReader, columnNames, nameof(Project.ProjectPriority)),

                    ProposalRev = GetValue<int>(dataReader, columnNames, nameof(Project.ProposalRev)),
                    CRMOpportunityNumber = GetValue<string>(dataReader, columnNames, nameof(Project.CRMOpportunityNumber)),

                    PotentialDrydockDate = GetValue<DateTime?>(dataReader, columnNames, "PotentialDrydockDate"),
                    SurveyFirstVisit = GetValue<DateTime?>(dataReader, columnNames, "SurveyFirstVisit"),
                    SurveyLastVisit = GetValue<DateTime?>(dataReader, columnNames, "SurveyLastVisit"),

                    BillToWCN = GetValue<string>(dataReader, columnNames, nameof(Project.BillToWCN)),

                    ClientProfileId = GetValue<Guid?>(dataReader, columnNames, "ClientProfileId") ?? Guid.Empty,
                    BillToClientProfileId = GetValue<Guid?>(dataReader, columnNames, "BillToClientProfileId") ?? Guid.Empty,

                    LeadSurveyor = GetValue<string>(dataReader, columnNames, nameof(Project.LeadSurveyor)),
                    LeadSurveyorEID = GetValue<string>(dataReader, columnNames, nameof(Project.LeadSurveyorEID)),
                    SecondSurveyor = GetValue<string>(dataReader, columnNames, nameof(Project.SecondSurveyor))?.Split(','),
                    SecondSurveyorEID = GetValue<string>(dataReader, columnNames, nameof(Project.SecondSurveyorEID)),
                    ThirdSurveyor = GetValue<string>(dataReader, columnNames, nameof(Project.ThirdSurveyor)),
                    ThirdSurveyorEID = GetValue<string>(dataReader, columnNames, nameof(Project.ThirdSurveyorEID)),
                    FourthSurveyor = GetValue<string>(dataReader, columnNames, nameof(Project.FourthSurveyor)),
                    FourthSurveyorEID = GetValue<string>(dataReader, columnNames, nameof(Project.FourthSurveyorEID)),

                    AdditionalSurveyors = GetValue<bool>(dataReader, columnNames, "AdditionalSurveyors"),

                    ClassSociety = GetValue<string>(dataReader, columnNames, "ClassSociety")?.Split(','),
                    ClassSocietyOther = GetValue<string>(dataReader, columnNames, "ClassSocietyOther"),

                    CreatedDate = GetValue<DateTime?>(dataReader, columnNames, nameof(Project.CreatedDate))?.ToLocalTime(),
                    CreatedBy = GetValue<string>(dataReader, columnNames, "CreatedByDisplayName"),
                    StatusChangedBy = GetValue<string>(dataReader, columnNames, "StatusChangedByDisplayName"),
                    LastModifiedBy = GetValue<string>(dataReader, columnNames, nameof(Project.LastModifiedBy)),

                    StatusChangedDate = GetValue<DateTimeOffset?>(dataReader, columnNames, nameof(Project.StatusChangedDate)),
                    VesselType = GetValue<string>(dataReader, columnNames, "VesselType"),
                    CapRegion = GetValue<string>(dataReader, columnNames, "CapRegion"),

                    IsDefaultTank = GetValue<bool>(dataReader, columnNames, "IsDefaultTank"),
                    CopyingVesselID = GetValue<int>(dataReader, columnNames, nameof(Project.CopyingVesselID))
                };

                if (!FormValues.CAPRegion.Contains(project.CapRegion))
                    project.CapRegion = FormValues.CAPRegion.FirstOrDefault();

                if (columnNames.Contains(nameof(Project.ProjectStatusName)))
                    project.ProjectStatusName = GetValue<string>(dataReader, columnNames, nameof(Project.ProjectStatusName));

                if (columnNames.Contains(nameof(Project.DrydockLocationName)))
                    project.DrydockLocationName = GetValue<string>(dataReader, columnNames, nameof(Project.DrydockLocationName));

                yield return project;
            }
        }

        public async Task<string> GetLoggedInUserName()
        {
            ClaimsPrincipal principal = _httpContext.HttpContext.User as ClaimsPrincipal;
            var userId = _userManager.GetUserId(principal);
            if (userId == null) return string.Empty;
            var user = await _userManager.FindByIdAsync(userId);
            return user.UserName;
        }

        public async Task<int> UpdatePercentComplete(int projectId, byte percentComplete)
        {
            var currentUser = await GetLoggedInUserName();
            return await Connection.ExecuteAsync(
                sql: "CAP.Update_Project_Percent_Complete",
                param: new
                {
                    ID = projectId,
                    PercentComplete = percentComplete,
                    User = currentUser
                },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction
            );
        }

        public async Task<bool> CheckProjectExistsByProjectName(string projectName, int imoNumber, int vesseltypeid)
        {
            var result = await Connection.QueryFirstOrDefaultAsync<string>(
                sql: "CAP.Read_ProjectExists_ByProjectName",
                param: new { ProjectName = projectName, IMO = imoNumber, VesselTypeID = vesseltypeid },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction);
            return result != null ? true : false;

        }

        public async Task Update_Project(Project project)
        {
            var currentUser = await GetLoggedInUserName();

            var parameters = new
            {
                project.ID,
                project.Name,
                project.PIDNumber,
                project.CapScope,
                project.CapScopeOther,
                project.DrydockLocation,
                project.DrydockStart,
                project.DrydockEnd,
                project.SpecialHull,
                project.Fatigue,
                project.ExpenseMarkup,
                project.SurveyDayrateClient,
                project.SurveyDayrateAbs,
                project.SedRate1,
                project.SedRate2,
                project.AgreementNumber,
                project.CapContractValue,
                project.AgreementOwner,
                project.AgreementDate,
                project.AgreementSignedDate,
                project.CertificateGrade,
                project.ProjectComments,
                project.CompanyName,
                project.CompanyAddress,
                project.CompanyBillingEmail,
                project.CompanyBillingSystemUrl,
                project.BillSameAsCapClient,
                project.BillToCompanyName,
                project.BillToCompanyAddress,
                project.BillToCompanyEmail,
                project.BillToCompanyBillingUrl,
                project.PocName,
                project.PocEmail,
                project.PocPhone,
                project.IMO,
                project.VesselName,
                project.ShipType,
                ClassSociety = project.ClassSociety != null
                    ? string.Join(",", project.ClassSociety)
                    : null,
                SecondSurveyor = project.SecondSurveyor != null
                    ? string.Join(",", project.SecondSurveyor)
                    : null,
                User = currentUser
            };

            await Connection.ExecuteAsync(
                sql: "CAP.Update_Project",
                param: parameters,
                commandType: CommandType.StoredProcedure,
                transaction: Transaction
            );
        }
        public async Task<string> GetProjectName(int id)
        {
            var result = await Connection.QueryFirstOrDefaultAsync<string>(
                sql: "CAP.Read_Project_Name_ById",
                param: new { ID = id },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction);

            return result;

        }

        public async Task UpdateProjectStatus(int id, string currentUserId, int projectStatus)
        {
            var currentUser = await GetLoggedInUserName();

            await Connection.ExecuteAsync(
                sql: "CAP.Update_Project_Status_Manual",
                param: new
                {
                    Id = id,
                    ProjectStatus = projectStatus,
                    StatusChangedBy = currentUserId,
                    User = currentUser
                },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction
            );
        }

        public async Task DeleteProject(int id, string currentUserId)
        {
            var currentUser = await GetLoggedInUserName();

            await Connection.ExecuteAsync(
                sql: "CAP.Delete_Project",
                param: new
                {
                    Id = id,
                    User = currentUserId
                },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction);
        }

        public async Task<IDictionary<int, int>> GetProjectIdByIMONumberLookup()
        {

            var rows = await Connection.QueryAsync(
                @"SELECT DISTINCT ID, IMO 
              FROM CAP.Projects 
              WHERE IMO IS NOT NULL");

            var dict = new Dictionary<int, int>();

            foreach (var row in rows)
            {
                int imo = row.IMO;
                int id = row.ID;

                if (!dict.ContainsKey(imo))
                    dict.Add(imo, id);
            }

            return dict;
        }

        public async Task<string> GetIMONumberByProjectId(int projectId)
        {

            var result = await Connection.QueryAsync<string>(
                sql: "CAP.Read_IMONumber_ByProjectId",
                new { ProjectId = projectId },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction);

            return result.FirstOrDefault();

        }


        private static IEnumerable<AllProjectsGridExport> ReadProjectsForGridExport(IDataReader dataReader)
        {
            if (dataReader == null)
                yield break;

            var columnNames = Enumerable.Range(0, dataReader.FieldCount)
                .Select(dataReader.GetName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            while (dataReader.Read())
            {
                var project = new AllProjectsGridExport
                {
                    ID = GetValue<int>(dataReader, columnNames, "ID"),
                    Name = GetValue<string>(dataReader, columnNames, nameof(AllProjectsGridExport.Name)),
                    PIDNumber = GetValue<int?>(dataReader, columnNames, nameof(AllProjectsGridExport.PIDNumber)),

                    CapScope = GetValue<string>(dataReader, columnNames, "CapScope"),
                    CapScopeOther = GetValue<string>(dataReader, columnNames, "CapScopeOther"),

                    DrydockLocation = GetValue<int?>(dataReader, columnNames, nameof(AllProjectsGridExport.DrydockLocation)),
                    DrydockStart = GetValue<DateTime?>(dataReader, columnNames, nameof(AllProjectsGridExport.DrydockStart)),
                    DrydockEnd = GetValue<DateTime?>(dataReader, columnNames, nameof(AllProjectsGridExport.DrydockEnd)),

                    SpecialHull = GetValue<bool>(dataReader, columnNames, "SpecialHull"),
                    Fatigue = GetValue<bool>(dataReader, columnNames, "FatigueAnalysis"),

                    ExpenseMarkup = GetValue<int?>(dataReader, columnNames, nameof(AllProjectsGridExport.ExpenseMarkup)),
                    SurveyDayrateClient = GetValue<decimal?>(dataReader, columnNames, nameof(AllProjectsGridExport.SurveyDayrateClient)),
                    SurveyDayrateAbs = GetValue<decimal?>(dataReader, columnNames, nameof(AllProjectsGridExport.SurveyDayrateAbs)),
                    SedRate1 = GetValue<decimal?>(dataReader, columnNames, nameof(AllProjectsGridExport.SedRate1)),
                    SedRate2 = GetValue<decimal?>(dataReader, columnNames, nameof(AllProjectsGridExport.SedRate2)),

                    AgreementNumber = GetValue<string>(dataReader, columnNames, "AgreementNumber"),
                    AgreementOwner = GetValue<string>(dataReader, columnNames, "AgreementOwner"),
                    AgreementDate = GetValue<DateTime?>(dataReader, columnNames, "AgreementDate"),
                    AgreementSignedDate = GetValue<DateTime?>(dataReader, columnNames, "AgreementSignedDate"),

                    CertificateGrade = GetValue<string>(dataReader, columnNames, "CertificateGrade"),
                    ProjectComments = GetValue<string>(dataReader, columnNames, "ProjectComments"),

                    CompanyName = GetValue<string>(dataReader, columnNames, "CompanyName"),
                    CompanyAddress = GetValue<string>(dataReader, columnNames, "CompanyAddress"),
                    CompanyBillingEmail = GetValue<string>(dataReader, columnNames, "CompanyBillingEmail"),
                    CompanyBillingSystemUrl = GetValue<string>(dataReader, columnNames, "CompanyBillingSystemUrl"),

                    BillSameAsCapClient = GetValue<bool>(dataReader, columnNames, "BillSameAsCapClient"),
                    BillToCompanyName = GetValue<string>(dataReader, columnNames, "BillToCompanyName"),
                    BillToCompanyAddress = GetValue<string>(dataReader, columnNames, "BillToCompanyAddress"),
                    BillToCompanyEmail = GetValue<string>(dataReader, columnNames, "BillToCompanyEmail"),
                    BillToCompanyBillingUrl = GetValue<string>(dataReader, columnNames, "BillToCompanyBillingUrl"),

                    PocName = GetValue<string>(dataReader, columnNames, "PocName"),
                    PocEmail = GetValue<string>(dataReader, columnNames, "PocEmail"),
                    PocPhone = GetValue<string>(dataReader, columnNames, "PocPhone"),

                    IMO = GetValue<int?>(dataReader, columnNames, nameof(Project.IMO)),
                    VesselName = GetValue<string>(dataReader, columnNames, "VesselName"),

                    ShipType = (ShipType)GetValue<int>(dataReader, columnNames, nameof(AllProjectsGridExport.ShipType)),
                    CapType = (CapType)GetValue<int>(dataReader, columnNames, nameof(AllProjectsGridExport.CapType)),

                    WCN = GetValue<string>(dataReader, columnNames, "WCN"),
                    AbsClassID = GetValue<string>(dataReader, columnNames, "AbsClassID"),

                    SisterVessel = GetValue<bool>(dataReader, columnNames, "SisterVessel"),
                    SisterVesselImoNumber = GetValue<string>(dataReader, columnNames, "SisterVesselImoNumber"),
                    SisterVesselName = GetValue<string>(dataReader, columnNames, "SisterVesselName"),

                    Builder = GetValue<string>(dataReader, columnNames, "Builder"),
                    HullNumber = GetValue<string>(dataReader, columnNames, "HullNumber"),

                    MonthBuilt = GetValue<int?>(dataReader, columnNames, nameof(AllProjectsGridExport.MonthBuilt)),
                    YearBuilt = GetValue<int?>(dataReader, columnNames, nameof(AllProjectsGridExport.YearBuilt)),
                    LengthOverall = GetValue<decimal?>(dataReader, columnNames, nameof(AllProjectsGridExport.LengthOverall)),

                    ProjectStatus = GetValue<int>(dataReader, columnNames, "ProjectStatus"),
                    ProjectPriority = GetValue<int?>(dataReader, columnNames, nameof(AllProjectsGridExport.ProjectPriority)),

                    ProposalRev = GetValue<int>(dataReader, columnNames, nameof(AllProjectsGridExport.ProposalRev)),
                    CRMOpportunityNumber = GetValue<string>(dataReader, columnNames, nameof(AllProjectsGridExport.CRMOpportunityNumber)),

                    PotentialDrydockDate = GetValue<DateTime?>(dataReader, columnNames, "PotentialDrydockDate"),
                    SurveyFirstVisit = GetValue<DateTime?>(dataReader, columnNames, "SurveyFirstVisit"),
                    SurveyLastVisit = GetValue<DateTime?>(dataReader, columnNames, "SurveyLastVisit"),

                    BillToWCN = GetValue<string>(dataReader, columnNames, nameof(AllProjectsGridExport.BillToWCN)),

                    ClientProfileId = GetValue<Guid?>(dataReader, columnNames, "ClientProfileId") ?? Guid.Empty,
                    BillToClientProfileId = GetValue<Guid?>(dataReader, columnNames, "BillToClientProfileId") ?? Guid.Empty,

                    LeadSurveyor = GetValue<string>(dataReader, columnNames, nameof(AllProjectsGridExport.LeadSurveyor)),
                    LeadSurveyorEID = GetValue<string>(dataReader, columnNames, nameof(AllProjectsGridExport.LeadSurveyorEID)),
                    SecondSurveyor = GetValue<string>(dataReader, columnNames, nameof(AllProjectsGridExport.SecondSurveyor)),
                    SecondSurveyorEID = GetValue<string>(dataReader, columnNames, nameof(AllProjectsGridExport.SecondSurveyorEID)),
                    ThirdSurveyor = GetValue<string>(dataReader, columnNames, nameof(AllProjectsGridExport.ThirdSurveyor)),
                    ThirdSurveyorEID = GetValue<string>(dataReader, columnNames, nameof(AllProjectsGridExport.ThirdSurveyorEID)),
                    FourthSurveyor = GetValue<string>(dataReader, columnNames, nameof(AllProjectsGridExport.FourthSurveyor)),
                    FourthSurveyorEID = GetValue<string>(dataReader, columnNames, nameof(AllProjectsGridExport.FourthSurveyorEID)),

                    AdditionalSurveyors = GetValue<bool>(dataReader, columnNames, "AdditionalSurveyors"),

                    ClassSociety = GetValue<string>(dataReader, columnNames, "ClassSociety")?.Split(','),
                    ClassSocietyOther = GetValue<string>(dataReader, columnNames, "ClassSocietyOther"),

                    CreatedDate = GetValue<DateTime?>(dataReader, columnNames, nameof(Project.CreatedDate))?.ToLocalTime(),
                    LastModifiedBy = GetValue<string>(dataReader, columnNames, "LastModifiedBy"),

                    CapRegion = GetValue<string>(dataReader, columnNames, "CapRegion"),
                    MostRecentProjectComment = GetValue<string>(dataReader, columnNames, nameof(AllProjectsGridExport.MostRecentProjectComment))
                };

                if (!FormValues.CAPRegion.Contains(project.CapRegion))
                    project.CapRegion = FormValues.CAPRegion.FirstOrDefault();

                if (columnNames.Contains(nameof(Project.ProjectStatusName)))
                    project.ProjectStatusName = GetValue<string>(dataReader, columnNames, nameof(Project.ProjectStatusName));

                if (columnNames.Contains(nameof(Project.DrydockLocationName)))
                    project.DrydockLocationName = GetValue<string>(dataReader, columnNames, nameof(Project.DrydockLocationName));

                yield return project;
            }
        }

        private static IEnumerable<AllProjectsGrid> ReadProjectsForGrid(IDataReader dataReader)
        {
            if (dataReader == null)
                yield break;

            var columnNames = Enumerable.Range(0, dataReader.FieldCount)
                .Select(dataReader.GetName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            while (dataReader.Read())
            {
                var project = new AllProjectsGrid
                {
                    ID = GetValue<int>(dataReader, columnNames, "id"),
                    Name = GetValue<string>(dataReader, columnNames, "name"),
                    PIDNumber = GetValue<string>(dataReader, columnNames, "pidNumber"),
                    MostRecentComment = GetValue<string>(dataReader, columnNames, "mostRecentComment"),
                    CompanyName = GetValue<string>(dataReader, columnNames, "companyName"),
                    IMO = GetValue<string>(dataReader, columnNames, "imo"),
                    WCN = GetValue<string>(dataReader, columnNames, "wcn"),
                    Priority = GetValue<string>(dataReader, columnNames, "projectPriority"),
                    IsDownload = GetValue<string>(dataReader, columnNames, "IsDownload"),

                    PotentialDrydockDate = GetValue<DateTime?>(dataReader, columnNames, "potentialDrydockDate"),

                    CommentLastModifiedDate =
                        GetValue<DateTimeOffset?>(dataReader, columnNames, nameof(AllProjectsGrid.CommentLastModifiedDate)),

                    CommentCreatedBy = GetValue<string>(dataReader, columnNames, "commentCreatedBy"),

                    ProjectLastUpdatedDate =
                        GetValue<DateTime?>(dataReader, columnNames, "projectLastUpdatedDate"),

                    CapRegion = GetValue<string>(dataReader, columnNames, "capRegion"),

                    PercentComplete =
                        GetValue<byte?>(dataReader, columnNames, nameof(AllProjectsGrid.PercentComplete))
                };

                if (!FormValues.CAPRegion.Contains(project.CapRegion))
                    project.CapRegion = FormValues.CAPRegion.FirstOrDefault();

                if (columnNames.Contains("ProjectStatusName"))
                    project.StatusName = GetValue<string>(dataReader, columnNames, "ProjectStatusName");

                yield return project;
            }
        }

        private static IEnumerable<ProjectsByClientId> ReadProjectsByClientId(IDataReader dataReader)
        {
            if (dataReader == null)
                yield break;

            var columnNames = Enumerable.Range(0, dataReader.FieldCount)
                .Select(dataReader.GetName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            while (dataReader.Read())
            {
                var project = new ProjectsByClientId
                {
                    ID = GetValue<int>(dataReader, columnNames, "ID"),
                    Name = GetValue<string>(dataReader, columnNames, "Name"),
                    PIDNumber = GetValue<string>(dataReader, columnNames, "PIDNumber"),

                    SurveyFirstVisit =
                        GetValue<DateTime?>(dataReader, columnNames, "SurveyFirstVisit"),

                    SurveyLastVisit =
                        GetValue<DateTime?>(dataReader, columnNames, "SurveyLastVisit")
                };

                if (columnNames.Contains("Status"))
                    project.StatusName = GetValue<string>(dataReader, columnNames, "Status");

                yield return project;
            }
        }
        private static IEnumerable<ProjectCountGroupedByClientId> ReadProjectCountGroupedById(IDataReader dataReader)
        {
            if (dataReader == null)
                yield break;

            var columnNames = Enumerable.Range(0, dataReader.FieldCount)
                .Select(dataReader.GetName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            while (dataReader.Read())
            {
                yield return new ProjectCountGroupedByClientId
                {
                    ClientId = GetValue<Guid>(dataReader, columnNames, "ClientId"),
                    ProjectCount = GetValue<int>(dataReader, columnNames, "ProjectCount")
                };
            }
        }

        public async Task<IEnumerable<ProjectDetailItem>> GetDetails(int id)
        {

            var result = await Connection.QueryAsync<ProjectDetailItem>(
                sql: "CAP.Read_Project_Details",
                param: new { ProjectId = id },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction);

            return result;

        }
        public async Task<ProjectSummary> GetSummary(int id)
        {

            var result = await Connection.QueryFirstOrDefaultAsync<ProjectSummary>(
                sql: "CAP.Read_Project_Summary_ById",
                param: new { ID = id },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction);

            return result;

        }


        public async Task UpdatePriority(int id, int? priority)
        {
            var currentUser = await GetLoggedInUserName();

            await Connection.ExecuteAsync(
                sql: "CAP.Update_Project_Priority",
                param: new
                {
                    Id = id,
                    Priority = priority,
                    User = currentUser
                },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction
            );

        }

        public async Task<IEnumerable<AllProjectsGrid>> GetAllProjectsGridData()
        {
            var result = await Connection.QueryAsync<AllProjectsGrid>(
                sql: "CAP.Read_Projects_Grid",
                commandType: CommandType.StoredProcedure,
                transaction: Transaction
            );

            return result
                .Select(p =>
                {
                    if (!FormValues.CAPRegion.Contains(p.CapRegion))
                    {
                        p.CapRegion = FormValues.CAPRegion.FirstOrDefault();
                    }
                    return p;
                })
                .OrderBy(r => string.IsNullOrEmpty(r.Priority))
                .ThenBy(r => r.Priority)
                .ThenBy(r => r.Name);
        }

        public async Task<List<CAPCoordinator>> GetCAPCoordinator(string region, bool includeManager)
        {
            try
            {
                var result = await Connection.QueryAsync<CAPCoordinator>(
                    sql: "[CAP].[Read_CAPCoordinator_ByRegion]",
                    param: new
                    {
                        Region = region,
                        IncludeManager = includeManager
                    },
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction
                );

                return result.ToList();
            }
            catch
            {
                throw;
            }
        }
        public async Task<string> GetProjectVesselType(int id)
        {
            var result = await Connection.QueryFirstOrDefaultAsync<string>(
                   sql: "CAP.ReadProjectVesselTypeById",
                   param: new { ID = id },
                   commandType: CommandType.StoredProcedure,
                   transaction: Transaction);

            return result;
        }
    }
}
