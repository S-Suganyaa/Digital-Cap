using Dapper;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.Survey;
using DigitalCap.Core.Models.VesselModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        public async Task CreateVesselMainDataAsync(string classNumber, Project project, Core.Models.VesselModel.Vessel vessel)
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

        public async Task<Core.Models.Survey.Certificates> GetStatutoryCertificate(int projectId)
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
        private static DateTime? Pick(DateTime? newValue, DateTime? oldValue)
        {
            if (newValue != null && newValue != DateTime.MinValue)
                return newValue;

            if (oldValue != null && oldValue != DateTime.MinValue)
                return oldValue;

            return null;
        }
        private static DateTime? Normalize(DateTime? date)
        {
            return date == null || date == DateTime.MinValue ? null : date;
        }
        public async Task<bool> UpdateStatutoryCertificate(int projectId, Core.Models.Survey.Report report)
        {
            var data = new Core.Models.Survey.ReportStatutoryCertificates();

            var result = await Connection.QueryAsync<Core.Models.Survey.ReportStatutoryCertificates>(
                sql: "dbo.GetStatutoryCertificates",
                new { @projectId = projectId },
                commandType: CommandType.StoredProcedure,
                transaction: Transaction);
            data = result.FirstOrDefault();


            if (data == null)
            {
                await Connection.ExecuteAsync(
                    sql: "dbo.Insert_ReportStatutoryCertificates",
                    new
                    {
                        projectId,

                        llliIssueDate = Normalize(report.StatutoryCertificatesIssuedDates?.LLLIDate),
                        safetyConIssueDate = Normalize(report.StatutoryCertificatesIssuedDates?.SafetyConDate),
                        safetyRadioIssueDate = Normalize(report.StatutoryCertificatesIssuedDates?.SafetyRadioDate),
                        safetyEquipmentIssueDate = Normalize(report.StatutoryCertificatesIssuedDates?.SafetyEquipmentDate),
                        ioppIssueDate = Normalize(report.StatutoryCertificatesIssuedDates?.IOPPDate),
                        iapIssueDate = Normalize(report.StatutoryCertificatesIssuedDates?.IAPDate),
                        iafsIsuueDate = Normalize(report.StatutoryCertificatesIssuedDates?.IAFSDate),
                        ismIssueDate = Normalize(report.StatutoryCertificatesIssuedDates?.ISMDate),
                        ispsIssueDate = Normalize(report.StatutoryCertificatesIssuedDates?.ISPSDate),
                        cargoGearReTestIssueDate = Normalize(report.StatutoryCertificatesIssuedDates?.CargoGearReTestDate),
                        cargoGearAnnualIssueCertificate = Normalize(report.StatutoryCertificatesIssuedDates?.CargoGearAnnualCertificate),
                        ibwmIssueDate = Normalize(report.StatutoryCertificatesIssuedDates?.IBWMDate),
                        isppSewageIssueDate = Normalize(report.StatutoryCertificatesIssuedDates?.ISPPSewageDate),
                        maritimeLabourIssueDate = Normalize(report.StatutoryCertificatesIssuedDates?.MaritimeLabourDate),
                        ieeIssueDate = Normalize(report.StatutoryCertificatesIssuedDates?.IEEDate),
                        fitnessGasCertificateIssueDate = Normalize(report.StatutoryCertificatesIssuedDates?.GasCarrierFitness),

                        ieeExpiryDate = Normalize(report.StatutoryCertificatesExpirationDates?.IEEDate),
                        llliExpiryDate = Normalize(report.StatutoryCertificatesExpirationDates?.LLLIDate),
                        safetyConExpiryDate = Normalize(report.StatutoryCertificatesExpirationDates?.SafetyConDate),
                        safetyRadioExpiryDate = Normalize(report.StatutoryCertificatesExpirationDates?.SafetyRadioDate),
                        safetyEquipmentExpiryDate = Normalize(report.StatutoryCertificatesExpirationDates?.SafetyEquipmentDate),
                        cargoGearReTestExpiryDate = Normalize(report.StatutoryCertificatesExpirationDates?.CargoGearReTestDate),
                        cargoGearAnnualExpiryCertificate = Normalize(report.StatutoryCertificatesExpirationDates?.CargoGearAnnualCertificate),
                        ibwmExpiryDate = Normalize(report.StatutoryCertificatesExpirationDates?.IBWMDate),
                        isppSewageExpiryDate = Normalize(report.StatutoryCertificatesExpirationDates?.ISPPSewageDate),
                        maritimeLabourExpiryDate = Normalize(report.StatutoryCertificatesExpirationDates?.MaritimeLabourDate),
                        ioppExpiryDate = Normalize(report.StatutoryCertificatesExpirationDates?.IOPPDate),
                        iapExpiryDate = Normalize(report.StatutoryCertificatesExpirationDates?.IAPDate),
                        iafsExpiryDate = Normalize(report.StatutoryCertificatesExpirationDates?.IAFSDate),
                        ismExpiryDate = Normalize(report.StatutoryCertificatesExpirationDates?.ISMDate),
                        ispsExpiryDate = Normalize(report.StatutoryCertificatesExpirationDates?.ISPSDate),
                        fitnessGasCertificateExpiryDate = Normalize(report.StatutoryCertificatesExpirationDates?.GasCarrierFitness)

                    },
                    commandType: CommandType.StoredProcedure,
                    transaction: Transaction
                );
            }

            else
            {
                await Connection.ExecuteAsync(
      sql: "dbo.Update_ReportStatutoryCertificates",
      new
      {
          projectId,

          ieeIssueDate = Pick(report.StatutoryCertificatesIssuedDates?.IEEDate, data.ieeIssueDate),
          llliIssueDate = Pick(report.StatutoryCertificatesIssuedDates?.LLLIDate, data.llliIssueDate),
          safetyConIssueDate = Pick(report.StatutoryCertificatesIssuedDates?.SafetyConDate, data.safetyConIssueDate),
          safetyRadioIssueDate = Pick(report.StatutoryCertificatesIssuedDates?.SafetyRadioDate, data.safetyRadioIssueDate),
          safetyEquipmentIssueDate = Pick(report.StatutoryCertificatesIssuedDates?.SafetyEquipmentDate, data.safetyEquipmentIssueDate),
          ioppIssueDate = Pick(report.StatutoryCertificatesIssuedDates?.IOPPDate, data.ioppIssueDate),
          iapIssueDate = Pick(report.StatutoryCertificatesIssuedDates?.IAPDate, data.iapIssueDate),
          iafsIsuueDate = Pick(report.StatutoryCertificatesIssuedDates?.IAFSDate, data.iafsIsuueDate),
          ismIssueDate = Pick(report.StatutoryCertificatesIssuedDates?.ISMDate, data.ismIssueDate),
          ispsIssueDate = Pick(report.StatutoryCertificatesIssuedDates?.ISPSDate, data.ispsIssueDate),
          cargoGearReTestIssueDate = Pick(report.StatutoryCertificatesIssuedDates?.CargoGearReTestDate, data.cargoGearReTestIssueDate),
          cargoGearAnnualIssueCertificate = Pick(report.StatutoryCertificatesIssuedDates?.CargoGearAnnualCertificate, data.cargoGearAnnualIssueCertificate),
          ibwmIssueDate = Pick(report.StatutoryCertificatesIssuedDates?.IBWMDate, data.ibwmIssueDate),
          isppSewageIssueDate = Pick(report.StatutoryCertificatesIssuedDates?.ISPPSewageDate, data.isppSewageIssueDate),
          maritimeLabourIssueDate = Pick(report.StatutoryCertificatesIssuedDates?.MaritimeLabourDate, data.maritimeLabourIssueDate),
          fitnessGasCertificateIssueDate = Pick(report.StatutoryCertificatesIssuedDates?.GasCarrierFitness, data.fitnessGasCertificateIssueDate),

          ieeExpiryDate = Pick(report.StatutoryCertificatesExpirationDates?.IEEDate, data.ieeExpiryDate),
          llliExpiryDate = Pick(report.StatutoryCertificatesExpirationDates?.LLLIDate, data.llliExpiryDate),
          safetyConExpiryDate = Pick(report.StatutoryCertificatesExpirationDates?.SafetyConDate, data.safetyConExpiryDate),
          safetyRadioExpiryDate = Pick(report.StatutoryCertificatesExpirationDates?.SafetyRadioDate, data.safetyRadioExpiryDate),
          safetyEquipmentExpiryDate = Pick(report.StatutoryCertificatesExpirationDates?.SafetyEquipmentDate, data.safetyEquipmentExpiryDate),
          cargoGearReTestExpiryDate = Pick(report.StatutoryCertificatesExpirationDates?.CargoGearReTestDate, data.cargoGearReTestExpiryDate),
          cargoGearAnnualExpiryCertificate = Pick(report.StatutoryCertificatesExpirationDates?.CargoGearAnnualCertificate, data.cargoGearAnnualExpiryCertificate),
          ibwmExpiryDate = Pick(report.StatutoryCertificatesExpirationDates?.IBWMDate, data.ibwmExpiryDate),
          isppSewageExpiryDate = Pick(report.StatutoryCertificatesExpirationDates?.ISPPSewageDate, data.isppSewageExpiryDate),
          maritimeLabourExpiryDate = Pick(report.StatutoryCertificatesExpirationDates?.MaritimeLabourDate, data.maritimeLabourExpiryDate),
          ioppExpiryDate = Pick(report.StatutoryCertificatesExpirationDates?.IOPPDate, data.ioppExpiryDate),
          iapExpiryDate = Pick(report.StatutoryCertificatesExpirationDates?.IAPDate, data.iapExpiryDate),
          iafsExpiryDate = Pick(report.StatutoryCertificatesExpirationDates?.IAFSDate, data.iafsExpiryDate),
          ismExpiryDate = Pick(report.StatutoryCertificatesExpirationDates?.ISMDate, data.ismExpiryDate),
          ispsExpiryDate = Pick(report.StatutoryCertificatesExpirationDates?.ISPSDate, data.ispsExpiryDate),
          fitnessGasCertificateExpiryDate = Pick(report.StatutoryCertificatesExpirationDates?.GasCarrierFitness, data.fitnessGasCertificateExpiryDate)
      },
      commandType: CommandType.StoredProcedure,
      transaction: Transaction
  );

            }

            return true;
        }

        public async Task<bool> DeleteStatutoryCertificate(int projectId)
        {
            try
            {
                await Connection.ExecuteAsync(
               sql: "dbo.Delete_Statutorycertificates",
               param: new
               {
                   projectId = projectId,
               },
               commandType: CommandType.StoredProcedure,
               transaction: Transaction);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static DateTime? ResolveDate(DateTime? input, DateTime? existing)
        {
            if (input != null)
                return input == DateTime.MinValue ? null : input;

            return existing == DateTime.MinValue ? null : existing;
        }
        public async Task<bool> UpdateSurveyAudit(Core.Models.Survey.SurveyStatus surveyStatus, int projectId)
        {
            var data = this.GetSurveyStatus(projectId).Result;
            if (data.Count() == 0)
            {
                try
                {
                    await Connection.ExecuteAsync(
                     sql: "dbo.Insert_SurveyDetails",
                     new
                     {
                         projectId,
                         classSurveyDate = surveyStatus.ClassSurveyDate,
                         dryDockDate = surveyStatus.DryDockDate,
                         specialContinuousMachineryDate = surveyStatus.SpecialContinuousMachineryDate != DateTime.MinValue ? surveyStatus.SpecialContinuousMachineryDate : null,
                         boilerDate = surveyStatus.BoilerDate != DateTime.MinValue ? surveyStatus.BoilerDate : null,
                         tailshaftDate = surveyStatus.TailshaftDate != DateTime.MinValue ? surveyStatus.TailshaftDate : null,
                         specialContinuousHull1 = surveyStatus.SpecialContinuousHull1 != DateTime.MinValue ? surveyStatus.SpecialContinuousHull1 : null,
                         specialContinuousHull2 = surveyStatus.SpecialContinuousHull2 != DateTime.MinValue ? surveyStatus.SpecialContinuousHull2 : null,
                         specialContinuousHull3 = surveyStatus.SpecialContinuousHull3 != DateTime.MinValue ? surveyStatus.SpecialContinuousHull3 : null,
                         specialContinuousHull4 = surveyStatus.SpecialContinuousHull4 != DateTime.MinValue ? surveyStatus.SpecialContinuousHull4 : null,
                         specialContinuousHull5 = surveyStatus.SpecialContinuousHull5 != DateTime.MinValue ? surveyStatus.SpecialContinuousHull5 : null,
                         specialContinuousHull6 = surveyStatus.SpecialContinuousHull6 != DateTime.MinValue ? surveyStatus.SpecialContinuousHull6 : null,
                         specialContinuousHull7 = surveyStatus.SpecialContinuousHull7 != DateTime.MinValue ? surveyStatus.SpecialContinuousHull7 : null,
                         specialContinuousHull8 = surveyStatus.SpecialContinuousHull8 != DateTime.MinValue ? surveyStatus.SpecialContinuousHull8 : null,
                         specialContinuousHull9 = surveyStatus.SpecialContinuousHull9 != DateTime.MinValue ? surveyStatus.SpecialContinuousHull9 : null,
                         specialContinuousHull10 = surveyStatus.SpecialContinuousHull10 != DateTime.MinValue ? surveyStatus.SpecialContinuousHull10 : null,
                         specialContinuousHull11 = surveyStatus.SpecialContinuousHull11 != DateTime.MinValue ? surveyStatus.SpecialContinuousHull11 : null,
                         specialContinuousHull12 = surveyStatus.SpecialContinuousHull12 != DateTime.MinValue ? surveyStatus.SpecialContinuousHull12 : null,
                         specialContinuousHull13 = surveyStatus.SpecialContinuousHull13 != DateTime.MinValue ? surveyStatus.SpecialContinuousHull13 : null,
                         specialContinuousHull14 = surveyStatus.SpecialContinuousHull14 != DateTime.MinValue ? surveyStatus.SpecialContinuousHull14 : null,
                         specialContinuousHull15 = surveyStatus.SpecialContinuousHull15 != DateTime.MinValue ? surveyStatus.SpecialContinuousHull15 : null,
                         specialHullNo = surveyStatus.SpecialHullNo
                     },

                          commandType: CommandType.StoredProcedure,
                          transaction: Transaction
                        );
                }
                catch (Exception ex)
                { }

            }
            else
            {
                try
                {
                    var surveyStatusData = data.FirstOrDefault();

                    await Connection.ExecuteAsync(
                        sql: "dbo.Update_SurveyDetails",
                        param: new
                        {
                            ProjectId = projectId,

                            ClassSurveyDate = ResolveDate(surveyStatus.ClassSurveyDate, surveyStatusData.ClassSurveyDate),
                            DryDockDate = ResolveDate(surveyStatus.DryDockDate, surveyStatusData.DryDockDate),
                            SpecialContinuousMachineryDate = ResolveDate(surveyStatus.SpecialContinuousMachineryDate, surveyStatusData.SpecialContinuousMachineryDate),
                            BoilerDate = ResolveDate(surveyStatus.BoilerDate, surveyStatusData.BoilerDate),
                            TailshaftDate = ResolveDate(surveyStatus.TailshaftDate, surveyStatusData.TailshaftDate),
                            SpecialContinuousHull1 = ResolveDate(surveyStatus.SpecialContinuousHull1, surveyStatusData.SpecialContinuousHull1),
                            SpecialContinuousHull2 = ResolveDate(surveyStatus.SpecialContinuousHull2, surveyStatusData.SpecialContinuousHull2),
                            SpecialContinuousHull3 = ResolveDate(surveyStatus.SpecialContinuousHull3, surveyStatusData.SpecialContinuousHull3),
                            SpecialContinuousHull4 = ResolveDate(surveyStatus.SpecialContinuousHull4, surveyStatusData.SpecialContinuousHull4),
                            SpecialContinuousHull5 = ResolveDate(surveyStatus.SpecialContinuousHull5, surveyStatusData.SpecialContinuousHull5),
                            SpecialContinuousHull6 = ResolveDate(surveyStatus.SpecialContinuousHull6, surveyStatusData.SpecialContinuousHull6),
                            SpecialContinuousHull7 = ResolveDate(surveyStatus.SpecialContinuousHull7, surveyStatusData.SpecialContinuousHull7),
                            SpecialContinuousHull8 = ResolveDate(surveyStatus.SpecialContinuousHull8, surveyStatusData.SpecialContinuousHull8),
                            SpecialContinuousHull9 = ResolveDate(surveyStatus.SpecialContinuousHull9, surveyStatusData.SpecialContinuousHull9),
                            SpecialContinuousHull10 = ResolveDate(surveyStatus.SpecialContinuousHull10, surveyStatusData.SpecialContinuousHull10),
                            SpecialContinuousHull11 = ResolveDate(surveyStatus.SpecialContinuousHull11, surveyStatusData.SpecialContinuousHull11),
                            SpecialContinuousHull12 = ResolveDate(surveyStatus.SpecialContinuousHull12, surveyStatusData.SpecialContinuousHull12),
                            SpecialContinuousHull13 = ResolveDate(surveyStatus.SpecialContinuousHull13, surveyStatusData.SpecialContinuousHull13),
                            SpecialContinuousHull14 = ResolveDate(surveyStatus.SpecialContinuousHull14, surveyStatusData.SpecialContinuousHull14),
                            SpecialContinuousHull15 = ResolveDate(surveyStatus.SpecialContinuousHull15, surveyStatusData.SpecialContinuousHull15),
                            SpecialHullNo = surveyStatus.SpecialHullNo != 0 ? surveyStatus.SpecialHullNo : surveyStatusData.SpecialHullNo == 0 ? null : surveyStatusData.SpecialHullNo
                        },
                        commandType: CommandType.StoredProcedure,
                        transaction: Transaction
                    );
                }
                catch
                {
                    // log error
                }

            }

            return true;
        }

    }
}
