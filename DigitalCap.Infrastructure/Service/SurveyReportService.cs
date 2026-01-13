using DigitalCap.Core.Enumerations;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.Grading;
using DigitalCap.Core.Models.Permissions;
using DigitalCap.Core.Models.ReportConfig;
using DigitalCap.Core.Models.Skylight;
using DigitalCap.Core.Models.Survey;
using DigitalCap.Core.Models.Tank;
using DigitalCap.Core.ViewModels;
using DigitalCap.Infrastructure.Service;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Pipelines;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.Streaming;
using Telerik.Windows.Documents.Fixed.Model.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Collections.Specialized.BitVector32;

namespace DigitalCap.Infrastructure.Service
{
    public class SurveyReportService : ISurveyReportService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SurveyReportService> _logger;
        private readonly IUserPermissionRepository _userPermissionRepository;
        private readonly ITransferDataOnlinetoOfflineRepository _transferDataOnlinetoOfflineRepository;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly ISecurityService _securityService;
        private readonly ISurveyReportRepository _surveyReportRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ITankRepository _tankRepository;
        private readonly IVesselRepository _vesselRepository;
        private readonly IDescriptionRepository _descriptionRepository;
        private readonly IFreedomAPIRepository _freedomAPIRepository;
        private readonly IProjectReportRepository _projectReportRepository;
        public SurveyReportService(ILogger<SurveyReportService> logger, IConfiguration configuration,
            IUserPermissionRepository userPermissionRepository, ITransferDataOnlinetoOfflineRepository transferDataOnlinetoOfflineRepository,
            IUserAccountRepository userAccountRepository, ISecurityService securityService,
            ISurveyReportRepository surveyReportRepository, IProjectRepository projectRepository,
            ITankRepository tankRepository, IVesselRepository vesselRepository, IDescriptionRepository descriptionRepository
            , IFreedomAPIRepository freedomAPIRepository, IProjectReportRepository projectReportRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _userPermissionRepository = userPermissionRepository;
            _transferDataOnlinetoOfflineRepository = transferDataOnlinetoOfflineRepository;
            _userAccountRepository = userAccountRepository;
            _securityService = securityService;
            _surveyReportRepository = surveyReportRepository;
            _projectRepository = projectRepository;
            _tankRepository = tankRepository;
            _vesselRepository = vesselRepository;
            _descriptionRepository = descriptionRepository;
            _freedomAPIRepository = freedomAPIRepository;
            _projectReportRepository = projectReportRepository;
        }
        public async Task<ServiceResult<SurveyReportViewModel>> GetIndexAsync(int projectId, string currentUserName, int? templateSectionId = null)
        {
            // validation
            if (string.IsNullOrWhiteSpace(currentUserName))
            {
                return ServiceResult<SurveyReportViewModel>
                    .Failure("Invalid user");
            }

            if (projectId < _configuration.GetValue<int>("DefaultTemplateProjectId"))
            {
                return ServiceResult<SurveyReportViewModel>
                    .Failure("Invalid project");
            }

            // permission check
            var reportAccess =
                await _userPermissionRepository.GetRolePermissionByUserName(
                    currentUserName,
                    EnumExtensions.GetDescription(ManagePages.ReportEdit),
                    projectId);

            if (!(Convert.ToBoolean(reportAccess?.Edit) ||
                  Convert.ToBoolean(reportAccess?.Read)))
            {
                return ServiceResult<SurveyReportViewModel>
                    .Failure("Access denied");
            }

            // sync info
            var isSyncResult = await _transferDataOnlinetoOfflineRepository.GetDownloadOfflineProjects(projectId);

            //if (isSyncResult?.IsSuccess == true && isSyncResult.Data != null)
            //{
            //    var userAccount =
            //        await _userAccountRepository
            //            .GetByAspNetId(isSyncResult.Data.UserId);

            //    if (userAccount != null)
            //    {
            //        isSyncResult.Data.Name =
            //            $"{userAccount.FirstName} {userAccount.LastName}";
            //    }
            //}

            // prepare model
            var model = await PrepareSurveyReportViewModel(projectId, templateSectionId);


            // permissions
            model.ReportExport =
                (await _userPermissionRepository.GetRolePermissionByUserName(
                    currentUserName,
                    EnumExtensions.GetDescription(ManagePages.ReportExport),
                    projectId))?.Edit;

            model.ReportExportAll =
                (await _userPermissionRepository.GetRolePermissionByUserName(
                    currentUserName,
                    EnumExtensions.GetDescription(ManagePages.ReportExportAll),
                    projectId))?.Edit;

            model.ResetTemplate =
                (await _userPermissionRepository.GetRolePermissionByUserName(
                    currentUserName,
                    EnumExtensions.GetDescription(ManagePages.ResetTemplate),
                    projectId))?.Edit;

            model.IsSynched = isSyncResult?.IsSynched ?? true;
            model.Report.IsSynched = isSyncResult?.IsSynched ?? true;
            model.SynchedOnline = isSyncResult;

            return ServiceResult<SurveyReportViewModel>.Success(model);
        }

        public async Task<ServiceResult<Report>> IndexPartial(int projectId, int? templateSectionId = null)
        {
            var surveyReport = PrepareSurveyReportViewModel(projectId, null).Result;
            var isReportExist = surveyReport.ReportPartGrid.Where(x => x.TemplateId == templateSectionId).Select(x => x.ReportPartExists).FirstOrDefault();
            var isSync = await _transferDataOnlinetoOfflineRepository.GetDownloadOfflineProjects(projectId);
            if (!isReportExist)
            {
                var isCreated = CreateReport(projectId, templateSectionId.Value).Result;
            }

            SurveyReportViewModel model = await PrepareSurveyReportViewModel(projectId, templateSectionId);
            model.IsSynched = isSync == null ? true : isSync.IsSynched;
            model.Report.IsSynched = isSync == null ? true : isSync.IsSynched;
            return ServiceResult<Report>.Success(model.Report);
        }

        private async Task<SurveyReportViewModel> PrepareSurveyReportViewModel(int projectId, int? templateSectionId)
        {
            var sectionResult =
                await GetSectionIdsByProjectId(projectId, templateSectionId);


            var model = sectionResult.Data;

            model.ProjectName =
                await _projectRepository.GetProjectName(projectId);

            if (templateSectionId.HasValue)
            {
                var reportPart = model.ReportPartGrid
                    .FirstOrDefault(x => x.TemplateId == templateSectionId.Value);

                if (reportPart != null)
                {
                    var sectionIds = reportPart.SectionIds
                        .Select(x => x.ToString())
                        .ToList();

                    model.Report = await GetReport(projectId, templateSectionId.Value, sectionIds);
                }
            }
            else
            {
                model.UnplacedImages = await _surveyReportRepository.GetBulkUploadUnplacedImages();
            }

            return model;
        }
        public async Task<ServiceResult<bool>> CreateReport(int projectId, int templateSectionId)
        {
            try
            {
                var project = _projectRepository.GetProject(projectId).Result;
                var templatesections = _surveyReportRepository.GetTemplatSections(templateSectionId).Result;
                foreach (var section in templatesections)
                {
                    ProjectReportMapping projectReportMapping = new ProjectReportMapping();
                    projectReportMapping.Id = Guid.NewGuid();
                    projectReportMapping.ProjectId = projectId;
                    projectReportMapping.TemplateId = templateSectionId;
                    projectReportMapping.IsDeleted = false;
                    projectReportMapping.CreatedDttm = DateTime.Now;
                    projectReportMapping.SectionId = section.Id;
                    projectReportMapping.UpdateDttm = DateTime.Now;
                    var result = await _surveyReportRepository.CreateProjectTemplate(projectReportMapping);
                }


                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure("false");
            }

        }

        public async Task<bool> DeleteReport(int projectId, int templateSectionId)
        {

            var result = _surveyReportRepository.DeleteProjectTemplate(projectId, templateSectionId).Result;

            return result;
        }
        public async Task<ServiceResult<SurveyReportViewModel>> GetSectionIdsByProjectId(int projectId, int? templateId)
        {
            var model = new SurveyReportViewModel();
            model.ProjectId = projectId;
            model.Report = new Report();
            //model.Report.template = new View();

            var existingReportParts = await _surveyReportRepository.GetExistingReportPartsSectionIds(projectId);
            var allSectionTemplates = await _surveyReportRepository.GetReportTemplateList();
            //var allsectionIdList = await _surveyReportRepository.GetProjectSectionsId(projectId);
            model.ReportPartGrid = new List<ReportPart>();
            var project = _projectRepository.GetProject(projectId).Result;
            if (templateId != null)
                allSectionTemplates = allSectionTemplates.Where(x => x.Id == templateId.Value).ToList();
            foreach (var template in allSectionTemplates)
            {
                List<GradingSection> Sections = new List<GradingSection>();
                var sections = _surveyReportRepository.GetTemplateSections(template.Id).Result;
                if (template.Id == 2 || template.Id == 3 || template.Id == 4)
                {
                    sections = sections.Where(x => x.HasSubSection == false).ToList();
                    if (sections.Count > 0)
                        Sections.AddRange(sections.Select(x => new GradingSection() { SectionId = x.SectionId, SectionName = x.SectionName }));
                    var tanks = _tankRepository.GetTemplateTanks(template.Id, project?.IMO.ToString(), null, projectId).Result;
                    Sections.AddRange(tanks?.Select(x => new GradingSection { SectionId = x.Id, SectionName = x.TankName }).ToList());
                }
                else
                {
                    Sections.AddRange(sections.Select(x => new GradingSection { SectionId = x.Id, SectionName = x.SectionName }).ToList());
                }

                var reportPart = new ReportPart()
                {
                    TemplateId = template.Id,
                    Name = template.Name,
                    ReportPartExists = existingReportParts.Contains(template.Id),
                    SectionIds = Sections.Select(x => x.SectionId).ToList(),
                    SectionsList = Sections
                };
                model.ReportPartGrid.Add(reportPart);
            }
            return ServiceResult<SurveyReportViewModel>.Success(model);
        }

