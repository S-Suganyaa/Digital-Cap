using Dapper;
using DigitalCap.Core.Enumerations;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
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
using Microsoft.AspNetCore.Identity;
using DigitalCap.Core.Helpers.Constants;

namespace DigitalCap.Persistence.Repositories
{
    public class ProjectRepository : RepositoryBase<Project, int>, IProjectRepository
    {
        private readonly ILogger<ProjectRepository> _logger;
        private readonly IHttpContextAccessor _httpContext;
        private readonly UserManager<ApplicationUser> _userManager;
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
                return await Connection.QueryFirstOrDefaultAsync<Project>(
                    sql: "CAP.Read_Project_ById",
                    param: new { id },
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction
                );
            }
            catch
            {
                throw;
            }
        }


        private static T GetValue<T>(
    IDataReader reader,
    HashSet<string> columns,
    string columnName)
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

        public async Task UpdatePercentComplete(int projectId, byte percentComplete)
        {
            var currentUser = await GetLoggedInUserName();

            await ExecuteNonQuery(StoredProcedure.Project.UpdatePercentComplete,
                   command =>
                   {
                       command.Parameters.AddWithValue(@"ID", projectId);
                       command.Parameters.AddWithValue(@"PercentComplete", percentComplete);
                       command.Parameters.AddWithValue(@"User", currentUser);
                   });

            var data = projectId;


        }


    }
}
