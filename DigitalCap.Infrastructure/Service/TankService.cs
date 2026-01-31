using DigitalCap.Core.DTO;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.Permissions;
using DigitalCap.Core.Models.ReportConfig;
using DigitalCap.Core.Models.Tank;
using DigitalCap.Core.Models.VesselModel;
using DigitalCap.Core.Models.View.Admin;
using DigitalCap.Core.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DigitalCap.Core.Security.Extensions;


namespace DigitalCap.Infrastructure.Service
{
    public class TankService : ITankService
    {
        private readonly ITankRepository _tankRepository;
        private readonly ISurveyReportRepository _surveyReportRepository;
        private readonly ISurveyReportService _surveyReportService;
        private readonly IConfiguration _configuration;
        private readonly IProjectRepository _projectRepository;
        private readonly ISecurityService _securityService;
        private readonly IUserPermissionRepository _userPermissionRepository;
        private readonly IProjectService _projectService;
        private readonly Microsoft.AspNetCore.Identity.UserManager<DigitalCap.WebApi.Core.ApplicationUser> _userManager;

        public TankService(ITankRepository tankRepository, ISurveyReportRepository surveyReportRepository
            , ISurveyReportService surveyReportService, IConfiguration configuration,
            IProjectRepository projectRepository, ISecurityService securityService,
            IUserPermissionRepository userPermissionRepository, IProjectService projectService,
            Microsoft.AspNetCore.Identity.UserManager<DigitalCap.WebApi.Core.ApplicationUser> userManager)
        {
            _tankRepository = tankRepository;
            _surveyReportRepository = surveyReportRepository;
            _surveyReportService = surveyReportService;
            _configuration = configuration;
            _projectRepository = projectRepository;
            _securityService = securityService;
            _userPermissionRepository = userPermissionRepository;
            _projectService = projectService;
            _userManager = userManager;
        }

        public async Task<ServiceResult<bool>> UpdateTankImageDescriptionDropdownCard(int templateId, int projectId, Guid sectionId, int cardNumber, int value, string cardName)
        {
            try
            {
                cardName = cardName.Trim();
                var imageCard = _tankRepository.GetProjectTankImagCardByName(projectId, templateId, sectionId, cardNumber).Result;
                if (imageCard != null && imageCard.Id != 0)
                {
                    imageCard.DescriptionId = value;
                    imageCard.AdditionalDescription = null;
                    imageCard.UpdatedDttm = DateTime.Now;
                    imageCard.CardName = cardName;
                    var result = _tankRepository.UpdateProjectTankImageCard(imageCard).Result;
                }
                else
                {
                    TankImageCard tankImageCard = new TankImageCard();
                    tankImageCard.ProjectId = projectId;
                    tankImageCard.TemplateId = templateId;
                    tankImageCard.SectionId = sectionId;
                    tankImageCard.CardNumber = cardNumber;
                    tankImageCard.DescriptionId = value;
                    tankImageCard.CreatedDttm = DateTime.Now;
                    tankImageCard.UpdatedDttm = DateTime.Now;
                    tankImageCard.IsActive = true;
                    tankImageCard.CardName = cardName;
                    tankImageCard.AdditionalDescription = null;
                    var result = await _tankRepository.CreateProjectTankImageCard(tankImageCard);

                }
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure("false");
            }
        }

        public async Task<ServiceResult<bool>> UpdateTankCardAdditionalDescription(int templateId, int projectId, Guid sectionId, int cardNumber, string value, string cardName)
        {
            try
            {
                cardName = cardName.Trim();
                var imageCard = _tankRepository.GetProjectTankImagCardByName(projectId, templateId, sectionId, cardNumber).Result;
                if (imageCard != null && imageCard.Id != 0)
                {
                    imageCard.AdditionalDescription = value;
                    imageCard.UpdatedDttm = DateTime.Now;
                    imageCard.CardName = cardName;
                    var result = _tankRepository.UpdateProjectTankImageCard(imageCard).Result;
                }
                else
                {
                    TankImageCard tankImageCard = new TankImageCard();
                    tankImageCard.ProjectId = projectId;
                    tankImageCard.TemplateId = templateId;
                    tankImageCard.SectionId = sectionId;
                    tankImageCard.CardNumber = cardNumber;
                    tankImageCard.AdditionalDescription = value;
                    tankImageCard.CreatedDttm = DateTime.Now;
                    tankImageCard.UpdatedDttm = DateTime.Now;
                    tankImageCard.IsActive = true;
                    tankImageCard.CardName = cardName;
                    var result = await _tankRepository.CreateProjectTankImageCard(tankImageCard);

                }
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure("false");
            }
        }