        public async Task<Report> GetReport(int projectId, int templateSectionId, List<string> sectionIds)
        {

            try
            {
                var retApp = new Report();

                retApp.ProjectName = await _projectRepository.GetProjectName(projectId);


                retApp.Vessel = new Core.Models.Survey.Vessel();
                var ReportVessel = new ReportVesselMainData();
                //Get CERTIFICATE USING CLASSNUMBER
                try
                {
                    ReportVessel = _vesselRepository.GetVesselMainData(projectId).Result.First();
                }
                catch (Exception ex)
                { }
                if (ReportVessel != null)
                {
                    retApp.ReportPort = ReportVessel.reportPort;
                    retApp.ReportDate = ReportVessel.portDate != null && Convert.ToDateTime(ReportVessel.portDate) != DateTime.MinValue ? Convert.ToDateTime(ReportVessel.portDate).ToShortDateString() : DateTime.Now.ToString("dd-MM-yyy");

                    retApp.ReportNo = ReportVessel.reportNo;
                    retApp.Vessel.ReportNo = ReportVessel.reportNo;
                    if (ReportVessel.actualReportStartDate != null && Convert.ToDateTime(ReportVessel.actualReportStartDate).Date != DateTime.MinValue.Date)
                    {
                        retApp.Vessel.ActualReportStartDate = Convert.ToDateTime(ReportVessel.actualReportStartDate);
                    }
                    retApp.ReportPort = ReportVessel.reportPort;
                    retApp.Vessel.Classed = ReportVessel.classed;
                    retApp.Vessel.VesselName = ReportVessel.vesselName;
                    retApp.Vessel.VesselID = ReportVessel.vesselID;
                    retApp.Vessel.ImoNumber = ReportVessel.imoNumber;
                    retApp.Vessel.VesselType = ReportVessel.vesselType;
                    retApp.Vessel.YearBuilt = Convert.ToInt32(ReportVessel.yearBuilt);
                    retApp.Vessel.BuiltBy = ReportVessel.builtBy;
                    retApp.Vessel.HullNo = ReportVessel.hullNo;
                    retApp.Vessel.FlagName = ReportVessel.flagName;
                    retApp.Vessel.Homeport = ReportVessel.homeport;
                    retApp.Vessel.OfficalNumber = ReportVessel.officalNumber;
                    retApp.Vessel.CallSign = ReportVessel.callSign;
                    retApp.Vessel.PreviousName = ReportVessel.previousName;
                    retApp.Vessel.OwnerName = ReportVessel.ownerName;
                    retApp.Vessel.Manager = ReportVessel.manager;
                    retApp.Vessel.Length = ReportVessel.length != null ? float.Parse(ReportVessel.length) : 0;
                    retApp.Vessel.Breadth = ReportVessel.breadth != null ? float.Parse(ReportVessel.breadth) : 0;
                    retApp.Vessel.DEPTH = ReportVessel.depth != null ? float.Parse(ReportVessel.depth) : 0;
                    retApp.Vessel.Draft = ReportVessel.draft != null ? float.Parse(ReportVessel.draft) : 0;
                    retApp.Vessel.DEADWEIGHT = ReportVessel.deadweight != null ? ReportVessel.deadweight : null;
                    retApp.Vessel.GrossTons = ReportVessel.grossTons != null ? float.Parse(ReportVessel.grossTons) : 0;
                    retApp.Vessel.NetTons = ReportVessel.netTons != null ? float.Parse(ReportVessel.netTons) : 0;
                    retApp.Vessel.PropulsionType = ReportVessel.propulsionType;
                    retApp.Vessel.KW = ReportVessel.kw != null ? ReportVessel.kw : "";
                    retApp.Vessel.ShaftRPM = ReportVessel.shaftRPM != null ? float.Parse(ReportVessel.shaftRPM) : 0;
                    retApp.Vessel.PropulsionManufacturer = ReportVessel.propulsionManufacturer;
                    retApp.Vessel.GearManufacturer = ReportVessel.gearManufacturer;
                    retApp.Vessel.Classed = ReportVessel.classed;
                    retApp.Vessel.ClassNo = ReportVessel.classNo;
                    retApp.Vessel.ClassNotation = ReportVessel.classNotation;
                    retApp.Vessel.Machinery = ReportVessel.machinery;
                    retApp.Vessel.Other = ReportVessel.other;
                    retApp.Vessel.Environment = ReportVessel.environment;
                    retApp.Vessel.SelectedPrefix = ReportVessel.selectedPrefix.ToString();
                    retApp.Vessel.FirstVisitDate = ReportVessel.firstVisitDate;
                    //retApp.Vessel.FirstVisitDate = ReportVessel.firstVisitDate != null  ? DateTime.ParseExact(ReportVessel.firstVisitDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("dd-MMM-yyyy") : "";

                    retApp.Vessel.LastVisitDate = ReportVessel.lastVisitDate;
                    //retApp.Vessel.LastVisitDate = ReportVessel.lastVisitDate != null  ? DateTime.ParseExact(ReportVessel.lastVisitDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("dd-MMM-yyyy") : "";

                }

                retApp.SurveyStatus = _vesselRepository.GetSurveyStatus(projectId).Result.FirstOrDefault();
                var certificates = _vesselRepository.GetStatutoryCertificate(projectId).Result;
                retApp.StatutoryCertificatesExpirationDates = certificates.StatutoryCertificatesExpirationDates;
                retApp.StatutoryCertificatesIssuedDates = certificates.StatutoryCertificatesIssuedDates;
                retApp.ProjectId = projectId;
                var projectydetails = _projectRepository.GetProject(projectId).Result;
                //********GetReportTemplate*******
                retApp.ProjectStatus = projectydetails.ProjectStatus;
                Core.Models.Survey.View newReport = new Core.Models.Survey.View();

                var templateDetails = _surveyReportRepository.GetTemplateTitle(templateSectionId).Result;
                newReport.Id = templateDetails.Id;
                newReport.ProjectId = projectId;
                newReport.TemplateTitle = templateDetails.SectionName;
                newReport.handireport = new HandIReport();
                var currentCondition = _surveyReportRepository.GetCurrentCondition().Result;
                var imagedescriptions = _descriptionRepository.GetImageDescriptionsByProjectId(projectId).Result;
                //  newReport.handireport.maintanceOptions =new MaintanceOptions() { new MaintanceOptions { Label = "Yes", Value = "Yes" }, new MaintanceOptions { Label = "No", Value = "No" }, new MaintanceOptions { Label = "Others", Value = "Others" }};

                var sections = _surveyReportRepository.GetTemplatSections(templateSectionId).Result;
                if (templateSectionId == 2 || templateSectionId == 3 || templateSectionId == 4)
                {

                    sections = sections.Where(x => x.HasSubSection == false).ToList();
                    retApp.tankReportView = new Core.Models.Tank.TankReportView();
                    retApp.tankReportView.ProjectId = projectId;
                    retApp.tankReportView.TemplateId = templateSectionId;
                    retApp.tankReportView.TemplateName = templateDetails.SectionName.Replace("Part ", String.Empty);
                    retApp.tankReportView.sectionStartCount = sections != null ? sections.Count() + 1 : 1;
                    retApp.tankReportView.Sections = new List<TankUI>();
                    retApp.tankReportView.VesselType = projectydetails.VesselType;
                    retApp.tankReportView.Sections = await _tankRepository.GetTemplateTanks(templateSectionId, projectydetails?.IMO.ToString(), null, projectId);
                    //foreach (var section in retApp.tankReportView.Sections)
                    //{
                    //    section.OrderNumber = sectionIds.IndexOf(section.Id.ToString()) + 1;
                    //    section.Grading = new List<TankGradingUI>();
                    //    section.Grading = await _tankService.GetTemplateTankGrading(section.TankTypeId, projectId, projectydetails.VesselType);
                    //    section.GradingLabelName = new List<String>();

                    //    var tankreportnotes = await this.GetProjectReportTemplate(projectId, templateSectionId, section.Id);
                    //    if (tankreportnotes != null && tankreportnotes.Notes != null)
                    //    {
                    //        section.SectionNotes = tankreportnotes.Notes;
                    //    }
                    //    foreach (var grading in section.Grading)
                    //    {
                    //        var templategradingnotes = await this.GetProjectGrading(projectId, templateSectionId, section.Id);
                    //        grading.Checkbox = new List<TankCheckBox>();
                    //        grading.Checkbox = await _tankService.GetTemplateTankGradingCondition(grading.Id);
                    //        if (section.GradingLabelName.Count < grading.Checkbox.Count)
                    //        {
                    //            section.GradingLabelName = grading.Checkbox.Select(x => x.Name).ToList();
                    //        }
                    //        var templategradingconditions = await this.GetProjectGradingConditionMapping(projectId, templateSectionId, section.Id, grading.Id);

                    //        if (templategradingnotes != null && templategradingnotes.Count > 0)
                    //        {
                    //            grading.Value = templategradingnotes.Where(x => x.GradingId == grading.Id).Select(x => x.Value).FirstOrDefault();
                    //        }


                    //        if (grading.Checkbox != null && grading.Checkbox.Count > 0)
                    //        {
                    //            if (templategradingconditions != null && templategradingconditions.Count > 0)
                    //            {
                    //                foreach (var gradingcheckbox in grading.Checkbox)
                    //                {
                    //                    gradingcheckbox.Value = templategradingconditions.Where(x => x.GradingConditionLabel == gradingcheckbox.Name).Select(x => x.GradingConditionValue).FirstOrDefault();
                    //                }
                    //            }
                    //        }
                    //    }
                    //    section.currentConditions = new List<CurrentCondition>();
                    //    section.currentConditions = currentCondition;
                    //    section.imageDescriptions = new List<ImageDescriptionUI>();
                    //    section.imageDescriptions = imagedescriptions.Where(x => x.TankTypeId == section.TankTypeId).Select(x => new ImageDescriptionUI { Id = x.Id, Description = x.Description }).ToList();

                    //    section.tankImageCards = new List<TankImageCard>();
                    //    section.tankImageCards = await _tankService.GetProjectTankImageCard(projectId, templateSectionId, section.Id);


                    //}
                    var temp_Grading = await _tankRepository.GetTemplateTankGrading(0, projectId, projectydetails.VesselType);
                    var tankreportNotes = await _surveyReportRepository.GetProjectReportTemplate(projectId, templateSectionId);
                    var imageCardsTask = await _tankRepository.GetProjectTankImageCard(projectId, templateSectionId, Guid.Empty);
                    var temp_gradingNotes = await _surveyReportRepository.GetProjectGrading(projectId, templateSectionId, Guid.Empty);

                    var temp_gradingConditionMapTask = await _surveyReportRepository.GetProjectGradingConditionMapping(projectId, templateSectionId, Guid.Empty, 0);
                    foreach (var section in retApp.tankReportView.Sections)
                    {

                        section.OrderNumber = sectionIds.IndexOf(section.Id.ToString()) + 1;

                        section.Grading = temp_Grading.Where(x => x.TankTypeId == section.TankTypeId).Select(x => new TankGradingUI
                        {
                            Id = x.Id,
                            Name = x.Name,
                            TankTypeId = x.TankTypeId,
                            Value = x.Value,
                            SectionId = section.Id
                        })
                        .ToList();

                        section.GradingLabelName = new List<string>();

                        var tankReportNotesTask = tankreportNotes.FirstOrDefault(x => x.SectionId == section.Id);
                        var tankImageCardsTask = imageCardsTask.Where(x => x.SectionId == section.Id).ToList();

                        var tankReportNotes = tankReportNotesTask;
                        if (tankReportNotes?.Notes != null)
                        {
                            section.SectionNotes = tankReportNotes.Notes;
                        }

                        var gradingNotesTask = temp_gradingNotes != null ? temp_gradingNotes.Where(x => x.SectionId == section.Id).ToList() : new List<ProjectGradingMapping>();
                        var gradingNotes = gradingNotesTask;
                        foreach (var grading in section.Grading)
                        {
                            // Get the checkbox templates once
                            grading.Checkbox = (await _tankRepository.GetTemplateTankGradingCondition(grading.Id)).ToList();

                            // Update section labels if needed
                            if (section.GradingLabelName.Count < grading.Checkbox.Count)
                            {
                                section.GradingLabelName = grading.Checkbox.Select(x => x.Name).ToList();
                            }

                            // Apply grading note value if available
                            if (gradingNotes?.Count > 0)
                            {
                                grading.Value = gradingNotes.FirstOrDefault(x => x.GradingId == grading.Id)?.Value;
                            }

                            // Map existing condition values to the checkboxes
                            var conditionMappings = temp_gradingConditionMapTask
                                .Where(x => x.SectionId == section.Id && x.GradingId == grading.Id)
                                .ToList();

                            if (grading.Checkbox?.Count > 0 && conditionMappings.Count > 0)
                            {
                                foreach (var checkbox in grading.Checkbox)
                                {
                                    var match = conditionMappings.FirstOrDefault(x => x.GradingConditionLabel == checkbox.Name);

                                    if (match != null && match.GradingConditionValue != null)
                                    {
                                        checkbox.Value = Convert.ToBoolean(match.GradingConditionValue);
                                    }
                                }
                            }
                        }


                        section.currentConditions = currentCondition;
                        section.imageDescriptions = imagedescriptions
                            .Where(x => x.TankTypeId == section.TankTypeId)
                            .Select(x => new ImageDescriptionUI { Id = x.Id, Description = x.Description })
                            .ToList();

                        section.tankImageCards = tankImageCardsTask;
                    }

                }
                var gradings = await _surveyReportRepository.GetTemplatGradings(templateSectionId, projectId);
                var imageCards = await _surveyReportRepository.GetImageCard(templateSectionId);
                //  var imgDescriptions = this.GetImageCardDescription(templateSectionId).Result;
                var gradingCondition = _surveyReportRepository.GetGradingCondition(templateSectionId, projectId).Result;

                ProjectReportMapping reportnotes = new ProjectReportMapping();
                List<ProjectGradingMapping> gradingnotes = new List<ProjectGradingMapping>();
                List<ProjectGradingConditionMapping> gradingconditions = new List<ProjectGradingConditionMapping>();
                List<ProjectCardMapping> projectcards = new List<ProjectCardMapping>();


                newReport.Sections = new List<Sections>();
                newReport.currentConditions = new List<CurrentCondition>();
                newReport.currentConditions = currentCondition;

                newReport.Sections.AddRange(sections.GroupBy(x => x.SectionName).ToList().Select(x => new Sections { SectionName = x.Key, Id = x.FirstOrDefault().SectionId, subSections = new List<SubSection>() }).ToList());
                if (newReport.Sections != null)
                {
                    foreach (var item in newReport.Sections)
                    {
                        item.subSections.AddRange(sections.Where(x => x.SectionId == item.Id).Select(x => new SubSection { Id = x.Id, SubSectionName = x.SubSectionName, Grading = new List<Core.Models.Survey.Grading>(), ImageCards = new List<Core.Models.Survey.ImageCards>(), GradingLabelName = new List<String>() }).ToList());

                    }
                    foreach (var item in newReport.Sections)
                    {

                        if (newReport.TemplateTitle != "Part H & I")
                        {
                            foreach (var subitem in item.subSections)
                            {
                                subitem.imageDescriptions = new List<ImageDescriptionUI>();
                                subitem.imageDescriptions = imagedescriptions.Where(x => x.SectionId == subitem.Id).Select(x => new ImageDescriptionUI { Id = x.Id, Description = x.Description }).ToList();
                                subitem.OrderNumber = sectionIds.IndexOf(subitem.Id.ToString()) + 1;
                                subitem.Grading.AddRange(gradings.Where(x => x.SectionId == subitem.Id).Select(x => new Core.Models.Survey.Grading { Id = x.Id, Name = x.LabelName, Checkbox = new List<CheckBox>() }).ToList());
                                reportnotes = _surveyReportRepository.GetProjectReportTemplate(projectId, templateSectionId, subitem.Id)?.Result;
                                if (reportnotes != null && reportnotes.Notes != null)
                                {
                                    subitem.SectionNotes = reportnotes.Notes;
                                }

                                subitem.projectCards = _surveyReportRepository.GetProjectImageCard(projectId, templateSectionId, subitem.Id)?.Result;
                                if (imageCards != null && imageCards.Count > 0)
                                {
                                    // projectcards = this.GetProjectImageCard(projectId, templateSectionId, subitem.Id)?.Result;
                                    subitem.ImageCards.AddRange(imageCards.Where(x => x.SectionId == subitem.Id).Select(x => new Core.Models.Survey.ImageCards { Id = x.Id, CurrentCondition = x.CurrentCondition, DescriptionOptions = new List<Description>(), FileName = x.FileName, CurrentConditionOptions = new List<CurrentCondition>() }).ToList());
                                    subitem.ImageCards = subitem.ImageCards.OrderBy(x => Convert.ToInt16(x.FileName.Substring(x.FileName.IndexOf("-") + 1))).ToList();
                                    //foreach (var cards in subitem.ImageCards)
                                    //{
                                    //    var data = projectcards.Where(x => x.CardId == cards.Id && x.IsDeleted == false).FirstOrDefault();
                                    //    cards.CurrentConditionOptions.AddRange(currentCondition);
                                    //    cards.DescriptionOptions.AddRange(imgDescriptions.Where(x => x.DescriptionGroupId == cards.DescriptionGroupId).Select(x => new Description { Id = x.Id, Name = x.Description }).ToList());
                                    //    if (data != null)
                                    //    {
                                    //        cards.descriptionvalue = data.DescriptionId;
                                    //        cards.AdditionalDescription = data.AdditionalDescription;
                                    //        cards.currentconditionvalue = data.CurrentCondition;
                                    //        cards.ImageId = data.ImageId;
                                    //        if (cards.ImageId != 0)
                                    //        {
                                    //            cards.src = "data:image/jpeg;base64," + this.GetImageById(cards.ImageId)?.Result;
                                    //        }

                                    //    }
                                    //}
                                }
                                if (gradingCondition != null && gradingCondition.Count > 0)
                                {
                                    gradingnotes = _surveyReportRepository.GetProjectGrading(projectId, templateSectionId, subitem.Id)?.Result;

                                    foreach (var grading in subitem.Grading)
                                    {
                                        gradingconditions = _surveyReportRepository.GetProjectGradingConditionMapping(projectId, templateSectionId, subitem.Id, grading.Id)?.Result;
                                        grading.Checkbox.AddRange(gradingCondition.Where(x => x.GradingId == grading.Id && x.SectionId == subitem.Id).Select(x => new CheckBox { Id = x.Id, Name = x.Condition }).ToList());
                                        if (subitem.GradingLabelName.Count < grading.Checkbox.Count)
                                        {
                                            subitem.GradingLabelName = grading.Checkbox.Select(x => x.Name).ToList();
                                        }
                                        if (grading.Checkbox != null && grading.Checkbox.Count > 0)
                                        {
                                            if (gradingconditions != null && gradingconditions.Count > 0)
                                            {
                                                foreach (var gradingcheckbox in grading.Checkbox)
                                                {
                                                    gradingcheckbox.Value = gradingconditions.Where(x => x.GradingConditionLabel == gradingcheckbox.Name).Select(x => x.GradingConditionValue).FirstOrDefault();
                                                }
                                            }
                                        }
                                        if (gradingnotes != null && gradingnotes.Count > 0)
                                        {
                                            grading.Value = gradingnotes.Where(x => x.GradingId == grading.Id).Select(x => x.Value).FirstOrDefault();
                                        }

                                    }
                                }


                            }



                        }
                        else
                        {
                            newReport.handireport = _surveyReportRepository.GetHIReport(projectId).Result;
                            if (newReport.handireport == null)
                            {
                                newReport.handireport = new HandIReport();
                            }
                            newReport.handireport.sectionOptions = new List<Options>() { new Options { Label = "Yes", Value = "Yes" }, new Options { Label = "No", Value = "No" }, new Options { Label = "Others", Value = "Others" } };
                            newReport.handireport.maintanenceOptions = new List<Options>() { new Options { Label = "Yes", Value = "Yes" }, new Options { Label = "No", Value = "No" }, new Options { Label = "N/A", Value = "N/A" } };
                            newReport.handireport.Grading = new List<Core.Models.Survey.Grading>();
                            newReport.handireport.GradingLabelName = new List<String>();
                            newReport.handireport.SectionId = newReport.Sections.FirstOrDefault().Id;
                            newReport.handireport.Grading.AddRange(gradings.Where(x => x.SectionId == newReport.Sections.FirstOrDefault().Id).Select(x => new Core.Models.Survey.Grading { Id = x.Id, Name = x.LabelName, Checkbox = new List<CheckBox>() }).ToList());
                            if (gradingCondition != null && gradingCondition.Count > 0)
                            {

                                gradingnotes = _surveyReportRepository.GetProjectGrading(projectId, templateSectionId, newReport.handireport.SectionId)?.Result;
                                foreach (var grading in newReport.handireport.Grading)
                                {

                                    gradingconditions = _surveyReportRepository.GetProjectGradingConditionMapping(projectId, templateSectionId, newReport.handireport.SectionId, grading.Id)?.Result;
                                    grading.Checkbox.AddRange(gradingCondition.Where(x => x.GradingId == grading.Id && x.SectionId == newReport.Sections.FirstOrDefault().Id).Select(x => new CheckBox { Id = x.Id, Name = x.Condition }).ToList());
                                    if (newReport.handireport.GradingLabelName.Count < grading.Checkbox.Count)
                                    {
                                        newReport.handireport.GradingLabelName = grading.Checkbox.Select(x => x.Name).ToList();
                                    }
                                    if (grading.Checkbox != null && grading.Checkbox.Count > 0)
                                    {
                                        if (gradingconditions != null && gradingconditions.Count > 0)
                                        {
                                            foreach (var gradingcheckbox in grading.Checkbox)
                                            {
                                                gradingcheckbox.Value = gradingconditions.Where(x => x.GradingConditionLabel == gradingcheckbox.Name).Select(x => x.GradingConditionValue).FirstOrDefault();
                                            }
                                        }
                                    }
                                    if (gradingnotes != null && gradingnotes.Count > 0)
                                    {
                                        grading.Value = gradingnotes.Where(x => x.GradingId == grading.Id).Select(x => x.Value).FirstOrDefault();
                                    }
                                }

                            }
                        }


                    }

                }


                var sectionIdGroup = new List<GradingSection>();
                if (newReport.Sections != null && newReport.Sections.Count > 0)
                {
                    sectionIdGroup.AddRange(newReport.Sections.Select(x => new GradingSection { SectionId = x.Id, SectionName = x.SectionName }).ToList());
                }
                if (retApp.tankReportView?.Sections != null && retApp.tankReportView?.Sections?.Count > 0)
                {
                    sectionIdGroup.AddRange(retApp.tankReportView?.Sections?.Select(x => new GradingSection { SectionId = x.Id, SectionName = x.TankName }).ToList());
                }
                retApp.template = newReport;
                retApp.sections = sectionIdGroup;
                return await Task.FromResult(retApp);
            }
            catch (SqlException sqlEx)
            {

            }
            return new Report();
        }
        public async Task<ServiceResult<List<UpskillImageData>>> GetBulkUploadImagesList()
        {
            var result = await _surveyReportRepository.GetBulkUploadUnplacedImages();
            return ServiceResult<List<UpskillImageData>>.Success(result);
        }
        public async Task<ServiceResult<List<BulkUploadViewModel>>> GetBulkUploadSectionDropdown(DataSourceRequest request, int projectId)
        {
            var sectionResult = await GetSectionIdsByProjectId(projectId, null);

            if (!sectionResult.IsSuccess || sectionResult.Data == null || sectionResult.Data.ReportPartGrid == null)
            {
                return ServiceResult<List<BulkUploadViewModel>>.Failure("Failed to load sections");
            }

            var result = new List<BulkUploadViewModel>();

            foreach (var obj in sectionResult.Data.ReportPartGrid)
            {
                if (obj.TemplateId != 0 && obj.TemplateId != 8 && obj.SectionsList != null)
                {
                    foreach (var section in obj.SectionsList)
                    {
                        result.Add(new BulkUploadViewModel
                        {
                            Id = $"{obj.TemplateId}:{section.SectionId}",
                            Name = $"{obj.Name} : {section.SectionName}"
                        });
                    }
                }
            }

            return ServiceResult<List<BulkUploadViewModel>>.Success(result);
        }

        public async Task<ServiceResult<SurveyReportViewModel>> SurveyReportPDF(int projectId, int? templateId, string sectionId, bool allTemplate)
        {
            var imo = _projectRepository.GetIMONumberByProjectId(projectId).Result;
            var reportSectionModel = GetSectionIdsByProjectId(projectId, templateId).Result;
            List<Guid> templateSectionIds = reportSectionModel.Data.ReportPartGrid.Where(x => x.TemplateId == templateId.Value).FirstOrDefault().SectionIds;
            List<string> sectionIds = new List<string>();
            if (templateSectionIds.Count > 0)
                sectionIds = templateSectionIds.Select(x => x.ToString()).ToList();
            var reportResult = await GetReportBySectionId(projectId, templateId.Value, Guid.Parse(sectionId), imo, sectionIds);

            if (!reportResult.IsSuccess || reportResult.Data == null)
            {
                return ServiceResult<SurveyReportViewModel>.Failure("Failed to generate report");
            }

            Report ret = reportResult.Data;
            var surveyReportViewModel = new SurveyReportViewModel()
            {
                Report = ret,
                ProjectId = projectId,
                ProjectName = ret.ProjectName
            };

            //ViewBag.allTemplate = allTemplate;
            return ServiceResult<SurveyReportViewModel>.Success(surveyReportViewModel);
        }

