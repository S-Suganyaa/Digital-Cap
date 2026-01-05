using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.Grading;
using DigitalCap.Core.Models.Permissions;
using DigitalCap.Core.Models.Survey;
using DigitalCap.Core.Models.Tank;
using DigitalCap.Core.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
        public SurveyReportService(ILogger<SurveyReportService> logger, IConfiguration configuration,
            IUserPermissionRepository userPermissionRepository, ITransferDataOnlinetoOfflineRepository transferDataOnlinetoOfflineRepository,
            IUserAccountRepository userAccountRepository, ISecurityService securityService,
            ISurveyReportRepository surveyReportRepository, IProjectRepository projectRepository,
            ITankRepository tankRepository, IVesselRepository vesselRepository, IDescriptionRepository descriptionRepository)
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
        }
        //public async Task<IActionResult> Index(int projectId, int? templateSectionId = null)
        //{
        //    if (projectId >= _configuration.GetValue<int>("DefaultTemplateProjectId"))
        //    {
        //        return projectId;
        //    }

        //    HttpContext.Session.Clear();

        //    var templateGuid = Guid.NewGuid();
        //    var currentUserName = User.Identity?.Name;

        //    if (string.IsNullOrEmpty(currentUserName))
        //    {
        //        return Unauthorized();
        //    }

        //    var internalUser = await _securityService.GetCurrentUser();

        //    var reportAccess =
        //        await _userPermissionRepository.GetRolePermissionByUserName(
        //            currentUserName,
        //            EnumExtensions.GetDescription(ManagePages.ReportEdit),
        //            projectId);

        //    var isSync =
        //        await _transferDataOnlinetoOfflineRepository.GetDownloadOfflineProjects(projectId);

        //    if (isSync != null)
        //    {
        //        var userAccount =
        //            await _userAccountRepository.GetByAspNetId(isSync.UserId);

        //        if (userAccount != null)
        //        {
        //            isSync.Name = $"{userAccount.FirstName} {userAccount.LastName}";
        //        }
        //    }

        //    if (Convert.ToBoolean(reportAccess?.Edit) || Convert.ToBoolean(reportAccess?.Read))
        //    {
        //        var model = await PrepareSurveyReportViewModel(projectId, null);

        //        model.ReportExport =
        //            (await _userPermissionRepository.GetRolePermissionByUserName(
        //                currentUserName,
        //                EnumExtensions.GetDescription(ManagePages.ReportExport),
        //                projectId))?.Edit;

        //        model.ReportExportAll =
        //            (await _userPermissionRepository.GetRolePermissionByUserName(
        //                currentUserName,
        //                EnumExtensions.GetDescription(ManagePages.ReportExportAll),
        //                projectId))?.Edit;

        //        model.ResetTemplate =
        //            (await _userPermissionRepository.GetRolePermissionByUserName(
        //                currentUserName,
        //                EnumExtensions.GetDescription(ManagePages.ResetTemplate),
        //                projectId))?.Edit;

        //        model.IsSynched = isSync?.IsSynched ?? true;
        //        model.Report.IsSynched = isSync?.IsSynched ?? true;
        //        model.SynchedOnline = isSync;

        //        return Ok(new
        //        {
        //            TemplateGuid = templateGuid,
        //            Data = model
        //        });
        //    }

        //    return Forbid();
        //}

        //public async Task<IActionResult> IndexPartial(int projectId, int? templateSectionId = null)
        //{
        //    var surveyReport = PrepareSurveyReportViewModel(projectId, null).Result;
        //    var isReportExist = surveyReport.ReportPartGrid.Where(x => x.TemplateId == templateSectionId).Select(x => x.ReportPartExists).FirstOrDefault();
        //    var isSync = await _transferDataOnlinetoOfflineRepository.GetDownloadOfflineProjects(projectId);
        //    if (!isReportExist)
        //    {
        //        var isCreated = CreateReport(projectId, templateSectionId.Value).Result;
        //    }

        //    SurveyReportViewModel model = await PrepareSurveyReportViewModel(projectId, templateSectionId);
        //    model.IsSynched = isSync == null ? true : isSync.IsSynched;
        //    model.Report.IsSynched = isSync == null ? true : isSync.IsSynched;
        //    return PartialView("_ReportPartial", model.Report);
        //}


        private async Task<ServiceResult<SurveyReportViewModel>> PrepareSurveyReportViewModel(int projectId, int? templateSectionId)
        {
            var sectionResult =
                await GetSectionIdsByProjectId(projectId, templateSectionId);

            if (!sectionResult.IsSuccess || sectionResult.Data == null)
            {
                return ServiceResult<SurveyReportViewModel>
                    .Failure("Failed to get section ids");
            }

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

            return ServiceResult<SurveyReportViewModel>.Success(model);
        }

        
        private async Task<ServiceResult<SurveyReportViewModel>> GetSectionIdsByProjectId(int projectId, int? templateId)
        {
            var model = new SurveyReportViewModel();
            model.ProjectId = projectId;
            model.Report = new Report();
            model.Report.template = new View();

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
                View newReport = new View();

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

    }
}
