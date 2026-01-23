using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.Grading;
using DigitalCap.Core.Models.Survey;
using DigitalCap.Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Infrastructure.Service
{
    public class GradingService : IGradingService
    {

        private readonly IGradingRepository _gradingRepository;
        private readonly ITankRepository _tankRepository;

        public GradingService(IGradingRepository gradingRepository, ITankRepository tankRepository)
        {
            _gradingRepository = gradingRepository;
            _tankRepository = tankRepository;
        }
        public async Task<ServiceResult<bool>> PopulateGrading(string vesselType, int projectId)
        {
            try
            {
                // 1. Create section grading
                await _gradingRepository.CreateProjectSectionGrading(projectId, vesselType);

                // 2. Get tank grading template
                var tankGradings =
                    await _tankRepository.GetVessel_GradingByVesselType(vesselType);

                // 3. Populate grading
                foreach (var grading in tankGradings)
                {
                    grading.ProjectId = projectId;
                    grading.CreatedDttm = DateTime.Now;
                    grading.UpdateDttm = DateTime.Now;

                    await _gradingRepository.CreateVessel_Grading(grading);
                }

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure(ex.Message);
            }
        }

        //public Task<ServiceResult<List<Core.Models.Grading.Grading>>> GetAllGradingsAsync()
        //{
        //    return _gradingRepository.GetAllGradingsAsync();
        //}

        public async Task<ServiceResult<bool>> CreateGradingAsync(GradingListViewModel model)
        {

            var dup = await _gradingRepository.CheckGradingNameExistsAsync(
            model.VesselType, model.SectionName, model.TemplateName, model.GradingName);

            if (dup.Any())
                return ServiceResult<bool>.Failure("Grading name already exists");

            var sections = await _gradingRepository.GetGradingSectionsByTemplateAndVesselAsync(
            model.TemplateName, model.VesselType);

            var section = sections.FirstOrDefault(s => s.SectionName == model.SectionName);

            if (section == null)
                return ServiceResult<bool>.Failure("Invalid section");

            var grading = new Core.Models.Grading.Grading
            {
                GradingName = model.GradingName,
                VesselType = model.VesselType,
                SectionId = section.SectionId,
                TanktypeId = section.TanktypeId,
                IsActive = model.Status,
                RequiredInReport = model.RequiredInReport
            };

            if (grading.TanktypeId == 0)
                await _gradingRepository.CreateSectionGradingAsync(grading);
            else
                await _gradingRepository.CreateTankGradingAsync(grading);
            return ServiceResult<bool>.Ok(true);
        }


        public async Task<ServiceResult<bool>> UpdateGradingAsync(GradingListViewModel model)
        {
            var all = await _gradingRepository.GetAllGradingAsync();
            var grading = all.FirstOrDefault(x => x.GradingId == model.GradingId);

            if (grading == null)
                return ServiceResult<bool>.Failure("Grading not found");

            var dup = await _gradingRepository.CheckGradingNameExistsAsync(
            model.VesselType, model.SectionName, model.TemplateName, model.GradingName);

            if (dup.Any(x => x.GradingId != model.GradingId))
                return ServiceResult<bool>.Failure("Duplicate grading name");

            grading.GradingName = model.GradingName;
            grading.IsActive = model.Status;
            grading.RequiredInReport = model.RequiredInReport;

            if (grading.TanktypeId == 0)
                await _gradingRepository.UpdateSectionGradingAsync(grading);
            else
                await _gradingRepository.UpdateTankGradingAsync(grading);
            return ServiceResult<bool>.Ok(true);
        }


        public async Task<ServiceResult<bool>> DeleteGradingAsync(int gradingId, int tankId)
        {
            await _gradingRepository.DeleteGradingAsync(gradingId, tankId);
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<List<GradingTemplate>>> GetTemplateName([FromQuery] string vesselType = null)
        {
            try
            {
                var templates = await _gradingRepository.GetGradingTemplatesByVesselType(vesselType);
                return ServiceResult<List<GradingTemplate>>.Success(templates);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<GradingTemplate>>.Failure("Error retrieving templates");
            }
        }

        public async Task<ServiceResult<List<GradingSection>>> GetSectionNameByTemplateNameAndVesselType(string templateName, string vesselType)
        {
            try
            {
                var sections = await _gradingRepository.GetGradingSectionNamesByTemplateNameAndVesselType(templateName, vesselType);
                return ServiceResult<List<GradingSection>>.Success(sections);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<GradingSection>>.Failure("Error retrieving sections");
            }
        }
        public async Task<ServiceResult<List<GradingSection>>> GetGradingSections(int templateId, string vesselType)
        {
            try
            {
                var sections = await _gradingRepository.GetGradingSections(templateId, vesselType);
                return ServiceResult<List<GradingSection>>.Success(sections);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<GradingSection>>.Failure("Error retrieving sections");
            }
        }

        public async Task<ServiceResult<List<Core.Models.Grading.Grading>>> GetAllGradingsAsync()
        {
            var all = await _gradingRepository.GetAllGradingAsync();
            return ServiceResult<List<Core.Models.Grading.Grading>>.Success(all);
        }

    }
}
