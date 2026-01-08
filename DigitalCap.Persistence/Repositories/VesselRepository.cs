using Dapper;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.Survey;
using DigitalCap.Core.Models.VesselModel;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Transactions;

namespace DigitalCap.Persistence.Repositories
{
    public class VesselRepository : IVesselRepository
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected IDbConnection Connection => _unitOfWork?.Connection!;
        protected IDbTransaction Transaction => _unitOfWork?.Transaction!;
        public async Task CreateVesselMainDataAsync(string classNumber,Project project,Core.Models.VesselModel.Vessel vessel)
        {
            try
            {
                var parameters = new DynamicParameters();

                parameters.Add("@projectId", project.ID);
                parameters.Add("@classNo", project.AbsClassID);
                parameters.Add("@builtBy", project.Builder);
                parameters.Add("@vesselName", project.VesselName);
                parameters.Add("@length", project.LengthOverall);
                parameters.Add("@yearBuilt", project.YearBuilt);
                parameters.Add("@hullNo", project.HullNumber);
                parameters.Add("@imoNumber", project.IMO);
                parameters.Add("@classed", project.ClassSociety[0]);

                parameters.Add("@firstVisitDate",
                    project.SurveyFirstVisit != null &&
                    project.SurveyFirstVisit != DateTime.MinValue
                        ? project.SurveyFirstVisit.Value.ToString("dd-MM-yyyy")
                        : DateTime.Now.ToString("dd-MM-yyyy"));

                parameters.Add("@lastVisitDate",
                    project.SurveyLastVisit != null &&
                    project.SurveyLastVisit != DateTime.MinValue
                        ? project.SurveyLastVisit.Value.ToString("dd-MM-yyyy")
                        : DateTime.Now.ToString("dd-MM-yyyy"));

                // Optional vessel data
                if (vessel != null)
                {
                    parameters.Add("@vesselID", vessel.ImoNumber);
                    parameters.Add("@vesselType", vessel.VesselType);
                    parameters.Add("@flagName", vessel.FlagName);
                    parameters.Add("@homeport", vessel.PortRegistry);
                    parameters.Add("@officalNumber", vessel.OfficialNumber);
                    parameters.Add("@callSign", vessel.CallSign);
                    parameters.Add("@ownerName", vessel.RegisteredOwner.Name);
                    parameters.Add("@breadth", vessel?.BreadthMolded?.Value);
                    parameters.Add("@depth", vessel?.DepthMolded?.Value);
                    parameters.Add("@deadweight", vessel?.DesignDeadWeight?.Value);
                    parameters.Add("@netTons", vessel?.NetTonnage?.Value);
                    parameters.Add("@classNotation", vessel?.ClassNotation);
                }

                await Connection.ExecuteAsync(
                    sql: "[dbo].[Insert_VesselDetails]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction);
            }
            catch
            {
                throw;
            }
        }
        public async Task<IEnumerable<SurveyStatus>> GetSurveyStatus(int projectId)
        {

            try
            {

                var result = await Connection.QueryAsync<SurveyStatus>(
                    sql: "dbo.GetSurveyStatus",
                    new { projectId = projectId },
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction);

                return result;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> DeleteSurveyAudit(int projectId)
        {
            try
            {
                await Connection.ExecuteAsync(
                    sql: "dbo.Delete_SurveyStatus",
                    param: new
                    {
                        projectId = projectId
                    },
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction
                    );
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public async Task<IEnumerable<ReportVesselMainData>> GetVesselMainData(int projectId)
        {
            var result = await Connection.QueryAsync<ReportVesselMainData>(
                   sql: "CAP.GetVesselMainData",
                   new { projectId = projectId },
                   commandType: CommandType.StoredProcedure,
                   transaction: Transaction);

            return result;
        }

        public async Task<Core.Models.Survey.Certificates>GetStatutoryCertificate(int projectId)
        {
            var data =
                await Connection.QueryFirstOrDefaultAsync<
                    Core.Models.Survey.ReportStatutoryCertificates>(
                    sql: "dbo.GetStatutoryCertificates",
                    param: new { ProjectId = projectId },
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction
                );

            var issued = new Core.Models.Survey.StatutoryCertificates();
            var expiry = new Core.Models.Survey.StatutoryCertificates();

            if (data != null)
            {
                issued.LLLIDate = (data.llliIssueDate);
                expiry.LLLIDate = (data.llliExpiryDate);

                issued.SafetyConDate = (data.safetyConIssueDate);
                expiry.SafetyConDate = (data.safetyConExpiryDate);

                issued.SafetyRadioDate = (data.safetyRadioIssueDate);
                expiry.SafetyRadioDate = (data.safetyRadioExpiryDate);

                issued.SafetyEquipmentDate = (data.safetyEquipmentIssueDate);
                expiry.SafetyEquipmentDate = (data.safetyEquipmentExpiryDate);

                issued.IEEDate = (data.ieeIssueDate);
                expiry.IEEDate = (data.ieeExpiryDate);

                issued.IOPPDate = (data.ioppIssueDate);
                expiry.IOPPDate = (data.ioppExpiryDate);

                issued.IAPDate = (data.iapIssueDate);
                expiry.IAPDate = (data.iapExpiryDate);

                issued.IAFSDate = (data.iafsIsuueDate);
                expiry.IAFSDate = (data.iafsExpiryDate);

                issued.ISPSDate = (data.ispsIssueDate);
                expiry.ISPSDate = (data.ispsExpiryDate);

                issued.CargoGearReTestDate = (data.cargoGearReTestIssueDate);
                expiry.CargoGearReTestDate = (data.cargoGearReTestExpiryDate);

                issued.CargoGearAnnualCertificate =
                    (data.cargoGearAnnualIssueCertificate);
                expiry.CargoGearAnnualCertificate =
                    (data.cargoGearAnnualExpiryCertificate);

                issued.IBWMDate = (data.ibwmIssueDate);
                expiry.IBWMDate = (data.ibwmExpiryDate);

                issued.ISPPSewageDate = (data.isppSewageIssueDate);
                expiry.ISPPSewageDate = (data.isppSewageExpiryDate);

                issued.ISMDate = (data.ismIssueDate);
                expiry.ISMDate = (data.ismExpiryDate);

                issued.MaritimeLabourDate = (data.maritimeLabourIssueDate);
                expiry.MaritimeLabourDate = (data.maritimeLabourExpiryDate);

                issued.GasCarrierFitness =
                    (data.fitnessGasCertificateIssueDate);
                expiry.GasCarrierFitness =
                    (data.fitnessGasCertificateExpiryDate);
            }

            return new Core.Models.Survey.Certificates
            {
                StatutoryCertificatesIssuedDates = issued,
                StatutoryCertificatesExpirationDates = expiry
            };
        }

    }
}
