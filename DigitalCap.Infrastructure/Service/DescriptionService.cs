using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.Grading;
using DigitalCap.Core.Models.ImageDescription;
using DigitalCap.Core.Models.Permissions;
using DigitalCap.Core.ViewModels;
using DigitalCap.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalCap.Infrastructure.Service
{
    public class DescriptionService : IDescriptionService
    {
        private readonly IDescriptionRepository _descriptionRepository;
        private readonly IGradingRepository _gradingRepository;
        private readonly ITankRepository _tankRepository;
        private readonly IUserPermissionRepository _userPermissionRepository;


        public DescriptionService(IDescriptionRepository descriptionRepository, IGradingRepository gradingRepository, ITankRepository tankRepository, IUserPermissionRepository userPermissionRepository)
        {
            _descriptionRepository = descriptionRepository;
            _gradingRepository = gradingRepository;
            _tankRepository = tankRepository;
            _userPermissionRepository = userPermissionRepository;
        }

        public async Task<List<ImageDescriptions>> GetAllAsync()
         => await _descriptionRepository.GetAllDescription();

        public async Task<ImageDescriptions> GetByIdAsync(int id)
            => await _descriptionRepository.GetImageDescriptionById(id);

        
        public async Task<ServiceResult<ManageDescriptionResponse>> ManageDescriptionAsync(string username,bool isActive,int descriptionRestoreFilter,int searchDescriptionRestoreFilter)
        {
            // Permission check
            var permission = await _userPermissionRepository.GetRolePermissionByUserName(username,
            EnumExtensions.GetDescription(ManagePages.ManageDescription));

            if (permission == null ||
               (!Convert.ToBoolean(permission.Edit) && !Convert.ToBoolean(permission.Read)))
            {
                return ServiceResult<ManageDescriptionResponse>.Failure("AccessDenied");
            }

            // Load descriptions
            var descriptions = await _descriptionRepository.GetAllDescription();

            var response = new ManageDescriptionResponse
            {
                IsActive = isActive,
                Editable = Convert.ToBoolean(permission.Edit),
                ImageDescriptions = descriptions,

                // Filters now come from API parameters
                DescriptionRestoreFilter = descriptionRestoreFilter == 1 ? 1 : 0,
                SearchDescriptionRestoreFilter = searchDescriptionRestoreFilter == 1 ? 1 : 0
            };

            return ServiceResult<ManageDescriptionResponse>.Success(response);
        }



        public async Task<ServiceResult<bool>> SetManageDescriptionFiltersAsync(ManageDescriptionFilterRequest request)
        {
            await Task.CompletedTask;

            return ServiceResult<bool>.Success(true);
        }



        public async Task<ServiceResult<ImageDescriptions>> CreateAsync(ImageDescriptions model)
        {
            if (string.IsNullOrWhiteSpace(model.Description))
                return Core.Models.ServiceResult<ImageDescriptions>.Failure("Description required");

            var sections = await _descriptionRepository
           .GetDescriptionSectionNamesByTemplateNameAndVesselType(model.TemplateName, model.VesselType);

            var section = sections.FirstOrDefault(x => x.SectionName == model.SectionName);
            if (section == null)
                return ServiceResult<ImageDescriptions>.Failure("Invalid Section");

            model.SectionId = section.SectionId;
            model.TankTypeId = section.TanktypeId;
            model.CreatedDttm = DateTime.UtcNow;
            model.UpdatedDttm = DateTime.UtcNow;

            // Duplicate check
            var exists = await _descriptionRepository.CheckImageDescriptionExistsOrNot(
                        model.VesselType,
                        model.SectionName,
                        model.TemplateName,
                        model.Description);

            if (exists.Any())
                return ServiceResult<ImageDescriptions>.Failure("Already exists");

            await _descriptionRepository.CreateImageDescription(model);
            return ServiceResult<ImageDescriptions>.Success(model);
        }

        public async Task<ServiceResult<bool>> AddNewDescriptionAsync(ImageDescriptionViewModel model)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(model.VesselType))
                    return ServiceResult<bool>.Failure("Please select valid Vessel Type");

                if (string.IsNullOrWhiteSpace(model.TemplateName))
                    return ServiceResult<bool>.Failure("Please select valid Template Name");

                if (string.IsNullOrWhiteSpace(model.SectionName))
                    return ServiceResult<bool>.Failure("Please select valid Section Name");

                if (string.IsNullOrWhiteSpace(model.DescriptionName))
                    return ServiceResult<bool>.Failure("Description Name Required");

                // Resolve Section + TankType
                var sections = await _descriptionRepository
            .GetDescriptionSectionNamesByTemplateNameAndVesselType(model.TemplateName, model.VesselType);

                var section = sections.FirstOrDefault(x => x.SectionName == model.SectionName);

                if (section == null)
                    return ServiceResult<bool>.Failure("Invalid Section");

                //int sectionId = section.SectionId;
                //int tankTypeId = section.TanktypeId;

                // Duplicate Check
                var existing = await _descriptionRepository.CheckImageDescriptionExistsOrNot(
            model.VesselType,
            model.SectionName,
            model.TemplateName,
            model.DescriptionName);

                if (existing.Any())
                    return ServiceResult<bool>.Failure("The description is already available in the section");

                // Build Entity
                var entity = new ImageDescriptions
                {
                    Description = model.DescriptionName.Trim(),
                    VesselType = model.VesselType,
                    SectionId = section.SectionId,
                    TankTypeId = section.TanktypeId,
                    IsActive = model.Status,
                    CreatedDttm = DateTime.UtcNow,
                    UpdatedDttm = DateTime.UtcNow
                };

                await _descriptionRepository.CreateImageDescription(entity);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                // optional logging
                return ServiceResult<bool>.Failure("Internal server error");
            }
        }

        public async Task<ServiceResult<ImageDescriptions>> UpdateAsync(ImageDescriptions model)
        {
            var entity = await _descriptionRepository.GetImageDescriptionById(model.Id);
            if (entity == null)
                return ServiceResult<ImageDescriptions>.Failure("Not found");

            var exists = await _descriptionRepository.CheckImageDescriptionExistsOrNot(
                        model.VesselType,
                        model.SectionName,
                        model.TemplateName,
                        model.Description);

            if (exists.Any(x => x.Id != model.Id))
                return ServiceResult<ImageDescriptions>.Failure("Duplicate");

            entity.Description = model.Description;
            entity.IsActive = model.IsActive;
            entity.UpdatedDttm = DateTime.UtcNow;

            await _descriptionRepository.UpdateImageDescription(entity);
            return ServiceResult<ImageDescriptions>.Success(model);
        }

        public async Task<ServiceResult<List<GradingSection>>>GetSectionNamesByTemplateNameAndVesselTypeAsync(string templateName, string vesselType)
        {
            try
            {
                var sections = await _descriptionRepository.GetSectionNamesByTemplateAndVesselAsync(templateName, vesselType);

                var list = sections?.ToList() ?? new List<GradingSection>();

                return ServiceResult<List<GradingSection>>.Success(list);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<GradingSection>>
                    .Failure("Failed to load section names");
            }
        }

        public async Task<ServiceResult<bool>> EditDescriptionAsync(ImageDescriptionViewModel model)
        {
            // Load existing description
            var description = await _descriptionRepository.GetImageDescriptionById(model.Id);
            if (description == null)
                return ServiceResult<bool>.Failure("Description not found");

            // Validation
            if (string.IsNullOrWhiteSpace(model.DescriptionName))
                return ServiceResult<bool>.Failure("Description Name Required");

            // Duplicate check
            var duplicates = await _descriptionRepository.CheckImageDescriptionExistsOrNot(
            model.VesselType,
            model.SectionName,
            model.TemplateName,
            model.DescriptionName);

            if (duplicates.Any(x => x.Id != model.Id))
                return ServiceResult<bool>.Failure("The description already exists in this section");

            // Update entity
            description.Description = model.DescriptionName;
            description.IsActive = model.Status;
            description.UpdatedDttm = DateTime.UtcNow;

            await _descriptionRepository.UpdateImageDescription(description);

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> EditDescriptionByIdAsync(int id, string username)
        {
            // Permission check
            var permission = await _userPermissionRepository.GetRolePermissionByUserName(username,
            EnumExtensions.GetDescription(ManagePages.ManageDescription));

            if (permission?.Edit != true)
                return ServiceResult<bool>.Failure("AccessDenied");

            // Get description entity
            var description = await _descriptionRepository.GetImageDescriptionById(id);

            if (description == null)
                return ServiceResult<bool>.Failure("NotFound");

            // Load dropdown master data
            var templates = await _gradingRepository.GetGradingTemplates();
            var sections = await _gradingRepository.GetGradingSections(0, null);
            var vesselTypes = await _tankRepository.GetShipType();

            // Build response
            var response = new 
            {
                Id = description.Id,
                TemplateName = description.TemplateName,
                VesselType = description.VesselType,
                SectionName = description.SectionName,
                DescriptionName = description.Description,
                TankTypeId = description.TankTypeId,
                Status = description.IsActive,

                Templates = templates,
                Sections = sections,
                VesselTypes = vesselTypes
            };

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<List<string>>> GetDistinctVesselTypesAsync()
        {
            var descriptions = await _descriptionRepository.GetAllDescription();

            var vesselTypes = descriptions
               .Where(x => !string.IsNullOrWhiteSpace(x.VesselType))
               .Select(x => x.VesselType.Trim())
               .Distinct()
               .ToList();

            return ServiceResult<List<string>>.Success(vesselTypes);
        }



    }

}