        public async Task<ServiceResult<bool>> UpdateTankCardCurrentCondition(int templateId, int projectId, Guid sectionId, int cardNumber, int value, string cardName)
        {
            try
            {
                cardName = cardName.Trim();
                var imageCard = _tankRepository.GetProjectTankImagCardByName(projectId, templateId, sectionId, cardNumber).Result;
                if (imageCard != null && imageCard.Id != 0)
                {
                    imageCard.CurrentCondition = value;
                    imageCard.UpdatedDttm = DateTime.Now;
                    imageCard.CardName = cardName;
                    var result = _tankRepository.UpdateProjectTankImageCard(imageCard).Result;
                }
                else
                {
                    TankImageCard projectCardMapping = new TankImageCard();
                    projectCardMapping.ProjectId = projectId;
                    projectCardMapping.TemplateId = templateId;
                    projectCardMapping.SectionId = sectionId;
                    projectCardMapping.CardNumber = cardNumber;
                    projectCardMapping.CurrentCondition = value;
                    projectCardMapping.CreatedDttm = DateTime.Now;
                    projectCardMapping.UpdatedDttm = DateTime.Now;
                    projectCardMapping.CardName = cardName;
                    projectCardMapping.IsActive = true;
                    var result = await _tankRepository.CreateProjectTankImageCard(projectCardMapping);

                }
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure("false");
            }
        }
        public async Task<ServiceResult<UploadPhotoResultDto>> UploadTankCardImage(IFormFile tankfile, int templateId, int projectId, Guid sectionId, int cardNumber, int imageId, bool replaceImage = false, string cardName = null)
        {
            cardName = cardName.Trim();
            string resizedImage = "";
            string newfileId = Guid.NewGuid().ToString();
            if (tankfile != null && tankfile.Length > 0)
            {
                var fileContent = ContentDispositionHeaderValue.Parse(tankfile.ContentDisposition);

                using (var ms = new MemoryStream())
                {
                    tankfile.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    resizedImage = _surveyReportService.ResizeImage(fileBytes);
                    var imgId = await _surveyReportRepository.UploadImage("unknown", newfileId);
                    await _surveyReportRepository.AddEditImageNew(imgId, resizedImage);
                    var imageCard = _tankRepository.GetProjectTankImagCardByName(projectId, templateId, sectionId, cardNumber).Result;
                    if (imageCard == null || imageCard.Id == 0)
                    {
                        TankImageCard projectCardMapping = new TankImageCard();

                        projectCardMapping.ProjectId = projectId;
                        projectCardMapping.TemplateId = templateId;
                        projectCardMapping.SectionId = sectionId;
                        projectCardMapping.CardNumber = cardNumber;
                        projectCardMapping.ImageId = imgId;
                        projectCardMapping.CurrentCondition = 0;
                        projectCardMapping.DescriptionId = 0;
                        projectCardMapping.AdditionalDescription = null;
                        projectCardMapping.IsActive = true;
                        projectCardMapping.CreatedDttm = DateTime.Now;
                        projectCardMapping.UpdatedDttm = DateTime.Now;
                        projectCardMapping.CardName = cardName;
                        var result = await _tankRepository.CreateProjectTankImageCard(projectCardMapping);
                    }
                    else
                    {
                        imageCard.UpdatedDttm = DateTime.Now;
                        imageCard.CreatedDttm = DateTime.Now;
                        imageCard.ImageId = imgId;
                        imageCard.CardName = cardName;
                        var result = await _tankRepository.UpdateProjectTankImageCard(imageCard);
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

        public async Task<ServiceResult<bool>> DeleteTankCardImage(int templateId, int projectId, Guid sectionId, int cardNumber, int imageId)
        {
            var imageCard = _tankRepository.GetProjectTankImagCardByName(projectId, templateId, sectionId, cardNumber).Result;
            if (imageCard.ImageId != 0)
            {
                imageCard.ImageId = 0;
                imageCard.UpdatedDttm = DateTime.Now;
                var result = _tankRepository.UpdateProjectTankImageCard(imageCard).Result;
            }
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<List<TankTypes>>> GetTankTypes()
        {
            var tanktypes = _tankRepository.GetTankTypes().Result;
            return ServiceResult<List<TankTypes>>.Success(tanktypes);
        }
        public async Task<ServiceResult<List<TankTypes>>> GetMappedTankTypes(int projectId, string vesseltype)
        {
            List<TankTypes> tankTypes;

            if (projectId != 0 && projectId < _configuration.GetValue<int>("DefaultTemplateProjectId"))
            {
                // Get vessel type from project
                vesseltype = await _projectRepository.GetProjectVesselType(projectId);

                // Load default tank types
                tankTypes = await _tankRepository.GetTankTypes(0, vesseltype);
            }
            else
            {
                tankTypes = await _tankRepository.GetTankTypes(projectId, vesseltype);
            }

            return ServiceResult<List<TankTypes>>.Success(tankTypes);

        }
        public async Task<ServiceResult<List<VesselDetails>>> GetVesselDetails()
        {
            var vesseltypes = new List<VesselDetails>();
            vesseltypes = _tankRepository.GetVesselIMONo().Result;
            return ServiceResult<List<VesselDetails>>.Success(vesseltypes);
        }

        public async Task<ServiceResult<string>> GetIMONumberByVesselName(string vesselName)
        {
            var imo = _tankRepository.GetVesselIMONo().Result.Where(x => x.VesselName == vesselName).FirstOrDefault()?.IMO;
            return ServiceResult<string>.Success(imo);
        }

        public async Task<ServiceResult<bool>> UpdateTank(string tankId, string vesseltype, string vesselname, string imonumber, string tanktype, string tankname, bool isActive)
        {
            var tanks = _tankRepository.GetTanks_Vessel().Result;
            var tanktypes = _tankRepository.GetTankTypes().Result;

            if (string.IsNullOrEmpty(tankId))
            {

                var isAvailable = tanks.Where(x => x.TankType == tanktype && x.TankName == tankname && x.IMONumber == imonumber && x.VesselType == vesseltype).FirstOrDefault();
                //if (isAvailable != null && isAvailable.TankId != Guid.Empty)
                //{
                //    return BadRequest();
                //}
                var tanktypeId = tanktypes.Where(x => x.TankName == tanktype).Select(x => x.Id).FirstOrDefault();
                VesselTank vesselTank = new VesselTank();
                vesselTank.Id = Guid.NewGuid();
                vesselTank.TankTypeId = tanktypeId;
                vesselTank.TankName = tankname;
                if (!string.IsNullOrEmpty(vesseltype))
                {

                    vesselTank.VesselType = vesseltype;
                    vesselTank.ImoNumber = null;
                }
                else
                {
                    vesselTank.VesselType = null;
                    vesselTank.ImoNumber = imonumber;
                }

                vesselTank.CreatedDttm = DateTime.Now;
                vesselTank.IsActive = isActive;
                vesselTank.UpdateDttm = DateTime.Now;
                var result = _tankRepository.CreateTank(vesselTank).Result;
            }
            else
            {

                VesselTank vesselTank = _tankRepository.GetTanks_VesselById(Guid.Parse(tankId), null).Result;
                vesselTank.TankTypeId = tanktypes.Where(x => x.TankName == tanktype).Select(x => x.Id).FirstOrDefault();
                vesselTank.TankName = tankname;
                vesselTank.IsActive = isActive;
                vesselTank.UpdateDttm = DateTime.Now;
                var result = _tankRepository.UpdateTank(vesselTank).Result;
            }


            return ServiceResult<bool>.Success(true);
        }


        public async Task<ServiceResult<List<Core.Models.View.Admin.VesselTypes>>> GetVesselType()
        {
            var vesseltypes = await _tankRepository.GetVesselType();

            return ServiceResult<List<Core.Models.View.Admin.VesselTypes>>.Success(vesseltypes);
        }

        public async Task<ServiceResult<List<ShipType>>> GetShipType()
        {
            var shiptypes = new List<ShipType>();
            shiptypes.AddRange(_tankRepository.GetShipType().Result);
            return ServiceResult<List<ShipType>>.Success(shiptypes);
        }

        public async Task<ServiceResult<List<VesselTankDetails>>> GetVesselTypeList()
        {
            var vesseltypes = new List<Core.Models.VesselModel.VesselTankDetails>();
            vesseltypes = _tankRepository.GetVesselTypeList().Result;
            return ServiceResult<List<VesselTankDetails>>.Success(vesseltypes);
        }
        public async Task<ServiceResult<List<Core.Models.View.Admin.Tank>>> FilterMenuCustomization_Read()
        {
            var all = await _tankRepository.GetTanks_Vessel();
            return ServiceResult<List<Core.Models.View.Admin.Tank>>.Success(all);
        }

        public async Task<ServiceResult<List<string>>> ManageTankFilter_TankName(string IMO)
        {
            var tanks = await _tankRepository.GetTanks_Vessel();

            var tankNames = tanks.Where(x => x.TankName != null).Select(x => x.TankName).Distinct().ToList();

            if (!string.IsNullOrWhiteSpace(IMO))
            {
                tankNames = tanks.Where(x => x.TankName != null && x.IMONumber == IMO).Select(x => x.TankName).Distinct().ToList();
            }

            return ServiceResult<List<string>>.Success(tankNames);
        }

        public async Task<ServiceResult<List<string>>> ManageTankFilter_VesselType(string IMO)
        {
            var tanks = await _tankRepository.GetTanks_Vessel();
            var vesselTypes = tanks.Where(x => x.VesselType != null).Select(e => e.VesselType).Distinct().ToList();
            if (!string.IsNullOrEmpty(IMO))
            {
                vesselTypes = tanks.Where(x => x.VesselType != null && x.IMONumber == IMO).Select(e => e.VesselType).Distinct().ToList();
            }
            return ServiceResult<List<string>>.Success(vesselTypes);
        }

        public async Task<ServiceResult<List<string>>> ManageTankFilter_VesselName(string IMO)
        {
            var tanks = await _tankRepository.GetTanks_Vessel();
            var vesselNames = tanks.Where(x => x.VesselName != null).Select(e => e.VesselName).Distinct().ToList();
            if (!string.IsNullOrEmpty(IMO))
            {
                vesselNames = tanks.Where(x => x.VesselName != null && x.IMONumber == IMO).Select(e => e.VesselName).Distinct().ToList();
            }
            return ServiceResult<List<string>>.Success(vesselNames);
        }

        public async Task<ServiceResult<List<string>>> ManageTankFilter_IMONumber(string IMO)
        {
            var tanks = await _tankRepository.GetTanks_Vessel();
            var IMONumbers = tanks.Where(x => x.IMONumber != null).Select(e => e.IMONumber).Distinct().ToList();
            if (!string.IsNullOrEmpty(IMO))
            {
                IMONumbers = tanks.Where(x => x.IMONumber != null && x.IMONumber == IMO).Select(e => e.IMONumber).Distinct().ToList();
            }
            return ServiceResult<List<string>>.Success(IMONumbers);
        }

        public async Task<ServiceResult<List<string>>> ManageTankFilter_TankType(string IMO)
        {
            var tanks = await _tankRepository.GetTanks_Vessel();
            var TankTypes = tanks.Where(x => x.TankType != null).Select(e => e.TankType).Distinct().ToList();
            if (!string.IsNullOrEmpty(IMO))
            {
                TankTypes = tanks.Where(x => x.TankType != null && x.IMONumber == IMO).Select(e => e.TankType).Distinct().ToList();
            }
            return ServiceResult<List<string>>.Success(TankTypes);
        }

        public async Task<ServiceResult<List<IMOTankFilterModel>>> ManageTankFilter_Project(string IMO)
        {
            var projectnames = _tankRepository.GetProjectNames(IMO).Result;
            return ServiceResult<List<IMOTankFilterModel>>.Success(projectnames);
        }

        public async Task<ServiceResult<TanksViewModel>> ManageTankActiveCheckBox(string data, bool status, string IMO)
        {
            var datalist = JsonConvert.DeserializeObject<List<Guid>>(data);

            await _tankRepository.UpdateStatus(datalist, status);

            var tankViewModel = new TanksViewModel
            {
                IsActive = false,
                Tanks = await _tankRepository.GetTanks_Vessel()
            };

            var currentUser = await _securityService.GetCurrentUserAsync();

            if (currentUser.Data.IsABSAdministrator())
            {
                tankViewModel.Tanks = tankViewModel.Tanks
                    .Where(x => x.IMONumber != null || x.VesselType != null)
                    .ToList();
            }

            if (!string.IsNullOrEmpty(IMO))
            {
                tankViewModel.Tanks = tankViewModel.Tanks
                    .Where(x => x.IMONumber == IMO)
                    .ToList();
            }

            return ServiceResult<TanksViewModel>.Success(tankViewModel);
        }

        public async Task<ServiceResult<string>> ManageTankInActiveCheckBox()
        {
            var data = new
            {
                status = "Inactive"
            };

            return ServiceResult<string>.Success("Inactive");
        }


        public async Task<ServiceResult<List<Core.Models.View.Admin.Tank>>> DeleteTanks(Guid tankId, string IMO, int ProjectId)
        {
            var data = await _tankRepository.GetTanks_VesselById(tankId, ProjectId);
            data.IsDeleted = true;
            data.IsActive = false;
            var result = await _tankRepository.UpdateTank(data);

            var tanks = await _tankRepository.GetTanks_Vessel();

            if (!string.IsNullOrEmpty(IMO))
            {
                var filteredTanks = tanks.Where(t => t.IMONumber == IMO).ToList();
                return ServiceResult<List<Core.Models.View.Admin.Tank>>.Success(filteredTanks);
            }

            return ServiceResult<List<Core.Models.View.Admin.Tank>>.Success(tanks);
        }

        public async Task<ServiceResult<ManageTankResponse>> ManageTankAsync(string username, bool isActive, string imo, string projectId, int tankRestoreFilter, int searchRestoreFilter)
        {
            // Permission check
            var permission = await _userPermissionRepository.GetRolePermissionByUserName(username,
                EnumExtensions.GetDescription(ManagePages.ManageTank));

            if (permission == null || (!Convert.ToBoolean(permission?.Edit) && !Convert.ToBoolean(permission?.Read) && !Convert.ToBoolean(permission?.Delete)))
            {
                return ServiceResult<ManageTankResponse>.Failure("AccessDenied");
            }

            // Get current user for admin check
            var currentUserResult = await _securityService.GetCurrentUserAsync();
            if (!currentUserResult.IsSuccess)
                return ServiceResult<ManageTankResponse>.Failure("User not found");

            var currentUser = currentUserResult.Data;
            var isAdmin = await _userManager.GetRolesAsync(await _userManager.FindByNameAsync(username));
            var isAdminUser = isAdmin.Contains("Admin (CAP HQ)");

            // Load tanks
            List<Tank> tanks;
            if (isAdminUser)
            {
                tanks = await _tankRepository.GetTanks_Vessel();
            }
            else
            {
                tanks = await _tankRepository.GetTanks_Vessel(username);
                tanks = tanks.Where(x => x.IMONumber != null || x.VesselType != null).ToList();
            }

            // Filter by IMO and ProjectId if provided
            bool isDownload = false;
            if (!string.IsNullOrEmpty(imo) && !string.IsNullOrEmpty(projectId))
            {
                var projectNameResult = await _projectService.GetProjectName(Convert.ToInt32(projectId));
                if (projectNameResult.IsSuccess)
                {
                    var projectName = projectNameResult.Data.ToLower();
                    tanks = tanks.Where(x => x.IMONumber == imo && x.ProjectName?.ToLower() == projectName).ToList();
                    isDownload = tanks.Select(x => x.IsDownload).FirstOrDefault();
                }
            }

            // Build filter options
            var validTanks = tanks.Where(x => x.IMONumber != null && x.VesselType != null).ToList();

            var imoOptions = validTanks
                .Select(x => new TankFilterModel { Text = x.IMONumber + "-" + x.VesselName, Value = x.IMONumber })
                .GroupBy(x => new { x.Value })
                .Distinct()
                .Select(x => x.First())
                .ToList();

            var tankTypeOptions = validTanks
                .Select(x => new TankFilterModel { Text = x.TankType, Value = x.TankType })
                .GroupBy(x => new { x.Text, x.Value })
                .Select(x => x.First())
                .ToList();

            var tankNameOptions = validTanks
                .Select(x => new TankFilterModel { Text = x.TankName, Value = x.TankName })
                .GroupBy(x => new { x.Text, x.Value })
                .Select(x => x.First())
                .ToList();

            var statusOptions = validTanks
                .Select(x => new TankFilterModel { Text = x.Status ? "Active" : "InActive", Value = x.Status ? "Active" : "InActive" })
                .GroupBy(x => new { x.Text, x.Value })
                .Select(x => x.First())
                .ToList();

            var vesselTypeOptions = validTanks
                .Select(x => new TankFilterModel { Text = x.VesselType, Value = x.VesselType })
                .GroupBy(x => new { x.Text, x.Value })
                .Select(x => x.First())
                .ToList();

            var vesselNameOptions = validTanks
                .Select(x => new TankFilterModel { Text = x.VesselName, Value = x.VesselName })
                .GroupBy(x => new { x.Text, x.Value })
                .Select(x => x.First())
                .ToList();

            var response = new ManageTankResponse
            {
                IsActive = isActive,
                Editable = Convert.ToBoolean(permission?.Edit),
                CanDelete = Convert.ToBoolean(permission?.Delete),
                IsAdmin = isAdminUser,
                IsDownload = isDownload,
                IMO = imo,
                ProjectId = projectId,
                TankRestoreFilter = tankRestoreFilter == 1 ? 1 : 0,
                SearchRestoreFilter = searchRestoreFilter == 1 ? 1 : 0,
                Tanks = tanks,
                IMONumberOptions = imoOptions,
                TankTypeOptions = tankTypeOptions,
                TankNameOptions = tankNameOptions,
                TankStatusOptions = statusOptions,
                VesselTypeOptions = vesselTypeOptions,
                VesselNameOptions = vesselNameOptions
            };

            return ServiceResult<ManageTankResponse>.Success(response);
        }

        public async Task<ServiceResult<bool>> SetManageTankFiltersAsync(ManageTankFilterRequest request)
        {
            await Task.CompletedTask;
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<GetDataByIMOResponse>> GetDataByIMOAsync(string imo)
        {
            var tanks = await _tankRepository.GetTanks_Vessel();
            var filteredTanks = tanks.Where(x => x.IMONumber != null && x.VesselType != null && x.IMONumber == imo).ToList();

            var response = new GetDataByIMOResponse
            {
                TankTypes = filteredTanks
                    .Select(x => new TankFilterModel { Text = x.TankType, Value = x.TankType })
                    .GroupBy(x => new { x.Text, x.Value })
                    .Select(x => x.First())
                    .ToList(),
                TankNames = filteredTanks
                    .Select(x => new TankFilterModel { Text = x.TankName, Value = x.TankName })
                    .GroupBy(x => new { x.Text, x.Value })
                    .Select(x => x.First())
                    .ToList(),
                VesselTypes = filteredTanks
                    .Select(x => new TankFilterModel { Text = x.VesselType, Value = x.VesselType })
                    .GroupBy(x => new { x.Text, x.Value })
                    .Select(x => x.First())
                    .ToList()
            };

            return ServiceResult<GetDataByIMOResponse>.Success(response);
        }

        public async Task<ServiceResult<GetTankForAddResponse>> GetTankForAddAsync(string imo, string projectId, bool projectTanktype)
        {
            var vesselList = await _tankRepository.GetVesselIMONo();
            var response = new GetTankForAddResponse
            {
                IMO = imo,
                ProjectId = projectId,
                ProjectTanktype = projectTanktype,
                VesselList = vesselList
            };

            if (!string.IsNullOrEmpty(imo))
            {
                var vessel = vesselList.FirstOrDefault(x => x.IMO == imo);
                if (vessel != null)
                {
                    response.VesselName = vessel.VesselName;
                }

                // Get projects by IMO - need to check if this method exists
                // For now, leaving Projects empty - will need to implement GetProjectListByIMO
            }

            return ServiceResult<GetTankForAddResponse>.Success(response);
        }

        public async Task<ServiceResult<GetTankForEditResponse>> GetTankForEditAsync(Guid tankId, int projectId, string username)
        {
            // Permission check
            var permission = await _userPermissionRepository.GetRolePermissionByUserName(username,
                EnumExtensions.GetDescription(ManagePages.ManageTank));

            if (permission?.Edit != true)
                return ServiceResult<GetTankForEditResponse>.Failure("AccessDenied");

            Tank tank;
            if (projectId != 0)
            {
                var tanks = await _tankRepository.GetTanks_Vessel();
                tank = tanks.FirstOrDefault(x => x.TankId == tankId && x.ProjectId == projectId);
            }
            else
            {
                var tanks = await _tankRepository.GetTanks_Vessel();
                tank = tanks.FirstOrDefault(x => x.TankId == tankId);
            }

            if (tank == null)
                return ServiceResult<GetTankForEditResponse>.Failure("Tank not found");

            var response = new GetTankForEditResponse
            {
                TankId = tank.TankId,
                TankName = tank.TankName,
                VesselId = tank.VesselId,
                VesselName = tank.VesselName,
                VesselType = tank.VesselType,
                IMONumber = tank.IMONumber,
                TankType = tank.TankType,
                TaknTypeId = tank.TaknTypeId,
                Subheader = tank.Subheader,
                Status = tank.Status,
                ProjectName = tank.ProjectName,
                ProjectId = tank.ProjectId
            };

            return ServiceResult<GetTankForEditResponse>.Success(response);
        }

        public async Task<ServiceResult<bool>> CreateTankAsync(Tank model, string imo, string projectId, bool projectTanktype, string username)
        {
            // Permission check
            var permission = await _userPermissionRepository.GetRolePermissionByUserName(username,
                EnumExtensions.GetDescription(ManagePages.ManageTank));

            if (permission?.Edit != true)
                return ServiceResult<bool>.Failure("AccessDenied");

            // Validation
            if (string.IsNullOrEmpty(model.TankName))
                return ServiceResult<bool>.Failure("Tank Name Required");

            if (string.IsNullOrEmpty(model.TankType))
                return ServiceResult<bool>.Failure("Tank Type Required");

            if (string.IsNullOrEmpty(model.VesselType) && string.IsNullOrEmpty(model.VesselName))
                return ServiceResult<bool>.Failure("Please select Vessel Type or Vessel Name");

            if (!string.IsNullOrEmpty(model.IMONumber) && string.IsNullOrEmpty(model.VesselName) && string.IsNullOrEmpty(model.VesselType))
                return ServiceResult<bool>.Failure("Please select Vessel Name");

            // Validate IMO and VesselName match
            if (!string.IsNullOrEmpty(model.VesselName))
            {
                var vessels = await _tankRepository.GetVesselIMONo();
                var vessel = vessels.FirstOrDefault(x => x.VesselName == model.VesselName && model.IMONumber == x.IMO);
                if (vessel == null)
                    return ServiceResult<bool>.Failure("Please select valid IMO number");
            }

            // Duplicate check
            var tanks = await _tankRepository.GetTanks_Vessel();
            if (!string.IsNullOrEmpty(model.TankName) && !string.IsNullOrEmpty(model.TankType) && (!string.IsNullOrEmpty(model.VesselType) || !string.IsNullOrEmpty(model.VesselName)))
            {
                var isAvailable = tanks.FirstOrDefault(x => x.TankType == model.TankType && x.TankName == model.TankName && x.IMONumber == model.IMONumber && x.VesselType == model.VesselType);
                if (isAvailable != null)
                    return ServiceResult<bool>.Failure("This tank is already available in the section");
            }

            // Project validation
            if (string.IsNullOrEmpty(model.VesselType) && (string.IsNullOrEmpty(projectId) || projectId == "") && string.IsNullOrEmpty(model.ProjectName))
                return ServiceResult<bool>.Failure("Please select Project Name");

            // Create tank entity
            var tankTypes = await _tankRepository.GetTankTypes();
            var tanktypeId = tankTypes.FirstOrDefault(x => x.TankName == model.TankType)?.Id ?? 0;

            var vesselTank = new VesselTank
            {
                Id = Guid.NewGuid(),
                TankTypeId = tanktypeId,
                TankName = model.TankName,
                Subheader = model.Subheader,
                CreatedDttm = DateTime.Now,
                IsActive = model.Status,
                UpdateDttm = DateTime.Now
            };

            if (!string.IsNullOrEmpty(model.VesselType))
            {
                vesselTank.VesselType = model.VesselType;
                vesselTank.ImoNumber = null;
            }
            else
            {
                if (!string.IsNullOrEmpty(projectId))
                {
                    vesselTank.ProjectId = Convert.ToInt32(projectId);
                }
                else
                {
                    // Get project by IMO - simplified for now
                    // Would need GetProjectListByIMO implementation
                    var projectsResult = await _projectService.GetProjectName(0); // Placeholder
                    vesselTank.ProjectId = !string.IsNullOrEmpty(projectId) ? Convert.ToInt32(projectId) : null;
                    if (vesselTank.ProjectId.HasValue)
                    {
                        var vesselTypeResult = await _projectRepository.GetProjectVesselType(vesselTank.ProjectId.Value);
                        vesselTank.VesselType = vesselTypeResult;
                    }
                }
                vesselTank.ImoNumber = model.IMONumber;
            }

            await _tankRepository.CreateTank(vesselTank);
            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> UpdateTankAsync(Tank model, string imo, string projectId, string username)
        {
            // Permission check
            var permission = await _userPermissionRepository.GetRolePermissionByUserName(username,
                EnumExtensions.GetDescription(ManagePages.ManageTank));

            if (permission?.Edit != true)
                return ServiceResult<bool>.Failure("AccessDenied");

            // Get existing tank
            VesselTank vesselTank;
            if (model.ProjectId == 0 || model.ProjectId == null)
            {
                vesselTank = await _tankRepository.GetTanks_VesselById(model.TankId, null);
            }
            else
            {
                vesselTank = await _tankRepository.GetTanks_VesselById(model.TankId, model.ProjectId);
            }

            if (vesselTank == null)
                return ServiceResult<bool>.Failure("Tank not found");

            // Duplicate check
            var tanks = await _tankRepository.GetTanks_Vessel();
            var tankTypes = await _tankRepository.GetTankTypes();
            var tanktype = tankTypes.FirstOrDefault(x => x.Id == vesselTank.TankTypeId)?.TankName;
            var isAvailable = tanks.Where(x => x.TankType == tanktype && x.TankName == model.TankName && x.IMONumber == vesselTank.ImoNumber && x.VesselType == vesselTank.VesselType && model.TankId != x.TankId).ToList();
            if (isAvailable.Any())
                return ServiceResult<bool>.Failure("This tank is already available in the section");

            // Update tank
            vesselTank.Subheader = model.Subheader;
            vesselTank.TankName = model.TankName;
            vesselTank.IsActive = model.Status;
            vesselTank.UpdateDttm = DateTime.Now;

            await _tankRepository.UpdateTank(vesselTank);
            return ServiceResult<bool>.Success(true);
        }

    }
}