        public async Task<ServiceResult<Report>> GetReportBySectionId(int projectId, int templateSectionId, Guid SectionId, string imoNumber, List<string> SectionIds)
        {
            try
            {
                var retApp = new Report();

                retApp.ProjectName = await _projectRepository.GetProjectName(projectId);


                retApp.Vessel = new Vessel();
                var ReportVessel = new ReportVesselMainData();
                //Get CERTIFICATE USING CLASSNUMBER
                try
                {
                    ReportVessel = _vesselRepository.GetVesselMainData(projectId).Result.First();
                }
                catch (Exception ex)
                { }
                if (ReportVessel != null)
                {
                    retApp.ReportPort = ReportVessel.reportPort;
                    retApp.ReportDate = ReportVessel.portDate != null && Convert.ToDateTime(ReportVessel.portDate) != DateTime.MinValue ? Convert.ToDateTime(ReportVessel.portDate).ToShortDateString() : DateTime.Now.ToString("dd-MM-yyy");

                    retApp.ReportNo = ReportVessel.reportNo;
                    retApp.Vessel.ReportNo = ReportVessel.reportNo;
                    if (ReportVessel.actualReportStartDate != null && Convert.ToDateTime(ReportVessel.actualReportStartDate).Date != DateTime.MinValue.Date)
                    {
                        retApp.Vessel.ActualReportStartDate = Convert.ToDateTime(ReportVessel.actualReportStartDate);
                    }
                    retApp.ReportPort = ReportVessel.reportPort;
                    retApp.Vessel.Classed = ReportVessel.classed;
                    retApp.Vessel.VesselName = ReportVessel.vesselName;
                    retApp.Vessel.VesselID = ReportVessel.vesselID;
                    retApp.Vessel.ImoNumber = ReportVessel.imoNumber;
                    retApp.Vessel.VesselType = ReportVessel.vesselType;
                    retApp.Vessel.YearBuilt = Convert.ToInt32(ReportVessel.yearBuilt);
                    retApp.Vessel.BuiltBy = ReportVessel.builtBy;
                    retApp.Vessel.HullNo = ReportVessel.hullNo;
                    retApp.Vessel.FlagName = ReportVessel.flagName;
                    retApp.Vessel.Homeport = ReportVessel.homeport;
                    retApp.Vessel.OfficalNumber = ReportVessel.officalNumber;
                    retApp.Vessel.CallSign = ReportVessel.callSign;
                    retApp.Vessel.PreviousName = ReportVessel.previousName;
                    retApp.Vessel.OwnerName = ReportVessel.ownerName;
                    retApp.Vessel.Manager = ReportVessel.manager;
                    retApp.Vessel.Length = ReportVessel.length != null ? float.Parse(ReportVessel.length) : 0;
                    retApp.Vessel.Breadth = ReportVessel.breadth != null ? float.Parse(ReportVessel.breadth) : 0;
                    retApp.Vessel.DEPTH = ReportVessel.depth != null ? float.Parse(ReportVessel.depth) : 0;
                    retApp.Vessel.Draft = ReportVessel.draft != null ? float.Parse(ReportVessel.draft) : 0;
                    retApp.Vessel.DEADWEIGHT = ReportVessel.deadweight != null ? ReportVessel.deadweight : null;
                    retApp.Vessel.GrossTons = ReportVessel.grossTons != null ? float.Parse(ReportVessel.grossTons) : 0;
                    retApp.Vessel.NetTons = ReportVessel.netTons != null ? float.Parse(ReportVessel.netTons) : 0;
                    retApp.Vessel.PropulsionType = ReportVessel.propulsionType;
                    retApp.Vessel.KW = ReportVessel.kw != null ? ReportVessel.kw : "";
                    retApp.Vessel.ShaftRPM = ReportVessel.shaftRPM != null ? float.Parse(ReportVessel.shaftRPM) : 0;
                    retApp.Vessel.PropulsionManufacturer = ReportVessel.propulsionManufacturer;
                    retApp.Vessel.GearManufacturer = ReportVessel.gearManufacturer;
                    retApp.Vessel.Classed = ReportVessel.classed;
                    retApp.Vessel.ClassNo = ReportVessel.classNo;
                    retApp.Vessel.ClassNotation = ReportVessel.classNotation;
                    retApp.Vessel.Machinery = ReportVessel.machinery;
                    retApp.Vessel.Other = ReportVessel.other;
                    retApp.Vessel.Environment = ReportVessel.environment;
                    retApp.Vessel.SelectedPrefix = ReportVessel.selectedPrefix.ToString();
                    //retApp.Vessel.FirstVisitDate = ReportVessel.firstVisitDate;
                    retApp.Vessel.FirstVisitDate = ReportVessel.firstVisitDate != null ? DateTime.ParseExact(ReportVessel.firstVisitDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("dd-MMM-yyyy") : "";

                    //retApp.Vessel.LastVisitDate = ReportVessel.lastVisitDate;
                    retApp.Vessel.LastVisitDate = ReportVessel.lastVisitDate != null ? DateTime.ParseExact(ReportVessel.lastVisitDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("dd-MMM-yyyy") : "";

                }

                retApp.SurveyStatus = _vesselRepository.GetSurveyStatus(projectId).Result.FirstOrDefault();
                var certificates = _vesselRepository.GetStatutoryCertificate(projectId).Result;
                retApp.StatutoryCertificatesExpirationDates = certificates.StatutoryCertificatesExpirationDates;
                retApp.StatutoryCertificatesIssuedDates = certificates.StatutoryCertificatesIssuedDates;
                retApp.ProjectId = projectId;
                var projectydetails = _projectRepository.GetProject(projectId).Result;
                //********GetReportTemplate*******

                Core.Models.Survey.View newReport = new Core.Models.Survey.View();

                var templateDetails = _surveyReportRepository.GetTemplateTitle(templateSectionId).Result;
                newReport.Id = templateDetails.Id;
                newReport.ProjectId = projectId;
                newReport.TemplateTitle = templateDetails.SectionName;
                newReport.handireport = new HandIReport();
                var currentCondition = _surveyReportRepository.GetCurrentCondition().Result;
                var imageDescription = _descriptionRepository.GetImageDescriptionsByProjectId(projectId).Result;


                //  newReport.handireport.maintanceOptions =new MaintanceOptions() { new MaintanceOptions { Label = "Yes", Value = "Yes" }, new MaintanceOptions { Label = "No", Value = "No" }, new MaintanceOptions { Label = "Others", Value = "Others" }};

                var sections = _surveyReportRepository.GetTemplatSections(templateSectionId).Result;
                if (templateSectionId == 2 || templateSectionId == 3 || templateSectionId == 4)
                {
                    sections = sections.Where(x => x.HasSubSection == false).ToList();
                    retApp.tankReportView = new Core.Models.Tank.TankReportView();
                    retApp.tankReportView.ProjectId = projectId;
                    retApp.tankReportView.TemplateId = templateSectionId;
                    retApp.tankReportView.TemplateName = templateDetails.SectionName.Replace("Part ", String.Empty);
                    retApp.tankReportView.sectionStartCount = sections != null ? sections.Count() + 1 : 1;
                    retApp.tankReportView.Sections = new List<TankUI>();
                    retApp.tankReportView.Sections = _tankRepository.GetTemplateTanks(templateSectionId, projectydetails?.IMO.ToString(), null, projectId).Result;
                    retApp.tankReportView.Sections = retApp.tankReportView.Sections.Where(x => x.Id == SectionId).ToList();
                    foreach (var section in retApp.tankReportView.Sections)
                    {

                        section.OrderNumber = SectionIds.IndexOf(section.Id.ToString()) + 1;
                        section.Grading = new List<TankGradingUI>();
                        section.Grading = _tankRepository.GetTemplateTankGrading(section.TankTypeId, projectId, projectydetails.VesselType).Result;
                        section.GradingLabelName = new List<String>();

                        var tankreportnotes = _surveyReportRepository.GetProjectReportTemplate(projectId, templateSectionId, section.Id)?.Result;
                        if (tankreportnotes != null && tankreportnotes.Notes != null)
                        {
                            section.SectionNotes = tankreportnotes.Notes;
                        }
                        foreach (var grading in section.Grading)
                        {
                            var templategradingnotes = _surveyReportRepository.GetProjectGrading(projectId, templateSectionId, section.Id)?.Result;
                            grading.Checkbox = new List<TankCheckBox>();
                            grading.Checkbox = _tankRepository.GetTemplateTankGradingCondition(grading.Id).Result;
                            if (section.GradingLabelName.Count < grading.Checkbox.Count)
                            {
                                section.GradingLabelName = grading.Checkbox.Select(x => x.Name).ToList();
                            }
                            var templategradingconditions = _surveyReportRepository.GetProjectGradingConditionMapping(projectId, templateSectionId, section.Id, grading.Id)?.Result;

                            if (templategradingnotes != null && templategradingnotes.Count > 0)
                            {
                                grading.Value = templategradingnotes.Where(x => x.GradingId == grading.Id).Select(x => x.Value).FirstOrDefault();
                            }


                            if (grading.Checkbox != null && grading.Checkbox.Count > 0)
                            {
                                if (templategradingconditions != null && templategradingconditions.Count > 0)
                                {
                                    foreach (var gradingcheckbox in grading.Checkbox)
                                    {
                                        gradingcheckbox.Value = templategradingconditions.Where(x => x.GradingConditionLabel == gradingcheckbox.Name).Select(x => x.GradingConditionValue).FirstOrDefault();
                                    }
                                }
                            }
                        }
                        section.currentConditions = new List<CurrentCondition>();
                        section.currentConditions = currentCondition;
                        section.imageDescriptions = new List<ImageDescriptionUI>();
                        section.imageDescriptions.AddRange(imageDescription.Where(x => x.TankTypeId == section.TankTypeId).Select(x => new ImageDescriptionUI { Id = x.Id, Description = x.Description }).ToList());
                        section.tankImageCards = new List<TankImageCard>();
                        section.tankImageCards = _tankRepository.GetProjectTankImageCard(projectId, templateSectionId, section.Id).Result;

                    }
                }

                var gradings = _surveyReportRepository.GetTemplatGradings(templateSectionId, projectId).Result;
                var imageCards = _surveyReportRepository.GetImageCard(templateSectionId).Result;

                var gradingCondition = _surveyReportRepository.GetGradingCondition(templateSectionId, projectId).Result;

                ProjectReportMapping reportnotes = new ProjectReportMapping();
                List<ProjectGradingMapping> gradingnotes = new List<ProjectGradingMapping>();
                List<ProjectGradingConditionMapping> gradingconditions = new List<ProjectGradingConditionMapping>();
                List<ProjectCardMapping> projectcards = new List<ProjectCardMapping>();


                newReport.Sections = new List<Sections>();
                newReport.currentConditions = new List<CurrentCondition>();
                newReport.currentConditions = currentCondition;

                newReport.Sections.AddRange(sections.GroupBy(x => x.SectionName).ToList().Select(x => new Sections { SectionName = x.Key, Id = x.FirstOrDefault().SectionId, subSections = new List<SubSection>(), SpecialSectionName = x.FirstOrDefault().SpecialSectionName }).ToList());
                newReport.Sections = newReport.Sections.Where(x => x.Id == SectionId).ToList();
                if (SectionId.ToString().ToLower() != "00b4955b-2cda-4605-af94-555b38fbcda0")
                {

                    if (newReport.Sections != null)
                    {
                        foreach (var item in newReport.Sections)
                        {
                            item.subSections.AddRange(sections.Where(x => x.SectionId == item.Id).Select(x => new SubSection { Id = x.Id, SubSectionName = x.SubSectionName, Grading = new List<Core.Models.Survey.Grading>(), ImageCards = new List<Core.Models.Survey.ImageCards>(), GradingLabelName = new List<String>(), SpecialSectionName = x.SpecialSectionName }).ToList());

                        }
                        foreach (var item in newReport.Sections)
                        {

                            if (newReport.TemplateTitle != "Part H & I")
                            {

                                foreach (var subitem in item.subSections)
                                {
                                    Guid specialSectionId = Guid.Empty;
                                    if (templateSectionId != 6)
                                    {
                                        subitem.OrderNumber = SectionIds.IndexOf(subitem.Id.ToString()) + 1;
                                    }
                                    else
                                    {
                                        subitem.OrderNumber = 2;
                                    }



                                    subitem.Grading.AddRange(gradings.Where(x => x.SectionId == subitem.Id).Select(x => new Core.Models.Survey.Grading { Id = x.Id, Name = x.LabelName, Checkbox = new List<CheckBox>() }).ToList());
                                    reportnotes = _surveyReportRepository.GetProjectReportTemplate(projectId, templateSectionId, subitem.Id)?.Result;
                                    subitem.imageDescriptions = new List<ImageDescriptionUI>();
                                    subitem.imageDescriptions.AddRange(imageDescription.Where(x => x.SectionId == subitem.Id).Select(x => new ImageDescriptionUI { Id = x.Id, Description = x.Description }).ToList());
                                    if (reportnotes != null && reportnotes.Notes != null)
                                    {
                                        subitem.SectionNotes = reportnotes.Notes;
                                    }

                                    subitem.projectCards = _surveyReportRepository.GetProjectImageCard(projectId, templateSectionId, subitem.Id)?.Result;
                                    if (imageCards != null && imageCards.Count > 0)
                                    {

                                        subitem.ImageCards.AddRange(imageCards.Where(x => x.SectionId == subitem.Id).Select(x => new Core.Models.Survey.ImageCards { Id = x.Id, CurrentCondition = x.CurrentCondition, DescriptionOptions = new List<Description>(), FileName = x.FileName, CurrentConditionOptions = new List<CurrentCondition>() }).ToList());
                                        subitem.ImageCards = subitem.ImageCards.OrderBy(x => Convert.ToInt16(x.FileName.Substring(x.FileName.IndexOf("-") + 1))).ToList();

                                    }
                                    if (gradingCondition != null && gradingCondition.Count > 0)
                                    {
                                        gradingnotes = _surveyReportRepository.GetProjectGrading(projectId, templateSectionId, subitem.Id)?.Result;

                                        foreach (var grading in subitem.Grading)
                                        {
                                            gradingconditions = _surveyReportRepository.GetProjectGradingConditionMapping(projectId, templateSectionId, subitem.Id, grading.Id)?.Result;
                                            grading.Checkbox.AddRange(gradingCondition.Where(x => x.GradingId == grading.Id && x.SectionId == subitem.Id).Select(x => new CheckBox { Id = x.Id, Name = x.Condition }).ToList());
                                            if (subitem.GradingLabelName.Count < grading.Checkbox.Count)
                                            {
                                                subitem.GradingLabelName = grading.Checkbox.Select(x => x.Name).ToList();
                                            }
                                            if (grading.Checkbox != null && grading.Checkbox.Count > 0)
                                            {
                                                if (gradingconditions != null && gradingconditions.Count > 0)
                                                {
                                                    foreach (var gradingcheckbox in grading.Checkbox)
                                                    {
                                                        gradingcheckbox.Value = gradingconditions.Where(x => x.GradingConditionLabel == gradingcheckbox.Name).Select(x => x.GradingConditionValue).FirstOrDefault();
                                                    }
                                                }
                                            }
                                            if (gradingnotes != null && gradingnotes.Count > 0)
                                            {
                                                grading.Value = gradingnotes.Where(x => x.GradingId == grading.Id).Select(x => x.Value).FirstOrDefault();
                                            }

                                        }
                                    }


                                }



                            }
                            else
                            {
                                newReport.handireport = _surveyReportRepository.GetHIReport(projectId).Result;
                                if (newReport.handireport == null)
                                {
                                    newReport.handireport = new HandIReport();
                                }
                                newReport.handireport.sectionOptions = new List<Options>() { new Options { Label = "Yes", Value = "Yes" }, new Options { Label = "No", Value = "No" }, new Options { Label = "Others", Value = "Others" } };
                                newReport.handireport.maintanenceOptions = new List<Options>() { new Options { Label = "Yes", Value = "Yes" }, new Options { Label = "No", Value = "No" }, new Options { Label = "N/A", Value = "N/A" } };
                                newReport.handireport.Grading = new List<Core.Models.Survey.Grading>();
                                newReport.handireport.GradingLabelName = new List<String>();
                                newReport.handireport.SectionId = newReport.Sections.FirstOrDefault().Id;
                                newReport.handireport.Grading.AddRange(gradings.Where(x => x.SectionId == newReport.Sections.FirstOrDefault().Id).Select(x => new Core.Models.Survey.Grading { Id = x.Id, Name = x.LabelName, Checkbox = new List<CheckBox>() }).ToList());
                                if (gradingCondition != null && gradingCondition.Count > 0)
                                {

                                    gradingnotes = _surveyReportRepository.GetProjectGrading(projectId, templateSectionId, newReport.handireport.SectionId)?.Result;
                                    foreach (var grading in newReport.handireport.Grading)
                                    {

                                        gradingconditions = _surveyReportRepository.GetProjectGradingConditionMapping(projectId, templateSectionId, newReport.handireport.SectionId, grading.Id)?.Result;
                                        grading.Checkbox.AddRange(gradingCondition.Where(x => x.GradingId == grading.Id && x.SectionId == newReport.Sections.FirstOrDefault().Id).Select(x => new CheckBox { Id = x.Id, Name = x.Condition }).ToList());
                                        if (newReport.handireport.GradingLabelName.Count < grading.Checkbox.Count)
                                        {
                                            newReport.handireport.GradingLabelName = grading.Checkbox.Select(x => x.Name).ToList();
                                        }
                                        if (grading.Checkbox != null && grading.Checkbox.Count > 0)
                                        {
                                            if (gradingconditions != null && gradingconditions.Count > 0)
                                            {
                                                foreach (var gradingcheckbox in grading.Checkbox)
                                                {
                                                    gradingcheckbox.Value = gradingconditions.Where(x => x.GradingConditionLabel == gradingcheckbox.Name).Select(x => x.GradingConditionValue).FirstOrDefault();
                                                }
                                            }
                                        }
                                        if (gradingnotes != null && gradingnotes.Count > 0)
                                        {
                                            grading.Value = gradingnotes.Where(x => x.GradingId == grading.Id).Select(x => x.Value).FirstOrDefault();
                                        }
                                    }

                                }
                            }


                        }

                    }
                }
                else
                {
                    var sectionName = sections.Where(x => x.Id == SectionId).Select(x => x.SpecialSectionName).FirstOrDefault();
                    var templates = sections.Where(x => x.SpecialSectionName == sectionName).ToList();
                    List<SubSection> specialsections = new List<SubSection>();
                    specialsections.AddRange(templates.Select(x => new SubSection { Id = x.Id, SubSectionName = x.SubSectionName, Grading = new List<Core.Models.Survey.Grading>(), ImageCards = new List<Core.Models.Survey.ImageCards>(), GradingLabelName = new List<String>(), SpecialSectionName = x.SpecialSectionName }).ToList());
                    foreach (var listsubitem in specialsections)
                    {



                        listsubitem.Grading.AddRange(gradings.Where(x => x.SectionId == listsubitem.Id).Select(x => new Core.Models.Survey.Grading { Id = x.Id, Name = x.LabelName, Checkbox = new List<CheckBox>() }).ToList());
                        reportnotes = _surveyReportRepository.GetProjectReportTemplate(projectId, templateSectionId, listsubitem.Id)?.Result;
                        listsubitem.imageDescriptions = new List<ImageDescriptionUI>();
                        listsubitem.imageDescriptions.AddRange(imageDescription.Where(x => x.SectionId == listsubitem.Id).Select(x => new ImageDescriptionUI { Id = x.Id, Description = x.Description }).ToList());
                        if (reportnotes != null && reportnotes.Notes != null)
                        {
                            listsubitem.SectionNotes = reportnotes.Notes;
                        }

                        listsubitem.projectCards = _surveyReportRepository.GetProjectImageCard(projectId, templateSectionId, listsubitem.Id)?.Result;
                        if (imageCards != null && imageCards.Count > 0)
                        {

                            listsubitem.ImageCards.AddRange(imageCards.Where(x => x.SectionId == listsubitem.Id).Select(x => new Core.Models.Survey.ImageCards { Id = x.Id, CurrentCondition = x.CurrentCondition, DescriptionOptions = new List<Description>(), FileName = x.FileName, CurrentConditionOptions = new List<CurrentCondition>() }).ToList());
                            listsubitem.ImageCards = listsubitem.ImageCards.OrderBy(x => Convert.ToInt16(x.FileName.Substring(x.FileName.IndexOf("-") + 1))).ToList();

                        }
                        if (gradingCondition != null && gradingCondition.Count > 0)
                        {
                            gradingnotes = _surveyReportRepository.GetProjectGrading(projectId, templateSectionId, listsubitem.Id)?.Result;

                            foreach (var grading in listsubitem.Grading)
                            {
                                gradingconditions = _surveyReportRepository.GetProjectGradingConditionMapping(projectId, templateSectionId, listsubitem.Id, grading.Id)?.Result;
                                grading.Checkbox.AddRange(gradingCondition.Where(x => x.GradingId == grading.Id && x.SectionId == listsubitem.Id).Select(x => new CheckBox { Id = x.Id, Name = x.Condition }).ToList());
                                if (listsubitem.GradingLabelName.Count < grading.Checkbox.Count)
                                {
                                    listsubitem.GradingLabelName = grading.Checkbox.Select(x => x.Name).ToList();
                                }
                                if (grading.Checkbox != null && grading.Checkbox.Count > 0)
                                {
                                    if (gradingconditions != null && gradingconditions.Count > 0)
                                    {
                                        foreach (var gradingcheckbox in grading.Checkbox)
                                        {
                                            gradingcheckbox.Value = gradingconditions.Where(x => x.GradingConditionLabel == gradingcheckbox.Name).Select(x => x.GradingConditionValue).FirstOrDefault();
                                        }
                                    }
                                }
                                if (gradingnotes != null && gradingnotes.Count > 0)
                                {
                                    grading.Value = gradingnotes.Where(x => x.GradingId == grading.Id).Select(x => x.Value).FirstOrDefault();
                                }

                            }
                        }


                    }
                    foreach (var item in newReport.Sections)
                    {
                        item.subSections.Add(templates.Where(x => x.Id == SectionId).Select(x => new SubSection { Id = x.Id, SubSectionName = x.SubSectionName, Grading = new List<Core.Models.Survey.Grading>(), ImageCards = new List<Core.Models.Survey.ImageCards>(), GradingLabelName = new List<String>(), SpecialSectionName = x.SpecialSectionName }).FirstOrDefault());
                        foreach (var subitem in item.subSections)
                        {
                            subitem.OrderNumber = 1;
                            foreach (var sp_section in specialsections)
                            {
                                if (subitem.Grading == null)
                                {
                                    subitem.Grading = new List<Core.Models.Survey.Grading>();
                                }
                                if (subitem.imageDescriptions == null)
                                {
                                    subitem.imageDescriptions = new List<ImageDescriptionUI>();
                                }
                                if (subitem.ImageCards == null)
                                {
                                    subitem.ImageCards = new List<ImageCards>();
                                }
                                if (subitem.projectCards == null)
                                {
                                    subitem.projectCards = new List<ProjectCardMapping>();
                                }
                                subitem.Grading.AddRange(sp_section.Grading);
                                if (sp_section.SectionNotes != null)
                                {
                                    subitem.SectionNotes = sp_section.SectionNotes;
                                }
                                subitem.imageDescriptions.AddRange(sp_section.imageDescriptions);
                                subitem.ImageCards.AddRange(sp_section.ImageCards);
                                subitem.projectCards.AddRange(sp_section.projectCards);
                            }
                        }
                    }


                }

                var sectionIdGroup = new List<GradingSection>();
                if (newReport.Sections != null && newReport.Sections.Count > 0)
                {
                    sectionIdGroup.AddRange(newReport.Sections.Select(x => new GradingSection { SectionId = x.Id, SectionName = x.SectionName }).ToList());
                }
                if (retApp.tankReportView?.Sections != null && retApp.tankReportView?.Sections?.Count > 0)
                {
                    sectionIdGroup.AddRange(retApp.tankReportView?.Sections?.Select(x => new GradingSection { SectionId = x.Id, SectionName = x.TankName }).ToList());
                }
                retApp.template = newReport;
                retApp.sections = sectionIdGroup.Where(x => x.SectionId == SectionId).ToList();


                return ServiceResult<Report>.Success(retApp);
            }
            catch (SqlException sqlEx)
            {
                return ServiceResult<Report>.Failure("Error while generating report");
            }
            //return ServiceResult<Report>.Success(Report);
        }

        public async Task<ServiceResult<bool>> SaveReportFile(string contentType, string base64, bool allTemplate, string id, string sectionId, int totalTemplates, int totalSections, int templateId, int projectId, byte[] fileContents)
        {
            try
            {
                string root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "tempdocument", id);

                string path = Path.Combine(root, templateId.ToString());

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string fileName = templateId == 0 ? $"{templateId}.pdf" : $"{sectionId}.pdf";

                string filePath = Path.Combine(path, fileName);

                await File.WriteAllBytesAsync(filePath, fileContents);

                var totalFilesInDirectory =
                    Directory.GetFiles(root, "*.pdf", SearchOption.AllDirectories).Length;

                bool isAllTemplateCompleted =
                    templateId != 6
                        ? totalSections == totalFilesInDirectory
                        : totalFilesInDirectory == 2;

                return ServiceResult<bool>.Success(isAllTemplateCompleted);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure("Failed to save report file");
            }
        }
        public async Task<ServiceResult<SurveyReportViewModel>> Export(int projectId, bool allTemplate, int templateId)
        {
            var surveyReportViewModel = PrepareSurveyReportViewModel(projectId, null).Result;
            if (!allTemplate)
                surveyReportViewModel.ReportPartGrid = surveyReportViewModel.ReportPartGrid.Where(x => x.TemplateId == templateId).ToList();
            return ServiceResult<SurveyReportViewModel>.Success(surveyReportViewModel);
        }

        public async Task<ServiceResult<MergeReportResultViewModel>> MergeReportPDF(string id, bool allTemplate, int projectId, int templateId)
        {
            var surveyReportViewModel = PrepareSurveyReportViewModel(projectId, null).Result;
            if (!allTemplate)
                surveyReportViewModel.ReportPartGrid = surveyReportViewModel.ReportPartGrid.Where(x => x.TemplateId == templateId).ToList();
            //string root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string root = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\tempdocument");

            string path = root + @"\" + id + @"\";
            var documents = Directory.GetFiles(path, "*.pdf", SearchOption.AllDirectories);

            //string resultPDFPath = path + id + ".pdf";

            string resultPDFPath = path + id + "_" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss") + ".pdf";

            using (PdfStreamWriter fileWriter = new PdfStreamWriter(System.IO.File.OpenWrite(resultPDFPath)))
            {
                foreach (var report in surveyReportViewModel.ReportPartGrid)
                {
                    List<GradingUI> gradings = new List<GradingUI>();
                    if (report.TemplateId == 6)
                    {
                        gradings = _surveyReportRepository.GetTemplatGradings(report.TemplateId, projectId).Result;
                    }

                    if (report.TemplateId == 0)
                        report.SectionIds.Add(Guid.NewGuid());

                    foreach (var sectionid in report.SectionIds)
                    {
                        string sectionFilePath = string.Empty;
                        if (report.TemplateId == 0)
                            sectionFilePath = path + report.TemplateId + @"\" + report.TemplateId + ".pdf";
                        else
                            sectionFilePath = path + report.TemplateId + @"\" + sectionid + ".pdf";

                        if (documents.Contains(sectionFilePath))
                        {
                            if (templateId == 6)
                            {
                                var imageCount = GetUploadedImageCount(surveyReportViewModel.ProjectId, report.TemplateId, sectionid);
                                var gradingCount = GetGradingCount(gradings, sectionid);

                                if (imageCount.Id == 0 && gradingCount == 0)
                                    continue;
                            }

                            using (PdfFileSource fileToMerge = new PdfFileSource(System.IO.File.OpenRead(sectionFilePath)))
                            {
                                foreach (PdfPageSource pageToMerge in fileToMerge.Pages)
                                {
                                    fileWriter.WritePage(pageToMerge);
                                }
                            }
                        }
                    }
                }
            }

            var base64 = Convert.ToBase64String(System.IO.File.ReadAllBytes(resultPDFPath));
            if (!string.IsNullOrEmpty(base64))
            {
                Directory.Delete(path, true);
            }

            var fileName = string.Empty;
            var projectName = _projectRepository.GetProjectName(projectId).Result;
            if (allTemplate)
            {
                //fileName = projectName + " All Template";
                fileName = projectName + "_All_Template_" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss");
            }
            else
            {
                var projectDetails = _projectRepository.GetProject(projectId).Result;
                ReportTemplateUI templateTitle = _surveyReportRepository.GetTemplateTitle(templateId).Result;
                //fileName = projectName + "_" + projectDetails.IMO.GetValueOrDefault() + "_" + templateTitle.SectionName;
                fileName = projectName + "_" + projectDetails.IMO.GetValueOrDefault() + "_" + templateTitle.SectionName + "_" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss");
            }
            var result = new MergeReportResultViewModel
            {
                Base64 = base64,
                FileName = fileName
            };

            return ServiceResult<MergeReportResultViewModel>.Success(result);
        }

        public int GetGradingCount(List<GradingUI> gradings, Guid sectionId)
        {
            try
            {
                var count = gradings.Where(x => x.SectionId == sectionId).ToList().Count;
                return count;
            }
            catch { }
            return 0;
        }

        public async Task<int> GetUploadedImageCount(int projectId, int templateId, Guid sectionId)
        {
            try
            {
                var sectionImageCards = await _surveyReportRepository.GetProjectImageCard(projectId, templateId, sectionId);

                return sectionImageCards?.Count(x => !string.IsNullOrEmpty(x.Src)) ?? 0;
            }
            catch
            {
                return 0;
            }
        }
        public async Task<ServiceResult<SurveyPhotoPlacementDropdownsViewModel>> GetPhotoPlacementDropdownPartial(int projectId, int templateId, Guid sectionId)
        {
            var sectionIds = GetSectionIdsByProjectId(projectId, templateId).Result.Data.ReportPartGrid.Where(x => x.TemplateId == templateId).FirstOrDefault().SectionIds;

            SurveyPhotoPlacementDropdownsViewModel surveyPhotoPlacementDropdownViewModel = GetBulkUploadDropdownValues(projectId, templateId, sectionId, sectionIds);

            return ServiceResult<SurveyPhotoPlacementDropdownsViewModel>.Success(surveyPhotoPlacementDropdownViewModel);
        }
        public List<ImageCards> GetEmptyTankImageCard(List<TankImageCard> existingcards, string templeName, string index)
        {
            List<TankImageCard> imageCards = Enumerable.Range(1, 32)
            .Select(x => new TankImageCard { CardNumber = x, CardName = templeName + index + "-" + (x), CurrentCondition = ((x > 16) ? 1 : 0) }).ToList();

            if (existingcards.Count > 0)
            {
                imageCards = imageCards.Except(existingcards, new TankCardComparer()).ToList();
            }
            return imageCards.Select(x => new ImageCards() { Id = x.CardNumber, FileName = x.CardName, CurrentCondition = (x.CurrentCondition == 1) }).OrderBy(x => x.Id).ToList();
        }
        public SurveyPhotoPlacementDropdownsViewModel GetBulkUploadDropdownValues(int projectId, int templateId, Guid sectionId, List<Guid> sectionIds)
        {
            SurveyPhotoPlacementDropdownsViewModel model = new SurveyPhotoPlacementDropdownsViewModel();
            if (templateId != 0 && templateId != 8)
            {
                model.ProjectId = projectId;
                model.TemplateId = templateId;
                model.SectionId = sectionId;
                var projectydetails = _projectRepository.GetProject(projectId).Result;
                var imagedescriptions = _descriptionRepository.GetImageDescriptionsByProjectId(projectId).Result;
                var imageCards = _surveyReportRepository.GetImageCard(templateId).Result;
                model.CurrentCondition = _surveyReportRepository.GetCurrentCondition().Result;
                if (templateId == 2 || templateId == 3 || templateId == 4)
                {
                    var sections = _surveyReportRepository.GetTemplatSections(templateId).Result.Where(x => x.HasSubSection == false && x.Id == sectionId).FirstOrDefault();
                    var tankSections = _tankRepository.GetTemplateTanks(templateId, projectydetails?.IMO.ToString(), null, projectId).Result;
                    var tankSection = tankSections.Where(x => x.Id == sectionId).FirstOrDefault();
                    if (tankSection != null)
                    {
                        var templateName = templateId == 2 ? "B" : templateId == 3 ? "C" : "D";
                        model.ImageDescription = imagedescriptions.Where(x => x.TankTypeId == tankSection.TankTypeId).Select(x => new ImageDescriptionUI { Id = x.Id, Description = x.Description }).ToList();
                        var TankImageCard = _tankRepository.GetProjectTankImageCard(projectId, templateId, tankSection.Id).Result;
                        int index = sectionIds.IndexOf(sectionId) + 1;
                        var ImageCardlist = GetEmptyTankImageCard(TankImageCard, templateName, index.ToString());
                        foreach (var image in ImageCardlist)
                        {
                            if (image.Id > tankSection.CurrentconditionPlaceholders) { image.CurrentCondition = true; }
                            else { image.CurrentCondition = false; }
                        }
                        model.ImageCard = ImageCardlist;
                    }
                    else if (sections != null)
                    {
                        model.ImageDescription = imagedescriptions.Where(x => x.SectionId == sectionId).Select(x => new ImageDescriptionUI { Id = x.Id, Description = x.Description }).ToList();
                        var sectionImageCards = _surveyReportRepository.GetProjectImageCard(projectId, templateId, sectionId)?.Result;
                        if (sectionImageCards != null && sectionImageCards.Count > 0)
                            sectionImageCards = sectionImageCards.Where(x => !string.IsNullOrEmpty(x.Src)).ToList();
                        else
                            sectionImageCards = new List<ProjectCardMapping>();

                        var imageCard = new List<ImageCards>();
                        imageCard.AddRange(imageCards.Where(x => x.SectionId == sectionId).Select(x => new ImageCards { Id = x.Id, CurrentCondition = x.CurrentCondition, DescriptionOptions = new List<Description>(), FileName = x.FileName, CurrentConditionOptions = new List<CurrentCondition>() }).OrderBy(x => x.Id).ToList());
                        model.ImageCard = imageCard.Where(x => !sectionImageCards.Any(y => y.CardId == x.Id)).ToList();
                    }
                }
                else
                {
                    model.ImageDescription = imagedescriptions.Where(x => x.SectionId == sectionId).Select(x => new ImageDescriptionUI { Id = x.Id, Description = x.Description }).ToList();
                    var sectionImageCards = _surveyReportRepository.GetProjectImageCard(projectId, templateId, sectionId)?.Result;
                    if (sectionImageCards != null && sectionImageCards.Count > 0)
                        sectionImageCards = sectionImageCards.Where(x => !string.IsNullOrEmpty(x.Src)).ToList();
                    else
                        sectionImageCards = new List<ProjectCardMapping>();

                    var imageCard = new List<ImageCards>();
                    imageCard.AddRange(imageCards.Where(x => x.SectionId == sectionId).Select(x => new ImageCards { Id = x.Id, CurrentCondition = x.CurrentCondition, DescriptionOptions = new List<Description>(), FileName = x.FileName, CurrentConditionOptions = new List<CurrentCondition>() }).OrderBy(x => x.Id).ToList());
                    model.ImageCard = imageCard.Where(x => !sectionImageCards.Any(y => y.CardId == x.Id)).ToList();
                }
            }
            return model;
        }

        public PhotoPlacementSequence GetPhotoCardsInList(List<Template> allCards, List<UpSkillCard> sequenceCards, List<string> populatedCardIds, string type)
        {
            var placementSequence = new PhotoPlacementSequence();
            var cards = new List<UpSkillCard>();
            if (type == "Photo")
            {
                cards = sequenceCards.Where(c => c.Label.Contains(type)).OrderBy(c => c.Position).ToList();
            }
            else
            {
                var sequenceId = sequenceCards.Where(c => c.Label.Contains(type)).FirstOrDefault()?.Id;

                var template = allCards.
                    Where(t => t.SequenceId == sequenceId).FirstOrDefault();
                if (template == null)
                {
                    return null;
                }
                cards = template.Cards.
                    Where(c => c.Component.ComponentType.Equals(ComponentType.OpenSequence)).
                    OrderBy(c => c.Position).ToList();
            }

            foreach (var card in cards)
            {
                var photoCardsCheck = allCards.Where(t => t.SequenceId == card.Id).FirstOrDefault()?.Cards;
                var captureCardCheck = photoCardsCheck.Where(c => c.Label == "Capture photo").FirstOrDefault();
                if (!populatedCardIds.Contains(captureCardCheck.Id))
                {
                    placementSequence.Id = card.Id;
                    break;
                }
            }

            if (placementSequence.Id == null)
            {
                return null;
            }

            var photoCards = allCards.Where(t => t.SequenceId == placementSequence.Id).FirstOrDefault()?.Cards;

            var captureCard = photoCards.Where(c => c.Label == "Capture photo").FirstOrDefault();
            placementSequence.CaptureCard.Id = captureCard?.Id;
            placementSequence.CaptureCard.Label = captureCard?.Label;
            placementSequence.CaptureCard.SequenceId = placementSequence.Id;

            var imageDescriptionCard = photoCards.Where(c => c.Label == "Image Description").FirstOrDefault();
            if (imageDescriptionCard == null)
            {
                imageDescriptionCard = photoCards.Where(c => c.Label == "Image Location Description (i.e. Port, Starboard, Forward, Aft, etc.)").FirstOrDefault();
            }
            placementSequence.ImageDescriptionCard.Id = imageDescriptionCard?.Id;
            placementSequence.ImageDescriptionCard.Label = imageDescriptionCard?.Label;
            placementSequence.ImageDescriptionCard.SequenceId = placementSequence.Id;

            var additionalDescriptionCard = photoCards.Where(c => c.Label == "Additional Description").FirstOrDefault();
            placementSequence.AdditionalDescriptionCard.Id = additionalDescriptionCard?.Id;
            placementSequence.AdditionalDescriptionCard.Label = additionalDescriptionCard?.Label;
            placementSequence.AdditionalDescriptionCard.SequenceId = placementSequence.Id;

            var conditionCard = photoCards.Where(c => c.Label == "Condition").FirstOrDefault();
            placementSequence.ConditionCard.Id = conditionCard?.Id;
            placementSequence.ConditionCard.Label = conditionCard?.Label;
            placementSequence.ConditionCard.SequenceId = placementSequence.Id;

            return placementSequence;
        }
        public async Task<int> UpdatePhotoUploadPartial(int projectId, string assignmentId)
        {
            var unplacedImages = await _surveyReportRepository.GetUnplacedImages(projectId, assignmentId);

            return unplacedImages.Count;
        }
        public async Task<ServiceResult<SurveyPhotoPlacementViewModel>> GetSurveyPhotoPlacementPartial(int projectId, int templateSectionId, string assignmentId, string appIds)
        {
            var model = new SurveyPhotoPlacementViewModel();
            model.ProjectId = projectId;
            //model.ReportTemplateSectionId = templateSectionId;
            //model.TemplateId = templateSectionId;

            // Need to get section id with section name
            //model.reportSections = PrepareSurveyReportViewModel(projectId, null).Result.ReportPartGrid.Where(x => x.TemplateId == templateSectionId).Select(x => x.SectionsList).FirstOrDefault();
            model.ImageData = await _surveyReportRepository.GetBulkUploadUnplacedImages();

            return ServiceResult<SurveyPhotoPlacementViewModel>.Success(model);
        }

        public async Task<ServiceResult<DownloadImageResultDto>> DownloadImageForPlacement(int imageId)
        {
            var images = await _surveyReportRepository.GetBulkUploadUnplacedImages();

            var image = images.FirstOrDefault(x => x.ImageId == imageId);

            if (image == null)
            {
                return ServiceResult<DownloadImageResultDto>
                    .Failure("Image not found");
            }

            var result = new DownloadImageResultDto
            {
                Base64 = image.Base64String,
                ContentType = "image/jpeg"
            };

            return ServiceResult<DownloadImageResultDto>.Success(result);
        }

        public async Task<ServiceResult<List<AssignmentsDropdownModel>>> GetUserApplications()
        {
            var final = await _surveyReportRepository.GetReportFromAssignments("0");

            return ServiceResult<List<AssignmentsDropdownModel>>.Success(final);
        }

        public async Task<ServiceResult<Report>> RefreshCertificate(int projectId)
        {
            var retApp = new Report();
            var projectydetails = _projectRepository.GetProject(projectId).Result;
            retApp.ProjectId = projectId;
            retApp.ProjectStatus = projectydetails.ProjectStatus;
            var ReportVessel = new ReportVesselMainData();
            try
            {
                ReportVessel = _vesselRepository.GetVesselMainData(projectId).Result.First();
            }
            catch (Exception ex)
            { }
            if (ReportVessel != null)
            {
                retApp.ReportPort = ReportVessel.reportPort;
                retApp.ReportDate = ReportVessel.portDate != null && Convert.ToDateTime(ReportVessel.portDate) != DateTime.MinValue ? Convert.ToDateTime(ReportVessel.portDate).ToShortDateString() : DateTime.Now.ToString("dd-MM-yyy");
                retApp.template = new Core.Models.Survey.View();
                retApp.template.Id = 0;
                retApp.ReportNo = ReportVessel.reportNo;
                retApp.Vessel = new Core.Models.Survey.Vessel();
                retApp.Vessel.ReportNo = ReportVessel.reportNo;
                if (ReportVessel.actualReportStartDate != null && Convert.ToDateTime(ReportVessel.actualReportStartDate).Date != DateTime.MinValue.Date)
                {
                    retApp.Vessel.ActualReportStartDate = Convert.ToDateTime(ReportVessel.actualReportStartDate);
                }
                retApp.ReportPort = ReportVessel.reportPort;
                retApp.Vessel.Classed = ReportVessel.classed;
                retApp.Vessel.VesselName = ReportVessel.vesselName;
                retApp.Vessel.VesselID = ReportVessel.vesselID;
                retApp.Vessel.ImoNumber = ReportVessel.imoNumber;
                retApp.Vessel.VesselType = ReportVessel.vesselType;
                retApp.Vessel.YearBuilt = Convert.ToInt32(ReportVessel.yearBuilt);
                retApp.Vessel.BuiltBy = ReportVessel.builtBy;
                retApp.Vessel.HullNo = ReportVessel.hullNo;
                retApp.Vessel.FlagName = ReportVessel.flagName;
                retApp.Vessel.Homeport = ReportVessel.homeport;
                retApp.Vessel.OfficalNumber = ReportVessel.officalNumber;
                retApp.Vessel.CallSign = ReportVessel.callSign;
                retApp.Vessel.PreviousName = ReportVessel.previousName;
                retApp.Vessel.OwnerName = ReportVessel.ownerName;
                retApp.Vessel.Manager = ReportVessel.manager;
                retApp.Vessel.Length = ReportVessel.length != null ? float.Parse(ReportVessel.length) : 0;
                retApp.Vessel.Breadth = ReportVessel.breadth != null ? float.Parse(ReportVessel.breadth) : 0;
                retApp.Vessel.DEPTH = ReportVessel.depth != null ? float.Parse(ReportVessel.depth) : 0;
                retApp.Vessel.Draft = ReportVessel.draft != null ? float.Parse(ReportVessel.draft) : 0;
                retApp.Vessel.DEADWEIGHT = ReportVessel.deadweight != null ? ReportVessel.deadweight : null;
                retApp.Vessel.GrossTons = ReportVessel.grossTons != null ? float.Parse(ReportVessel.grossTons) : 0;
                retApp.Vessel.NetTons = ReportVessel.netTons != null ? float.Parse(ReportVessel.netTons) : 0;
                retApp.Vessel.PropulsionType = ReportVessel.propulsionType;
                retApp.Vessel.KW = ReportVessel.kw != null ? ReportVessel.kw : "";
                retApp.Vessel.ShaftRPM = ReportVessel.shaftRPM != null ? float.Parse(ReportVessel.shaftRPM) : 0;
                retApp.Vessel.PropulsionManufacturer = ReportVessel.propulsionManufacturer;
                retApp.Vessel.GearManufacturer = ReportVessel.gearManufacturer;
                retApp.Vessel.Classed = ReportVessel.classed;
                retApp.Vessel.ClassNo = ReportVessel.classNo;
                retApp.Vessel.ClassNotation = ReportVessel.classNotation;
                retApp.Vessel.Machinery = ReportVessel.machinery;
                retApp.Vessel.Other = ReportVessel.other;
                retApp.Vessel.Environment = ReportVessel.environment;
                retApp.Vessel.SelectedPrefix = ReportVessel.selectedPrefix.ToString();
                retApp.Vessel.FirstVisitDate = ReportVessel.firstVisitDate;
                //retApp.Vessel.FirstVisitDate = ReportVessel.firstVisitDate != null  ? DateTime.ParseExact(ReportVessel.firstVisitDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("dd-MMM-yyyy") : "";

                retApp.Vessel.LastVisitDate = ReportVessel.lastVisitDate;
                //retApp.Vessel.LastVisitDate = ReportVessel.lastVisitDate != null  ? DateTime.ParseExact(ReportVessel.lastVisitDate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None).ToString("dd-MMM-yyyy") : "";

            }

            if (projectydetails.ProjectStatus != 11 && projectydetails.ProjectStatus != 10)
            {
                retApp = this.MapStatuatoryCertificates(projectydetails.AbsClassID, retApp);
                retApp.SurveyStatus = await _surveyReportRepository.MapSurveyStatus(projectydetails.AbsClassID, retApp.Vessel.ImoNumber);
                var data = _vesselRepository.UpdateStatutoryCertificate(projectId, retApp).Result;
                var certificates = _vesselRepository.UpdateSurveyAudit(retApp.SurveyStatus, projectId).Result;
            }
            //Get CERTIFICATE USING CLASSNUMBER
            var sections = _surveyReportRepository.GetTemplatSections(0).Result;
            retApp.SurveyStatus = _vesselRepository.GetSurveyStatus(projectId).Result.FirstOrDefault();
            var certificatedates = _vesselRepository.GetStatutoryCertificate(projectId).Result;
            retApp.StatutoryCertificatesExpirationDates = certificatedates.StatutoryCertificatesExpirationDates;
            retApp.StatutoryCertificatesIssuedDates = certificatedates.StatutoryCertificatesIssuedDates;
            Core.Models.Survey.View newReport = new Core.Models.Survey.View();
            newReport.Sections = new List<Sections>();

            newReport.Sections.AddRange(sections.GroupBy(x => x.SectionName).ToList().Select(x => new Sections { SectionName = x.Key, Id = x.FirstOrDefault().SectionId, subSections = new List<SubSection>() }).ToList());
            retApp.template = newReport;
            return ServiceResult<Report>.Success(retApp);
        }

        public Report MapStatuatoryCertificates(string classNumber, Report report)
        {
            try
            {

                var certificates = _freedomAPIRepository.GetCertificates(classNumber).Result;
                if (certificates != null)
                {
                    report.StatutoryCertificatesIssuedDates = new StatutoryCertificates();
                    report.StatutoryCertificatesExpirationDates = new StatutoryCertificates();
                    var LLLICerticate = certificates.Where(certificate => certificate.Name != null && certificate.Name.ToLower().Contains("load") && certificate.Name.ToLower().Contains("line") && !String.IsNullOrEmpty(certificate.ExpiryDate)).FirstOrDefault();
                    if (LLLICerticate != null)
                    {
                        report.StatutoryCertificatesIssuedDates.LLLIDate = !string.IsNullOrEmpty(LLLICerticate.IssueDate) ? Convert.ToDateTime(LLLICerticate.IssueDate) : null;
                        report.StatutoryCertificatesExpirationDates.LLLIDate = !string.IsNullOrEmpty(LLLICerticate.ExpiryDate) ? Convert.ToDateTime(LLLICerticate.ExpiryDate) : null;

                    }



                    var SafetyConstructionDate = certificates.Where(certificate => certificate.Name != null && certificate.Name.ToLower().Contains("safety") && certificate.Name.ToLower().Contains("construction") && !String.IsNullOrEmpty(certificate.ExpiryDate)).FirstOrDefault();
                    if (SafetyConstructionDate != null)
                    {
                        report.StatutoryCertificatesIssuedDates.SafetyConDate = !string.IsNullOrEmpty(SafetyConstructionDate.IssueDate) ? Convert.ToDateTime(SafetyConstructionDate.IssueDate) : null;
                        report.StatutoryCertificatesExpirationDates.SafetyConDate = !string.IsNullOrEmpty(SafetyConstructionDate.ExpiryDate) ? Convert.ToDateTime(SafetyConstructionDate.ExpiryDate) : null;

                    }

                    var SafetyRadioDate = certificates.Where(certificate => certificate.Name != null && certificate.Name.ToLower().Contains("safety") && certificate.Name.ToLower().Contains("radio") && !String.IsNullOrEmpty(certificate.ExpiryDate)).FirstOrDefault();
                    if (SafetyRadioDate != null)
                    {
                        report.StatutoryCertificatesIssuedDates.SafetyRadioDate = !string.IsNullOrEmpty(SafetyRadioDate.IssueDate) ? Convert.ToDateTime(SafetyRadioDate.IssueDate) : null;
                        report.StatutoryCertificatesExpirationDates.SafetyRadioDate = !string.IsNullOrEmpty(SafetyRadioDate.ExpiryDate) ? Convert.ToDateTime(SafetyRadioDate.ExpiryDate) : null;

                    }

                    var SafetyEquipmentDate = certificates.Where(certificate => certificate.Name != null && certificate.Name.ToLower().Contains("safety") && certificate.Name.ToLower().Contains("equipment") && certificate.Abbr.Trim().ToLower().Equals("sle-cert")).FirstOrDefault();
                    if (SafetyEquipmentDate != null)
                    {
                        report.StatutoryCertificatesIssuedDates.SafetyEquipmentDate = !string.IsNullOrEmpty(SafetyEquipmentDate.IssueDate) ? Convert.ToDateTime(SafetyEquipmentDate.IssueDate) : null;
                        report.StatutoryCertificatesExpirationDates.SafetyEquipmentDate = !string.IsNullOrEmpty(SafetyEquipmentDate.ExpiryDate) ? Convert.ToDateTime(SafetyEquipmentDate.ExpiryDate) : null;

                    }
                    var IOPPDate = certificates.Where(certificate => certificate.Name != null && certificate.Name.ToLower().Contains("international") && certificate.Name.ToLower().Contains("oil") && certificate.Name.ToLower().Contains("pollution") && certificate.Name.ToLower().Contains("prevention") && certificate.Abbr.Trim().ToLower().Equals("iopp-cert")).FirstOrDefault();
                    if (IOPPDate != null)
                    {
                        report.StatutoryCertificatesIssuedDates.IOPPDate = !string.IsNullOrEmpty(IOPPDate.IssueDate) ? Convert.ToDateTime(IOPPDate.IssueDate) : null;
                        report.StatutoryCertificatesExpirationDates.IOPPDate = !string.IsNullOrEmpty(IOPPDate.ExpiryDate) ? Convert.ToDateTime(IOPPDate.ExpiryDate) : null;

                    }
                    var IAPDate = certificates.Where(certificate => certificate.Name != null && certificate.Name.ToLower().Contains("international") && certificate.Name.ToLower().Contains("air") && certificate.Name.ToLower().Contains("pollution") && certificate.Name.ToLower().Contains("prevention") && certificate.Abbr.Trim().ToLower().Equals("iapp-cert")).FirstOrDefault();
                    if (IAPDate != null)
                    {
                        report.StatutoryCertificatesIssuedDates.IAPDate = !string.IsNullOrEmpty(IAPDate.IssueDate) ? Convert.ToDateTime(IAPDate.IssueDate) : null;
                        report.StatutoryCertificatesExpirationDates.IAPDate = !string.IsNullOrEmpty(IAPDate.ExpiryDate) ? Convert.ToDateTime(IAPDate.ExpiryDate) : null;

                    }

                    var IAFSDate = certificates.Where(certificate => certificate.Name != null && certificate.Name.ToLower().Contains("international") && certificate.Name.ToLower().Contains("anti") && certificate.Name.ToLower().Contains("fouling") && certificate.Name.ToLower().Contains("system") && certificate.Abbr.Trim().ToLower().Equals("iafs-cert")).FirstOrDefault();
                    if (IAFSDate != null)
                    {
                        report.StatutoryCertificatesIssuedDates.IAFSDate = !string.IsNullOrEmpty(IAFSDate.IssueDate) ? Convert.ToDateTime(IAFSDate.IssueDate) : null;
                        report.StatutoryCertificatesExpirationDates.IAFSDate = !string.IsNullOrEmpty(IAFSDate.ExpiryDate) ? Convert.ToDateTime(IAFSDate.ExpiryDate) : null;

                    }


                    var ISPSDate = certificates.Where(certificate => certificate.Abbr.Trim().ToLower().Equals("isps-cert") && certificate.Name.ToLower().Contains("international") && certificate.Name.ToLower().Contains("ship") && certificate.Name.ToLower().Contains("security")).FirstOrDefault();
                    if (ISPSDate != null)
                    {
                        report.StatutoryCertificatesIssuedDates.ISPSDate = !string.IsNullOrEmpty(ISPSDate.IssueDate) ? Convert.ToDateTime(ISPSDate.IssueDate) : null;
                        report.StatutoryCertificatesExpirationDates.ISPSDate = !string.IsNullOrEmpty(ISPSDate.ExpiryDate) ? Convert.ToDateTime(ISPSDate.ExpiryDate) : null;

                    }
                    var CargoGearReTestDate = certificates.Where(certificate => certificate.Name.ToLower().Contains("cargo") && certificate.Name.ToLower().Contains("gear") && (certificate.Name.ToLower().Contains("retesting") || certificate.Name.ToLower().Contains("re") && certificate.Name.ToLower().Contains("testing"))).FirstOrDefault();
                    if (CargoGearReTestDate != null)
                    {
                        report.StatutoryCertificatesIssuedDates.CargoGearReTestDate = !string.IsNullOrEmpty(CargoGearReTestDate.IssueDate) ? Convert.ToDateTime(CargoGearReTestDate.IssueDate) : null;
                        report.StatutoryCertificatesExpirationDates.CargoGearReTestDate = !string.IsNullOrEmpty(CargoGearReTestDate.ExpiryDate) ? Convert.ToDateTime(CargoGearReTestDate.ExpiryDate) : null;

                    }
                    var CargoGearAnnualCertificate = certificates.Where(certificate => certificate.Abbr.Trim().ToLower().Equals("cg-ann-cert") && certificate.Name.ToLower().Contains("cargo") && certificate.Name.ToLower().Contains("gear") && certificate.Name.ToLower().Contains("annual")).FirstOrDefault();
                    if (CargoGearAnnualCertificate != null)
                    {
                        report.StatutoryCertificatesIssuedDates.CargoGearAnnualCertificate = !string.IsNullOrEmpty(CargoGearAnnualCertificate.IssueDate) ? Convert.ToDateTime(CargoGearAnnualCertificate.IssueDate) : null;
                        report.StatutoryCertificatesExpirationDates.CargoGearAnnualCertificate = !string.IsNullOrEmpty(CargoGearAnnualCertificate.ExpiryDate) ? Convert.ToDateTime(CargoGearAnnualCertificate.ExpiryDate) : null;

                    }
                    var IBWMDate = certificates.Where(certificate => certificate.Abbr.ToLower().Trim().Equals("ibwm-cert") && certificate.Name.ToLower().Contains("international") && certificate.Name.ToLower().Contains("ballast") && certificate.Name.ToLower().Contains("management") && certificate.Name.ToLower().Contains("water")).FirstOrDefault();
                    if (IBWMDate != null)
                    {
                        report.StatutoryCertificatesIssuedDates.IBWMDate = !string.IsNullOrEmpty(IBWMDate.IssueDate) ? Convert.ToDateTime(IBWMDate.IssueDate) : null;
                        report.StatutoryCertificatesExpirationDates.IBWMDate = !string.IsNullOrEmpty(IBWMDate.ExpiryDate) ? Convert.ToDateTime(IBWMDate.ExpiryDate) : null;

                    }
                    var ISPPSewageDate = certificates.Where(certificate => certificate.Abbr.ToLower().Trim().Equals("ispp-cert") && certificate.Name.ToLower().Contains("international") && certificate.Name.ToLower().Contains("sewage") && certificate.Name.ToLower().Contains("pollution") && certificate.Name.ToLower().Contains("prevention")).FirstOrDefault();
                    if (ISPPSewageDate != null)
                    {
                        report.StatutoryCertificatesIssuedDates.ISPPSewageDate = !string.IsNullOrEmpty(ISPPSewageDate.IssueDate) ? Convert.ToDateTime(ISPPSewageDate.IssueDate) : null;
                        report.StatutoryCertificatesExpirationDates.ISPPSewageDate = !string.IsNullOrEmpty(ISPPSewageDate.ExpiryDate) ? Convert.ToDateTime(ISPPSewageDate.ExpiryDate) : null;

                    }

                    var ISMCertificate = certificates.Where(certificate => certificate.ServiceType.ToLower().Contains("ism")).FirstOrDefault();
                    if (ISMCertificate != null)
                    {
                        report.StatutoryCertificatesIssuedDates.ISMDate = !string.IsNullOrEmpty(ISMCertificate.IssueDate) ? Convert.ToDateTime(ISMCertificate.IssueDate) : null;
                        report.StatutoryCertificatesExpirationDates.ISMDate = !string.IsNullOrEmpty(ISMCertificate.ExpiryDate) ? Convert.ToDateTime(ISMCertificate.ExpiryDate) : null;

                    }

                    var MaritimeCertificate = certificates.Where(certificate => certificate.Name != null && certificate.Name.ToLower().Contains("maritime") && certificate.Name.ToLower().Contains("labour") && certificate.Name.ToLower().Contains("certificate") && !String.IsNullOrEmpty(certificate.ExpiryDate)).FirstOrDefault();
                    if (MaritimeCertificate != null)
                    {
                        report.StatutoryCertificatesIssuedDates.MaritimeLabourDate = !string.IsNullOrEmpty(MaritimeCertificate.IssueDate) ? Convert.ToDateTime(MaritimeCertificate.IssueDate) : null;
                        report.StatutoryCertificatesExpirationDates.MaritimeLabourDate = !string.IsNullOrEmpty(MaritimeCertificate.ExpiryDate) ? Convert.ToDateTime(MaritimeCertificate.ExpiryDate) : null;

                    }

                    var IEECertificate = certificates.Where(certificate => certificate.Name != null && certificate.Name.ToLower().Contains("international") && certificate.Name.ToLower().Contains("energy") && certificate.Name.ToLower().Contains("efficiency")).FirstOrDefault();
                    if (IEECertificate != null)
                    {
                        report.StatutoryCertificatesIssuedDates.IEEDate = !string.IsNullOrEmpty(IEECertificate.IssueDate) ? Convert.ToDateTime(IEECertificate.IssueDate) : null;
                        report.StatutoryCertificatesExpirationDates.IEEDate = !string.IsNullOrEmpty(IEECertificate.ExpiryDate) ? Convert.ToDateTime(IEECertificate.ExpiryDate) : null;

                    }

                    var cargogearretest = certificates.Where(certificate => certificate.Name != null && certificate.Name.ToLower().Contains("cargo") && certificate.Name.ToLower().Contains("gear") && certificate.Name.ToLower().Contains("re-testing")).FirstOrDefault();
                    if (cargogearretest != null)
                    {
                        report.StatutoryCertificatesIssuedDates.CargoGearReTestDate = !string.IsNullOrEmpty(cargogearretest.IssueDate) ? Convert.ToDateTime(cargogearretest.IssueDate) : null;
                        report.StatutoryCertificatesExpirationDates.CargoGearReTestDate = !string.IsNullOrEmpty(cargogearretest.ExpiryDate) ? Convert.ToDateTime(cargogearretest.ExpiryDate) : null;

                    }


                    var gascarrierfitness = certificates.Where(certificate => certificate.Name != null && certificate.Name.ToLower().Contains("gases") && certificate.Name.ToLower().Contains("fitness") && certificate.Name.ToLower().Contains("liquefied")).FirstOrDefault();
                    if (gascarrierfitness != null)
                    {
                        report.StatutoryCertificatesIssuedDates.GasCarrierFitness = !string.IsNullOrEmpty(gascarrierfitness.IssueDate) ? Convert.ToDateTime(gascarrierfitness.IssueDate) : null;
                        report.StatutoryCertificatesExpirationDates.GasCarrierFitness = !string.IsNullOrEmpty(gascarrierfitness.ExpiryDate) ? Convert.ToDateTime(gascarrierfitness.ExpiryDate) : null;

                    }


                }
                else
                {
                    report.StatutoryCertificatesIssuedDates = new StatutoryCertificates();
                    report.StatutoryCertificatesExpirationDates = new StatutoryCertificates();
                }


                return report;

            }
            catch (Exception ex)
            {
                return report;
            }

        }


        public async Task<ValidateReportResponse> ValidateReport(int projectId, int templateSectionId)
        {
            // STEP 1: Get section ids
            var sectionResult = await GetSectionIdsByProjectId(projectId, templateSectionId);

            var sectionIds = sectionResult.Data.ReportPartGrid
                .Where(x => x.TemplateId == templateSectionId)
                .SelectMany(x => x.SectionIds)
                .Select(x => x.ToString())
                .ToList();

            // STEP 2: Get report
            var ret = await GetReport(projectId, templateSectionId, sectionIds);

            if (ret == null || ret.template == null)
                return new ValidateReportResponse { ResultSession = string.Empty };

            var templateTitle = ret.template.TemplateTitle;

            if (string.IsNullOrWhiteSpace(templateTitle))
                return new ValidateReportResponse { ResultSession = string.Empty };

            // Only these parts are valid
            var validParts = new[]
            {
        "Part A","Part B","Part C","Part D",
        "Part E","Part F","Part G","Part H & I"
    };

            if (!validParts.Contains(templateTitle))
                return new ValidateReportResponse { ResultSession = string.Empty };

            // RESULT HOLDERS
            string fileNames = "";
            string descFileNames = "";
            string tankGradingNames = "";
            string emptyAdditionalDesc = "";
            string filename = ""; var tankfilename = ""; var resultsession = "";
            string desc_filename = ""; var desc_tankfilename = ""; var tankgradingfilename = "";
            string EmptyAdditionalDescription = ""; string TankEmptyAdditionalDescription = "";

            var subSections = ret.template.Sections.SelectMany(x => x.subSections).ToList();

            var TemplateTitle = ret != null && ret.template != null && ret.template.TemplateTitle != null && ret.template.TemplateTitle != "" ? ret.template.TemplateTitle : "";


            if (ret != null && TemplateTitle != "" && (TemplateTitle == "Part A" || TemplateTitle == "Part B" || TemplateTitle == "Part C" || TemplateTitle == "Part D" || TemplateTitle == "Part E" || TemplateTitle == "Part F" || TemplateTitle == "Part G" || TemplateTitle == "Part H & I"))
            {
                #region validation Part
                //Current Condition
                var imagecardList = new List<SubSection>();
                imagecardList = ret.template.Sections.SelectMany(x => x.subSections).ToList();

                //Grading
                if (imagecardList.Select(p => p.Grading.Count).FirstOrDefault() > 0)
                {
                    var gradingcount = 0; var finalcount = 0; var checkbox = 0;
                    imagecardList.All(p =>
                    {
                        gradingcount = p.Grading.Count;
                        p.Grading.ForEach(g =>
                        {
                            if (g.Checkbox?.Count > 0)
                            {
                                g.Checkbox.ForEach(u =>
                                {
                                    if (u.Value == true && checkbox == 0)
                                    {
                                        checkbox++;
                                        finalcount++;
                                    }
                                });
                                checkbox = 0;
                            }
                            else
                            {
                                gradingcount--;
                            }
                        });

                        if (gradingcount != finalcount)
                        {
                            tankgradingfilename = tankgradingfilename + p.SubSectionName.ToString() + ",";
                        }
                        finalcount = 0;
                        return true;
                    });
                }

                var imagecards = new List<ImageCards>();
                if (imagecardList.Select(p => p.ImageCards) != null && imagecardList.Select(p => p.ImageCards.Count).FirstOrDefault() > 0 && imagecardList.Select(p => p.projectCards) != null && imagecardList.Select(p => p.projectCards.Count).FirstOrDefault() > 0)
                {
                    imagecards = imagecardList.SelectMany(s => s.ImageCards).Where(s => s.CurrentCondition == true).ToList();
                    var prjcardmap = new List<ProjectCardMapping>();
                    prjcardmap = imagecardList.SelectMany(s => s.projectCards).Where(s => s.CurrentCondition == 0 && (s.Src != null || s.DescriptionId != 0)).ToList();
                    var all = imagecards.Where(b => prjcardmap.Any(a => a.CardId == b.Id)).ToList();
                    if (all.Count > 0)
                    {
                        filename = string.Join(",", all.Select(a => a.FileName));
                    }
                }

                //Description
                var desc_imagecards = new List<ImageCards>();
                if (imagecardList.Select(p => p.ImageCards) != null && imagecardList.Select(p => p.ImageCards.Count).FirstOrDefault() > 0 && imagecardList.Select(p => p.projectCards) != null && imagecardList.Select(p => p.projectCards.Count).FirstOrDefault() > 0)
                {
                    desc_imagecards = imagecardList.SelectMany(s => s.ImageCards).ToList();
                    var desc_prjcardmap = new List<ProjectCardMapping>();
                    desc_prjcardmap = imagecardList.SelectMany(s => s.projectCards).Where(s => (s.DescriptionId == 0 && s.Src != null) || (s.DescriptionId == 0 && s.CurrentCondition != 0)).ToList();
                    var all2 = desc_imagecards.Where(b => desc_prjcardmap.Any(a => a.CardId == b.Id)).ToList();
                    if (all2.Count > 0)
                    {
                        desc_filename = string.Join(",", all2.Select(a => a.FileName));
                    }
                }

                if (imagecardList.Select(p => p.ImageCards) != null && imagecardList.Select(p => p.ImageCards.Count).FirstOrDefault() > 0 && imagecardList.Select(p => p.imageDescriptions) != null && imagecardList.Select(p => p.imageDescriptions.Count).FirstOrDefault() > 0 && imagecardList.Select(p => p.projectCards) != null && imagecardList.Select(p => p.projectCards.Count).FirstOrDefault() > 0)
                {
                    var adddesc_imagecards = new List<ImageCards>();
                    adddesc_imagecards = imagecardList.SelectMany(s => s.ImageCards).ToList();
                    var AdditionalDescId = imagecardList.SelectMany(s => s.imageDescriptions).Where(s => s.Description == "Others").ToList();
                    var AdditionalDesc = imagecardList.SelectMany(s => s.projectCards).Where(s => s.DescriptionId != 0 && (s.AdditionalDescription == "" || s.AdditionalDescription == null)).ToList();
                    var EmptyAdditionaldesc = AdditionalDesc.Where(a => AdditionalDescId.Any(b => b.Id == a.DescriptionId)).ToList();
                    var all3 = adddesc_imagecards.Where(b => EmptyAdditionaldesc.Any(a => a.CardId == b.Id)).ToList();
                    if (all3.Count > 0)
                    {
                        EmptyAdditionalDescription = string.Join(",", all3.Select(a => a.FileName));
                    }
                }

                //Tanks Image Card
                var tankimagecardList = new List<TankImageCard>();

                var tankgradingList = new List<TankGradingUI>();
                var tankui = new List<TankUI>();

                if (ret.tankReportView != null)
                {
                    //Grading
                    tankui = ret.tankReportView.Sections.ToList();
                    var tankgradingcount = 0; var tankfinalcount = 0; var tankcheckbox = 0;
                    tankui.All(p =>
                    {
                        tankgradingcount = p.Grading.Count;
                        p.Grading.ForEach(g =>
                        {
                            g.Checkbox.ForEach(u =>
                            {
                                if (u.Value == true && tankcheckbox == 0)
                                {
                                    tankcheckbox++;
                                    tankfinalcount++;
                                }
                            });
                            tankcheckbox = 0;
                        });
                        if (tankgradingcount != tankfinalcount)
                        {
                            tankgradingfilename = tankgradingfilename + p.TankName + ",";

                        }
                        tankfinalcount = 0;
                        return true;
                    });



                    tankimagecardList = ret.tankReportView.Sections.SelectMany(x => x.tankImageCards).ToList();

                    //Tank Current Condition
                    var tanklist = new List<TankImageCard>(); var totaltanklist = new List<TankImageCard>();
                    foreach (var tui in tankui)
                    {
                        tanklist = new List<TankImageCard>();
                        var CurrentconditionPlaceholders = tui.CurrentconditionPlaceholders;
                        var tankimagecards = tui.tankImageCards;
                        tanklist = tankimagecards.Where(x => x.CardNumber > CurrentconditionPlaceholders && x.CurrentCondition == 0 && (x.src != null || x.DescriptionId != 0)).ToList();
                        totaltanklist.AddRange(tanklist);
                    }
                    if (totaltanklist.Count > 0)
                    {
                        tankfilename = string.Join(",", totaltanklist.Select(a => a.CardName));
                    }

                    //Tank Description
                    var desc_tanklist = tankimagecardList.Where(d => (d.DescriptionId == 0 && d.src != null) || (d.DescriptionId == 0 && d.CurrentCondition != 0)).ToList();
                    if (desc_tanklist.Count > 0)
                    {
                        desc_tankfilename = string.Join(",", desc_tanklist.Select(a => a.CardName));
                    }

                    //Tank Add'l description
                    //var adddesc_Tankimagecards = tankui.SelectMany(s => s.tankImageCards).ToList();
                    var TankAdditionalDescId = tankui.SelectMany(s => s.imageDescriptions).Where(s => s.Description == "Others").ToList();
                    var TankAdditionalDesc = tankimagecardList.Where(d => d.DescriptionId != 0 && (d.AdditionalDescription == null || d.AdditionalDescription == "")).ToList();
                    var TankEmptyAdditionaldesc = TankAdditionalDesc.Where(a => TankAdditionalDescId.Any(b => b.Id == a.DescriptionId)).ToList();
                    //var TankEmptyAdditionalDescriptionlist = adddesc_Tankimagecards.Where(b => TankEmptyAdditionaldesc.Any(a => a.CardNumber == b.CardNumber)).ToList();
                    if (TankEmptyAdditionaldesc.Count > 0)
                    {
                        TankEmptyAdditionalDescription = string.Join(",", TankEmptyAdditionaldesc.Select(a => a.CardName));
                    }


                }


                if (ret.template != null && ret.template.handireport != null && ret.template.handireport.Grading != null && ret.template.handireport.Grading.Count > 0)
                {
                    var handireport = ret.template.handireport.Grading.ToList();
                    var HRgradingcount = 0; var HRfinalcount = 0; var HRcheckbox = 0;
                    HRgradingcount = handireport.Count;
                    handireport.ForEach(e =>
                    {
                        e.Checkbox.ForEach(u =>
                        {
                            if (u.Value == true && HRcheckbox == 0)
                            {
                                HRcheckbox++;
                                HRfinalcount++;

                            }
                        });
                        HRcheckbox = 0;

                    });
                    if (HRgradingcount != HRfinalcount)
                    {
                        tankgradingfilename = tankgradingfilename + "Part H & I" + ",";
                    }

                }

                if (filename != "" && tankfilename != "")
                { filename = filename + "," + tankfilename; }
                else if (filename == "" && tankfilename != "")
                { filename = tankfilename; }

                if (desc_filename != "" && desc_tankfilename != "")
                { desc_filename = desc_filename + "," + desc_tankfilename; }
                else if (desc_filename == "" && desc_tankfilename != "")
                { desc_filename = desc_tankfilename; }

                if (EmptyAdditionalDescription != "" && TankEmptyAdditionalDescription != "")
                { EmptyAdditionalDescription = EmptyAdditionalDescription + "," + TankEmptyAdditionalDescription; }
                else if (EmptyAdditionalDescription == "" && TankEmptyAdditionalDescription != "")
                { EmptyAdditionalDescription = TankEmptyAdditionalDescription; }

                tankgradingfilename = tankgradingfilename != "" ? tankgradingfilename.Remove(tankgradingfilename.Length - 1) : "";

                filename = filename + "#" + desc_filename + "#" + tankgradingfilename + "#" + EmptyAdditionalDescription;

                #endregion

                #region Session
                var session = (filename != "" && filename != "#" && filename != "##" && filename != "###") ? TemplateTitle + "$" + filename : "";
                var isExisting = false;
                var eachTemplate = new List<string>();
                var templateSep = new List<string>();

                //if (HttpContext.Session.GetString("reportsession") != null)
                //{
                //    resultsession = HttpContext.Session.GetString("reportsession");
                //    eachTemplate = resultsession.Split("^").ToList();
                //    foreach (var each in eachTemplate)
                //    {
                //        templateSep = each.Split("$").ToList();
                //        if (TemplateTitle == templateSep[0])
                //        {
                //            isExisting = true;
                //            if (filename != "" && filename != "###")
                //            { resultsession = resultsession.Replace(templateSep[1], filename); }
                //            else
                //            {
                //                resultsession = resultsession.Replace(templateSep[1], "");
                //                var removetitle = "^" + TemplateTitle + "$";
                //                var removetitle1 = TemplateTitle + "$^";
                //                var removefile = TemplateTitle + "$";
                //                resultsession = resultsession.Replace(removetitle, "").Replace(removetitle1, "").Replace(removefile, "").Replace("^^", "^");
                //            }

                //            HttpContext.Session.SetString("reportsession", resultsession);
                //            break;
                //        }
                //    }
                //    if (isExisting == false && session != "")
                //    {
                //        session = session + "^" + resultsession;
                //        HttpContext.Session.SetString("reportsession", session);
                //    }
                //    resultsession = HttpContext.Session.GetString("reportsession");
                //}
                //else
                //{
                //    if (session != "")
                //    {
                //        HttpContext.Session.SetString("reportsession", session);
                //        resultsession = HttpContext.Session.GetString("reportsession");
                //    }
                //}

                #endregion session
            }


            // FINAL FORMAT
            var result =
                $"{templateTitle}${fileNames}#{descFileNames}#{tankGradingNames}#{emptyAdditionalDesc}";

            return new ValidateReportResponse
            {
                ResultSession = result
            };
        }

        public async Task<ServiceResult<string>> ValidateGrading(int projectId, int templateSectionId)
        {
            try
            {
                List<string> sectionIds = new List<string>();
                var reportParts = GetSectionIdsByProjectId(projectId, templateSectionId == 10 ? null : templateSectionId).Result;
                if (templateSectionId != 10)
                    sectionIds = reportParts.Data.ReportPartGrid.Where(x => x.TemplateId == templateSectionId).FirstOrDefault().SectionIds.Select(x => x.ToString()).ToList();
                string filename = "";
                if (templateSectionId != 0 /*&& templateSectionId != 8*/ && templateSectionId != 10)
                {
                    var ret = await GetReport(projectId, templateSectionId, sectionIds);
                    filename = "";
                    var TemplateTitle = ret != null && ret.template != null && ret.template.TemplateTitle != null && ret.template.TemplateTitle != "" ? ret.template.TemplateTitle : "";

                    if (ret != null && TemplateTitle != "" && (TemplateTitle == "Part A" || TemplateTitle == "Part B" || TemplateTitle == "Part C" || TemplateTitle == "Part D" || TemplateTitle == "Part E" || TemplateTitle == "Part F" || TemplateTitle == "Part G" || TemplateTitle == "Part H & I"))
                    {
                        var tankgradingList = new List<TankGradingUI>();
                        var tankui = new List<TankUI>();
                        var tankgradingfilename = "";

                        var imagecardList = new List<SubSection>();
                        imagecardList = ret.template.Sections.SelectMany(x => x.subSections).ToList();

                        var gradingcount = 0; var finalcount = 0; var checkbox = 0;
                        if (imagecardList.Select(p => p.Grading.Count).FirstOrDefault() > 0)
                        {
                            imagecardList.All(p =>
                            {

                                gradingcount = p.Grading.Count;
                                p.Grading.ForEach(g =>
                                {
                                    if (g.Checkbox?.Count > 0)
                                    {
                                        g.Checkbox.ForEach(u =>
                                        {
                                            if (u.Value == true && checkbox == 0)
                                            {
                                                checkbox++;
                                                finalcount++;
                                            }
                                        });
                                        checkbox = 0;
                                    }
                                    else
                                    {
                                        gradingcount--;
                                    }
                                });

                                if (gradingcount != finalcount)
                                {
                                    tankgradingfilename = tankgradingfilename + p.SubSectionName.ToString() + ",";
                                }
                                finalcount = 0;
                                return true;
                            });
                        }

                        if (ret.tankReportView != null)
                        {
                            //Tank Grading
                            tankui = ret.tankReportView.Sections.ToList();
                            var tankgradingcount = 0; var tankfinalcount = 0; var tankcheckbox = 0;
                            tankui.All(p =>
                            {
                                tankgradingcount = Convert.ToInt32(p.Grading?.Count);

                                p?.Grading.ForEach(g =>
                                {
                                    g.Checkbox.ForEach(u =>
                                    {
                                        if (u.Value == true && tankcheckbox == 0)
                                        {
                                            tankcheckbox++;
                                            tankfinalcount++;
                                        }
                                    });
                                    tankcheckbox = 0;
                                });

                                if (tankgradingcount != tankfinalcount)
                                {
                                    tankgradingfilename = tankgradingfilename + p.TankName + ",";

                                }
                                tankfinalcount = 0;
                                return true;
                            });

                        }

                        if (ret.template != null && ret.template.handireport != null && ret.template.handireport.Grading != null && ret.template.handireport.Grading.Count > 0)
                        {
                            var handireport = ret.template.handireport.Grading.ToList();
                            var HRgradingcount = 0; var HRfinalcount = 0; var HRcheckbox = 0;
                            HRgradingcount = handireport.Count;
                            handireport.ForEach(e =>
                            {
                                e.Checkbox.ForEach(u =>
                                {
                                    if (u.Value == true && HRcheckbox == 0)
                                    {
                                        HRcheckbox++;
                                        HRfinalcount++;

                                    }
                                });
                                HRcheckbox = 0;

                            });
                            if (HRgradingcount != HRfinalcount)
                            {
                                tankgradingfilename = tankgradingfilename + "Part H & I" + ",";
                            }

                        }

                        tankgradingfilename = tankgradingfilename != "" ? tankgradingfilename.Remove(tankgradingfilename.Length - 1) : "";

                        filename = tankgradingfilename;
                    }
                }
                else if (templateSectionId == 10)
                {
                    templateSectionId = 8;
                    for (int sectionid = 1; sectionid < templateSectionId; sectionid++)
                    {
                        var ret = await GetReport(projectId, sectionid, sectionIds);
                        var TemplateTitle = ret != null && ret.template != null && ret.template.TemplateTitle != null && ret.template.TemplateTitle != "" ? ret.template.TemplateTitle : "";

                        if (ret != null && TemplateTitle != "" && (TemplateTitle == "Part A" || TemplateTitle == "Part B" || TemplateTitle == "Part C" || TemplateTitle == "Part D" || TemplateTitle == "Part E" || TemplateTitle == "Part F" || TemplateTitle == "Part G" || TemplateTitle == "Part H & I"))
                        {
                            var tankgradingList = new List<TankGradingUI>();
                            var tankui = new List<TankUI>();
                            var tankgradingfilename = "";

                            var imagecardList = new List<SubSection>();
                            imagecardList = ret.template.Sections.SelectMany(x => x.subSections).ToList();
                            var gradingcount = 0; var finalcount = 0; var checkbox = 0;
                            if (imagecardList.Select(p => p.Grading.Count).FirstOrDefault() > 0)
                            {
                                imagecardList.All(p =>
                                {
                                    gradingcount = p.Grading.Count;
                                    p.Grading.ForEach(g =>
                                    {
                                        if (g.Checkbox?.Count > 0)
                                        {
                                            g.Checkbox.ForEach(u =>
                                            {
                                                if (u.Value == true && checkbox == 0)
                                                {
                                                    checkbox++;
                                                    finalcount++;
                                                }
                                            });
                                            checkbox = 0;
                                        }
                                        else
                                        {
                                            gradingcount--;
                                        }
                                    });

                                    if (gradingcount != finalcount)
                                    {
                                        tankgradingfilename = tankgradingfilename + p.SubSectionName.ToString() + ",";
                                    }
                                    finalcount = 0;
                                    return true;
                                });
                            }

                            if (ret.tankReportView != null)
                            {
                                //Tank Grading
                                tankui = ret.tankReportView.Sections.ToList();
                                var tankgradingcount = 0; var tankfinalcount = 0; var tankcheckbox = 0;
                                tankui.All(p =>
                                {
                                    tankgradingcount = p.Grading.Count;
                                    p.Grading.ForEach(g =>
                                    {
                                        g.Checkbox.ForEach(u =>
                                        {
                                            if (u.Value == true && tankcheckbox == 0)
                                            {
                                                tankcheckbox++;
                                                tankfinalcount++;
                                            }
                                        });
                                        tankcheckbox = 0;
                                    });
                                    if (tankgradingcount != tankfinalcount)
                                    {
                                        tankgradingfilename = tankgradingfilename + p.TankName + ",";

                                    }
                                    tankfinalcount = 0;
                                    return true;
                                });

                            }

                            if (ret.template != null && ret.template.handireport != null && ret.template.handireport.Grading != null && ret.template.handireport.Grading.Count > 0)
                            {
                                var handireport = ret.template.handireport.Grading.ToList();
                                var HRgradingcount = 0; var HRfinalcount = 0; var HRcheckbox = 0;
                                HRgradingcount = handireport.Count;
                                handireport.ForEach(e =>
                                {
                                    e.Checkbox.ForEach(u =>
                                    {
                                        if (u.Value == true && HRcheckbox == 0)
                                        {
                                            HRcheckbox++;
                                            HRfinalcount++;

                                        }
                                    });
                                    HRcheckbox = 0;

                                });
                                if (HRgradingcount != HRfinalcount)
                                {
                                    tankgradingfilename = tankgradingfilename + "Part H & I" + ",";
                                }

                            }
                            tankgradingfilename = tankgradingfilename != "" ? tankgradingfilename.Remove(tankgradingfilename.Length - 1) : "";
                            if (tankgradingfilename != "")
                            {
                                filename = filename + tankgradingfilename + ",";
                            }
                        }
                    }

                    filename = filename != "" ? filename.Remove(filename.Length - 1) : "";

                }

                return ServiceResult<string>.Success(filename);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Failure(ex.Message);
            }
        }

        public async Task<ServiceResult<string>> ExportAllValidation(int projectId)
        {
            try
            {
                List<string> sectionIds = new List<string>();
                string Result = "";
                var Allgrading = ""; var Allcurrentcondition = ""; var Alldescription = ""; var Alladditionaldescription = "";
                int templateSectionId = 9;
                for (int sectionid = 1; sectionid < templateSectionId; sectionid++)
                {
                    var ret = await GetReport(projectId, sectionid, sectionIds);
                    var TemplateTitle = ret != null && ret.template != null && ret.template.TemplateTitle != null && ret.template.TemplateTitle != "" ? ret.template.TemplateTitle : "";
                    var grading = ""; var currentcondition = ""; var description = ""; var additionaldescription = "";

                    if (ret != null && TemplateTitle != "" && (TemplateTitle == "Part A" || TemplateTitle == "Part B" || TemplateTitle == "Part C" || TemplateTitle == "Part D" || TemplateTitle == "Part E" || TemplateTitle == "Part F" || TemplateTitle == "Part G" || TemplateTitle == "Part H & I"))
                    {
                        var tankgradingList = new List<TankGradingUI>();
                        var tankui = new List<TankUI>();
                        var tankimagecardList = new List<TankImageCard>();

                        var imagecardList = new List<SubSection>();
                        imagecardList = ret.template.Sections.SelectMany(x => x.subSections).ToList();

                        #region Grading
                        var gradingcount = 0; var finalcount = 0; var checkbox = 0;
                        if (imagecardList.Select(p => p.Grading.Count).FirstOrDefault() > 0)
                        {
                            imagecardList.All(p =>
                            {
                                gradingcount = p.Grading.Count;
                                p.Grading.ForEach(g =>
                                {
                                    if (g.Checkbox?.Count > 0)
                                    {
                                        g.Checkbox.ForEach(u =>
                                        {
                                            if (u.Value == true && checkbox == 0)
                                            {
                                                checkbox++;
                                                finalcount++;
                                            }
                                        });
                                        checkbox = 0;
                                    }
                                    else
                                    {
                                        gradingcount--;
                                    }
                                });

                                if (gradingcount != finalcount)
                                {
                                    grading = grading + p.SubSectionName.ToString() + ",";
                                }
                                finalcount = 0;
                                return true;
                            });
                        }
                        #endregion

                        #region CurrentCondition
                        var imagecards = new List<ImageCards>();
                        if (imagecardList.Select(p => p.ImageCards) != null && imagecardList.Select(p => p.ImageCards.Count).FirstOrDefault() > 0 && imagecardList.Select(p => p.projectCards) != null && imagecardList.Select(p => p.projectCards.Count).FirstOrDefault() > 0)
                        {
                            imagecards = imagecardList.SelectMany(s => s.ImageCards).Where(s => s.CurrentCondition == true).ToList();
                            var prjcardmap = new List<ProjectCardMapping>();
                            prjcardmap = imagecardList.SelectMany(s => s.projectCards).Where(s => s.CurrentCondition == 0 && (s.Src != null || s.DescriptionId != 0)).ToList();
                            var all = imagecards.Where(b => prjcardmap.Any(a => a.CardId == b.Id)).ToList();
                            if (all.Count > 0)
                            {
                                currentcondition = string.Join(",", all.Select(a => a.FileName));
                            }
                        }
                        #endregion

                        #region Description
                        var desc_imagecards = new List<ImageCards>();
                        if (imagecardList.Select(p => p.ImageCards) != null && imagecardList.Select(p => p.ImageCards.Count).FirstOrDefault() > 0 && imagecardList.Select(p => p.projectCards) != null && imagecardList.Select(p => p.projectCards.Count).FirstOrDefault() > 0)
                        {
                            desc_imagecards = imagecardList.SelectMany(s => s.ImageCards).ToList();
                            var desc_prjcardmap = new List<ProjectCardMapping>();
                            desc_prjcardmap = imagecardList.SelectMany(s => s.projectCards).Where(s => (s.DescriptionId == 0 && s.Src != null) || (s.DescriptionId == 0 && s.CurrentCondition != 0)).ToList();
                            var all2 = desc_imagecards.Where(b => desc_prjcardmap.Any(a => a.CardId == b.Id)).ToList();
                            if (all2.Count > 0)
                            {
                                description = string.Join(",", all2.Select(a => a.FileName));
                            }
                        }
                        #endregion

                        #region Add'l description
                        var adddesc_imagecards = new List<ImageCards>();
                        if (imagecardList.Select(p => p.ImageCards) != null && imagecardList.Select(p => p.ImageCards.Count).FirstOrDefault() > 0 && imagecardList.Select(p => p.imageDescriptions) != null && imagecardList.Select(p => p.imageDescriptions.Count).FirstOrDefault() > 0 && imagecardList.Select(p => p.projectCards) != null && imagecardList.Select(p => p.projectCards.Count).FirstOrDefault() > 0)
                        {
                            adddesc_imagecards = imagecardList.SelectMany(s => s.ImageCards).ToList();
                            var AdditionalDescId = imagecardList.SelectMany(s => s.imageDescriptions).Where(s => s.Description == "Others").ToList();
                            var AdditionalDesc = imagecardList.SelectMany(s => s.projectCards).Where(s => s.DescriptionId != 0 && (s.AdditionalDescription == "" || s.AdditionalDescription == null)).ToList();
                            var EmptyAdditionaldesc = AdditionalDesc.Where(a => AdditionalDescId.Any(b => b.Id == a.DescriptionId)).ToList();
                            var all3 = adddesc_imagecards.Where(b => EmptyAdditionaldesc.Any(a => a.CardId == b.Id)).ToList();
                            if (all3.Count > 0)
                            {
                                additionaldescription = string.Join(",", all3.Select(a => a.FileName));
                            }
                        }
                        #endregion

                        #region Tank Validation
                        if (ret.tankReportView != null)
                        {

                            tankui = ret.tankReportView.Sections.ToList();

                            #region Tank Grading
                            var tankgradingcount = 0; var tankfinalcount = 0; var tankcheckbox = 0;
                            if (tankui.Select(p => p.Grading.Count).FirstOrDefault() > 0)
                            {
                                tankui.All(p =>
                                {
                                    tankgradingcount = p.Grading.Count;
                                    p.Grading.ForEach(g =>
                                    {
                                        g.Checkbox.ForEach(u =>
                                        {
                                            if (u.Value == true && tankcheckbox == 0)
                                            {
                                                tankcheckbox++;
                                                tankfinalcount++;
                                            }
                                        });
                                        tankcheckbox = 0;
                                    });
                                    if (tankgradingcount != tankfinalcount)
                                    {
                                        grading = grading + p.TankName + ",";

                                    }
                                    tankfinalcount = 0;
                                    return true;
                                });
                            }
                            #endregion

                            #region Tank Current Condition
                            var tanklistnew = new List<TankImageCard>(); var totaltanklist = new List<TankImageCard>();
                            if (tankui != null && tankui.Count > 0)
                            {
                                foreach (var tui in tankui)
                                {
                                    tanklistnew = new List<TankImageCard>();
                                    var CurrentconditionPlaceholders = tui.CurrentconditionPlaceholders;
                                    var tankimagecards = tui.tankImageCards;
                                    if (tankimagecards != null && tankimagecards.Count > 0)
                                    {
                                        tanklistnew = tankimagecards.Where(x => x.CardNumber > CurrentconditionPlaceholders && x.CurrentCondition == 0 && (x.src != null || x.DescriptionId != 0)).ToList();
                                    }
                                    totaltanklist.AddRange(tanklistnew);

                                }
                                if (totaltanklist.Count > 0)
                                {
                                    currentcondition = currentcondition == "" ? string.Join(",", totaltanklist.Select(a => a.CardName)) : currentcondition + "," + string.Join(",", totaltanklist.Select(a => a.CardName));
                                }
                            }
                            #endregion

                            tankimagecardList = ret.tankReportView.Sections.SelectMany(x => x.tankImageCards).ToList();

                            #region Tank Description

                            var desc_tanklist = tankimagecardList.Where(d => (d.DescriptionId == 0 && d.src != null) || (d.DescriptionId == 0 && d.CurrentCondition != 0)).ToList();
                            if (desc_tanklist.Count > 0)
                            {
                                description = description == "" ? string.Join(",", desc_tanklist.Select(a => a.CardName)) : description + "," + string.Join(",", desc_tanklist.Select(a => a.CardName));
                            }
                            #endregion

                            #region Tank Add'l description
                            var TankAdditionalDescId = tankui.SelectMany(s => s.imageDescriptions).Where(s => s.Description == "Others").ToList();
                            var TankAdditionalDesc = tankimagecardList.Where(d => d.DescriptionId != 0 && (d.AdditionalDescription == null || d.AdditionalDescription == "")).ToList();
                            var TankEmptyAdditionalDescriptionlist = TankAdditionalDesc.Where(a => TankAdditionalDescId.Any(b => b.Id == a.DescriptionId)).ToList();

                            if (TankEmptyAdditionalDescriptionlist.Count > 0)
                            {
                                additionaldescription = additionaldescription == "" ? string.Join(",", TankEmptyAdditionalDescriptionlist.Select(a => a.CardName)) : additionaldescription + "," + string.Join(",", TankEmptyAdditionalDescriptionlist.Select(a => a.CardName));
                            }
                            #endregion

                        }
                        #endregion

                        #region H & I Grading

                        if (ret.template != null && ret.template.handireport != null && ret.template.handireport.Grading != null && ret.template.handireport.Grading.Count > 0)
                        {
                            var handireport = ret.template.handireport.Grading.ToList();
                            var HRgradingcount = 0; var HRfinalcount = 0; var HRcheckbox = 0;
                            HRgradingcount = handireport.Count;
                            handireport.ForEach(e =>
                            {
                                e.Checkbox.ForEach(u =>
                                {
                                    if (u.Value == true && HRcheckbox == 0)
                                    {
                                        HRcheckbox++;
                                        HRfinalcount++;

                                    }
                                });
                                HRcheckbox = 0;

                            });
                            if (HRgradingcount != HRfinalcount)
                            {
                                grading = grading + "Part H & I" + ",";
                            }

                        }
                        #endregion

                    }

                    grading = grading != "" ? grading.Remove(grading.Length - 1) : "";

                    Allgrading = grading != "" ? Allgrading == "" ? grading : Allgrading + "," + grading : Allgrading;

                    Allcurrentcondition = currentcondition != "" ? Allcurrentcondition == "" ? currentcondition : Allcurrentcondition + "," + currentcondition : Allcurrentcondition;

                    Alldescription = description != "" ? Alldescription == "" ? description : Alldescription + "," + description : Alldescription;

                    Alladditionaldescription = additionaldescription != "" ? Alladditionaldescription == "" ? additionaldescription : Alladditionaldescription + "," + additionaldescription : Alladditionaldescription;
                }

                Result = Allcurrentcondition + "#" + Alldescription + "#" + Allgrading + "#" + Alladditionaldescription;


                return ServiceResult<string>.Success(Result);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Failure(ex.Message);
            }
        }

        public async Task<ServiceResult<string>> ExportValidationPartial(int projectId, int sectionid)
        {
            try
            {
                List<string> sectionIds = new List<string>();
                string Result = "";
                var Allgrading = ""; var Allcurrentcondition = ""; var Alldescription = ""; var Alladditionaldescription = "";
                //int templateSectionId = 9;
                //for (int sectionid = 1; sectionid < templateSectionId; sectionid++)
                //{
                var ret = await GetReport(projectId, sectionid, sectionIds);
                var TemplateTitle = ret != null && ret.template != null && ret.template.TemplateTitle != null && ret.template.TemplateTitle != "" ? ret.template.TemplateTitle : "";
                var grading = ""; var currentcondition = ""; var description = ""; var additionaldescription = "";

                if (ret != null && TemplateTitle != "" && (TemplateTitle == "Part A" || TemplateTitle == "Part B" || TemplateTitle == "Part C" || TemplateTitle == "Part D" || TemplateTitle == "Part E" || TemplateTitle == "Part F" || TemplateTitle == "Part G" || TemplateTitle == "Part H & I"))
                {
                    var tankgradingList = new List<TankGradingUI>();
                    var tankui = new List<TankUI>();
                    var tankimagecardList = new List<TankImageCard>();

                    var imagecardList = new List<SubSection>();
                    imagecardList = ret.template.Sections.SelectMany(x => x.subSections).ToList();

                    #region Grading
                    var gradingcount = 0; var finalcount = 0; var checkbox = 0;
                    if (imagecardList.Select(p => p.Grading.Count).FirstOrDefault() > 0)
                    {
                        imagecardList.All(p =>
                        {
                            gradingcount = p.Grading.Count;
                            p.Grading.ForEach(g =>
                            {
                                if (g.Checkbox?.Count > 0)
                                {
                                    g.Checkbox.ForEach(u =>
                                    {
                                        if (u.Value == true && checkbox == 0)
                                        {
                                            checkbox++;
                                            finalcount++;
                                        }
                                    });
                                    checkbox = 0;
                                }
                                else
                                {
                                    gradingcount--;
                                }
                            });

                            if (gradingcount != finalcount)
                            {
                                grading = grading + p.SubSectionName.ToString() + ",";
                            }
                            finalcount = 0;
                            return true;
                        });
                    }
                    #endregion

                    #region CurrentCondition
                    var imagecards = new List<ImageCards>();
                    if (imagecardList.Select(p => p.ImageCards) != null && imagecardList.Select(p => p.ImageCards.Count).FirstOrDefault() > 0 && imagecardList.Select(p => p.projectCards) != null && imagecardList.Select(p => p.projectCards.Count).FirstOrDefault() > 0)
                    {
                        imagecards = imagecardList.SelectMany(s => s.ImageCards).Where(s => s.CurrentCondition == true).ToList();
                        var prjcardmap = new List<ProjectCardMapping>();
                        prjcardmap = imagecardList.SelectMany(s => s.projectCards).Where(s => s.CurrentCondition == 0 && (s.Src != null || s.DescriptionId != 0)).ToList();
                        var all = imagecards.Where(b => prjcardmap.Any(a => a.CardId == b.Id)).ToList();
                        if (all.Count > 0)
                        {
                            currentcondition = string.Join(",", all.Select(a => a.FileName));
                        }
                    }
                    #endregion

                    #region Description
                    var desc_imagecards = new List<ImageCards>();
                    if (imagecardList.Select(p => p.ImageCards) != null && imagecardList.Select(p => p.ImageCards.Count).FirstOrDefault() > 0 && imagecardList.Select(p => p.projectCards) != null && imagecardList.Select(p => p.projectCards.Count).FirstOrDefault() > 0)
                    {
                        desc_imagecards = imagecardList.SelectMany(s => s.ImageCards).ToList();
                        var desc_prjcardmap = new List<ProjectCardMapping>();
                        desc_prjcardmap = imagecardList.SelectMany(s => s.projectCards).Where(s => (s.DescriptionId == 0 && s.Src != null) || (s.DescriptionId == 0 && s.CurrentCondition != 0)).ToList();
                        var all2 = desc_imagecards.Where(b => desc_prjcardmap.Any(a => a.CardId == b.Id)).ToList();
                        if (all2.Count > 0)
                        {
                            description = string.Join(",", all2.Select(a => a.FileName));
                        }
                    }
                    #endregion

                    #region Add'l description
                    var adddesc_imagecards = new List<ImageCards>();
                    if (imagecardList.Select(p => p.ImageCards) != null && imagecardList.Select(p => p.ImageCards.Count).FirstOrDefault() > 0 && imagecardList.Select(p => p.imageDescriptions) != null && imagecardList.Select(p => p.imageDescriptions.Count).FirstOrDefault() > 0 && imagecardList.Select(p => p.projectCards) != null && imagecardList.Select(p => p.projectCards.Count).FirstOrDefault() > 0)
                    {
                        adddesc_imagecards = imagecardList.SelectMany(s => s.ImageCards).ToList();
                        var AdditionalDescId = imagecardList.SelectMany(s => s.imageDescriptions).Where(s => s.Description == "Others").ToList();
                        var AdditionalDesc = imagecardList.SelectMany(s => s.projectCards).Where(s => s.DescriptionId != 0 && (s.AdditionalDescription == "" || s.AdditionalDescription == null)).ToList();
                        var EmptyAdditionaldesc = AdditionalDesc.Where(a => AdditionalDescId.Any(b => b.Id == a.DescriptionId)).ToList();
                        var all3 = adddesc_imagecards.Where(b => EmptyAdditionaldesc.Any(a => a.CardId == b.Id)).ToList();
                        if (all3.Count > 0)
                        {
                            additionaldescription = string.Join(",", all3.Select(a => a.FileName));
                        }
                    }
                    #endregion

                    #region Tank Validation
                    if (ret.tankReportView != null)
                    {

                        tankui = ret.tankReportView.Sections.ToList();

                        #region Tank Grading
                        var tankgradingcount = 0; var tankfinalcount = 0; var tankcheckbox = 0;
                        if (tankui.Select(p => p.Grading.Count).FirstOrDefault() > 0)
                        {
                            tankui.All(p =>
                            {
                                tankgradingcount = p.Grading.Count;
                                p.Grading.ForEach(g =>
                                {
                                    g.Checkbox.ForEach(u =>
                                    {
                                        if (u.Value == true && tankcheckbox == 0)
                                        {
                                            tankcheckbox++;
                                            tankfinalcount++;
                                        }
                                    });
                                    tankcheckbox = 0;
                                });
                                if (tankgradingcount != tankfinalcount)
                                {
                                    grading = grading + p.TankName + ",";

                                }
                                tankfinalcount = 0;
                                return true;
                            });
                        }
                        #endregion

                        #region Tank Current Condition
                        var tanklistnew = new List<TankImageCard>(); var totaltanklist = new List<TankImageCard>();
                        if (tankui != null && tankui.Count > 0)
                        {
                            foreach (var tui in tankui)
                            {
                                tanklistnew = new List<TankImageCard>();
                                var CurrentconditionPlaceholders = tui.CurrentconditionPlaceholders;
                                var tankimagecards = tui.tankImageCards;
                                if (tankimagecards != null && tankimagecards.Count > 0)
                                {
                                    tanklistnew = tankimagecards.Where(x => x.CardNumber > CurrentconditionPlaceholders && x.CurrentCondition == 0 && (x.src != null || x.DescriptionId != 0)).ToList();
                                }
                                totaltanklist.AddRange(tanklistnew);

                            }
                            if (totaltanklist.Count > 0)
                            {
                                currentcondition = currentcondition == "" ? string.Join(",", totaltanklist.Select(a => a.CardName)) : currentcondition + "," + string.Join(",", totaltanklist.Select(a => a.CardName));
                            }
                        }
                        #endregion

                        tankimagecardList = ret.tankReportView.Sections.SelectMany(x => x.tankImageCards).ToList();

                        #region Tank Description

                        var desc_tanklist = tankimagecardList.Where(d => (d.DescriptionId == 0 && d.src != null) || (d.DescriptionId == 0 && d.CurrentCondition != 0)).ToList();
                        if (desc_tanklist.Count > 0)
                        {
                            description = description == "" ? string.Join(",", desc_tanklist.Select(a => a.CardName)) : description + "," + string.Join(",", desc_tanklist.Select(a => a.CardName));
                        }
                        #endregion

                        #region Tank Add'l description
                        var TankAdditionalDescId = tankui.SelectMany(s => s.imageDescriptions).Where(s => s.Description == "Others").ToList();
                        var TankAdditionalDesc = tankimagecardList.Where(d => d.DescriptionId != 0 && (d.AdditionalDescription == null || d.AdditionalDescription == "")).ToList();
                        var TankEmptyAdditionalDescriptionlist = TankAdditionalDesc.Where(a => TankAdditionalDescId.Any(b => b.Id == a.DescriptionId)).ToList();

                        if (TankEmptyAdditionalDescriptionlist.Count > 0)
                        {
                            additionaldescription = additionaldescription == "" ? string.Join(",", TankEmptyAdditionalDescriptionlist.Select(a => a.CardName)) : additionaldescription + "," + string.Join(",", TankEmptyAdditionalDescriptionlist.Select(a => a.CardName));
                        }
                        #endregion

                    }
                    #endregion

                    #region H & I Grading

                    if (ret.template != null && ret.template.handireport != null && ret.template.handireport.Grading != null && ret.template.handireport.Grading.Count > 0)
                    {
                        var handireport = ret.template.handireport.Grading.ToList();
                        var HRgradingcount = 0; var HRfinalcount = 0; var HRcheckbox = 0;
                        HRgradingcount = handireport.Count;
                        handireport.ForEach(e =>
                        {
                            e.Checkbox.ForEach(u =>
                            {
                                if (u.Value == true && HRcheckbox == 0)
                                {
                                    HRcheckbox++;
                                    HRfinalcount++;

                                }
                            });
                            HRcheckbox = 0;

                        });
                        if (HRgradingcount != HRfinalcount)
                        {
                            grading = grading + "Part H & I" + ",";
                        }

                    }
                    #endregion

                }

                grading = grading != "" ? grading.Remove(grading.Length - 1) : "";

                Allgrading = grading != "" ? Allgrading == "" ? grading : Allgrading + "," + grading : Allgrading;

                Allcurrentcondition = currentcondition != "" ? Allcurrentcondition == "" ? currentcondition : Allcurrentcondition + "," + currentcondition : Allcurrentcondition;

                Alldescription = description != "" ? Alldescription == "" ? description : Alldescription + "," + description : Alldescription;

                Alladditionaldescription = additionaldescription != "" ? Alladditionaldescription == "" ? additionaldescription : Alladditionaldescription + "," + additionaldescription : Alladditionaldescription;
                //}

                Result = Allcurrentcondition + "#" + Alldescription + "#" + Allgrading + "#" + Alladditionaldescription;


                return ServiceResult<string>.Success(Result);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Failure(ex.Message);
            }
        }

        public async Task<ServiceResult<int>> DeleteBulkUploadImage(int imageId)
        {
            var result = await _surveyReportRepository.DeleteBulkUploadImage(imageId);

            return ServiceResult<int>.Success(result);
        }

        public async Task<ServiceResult<bool>> DeleteImage(string assignmentId, string fileId)
        {
            var ret = await _surveyReportRepository.DeleteImage(assignmentId, fileId);
            return ServiceResult<bool>.Success(ret);
        }

        public async Task<ServiceResult<bool>> DeleteCard(int projectId, string cardId, string sequenceId, string assignmentId, string applicationId)
        {
            var ret = await _surveyReportRepository.DeleteCard(projectId, cardId, sequenceId, assignmentId, applicationId);
            return ServiceResult<bool>.Success(ret);
        }

        public async Task<ServiceResult<UploadPhotoResultDto>> UploadPhoto(IFormFile file, int projectId, string cardId, string sequenceId, string assignmentId, string applicationId, string label, string appName, string fileId, bool replaceImage = false)
        {
            if (file == null || file.Length == 0)
                return ServiceResult<UploadPhotoResultDto>
                    .Failure("Invalid file");

            string resizedImage;
            string newFileId = Guid.NewGuid().ToString();

            try
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                var fileBytes = ms.ToArray();
                resizedImage = ResizeImage(fileBytes);

                var imgId = await _surveyReportRepository.UploadImage(assignmentId, newFileId);

                await _surveyReportRepository.AddEditImageNew(imgId, resizedImage);

                if (replaceImage && !string.IsNullOrEmpty(fileId))
                {
                    await _surveyReportRepository.DeleteImage(assignmentId, fileId);
                }

                return ServiceResult<UploadPhotoResultDto>.Success(
                    new UploadPhotoResultDto
                    {
                        FileId = newFileId,
                        Image = resizedImage
                    });
            }
            catch (Exception ex)
            {
                return ServiceResult<UploadPhotoResultDto>.Failure("Failed to upload photo");
            }
        }

