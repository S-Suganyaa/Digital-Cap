using Azure;
using Azure.Core;
using DigitalCap.Core;
using DigitalCap.Core.Enumerations;
using DigitalCap.Core.Helpers.Constants;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.Permissions;
using DigitalCap.Core.Models.Survey;
using DigitalCap.Core.ViewModels;
using DigitalCap.Infrastructure.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Web.Mvc;
using Section = DigitalCap.Core.ViewModels.Section;

namespace DigitalCap.Infrastructure.Service
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ITankRepository _tankRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IVesselService _vesselService;
        private readonly ISurveyReportRepository _surveyReportRepository;
        private readonly IReportPartRepository _reportPartRepository;
        private readonly IGradingService _gradingService;
        private readonly IDescriptionRepository _descriptionRepository;
        private readonly IUserPermissionRepository _userPermissionRepository;
        private readonly IFileStorageRepository _fileStorageRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISyncRepository _syncRepository;
        private readonly ISecurityService _securityService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClientRepository _clientRepository;
        private readonly IGradeRepository _gradeRepository;
        private readonly ITransferDataOnlinetoOfflineRepository _transferDataOnlinetoOfflineRepository;
        private readonly IUserAccountRepository _userAccountRepository;

        public object User { get; private set; }

        public ProjectService(IProjectRepository projectRepository, ITankRepository tankRepository
            , ITaskRepository taskRepository, IVesselService vesselService
            , ISurveyReportRepository surveyReportRepository, IReportPartRepository reportPartRepository
, IGradingService gradingService, IDescriptionRepository descriptionRepository
            , IUserPermissionRepository userPermissionRepository,
            IFileStorageRepository fileStorageRepository, IUserRepository userRepository
, ISyncRepository syncRepository, ISecurityService securityService, UserManager<ApplicationUser> userManager,
            IClientRepository clientRepository, IGradeRepository gradeRepository
            , ITransferDataOnlinetoOfflineRepository transferDataOnlinetoOfflineRepository, IUserAccountRepository userAccountRepository)
        {
            _projectRepository = projectRepository;
            _tankRepository = tankRepository;
            _taskRepository = taskRepository;
            _vesselService = vesselService;
            _surveyReportRepository = surveyReportRepository;
            _reportPartRepository = reportPartRepository;
            _gradingService = gradingService;
            _descriptionRepository = descriptionRepository;
            _userPermissionRepository = userPermissionRepository;
            _fileStorageRepository = fileStorageRepository;
            _userRepository = userRepository;
            _syncRepository = syncRepository;
            _securityService = securityService;
            _userManager = userManager;
            _clientRepository = clientRepository;
            _gradeRepository = gradeRepository;
            _transferDataOnlinetoOfflineRepository = transferDataOnlinetoOfflineRepository;
            _userAccountRepository = userAccountRepository;
        }


        public async Task<ServiceResult<int>> CreateProject(Project model, string Name)
        {
            Project copyproject = new();
            var ImoExists = await _projectRepository.CheckProjectExistsByImoNumber(Convert.ToInt32(model.IMO));
            if (ImoExists)
            {
                var tanksAlreadyExist = await _tankRepository.GetTanks_VesselByIMO(model.IMO.ToString());
                if (tanksAlreadyExist?.Count > 0)
                {
                    var LatestTankDetail = tanksAlreadyExist.OrderByDescending(x => x.ProjectId).FirstOrDefault();

                }
                model.CopyingVesselID = Convert.ToInt32(_projectRepository.GetProjectListByIMO(model.IMO).Result.Max(x => x.Value));

                // ModelState.ClearValidationState("CopyingVesselID");
                // ModelState.MarkFieldValid("CopyingVesselID");
            }

            //if (!ModelState.IsValid) Need to ask this part
            //{
            //    ViewBag.Title = "Create Project";
            //    ViewBag.SaveButtonText = "Create";
            //    return View("Edit", model);
            //}

            var vesseltype = _tankRepository.GetShipType().Result.Where(x => x.Id == (int)model.ShipType).Select(x => x.VesselType).FirstOrDefault();

            model.VesselType = vesseltype;

            var projectId = _projectRepository.CreateProject(model).Result;


            if (ImoExists)
            {
                model.TankVesselIMO = Convert.ToInt32(model.IMO.ToString());
                var createTank = _tankRepository.PopulateTemplate(model.IMO.ToString(), null, model.IMO.ToString(), projectId, Convert.ToInt32(model.CopyingVesselID));
                copyproject = _projectRepository.GetProject(Convert.ToInt16(model.ID)).Result;
            }
            else
            {
                if (model.IsDefaultTank == true)
                {
                    model.TankVesselType = model.ShipType.ToString();
                    var createTank = _tankRepository.PopulateTemplate(model.IMO.ToString(), vesseltype, null, projectId);
                }
                else
                {
                    copyproject = _projectRepository.GetProject(Convert.ToInt16(model.CopyingVesselID)).Result;

                    var tankAvailable = _tankRepository.GetTanks_VesselByProject(copyproject.IMO.ToString(), Convert.ToInt16(model.CopyingVesselID)).Result;
                    if (tankAvailable.Count() > 0)

                    {
                        model.TankVesselIMO = Convert.ToInt32(copyproject.IMO);
                        var createTank = _tankRepository.PopulateTemplate(model.IMO.ToString(), null, copyproject.IMO.ToString(), projectId, Convert.ToInt16(model.CopyingVesselID));

                    }
                }
            }

            await UpdateProjectPercentComplete(projectId);
            var projectdata = _projectRepository.GetProject(projectId).Result;

            var classNum = "";
            if (model.AbsClassID != null)
            {
                var vesselResult = await _vesselService.CreateVesselMainData(model.AbsClassID?.ToString(), projectdata);

                if (!vesselResult.IsSuccess)
                {
                    // log vesselResult.ErrorMessage
                    return ServiceResult<int>.Failure("Vessel creation failed");
                }

                classNum = vesselResult.Data;



                //classNum = _vesselService.CreateVesselMainData(model?.AbsClassID.ToString(), projectdata);
            }
            else
            {
                //  classNum = _vesselService.CreateVesselMainData("", projectdata).Result;
                var vesselResult = await _vesselService.CreateVesselMainData("", projectdata);

                if (!vesselResult.IsSuccess)
                {
                    // log vesselResult.ErrorMessage
                    return ServiceResult<int>.Failure("Vessel creation failed");
                }

                classNum = vesselResult.Data;

            }
            int imo = 0;
            if (model.IMO != null)
            {
                imo = Convert.ToInt32(model.IMO);

            }
            try
            {
                if (!string.IsNullOrEmpty(classNum))
                {
                    Report report = new Report();

                    report.SurveyStatus = await _surveyReportRepository.MapSurveyStatus(classNum, imo);
                    report = await _surveyReportRepository.MapStatuatoryCertificates(classNum, report);
                    await _vesselService.CreateSurveyAudit(report.SurveyStatus, Convert.ToInt32(model.ID));
                    await _vesselService.CreateStatutoryCertificate(Convert.ToInt32(projectId), report);
                }
            }

            catch (Exception ex)
            { }
            var templateresult = _reportPartRepository.CreateProjectReportTemplate((int)model.ShipType, Name, projectId, ImoExists, (ImoExists ? Convert.ToInt32(model.CopyingVesselID) : 0)).Result;
            var gradingresult = _gradingService.PopulateGrading(vesseltype, projectId).Result;
            var result = _descriptionRepository.CreateProjectImageDescription(projectId, vesseltype).Result;

            return ServiceResult<int>.Success((1));
        }

        public async Task<ServiceResult<byte>> UpdateProjectPercentComplete(int projectId)
        {
            var percentComplete = await _taskRepository.UpdateProjectPercentComplete(projectId);

            await _projectRepository.UpdatePercentComplete(projectId, percentComplete);

            return ServiceResult<byte>.Success(percentComplete);
        }
        public async Task<ServiceResult<IEnumerable<ProjectDetailItem>>> GetDetails(int id)
        {
            var result = await _projectRepository.GetDetails(id);
            return ServiceResult<IEnumerable<ProjectDetailItem>>.Success(result);
        }
        public async Task<ServiceResult<Project>> UpdateProject(int id, string currentUser)
        {

            //  if (string.IsNullOrEmpty(currentUser))
            //    return Unauthorized();

            var permission = await _userPermissionRepository
                .GetRolePermissionByUserName(currentUser,
                    EnumExtensions.GetDescription(ManagePages.ViewProject),
                    id);

            var canView = permission?.Read ?? false;
            var canEdit = permission?.Edit ?? false;

            //if (!canView && !canEdit)
            //    return Forbid(); // 403

            if (canView == true || canEdit == true)
            {
                var model = await _projectRepository.GetProject(id);

                return ServiceResult<Project>.Success(model);
            }

            else
            {
                return ServiceResult<Project>.Failure("");
            }

            // Prepare API-friendly response
            //var response = new
            //{
            //    Project = result,
            //    Permissions = new
            //    {
            //        CanView = canView,
            //        CanEdit = canEdit
            //    },
            //    OtherClassSocietyExists = result.ClassSociety
            //        ?.Any(x => x.Equals("OTHER", StringComparison.OrdinalIgnoreCase)) ?? false
            //};


        }

        public async Task<ServiceResult<int>> Update_Project(Project model, string currentUser)
        {
            if (model == null)
                return ServiceResult<int>.Failure("Invalid project data");

            var projectName = await _projectRepository.GetProjectName(model.Id);

            // If project not found
            if (string.IsNullOrWhiteSpace(projectName))
                return ServiceResult<int>.Failure("Project not found");

            // Check project name change (safe comparison)
            if (!string.Equals(model.Name, projectName, StringComparison.OrdinalIgnoreCase))
            {
                var isNameExists =
                    await _projectRepository.CheckProjectExistsByProjectName(
                        model.Name.Trim(),
                        model.IMO ?? 0,
                        (int)model.ShipType);

                if (isNameExists)
                {
                    return ServiceResult<int>.Failure(
                        "Project already exists with this Project Name");
                }
            }

            // Get vessel type
            var vesselType = (await _tankRepository.GetShipType())
                .Where(x => x.Id == (int)model.ShipType)
                .Select(x => x.VesselType)
                .FirstOrDefault();

            model.VesselType = vesselType;

            // Update project
            await _projectRepository.Update_Project(model);

            return ServiceResult<int>.Success(model.Id);
        }
        public async Task<ServiceResult<int>> UpdateProjectStatus(int id, string currentUserId, int ProjectStatus)
        {
            await _projectRepository.UpdateProjectStatus(id, currentUserId, ProjectStatus);
            return ServiceResult<int>.Success(id);
        }

        public async Task<ServiceResult<int>> DeleteProject(int id, string currentUserId)
        {
            var projectFiles = await _fileStorageRepository.GetProjectFiles(id);

            var currentUser = await _userRepository.GetLoggedInUserName();

            foreach (var file in projectFiles)
            {
                await _fileStorageRepository.DeleteFile(file.Id, currentUser);
            }

            await _projectRepository.DeleteProject(id, currentUserId);
            return ServiceResult<int>.Success(id);

        }
        public async Task<ServiceResult<UpdateProjectPriorityViewModel>> GetSummary(int id)
        {
            var project = await _projectRepository.GetSummary(id);

            //if (project == null)
            //    return new BadRequestResult();

            var model = new UpdateProjectPriorityViewModel
            {
                Id = project.ID.Value,
                Priority = project.ProjectPriority
            };

            return ServiceResult<UpdateProjectPriorityViewModel>.Success(model);
        }
        public async Task UpdatePriority(int id, int? priority)
        {
            await _projectRepository.UpdatePriority(id, priority);
        }

        public async Task CancelProject(int projectId, string userId)
        {
            await _projectRepository.UpdateProjectStatus(
                projectId,
                userId,
                (int)ProjectStatus.Cancelled);
        }
        public async Task<ServiceResult<object>> GetCompletionStatus(int id, ProjectStatusType statusType)
        {
            var summary = await _projectRepository.GetSummary(id);

            if (summary == null)
                return ServiceResult<object>.Failure("Invalid.");

            var result = new ProjectStatusViewModel
            {
                ProjectId = id,
                ProjectPriority = summary.ProjectPriority
            };

            switch (statusType)

            {
                case ProjectStatusType.Invoicing:
                    {
                        result.CompletionPercentage = summary.InvoiceTasksCompletionPercentage;
                        result.DaysWorked = summary.InvoiceDaysWorked;
                    }
                    break;
                case ProjectStatusType.Technical:
                    {
                        result.CompletionPercentage = summary.TechnicalTasksCompletionPercentage;
                        result.DaysWorked = summary.TechnicalDaysWorked;
                    }
                    break;
            }
            return ServiceResult<object>.Success(new { result });
        }

        public async Task<ServiceResult<object>> GetAllUsers()
        {
            var result = _userRepository.GetAllUsers().Result;
            return ServiceResult<object>.Success(result);
        }
        public async Task<ServiceResult<object>> GetList(string currentUser, bool isElectron)
        {
            // NON-ELECTRON FLOW
            if (!isElectron)
            {
                var addProjectPermission =
                    await _userPermissionRepository.GetRolePermissionByUserName(
                        currentUser,
                        EnumExtensions.GetDescription(ManagePages.AddProject));

                var exportProjectPermission =
                    await _userPermissionRepository.GetRolePermissionByUserName(
                        currentUser,
                        EnumExtensions.GetDescription(ManagePages.ExportProject));

                var projectPermissions = new ProjectPermissions
                {
                    AddProject = addProjectPermission?.Edit ?? false,
                    ExportProject = exportProjectPermission?.Edit ?? false
                };

                return ServiceResult<object>.Success(projectPermissions);
            }

            // ELECTRON FLOW
            var user = await _syncRepository.GetUserInfo();

            var results = (await _projectRepository.GetAllProjectsGridData()).ToList();

            if (results.Any())
            {
                return ServiceResult<object>.Success(results);
            }

            // IMPORTANT: handle empty result
            return ServiceResult<object>.Success(new List<AllProjectsGrid>());
        }

        public async Task<ServiceResult<List<AllProjectsGrid>>> GetAllProjectsGridData(bool isElectron, string userName)
        {
            var results = (await _projectRepository.GetAllProjectsGridData()).ToList();

            foreach (var project in results)
            {
                if (project.PercentComplete == null && project.ID.HasValue)
                {
                    var percentResult =
                        await UpdateProjectPercentComplete(project.ID.Value);

                    if (percentResult.IsSuccess)
                    {
                        project.PercentComplete = percentResult.Data;
                    }
                }
            }

            if (!isElectron)
            {
                var currentUserResult = await _securityService.GetCurrentUser();

                if (!currentUserResult.IsSuccess || currentUserResult.Data == null)
                {
                    return ServiceResult<List<AllProjectsGrid>>
                        .Failure("User not found");
                }

                var roles = await _userManager.GetRolesAsync(currentUserResult.Data);

                bool isAdmin = roles.Contains("Admin (CAP HQ)");
                bool isContributor = roles.Contains("Contributor (CAP Coordinator)");

                if (!isAdmin && !isContributor)
                {
                    var allowedProjectIds =
                        await _userPermissionRepository
                            .GetCurrentUserProjects(userName);

                    results = results
                        .Where(r => r.ID.HasValue &&
                                    allowedProjectIds.Contains(r.ID.Value))
                        .ToList();
                }
            }

            return ServiceResult<List<AllProjectsGrid>>.Success(results);
        }

        public async Task<ServiceResult<List<Client>>> GetAllClients(bool wcnList)
        {
            var clients = _clientRepository.GetAll().ToList();
            if (wcnList)
            {
                clients = clients.Where(c => !string.IsNullOrEmpty(c.Wcn)).ToList();
            }

            return ServiceResult<List<Client>>.Success(clients);
        }
        public async Task<ServiceResult<List<string>>> GetAllSurveyors(bool wcnList)
        {
            var users = _userRepository.GetAllUsers().Result.Select(x => x.UserName).ToList();

            return ServiceResult<List<string>>.Success(users);
        }
        public async Task<ServiceResult<WorkflowViewModel>> Workflow(int? id)
        {
            if (!id.HasValue)
                return ServiceResult<WorkflowViewModel>.Failure("Project id is required");

            var tasks = (await _taskRepository.GetCap2Tasks(id.Value))
                .Where(x => x.Category == CategoryNames.Project);

            var groups = await _taskRepository.GetStatusGroups(CategoryNames.Project);

            ProgressModel progressModel = new ProgressModel(tasks, groups);

            var taskViewModel = new TasksViewModel(id, tasks);
            taskViewModel.ProjectName = await _projectRepository.GetProjectName(id.Value);

            WorkflowViewModel model = new WorkflowViewModel
            {
                ProgressModel = progressModel,
                TasksViewModel = taskViewModel,
                PercentComplete = CalculatePercentComplete(taskViewModel.Sections)
            };

            return ServiceResult<WorkflowViewModel>.Success(model);
        }


        private static byte CalculatePercentComplete(List<Section> sections)
        {
            const int STARTING_PROGRESS = 4;
            return (byte)(STARTING_PROGRESS + sections.Sum(x => x.Tasks.Where(x => x.PercentageComplete > 0 && x.Status == CapTaskStatus.Completed.ToString()).Sum(x => x.PercentageComplete)));
        }


        public async Task<ServiceResult<ProjectLandingViewModel>> Landing(int? id, string currentuser)
        {
            var tasks = await _taskRepository.GetCap2Tasks(id.Value);
            var projectTasks = tasks.Where(x => x.Category == CategoryNames.Project);
            var invoiceSum = tasks.Where(t => t.Type == TypeNames.ClientInvoices).Select(i => i.Value).Sum();
            var projectGrading = await _gradeRepository.GetProjectGrades(id.Value);

            if (projectGrading == null)
            {
                var projectGradesId = await _gradeRepository.CreateProjectGrades(id.Value);
                var cap1Tasks = await _taskRepository.GetTasks(id.Value);
                projectGrading = new ProjectGrades
                {
                    CAP1CertificateIssuanceDate = cap1Tasks.FirstOrDefault(x => x.Task == "CAP Grade Received from CAPCOM")?.StatusDate,
                    CAP1FinalGrade = cap1Tasks.FirstOrDefault(x => x.Task == "CAP Grade Received from CAPCOM")?.Notes,
                    ProjectId = id.Value,
                    Id = projectGradesId
                };

                await _gradeRepository.UpdateProjectGrades(projectGrading);
            }

            var projectDetails = await _projectRepository.GetProject(id.Value);

            var projectDetailsViewModel = new ProjectDetailsViewModel(projectDetails);
            var groups = await _taskRepository.GetStatusGroups(CategoryNames.Project);

            ProgressModel progressModel = new ProgressModel(projectTasks, groups, projectDetails.CreatedDate);

            var taskViewModel = new TasksViewModel(id, projectTasks);

            var projectFiles = (await _fileStorageRepository.GetProjectFiles(id.Value)).OrderByDescending(f => f.LastModifiedDate).Take(4);
            var viewproject = _userPermissionRepository.GetRolePermissionByUserName(currentuser, EnumExtensions.GetDescription(ManagePages.ViewProject), id).Result;
            var reportEdit = _userPermissionRepository.GetRolePermissionByUserName(currentuser, EnumExtensions.GetDescription(ManagePages.ReportEdit), id).Result;
            var projectfiles = _userPermissionRepository.GetRolePermissionByUserName(currentuser, EnumExtensions.GetDescription(ManagePages.ProjectFiles), id).Result;
            var revenue = _userPermissionRepository.GetRolePermissionByUserName(currentuser, EnumExtensions.GetDescription(ManagePages.RevenueTracker), id).Result;

            var reportdeatils = new ReportDetailsViewModel(projectGrading);
            reportdeatils.ReportEdit = Convert.ToBoolean(reportEdit?.Edit) || Convert.ToBoolean(reportEdit?.Read) || Convert.ToBoolean(reportEdit?.Delete);
            var isSync = await _transferDataOnlinetoOfflineRepository.GetDownloadOfflineProjects(id.Value);
            if (isSync != null)
            {
                var username = currentuser;
                var currentUser = await _securityService.GetCurrentUser();
                UserAccountModel userAccount = await _userAccountRepository.GetByAspNetId(isSync.UserId);
                if (userAccount != null)
                {
                    isSync.Name = userAccount.FirstName + " " + userAccount.LastName;
                }
            }

            ProjectLandingViewModel model = new ProjectLandingViewModel
            {
                ProgressModel = progressModel,
                ProjectDetailsViewModel = projectDetailsViewModel,
                TasksViewModel = taskViewModel,
                ProjectFiles = projectFiles,
                ReportDetailsViewModel = reportdeatils,
                PercentComplete = CalculatePercentComplete(taskViewModel.Sections),
                InvoiceSum = invoiceSum,

                ViewProject = Convert.ToBoolean(viewproject?.Edit) || Convert.ToBoolean(viewproject?.Read) || Convert.ToBoolean(viewproject?.Delete),

                FileDetails = Convert.ToBoolean(projectfiles?.Edit) || Convert.ToBoolean(projectfiles?.Read) || Convert.ToBoolean(projectfiles?.Delete),
                RevenueTracker = Convert.ToBoolean(revenue?.Edit) || Convert.ToBoolean(revenue?.Read) || Convert.ToBoolean(revenue?.Delete),
                SynchedOnline = isSync

            };
            //ViewData["HideVesselProject"] = true;
            //ViewData["ActionName"] = "GetRecentActivities2ByProjectId";
            //ViewData["ProjectId"] = model.ProjectDetailsViewModel.Id;
            //ViewData["ClientPage"] = false;
            //ViewData["ReportDetailsAndWorkflowComplete"] = false;

            //TempData["HideVesselProject"] = true;
            //TempData["ActionName"] = "GetRecentActivities2ByProjectId";
            //TempData["ProjectId"] = model.ProjectDetailsViewModel.Id;
            //TempData["ClientPage"] = false;
            //TempData["ReportDetailsAndWorkflowComplete"] = false;

            return ServiceResult<ProjectLandingViewModel>.Success(model);
        }

        public async Task<ServiceResult<ProjectLandingViewModel>> ClientLanding(int? id)
        {
            var tasks = await _taskRepository.GetCap2Tasks(id.Value);
            var projectTasks = tasks.Where(x => x.Category == CategoryNames.Project);
            var projectGrading = await _gradeRepository.GetProjectGrades(id.Value);

            if (projectGrading == null)
            {
                var projectGradesId = await _gradeRepository.CreateProjectGrades(id.Value);
                var cap1Tasks = await _taskRepository.GetTasks(id.Value);
                projectGrading = new ProjectGrades
                {
                    CAP1CertificateIssuanceDate = cap1Tasks.FirstOrDefault(x => x.Task == "CAP Grade Received from CAPCOM")?.StatusDate,
                    CAP1FinalGrade = cap1Tasks.FirstOrDefault(x => x.Task == "CAP Grade Received from CAPCOM")?.Notes,
                    ProjectId = id.Value,
                    Id = projectGradesId
                };

                await _gradeRepository.UpdateProjectGrades(projectGrading);
            }

            var projectDetails = await _projectRepository.GetProject(id.Value);

            var capCoordinators = await _projectRepository.GetCAPCoordinator(projectDetails.CapRegion, true);

            var projectDetailsViewModel = new ProjectDetailsViewModel(projectDetails, capCoordinators);
            var groups = await _taskRepository.GetStatusGroups(CategoryNames.Project);

            ProgressModel progressModel = new ProgressModel(projectTasks, groups, projectDetails.CreatedDate);

            var projectFiles = (await _fileStorageRepository.GetProjectFiles(id.Value)).OrderByDescending(f => f.LastModifiedDate).Take(3);

            ProjectLandingViewModel model = new ProjectLandingViewModel
            {
                ProgressModel = progressModel,
                ProjectDetailsViewModel = projectDetailsViewModel,
                ProjectFiles = projectFiles,
                ReportDetailsViewModel = new ReportDetailsViewModel(projectGrading)
            };

            return ServiceResult<ProjectLandingViewModel>.Success(model);
        }

        public async Task<ServiceResult<bool>> SaveReportDetails(ReportDetailsViewModel reportDetails)
        {
            try
            {
                var projectGrades = new ProjectGrades
                {
                    Id = reportDetails.GradesId,
                    ProjectId = reportDetails.ProjectId,
                    CAPCertificateNumber = reportDetails.CAPCertificateNumber,
                    CAPCertificateIssuanceDate = reportDetails.CAPCertificateIssuanceDate,
                    ClassReportNumber = reportDetails.ClassReportNumber,
                    ClassReportDate = reportDetails.ClassReportDate,
                    StructuralReportNumber = reportDetails.StructuralReportNumber,
                    StructuralReportDate = reportDetails.StructuralReportDate,
                    FinalGrade = reportDetails.FinalGrade,
                    StructuralGrade = reportDetails.StructuralGrade,
                    FatigueGrade = reportDetails.FatigueGrade,
                    RenewalGrade = reportDetails.RenewalGrade,
                    MaterialGrade = reportDetails.MaterialGrade,
                    GaugingGrade = reportDetails.GaugingGrade,
                    HullGirderStrength = reportDetails.HullGirderStrength,
                    CAP1CertificateIssuanceDate = reportDetails.CAP1CertificateIssuanceDate,
                    CAP1FinalGrade = reportDetails.CAP1FinalGrade
                };

                await _gradeRepository.UpdateProjectGrades(projectGrades);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                // Log exception here
                return ServiceResult<bool>.Failure(ex.Message);
            }
        }

        public async Task<ServiceResult<string>> RecentActivityForProject(int projectId)
        {
            var projectName = await _projectRepository.GetProjectName(projectId);

            return ServiceResult<string>.Success(projectName);
        }

        public async Task<ServiceResult<string>> GetProjectName(int id)
        {
            var projectName = await _projectRepository.GetProjectName(id);

            return ServiceResult<string>.Success(projectName);
        }
        //RecentActivityForProject
    }
}
