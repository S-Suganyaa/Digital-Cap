using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.Survey;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Web.Mvc;

namespace DigitalCap.Infrastructure.Service
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ITankRepository _tankRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IVesselService _vesselService;
        private readonly ISurveyReportRepository _surveyReportRepository;
        private readonly IReportPartRepository _reportPartRepository ;

        public ProjectService(IProjectRepository projectRepository, ITankRepository tankRepository
            , ITaskRepository taskRepository, IVesselService vesselService
            , ISurveyReportRepository surveyReportRepository, IReportPartRepository reportPartRepository)
        {
            _projectRepository = projectRepository;
            _tankRepository = tankRepository;
            _taskRepository = taskRepository;
            _vesselService = vesselService;
            _surveyReportRepository = surveyReportRepository;
            _reportPartRepository = reportPartRepository;
        }



        public async Task<ServiceResult<int>> CreateProject(Project model)
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
            var templateresult = _reportPartRepository.CreateProjectReportTemplate((int)model.ShipType, User.Identity.Name, projectId, ImoExists, (ImoExists ? Convert.ToInt32(model.CopyingVesselID) : 0)).Result;
            var gradingresult = _gradingService.PopulateGrading(vesseltype, projectId).Result;
            var result = _descriptionService.CreateProjectImageDescription(projectId, vesseltype).Result;

            return ServiceResult<int>.Success((1));
        }

        public async Task<byte> UpdateProjectPercentComplete(int projectId)
        {
            var percentComplete = await _taskRepository.UpdateProjectPercentComplete(projectId);

            await _projectRepository.UpdatePercentComplete(projectId, percentComplete);

            return percentComplete;
        }
    }
}