        public string ResizeImage(byte[] imageBytes)
        {
            int imageWidthForScale = 624;
            using (var ms = new MemoryStream(imageBytes))
            {
                var img = Image.FromStream(ms);
                var curHt = img.Height;
                var curWd = img.Width;
                var WdRed = imageWidthForScale / (double)curWd;
                var HtRed = (int)(curHt * WdRed);
                var destRect = new Rectangle(0, 0, imageWidthForScale, HtRed);
                var destImage = new Bitmap(imageWidthForScale, HtRed);

                destImage.SetResolution(img.HorizontalResolution, img.VerticalResolution);

                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(img, destRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }
                byte[] finalBytes;
                using (var msFinal = new MemoryStream())
                {
                    destImage.Save(msFinal, System.Drawing.Imaging.ImageFormat.Jpeg);
                    msFinal.Position = 0;
                    finalBytes = msFinal.ToArray();
                }
                return Convert.ToBase64String(finalBytes);
            }
        }

        public async Task<ServiceResult<int>> UploadUnPlacedPhoto(IFormFile uploadedFile, FileDataType dataType, int projectId, int taskId, string assignmentId)
        {
            string newfileId = Guid.NewGuid().ToString();
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    uploadedFile.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    var resizedImage = ResizeImage(fileBytes);
                    var imgId = await _surveyReportRepository.UploadImage(assignmentId, newfileId);
                    await _surveyReportRepository.AddEditImageNew(imgId, resizedImage);
                    _surveyReportRepository.InsertBulkUploadImage(imgId);
                    return ServiceResult<int>.Success(imgId);
                }
            }
            return ServiceResult<int>.Success(0);
        }
        public async Task<ServiceResult<bool>> UpdateGenericImageDescriptionDropdownCard(int templateId, int projectId, Guid sectionId, int cardId, int value, string cardName)
        {
            try
            {
                cardName = cardName.Trim();
                var imageCard = _projectReportRepository.GetGenericImagCardByName(projectId, templateId, sectionId, cardId).Result;
                if (imageCard != null && imageCard.Id != 0)
                {
                    imageCard.DescriptionId = value;
                    imageCard.AdditionalDescription = null;
                    imageCard.UpdatedDttm = DateTime.Now;
                    imageCard.CardName = cardName;
                    var result = _projectReportRepository.UpdateGenericImageCard(imageCard).Result;
                }
                else
                {
                    GenericImageCard tankImageCard = new GenericImageCard();
                    tankImageCard.ProjectId = projectId;
                    tankImageCard.TemplateId = templateId;
                    tankImageCard.SectionId = sectionId;
                    tankImageCard.CardNumber = cardId;
                    tankImageCard.DescriptionId = value;
                    tankImageCard.CreatedDttm = DateTime.Now;
                    tankImageCard.UpdatedDttm = DateTime.Now;
                    tankImageCard.IsActive = true;
                    tankImageCard.CardName = cardName;
                    tankImageCard.AdditionalDescription = null;
                    var result = await _projectReportRepository.CreateGenericImageCard(tankImageCard);

                }
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure("false");
            }
        }

        public async Task<ServiceResult<bool>> UpdateGenericAdditionalDescription(int templateId, int projectId, Guid sectionId, int cardId, string value, string cardName)
        {
            try
            {
                cardName = cardName.Trim();
                var imageCard = _projectReportRepository.GetGenericImagCardByName(projectId, templateId, sectionId, cardId).Result;
                if (imageCard != null && imageCard.Id != 0)
                {
                    imageCard.AdditionalDescription = value;
                    imageCard.UpdatedDttm = DateTime.Now;
                    imageCard.CardName = cardName;
                    var result = _projectReportRepository.UpdateGenericImageCard(imageCard).Result;
                }
                else
                {
                    GenericImageCard tankImageCard = new GenericImageCard();
                    tankImageCard.ProjectId = projectId;
                    tankImageCard.TemplateId = templateId;
                    tankImageCard.SectionId = sectionId;
                    tankImageCard.CardNumber = cardId;
                    tankImageCard.AdditionalDescription = value;
                    tankImageCard.CreatedDttm = DateTime.Now;
                    tankImageCard.UpdatedDttm = DateTime.Now;
                    tankImageCard.IsActive = true;
                    tankImageCard.CardName = cardName;
                    var result = await _projectReportRepository.CreateGenericImageCard(tankImageCard);

                }
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure("false");
            }
        }
        public async Task<ServiceResult<bool>> UpdateGenericCurrentCondition(int templateId, int projectId, Guid sectionId, int cardId, int value, string cardName)
        {
            try
            {
                cardName = cardName.Trim();
                var imageCard = _projectReportRepository.GetGenericImagCardByName(projectId, templateId, sectionId, cardId).Result;
                if (imageCard != null && imageCard.Id != 0)
                {
                    imageCard.CurrentCondition = value;
                    imageCard.UpdatedDttm = DateTime.Now;
                    imageCard.CardName = cardName;
                    var result = _projectReportRepository.UpdateGenericImageCard(imageCard).Result;
                }
                else
                {
                    GenericImageCard projectCardMapping = new GenericImageCard();
                    projectCardMapping.ProjectId = projectId;
                    projectCardMapping.TemplateId = templateId;
                    projectCardMapping.SectionId = sectionId;
                    projectCardMapping.CardNumber = cardId;
                    projectCardMapping.CurrentCondition = value;
                    projectCardMapping.CreatedDttm = DateTime.Now;
                    projectCardMapping.UpdatedDttm = DateTime.Now;
                    projectCardMapping.CardName = cardName;
                    projectCardMapping.IsActive = true;
                    var result = await _projectReportRepository.CreateGenericImageCard(projectCardMapping);

                }
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure("false");
            }
        }

        public async Task<ServiceResult<UploadPhotoResultDto>> UploadGenericImage(IFormFile file, int templateId, int projectId, Guid sectionId, int cardId, int imageId, bool replaceImage = false, string cardName = null)
        {
            cardName = cardName.Trim();
            string resizedImage = "";
            string newfileId = Guid.NewGuid().ToString();
            if (file != null && file.Length > 0)
            {
                var fileContent = ContentDispositionHeaderValue.Parse(file.ContentDisposition);

                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    resizedImage = ResizeImage(fileBytes);
                    var imgId = await _surveyReportRepository.UploadImage("unknown", newfileId);
                    await _surveyReportRepository.AddEditImageNew(imgId, resizedImage);
                    var imageCard = _projectReportRepository.GetGenericImagCardByName(projectId, templateId, sectionId, cardId).Result;
                    if (imageCard == null || imageCard.Id == 0)
                    {
                        GenericImageCard projectCardMapping = new GenericImageCard();

                        projectCardMapping.ProjectId = projectId;
                        projectCardMapping.TemplateId = templateId;
                        projectCardMapping.SectionId = sectionId;
                        projectCardMapping.CardNumber = cardId;
                        projectCardMapping.ImageId = imgId;
                        projectCardMapping.CurrentCondition = 0;
                        projectCardMapping.DescriptionId = 0;
                        projectCardMapping.AdditionalDescription = null;
                        projectCardMapping.IsActive = true;
                        projectCardMapping.CreatedDttm = DateTime.Now;
                        projectCardMapping.UpdatedDttm = DateTime.Now;
                        projectCardMapping.CardName = cardName;
                        var result = await _projectReportRepository.CreateGenericImageCard(projectCardMapping);
                    }
                    else
                    {
                        imageCard.UpdatedDttm = DateTime.Now;
                        imageCard.CreatedDttm = DateTime.Now;
                        imageCard.ImageId = imgId;
                        imageCard.CardName = cardName;
                        var result = await _projectReportRepository.UpdateGenericImageCard(imageCard);
                    }

                }
            }

            return ServiceResult<UploadPhotoResultDto>.Success(
                   new UploadPhotoResultDto
                   {
                       FileId = newfileId,
                       Image = resizedImage
                   });
        }

        public async Task<ServiceResult<bool>> DeleteGenericImage(int templateId, int projectId, Guid sectionId, int cardId, int imageId)
        {
            var imageCard = _projectReportRepository.GetGenericImagCardByName(projectId, templateId, sectionId, cardId).Result;
            if (imageCard.ImageId != 0)
            {
                imageCard.ImageId = 0;
                imageCard.UpdatedDttm = DateTime.Now;
                var result = _projectReportRepository.UpdateGenericImageCard(imageCard).Result;
            }
            return ServiceResult<bool>.Success(true);
        }


    }
}
