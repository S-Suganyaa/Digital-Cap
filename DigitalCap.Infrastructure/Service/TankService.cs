using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
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
        public TankService(ITankRepository tankRepository, ISurveyReportRepository surveyReportRepository
            , ISurveyReportService surveyReportService, IConfiguration configuration,
            IProjectRepository projectRepository, ISecurityService securityService)
        {
            _tankRepository = tankRepository;
            _surveyReportRepository = surveyReportRepository;
            _surveyReportService = surveyReportService;
            _configuration = configuration;
            _projectRepository = projectRepository;
            _securityService = securityService;
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

        //public async Task<JsonResult> FilterMenuCustomization_Read(DataSourceRequest request)
        //{
        //    return Json(_tankRepository.GetTanks_Vessel().Result.ToDataSourceResult(request), new JsonSerializerOptions { PropertyNamingPolicy = null });
        //}
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


    }
}
