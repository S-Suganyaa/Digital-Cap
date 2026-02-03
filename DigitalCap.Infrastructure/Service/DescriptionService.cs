using DigitalCap.Core.DTO;
using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.Grading;
using DigitalCap.Core.Models.ImageDescription;
using DigitalCap.Core.Models.Permissions;
using DigitalCap.Core.Models.Survey;
using DigitalCap.Core.Models.Tank;
using DigitalCap.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Documents.Fixed.Model.ColorSpaces;
using static System.Collections.Specialized.BitVector32;

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


        public async Task<ServiceResult<bool>> CreateAsync(ImageDescriptionDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.Description))
                return ServiceResult<bool>.Failure("Description required");

            var sections = await _descriptionRepository.GetDescriptionSectionNamesByTemplateNameAndVesselType(
            model.TemplateName, model.VesselType);

            var section = sections.FirstOrDefault(x => x.SectionName == model.SectionName);
            if (section == null)
                return ServiceResult<bool>.Failure("Invalid Section");

            model.SectionId = section.SectionId;
            model.CreatedDttm = DateTime.UtcNow;
            model.UpdatedDttm = DateTime.UtcNow;

            var exists = await _descriptionRepository.CheckImageDescriptionExistsOrNot(
        model.VesselType,
        model.SectionName,
        model.TemplateName,
        model.Description);

            if (exists.Any())
                return ServiceResult<bool>.Failure("Already exists");

            var entity = new ImageDescriptions
            {
                //SectionId = model.SectionId,
                //TankTypeId = model.TankTypeId,
                TemplateName = model.TemplateName,
                VesselType = model.VesselType,
                SectionName = model.SectionName,
                Description = model.Description,
                ProjectId = 0,
                IsActive = model.IsActive ?? true,
                IsDeleted = false,
                //CreatedDttm = model.CreatedDttm,
                //UpdatedDttm = model.UpdatedDttm
            };

            await _descriptionRepository.CreateImageDescription(entity);

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<AddDescriptionMetaDTO>>GetAddDescriptionMetadataAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                return ServiceResult<AddDescriptionMetaDTO>.Failure("Unauthorized");

            var permission = await _userPermissionRepository.GetRolePermissionByUserName(userName,
            EnumExtensions.GetDescription(ManagePages.ManageDescription));

            if (permission?.Edit != true)
                return ServiceResult<AddDescriptionMetaDTO>.Failure("Access denied");

            var templates = await _gradingRepository.GetGradingTemplates();
            var sections = await _gradingRepository.GetGradingSections(0, null);
            var vesselTypes = await _tankRepository.GetShipTypes();

            var result = new AddDescriptionMetaDTO
            {
                Templates = templates,
                Sections = sections,
                VesselTypes = vesselTypes
            };

            return ServiceResult<AddDescriptionMetaDTO>.Success(result);
        }




        //public async Task<ServiceResult<bool>> CreateAsync(ImageDescriptionDTO model)
        //{
        //    if (string.IsNullOrWhiteSpace(model.Description))
        //        return Core.Models.ServiceResult<bool>.Failure("Description required");

        //    var sections = await _descriptionRepository
        //   .GetDescriptionSectionNamesByTemplateNameAndVesselType(model.TemplateName, model.VesselType);

        //    var section = sections.FirstOrDefault(x => x.SectionName == model.SectionName);
        //    if (section == null)
        //        return ServiceResult<bool>.Failure("Invalid Section");

        //    model.SectionId = section.SectionId;
        //    //model.TankTypeId = section.TanktypeId;
        //    model.CreatedDttm = DateTime.UtcNow;
        //    model.UpdatedDttm = DateTime.UtcNow;

        //    // Duplicate check
        //    var exists = await _descriptionRepository.CheckImageDescriptionExistsOrNot(
        //                model.VesselType,
        //                model.SectionName,
        //                model.TemplateName,
        //                model.Description);

        //    if (exists.Any())
        //        return ServiceResult<bool>.Failure("Already exists");

        //    var entity = new ImageDescriptions
        //    {

        //        TemplateName = model.TemplateName,
        //        VesselType = model.VesselType,
        //        SectionName = model.SectionName,
        //        Description = model.Description,

        //    };

        //    await _descriptionRepository.CreateImageDescription(entity);
        //    return ServiceResult<bool>.Success(true);
        //}

        public async Task<ServiceResult<GetDescriptionForAddResponse>> GetDescriptionForAddAsync(string username)
        {
            // Permission check
            var permission = await _userPermissionRepository.GetRolePermissionByUserName(username,
                EnumExtensions.GetDescription(ManagePages.ManageDescription));

            if (permission?.Edit != true)
                return ServiceResult<GetDescriptionForAddResponse>.Failure("AccessDenied");

            // Load dropdown master data
            var templates = await _gradingRepository.GetGradingTemplates();
            var sections = await _gradingRepository.GetGradingSections(0, null);
            var vesselTypes = await _tankRepository.GetShipType();

            var response = new GetDescriptionForAddResponse
            {
                Templates = templates,
                Sections = sections,
                VesselTypes = vesselTypes
            };

            return ServiceResult<GetDescriptionForAddResponse>.Success(response);
        }


        public async Task<ServiceResult<bool>> AddNewDescriptionAsync(ImageDescriptionViewModel model)
        {
            try
            {
                // 1. Validation
                if (string.IsNullOrWhiteSpace(model.VesselType))
                    return ServiceResult<bool>.Failure("Please select valid Vessel Type");

                if (string.IsNullOrWhiteSpace(model.TemplateName))
                    return ServiceResult<bool>.Failure("Please select valid Template Name");

                if (string.IsNullOrWhiteSpace(model.SectionName))
                    return ServiceResult<bool>.Failure("Please select valid Section Name");

                if (string.IsNullOrWhiteSpace(model.DescriptionName))
                    return ServiceResult<bool>.Failure("Description Name Required");

                // 2. Duplicate check (LET SP HANDLE EVERYTHING)
                var existing = await _descriptionRepository.CheckImageDescriptionExistsOrNot(
            model.VesselType,
            model.SectionName,
            model.TemplateName,
            model.DescriptionName
        );

                if (existing != null && existing.Any())
                    return ServiceResult<bool>.Failure(
                        "The description is already available in the section");

                // 3. Resolve section info (ONLY for insert)
                var sections = await _descriptionRepository
            .GetDescriptionSectionNamesByTemplateNameAndVesselType(
                model.TemplateName,
                model.VesselType);

                var section = sections?.FirstOrDefault(x => x.SectionName == model.SectionName);

                if (section == null)
                    return ServiceResult<bool>.Failure("Invalid Section selected");

                // 4. Build entity
                var entity = new ImageDescriptions
                {
                    Description = model.DescriptionName.Trim(),
                    VesselType = model.VesselType,

                    SectionId = section.TanktypeId == 0
                ? section.SectionId
                : null,

                    TankTypeId = section.TanktypeId != 0
                ? section.TanktypeId
                : null,

                    IsActive = model.Status,
                    IsDeleted = false,
                    ProjectId = 0,
                    CreatedDttm = DateTime.UtcNow,
                    UpdatedDttm = DateTime.UtcNow
                };

                // 5. Save
                await _descriptionRepository.CreateImageDescription(entity);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                // Ideally log ex here
                return ServiceResult<bool>.Failure("Internal server error");
            }
        }


        //public async Task<ServiceResult<bool>> AddNewDescriptionAsync(ImageDescriptionViewModel model)
        //{
        //    try
        //    {
        //        // Validation
        //        if (string.IsNullOrWhiteSpace(model.VesselType))
        //            return ServiceResult<bool>.Failure("Please select valid Vessel Type");

        //        if (string.IsNullOrWhiteSpace(model.TemplateName))
        //            return ServiceResult<bool>.Failure("Please select valid Template Name");

        //        if (string.IsNullOrWhiteSpace(model.SectionName))
        //            return ServiceResult<bool>.Failure("Please select valid Section Name");

        //        if (string.IsNullOrWhiteSpace(model.DescriptionName))
        //            return ServiceResult<bool>.Failure("Description Name Required");

        //        // Resolve Section + TankType
        //        var sections = await _descriptionRepository
        //            .GetDescriptionSectionNamesByTemplateNameAndVesselType(model.TemplateName, model.VesselType);

        //        if (sections == null || !sections.Any())
        //            return ServiceResult<bool>.Failure("Invalid Section");

        //        var section = sections.FirstOrDefault(x => x.SectionName == model.SectionName);

        //        if (section == null)
        //            return ServiceResult<bool>.Failure("Please select valid Section");

        //        // Set TankTypeId or SectionId based on TanktypeId
        //        if (section.TanktypeId != 0)
        //        {
        //            model.TankTypeId = section.TanktypeId;
        //        }
        //        else
        //        {
        //            model.SectionId = section.SectionId;
        //        }

        //        // Duplicate Check
        //        var existing = await _descriptionRepository.CheckImageDescriptionExistsOrNot(
        //            model.VesselType,
        //            model.SectionName,
        //            model.TemplateName,
        //            model.DescriptionName);

        //        if (existing != null && existing.Any())
        //            return ServiceResult<bool>.Failure("The description is already available in the section");

        //        // Build Entity
        //        var entity = new ImageDescriptions
        //        {

        //            Description = model.DescriptionName.Trim(),
        //            VesselType = model.VesselType,
        //            SectionId = section.TanktypeId == 0 ? section.SectionId : null,
        //            TankTypeId = section.TanktypeId != 0 ? section.TanktypeId : null,
        //            IsActive = model.Status,
        //            CreatedDttm = DateTime.UtcNow,
        //            UpdatedDttm = DateTime.UtcNow
        //            //Description = model.DescriptionName.Trim(),
        //            //VesselType = model.VesselType,
        //            //SectionId = section.SectionId,
        //            //TankTypeId = section.TanktypeId,
        //            //IsActive = model.Status,
        //            //CreatedDttm = DateTime.UtcNow,
        //            //UpdatedDttm = DateTime.UtcNow
        //        };


        //        await _descriptionRepository.CreateImageDescription(entity);

        //        return ServiceResult<bool>.Success(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        return ServiceResult<bool>.Failure("Internal server error");
        //    }
        //}

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

        public async Task<ServiceResult<List<GradingSection>>> GetSectionNamesByTemplateNameAndVesselTypeAsync(string templateName, string vesselType)
        {

            if (string.IsNullOrWhiteSpace(templateName))
                return ServiceResult<List<GradingSection>>
                    .Failure("Template name is required");

            if (string.IsNullOrWhiteSpace(vesselType))
                return ServiceResult<List<GradingSection>>
                    .Failure("Vessel type is required");
            try
            {
                var sections = await _descriptionRepository.GetDescriptionSectionNamesByTemplateNameAndVesselType(templateName, vesselType);

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

            if (duplicates != null && duplicates.Any(x => x.Id != model.Id))
                return ServiceResult<bool>.Failure("The description already exists in this section");

            // Update entity
            description.Description = model.DescriptionName;
            description.IsActive = model.Status;
            description.UpdatedDttm = DateTime.UtcNow;

            await _descriptionRepository.UpdateImageDescription(description);

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<GetDescriptionForEditResponse>>GetDescriptionForEditAsync(int descriptionId,int tankTypeId,Guid sectionId,string username)
        {
            // Permission check
            var permission = await _userPermissionRepository.GetRolePermissionByUserName(
            username,
            EnumExtensions.GetDescription(ManagePages.ManageDescription));

            if (permission?.Edit != true)
                return ServiceResult<GetDescriptionForEditResponse>.Failure("AccessDenied");

            // Get only required descriptions
            var descriptions = await _descriptionRepository.GetAllDescription();

            if (!descriptions.Any())
                return ServiceResult<GetDescriptionForEditResponse>.Failure("NotFound");

            Core.Models.ImageDescription.ImageDescriptions result;

            if (tankTypeId == 0 && sectionId == Guid.Empty)
            {
                result = descriptions.FirstOrDefault();
            }
            else if (tankTypeId != 0)
            {
                result = descriptions.FirstOrDefault(x => x.TankTypeId == tankTypeId);
            }
            else
            {
                result = descriptions.FirstOrDefault(x => x.SectionId == sectionId);
            }

            if (result == null)
                return ServiceResult<GetDescriptionForEditResponse>.Failure("NotFound");

            // Load dropdown data
            var templates = await _gradingRepository.GetGradingTemplates();
            var sections = await _gradingRepository.GetGradingSections(0, null);
            var vesselTypes = await _tankRepository.GetShipType();

            var response = new GetDescriptionForEditResponse
            {
                Id = result.Id,
                TemplateName = result.TemplateName,
                VesselType = result.VesselType,
                SectionName = result.SectionName,
                DescriptionName = result.Description,
                TankTypeId = result.TankTypeId ?? 0,
                Status = result.IsActive,
                SectionId = result.SectionId,
                Templates = templates,
                Sections = sections,
                VesselTypes = vesselTypes
            };

            return ServiceResult<GetDescriptionForEditResponse>.Success(response);
        }


        //    public async Task<ServiceResult<GetDescriptionForEditResponse>> EditDescriptionByIdAsync(int descriptionId,Guid sectionId,int tankTypeId,string username)
        //    {
        //        // 1. Permission check
        //        var permission = await _userPermissionRepository.GetRolePermissionByUserName(
        //    username,
        //    EnumExtensions.GetDescription(ManagePages.ManageDescription)
        //);

        //        if (permission?.Edit != true)
        //            return ServiceResult<GetDescriptionForEditResponse>.Failure("AccessDenied");

        //        // 2. Load descriptions
        //        var allDescriptions = await _descriptionRepository.GetAllDescription();

        //        if (allDescriptions == null || !allDescriptions.Any())
        //            return ServiceResult<GetDescriptionForEditResponse>.Failure("NotFound");

        //        // 3. Filter by DescriptionId first
        //        var filtered = allDescriptions.Where(x => x.Id == descriptionId).ToList();

        //        if (!filtered.Any())
        //            return ServiceResult<GetDescriptionForEditResponse>.Failure("NotFound");

        //        // 4. Apply TankType / Section logic
        //        Core.Models.ImageDescription.ImageDescriptions entity;

        //        if (tankTypeId > 0)
        //        {
        //            entity = filtered.FirstOrDefault(x => x.TanktypeId == tankTypeId);
        //        }
        //        else if (sectionId != Guid.Empty)
        //        {
        //            entity = filtered.FirstOrDefault(x => x.SectionId == sectionId);
        //        }
        //        else
        //        {
        //            entity = filtered.FirstOrDefault();
        //        }

        //        if (entity == null)
        //            return ServiceResult<GetDescriptionForEditResponse>.Failure("NotFound");

        //        // 5. Load dropdown master data
        //        var templates = await _gradingRepository.GetGradingTemplates();
        //        var sections = await _gradingRepository.GetGradingSections(0, null);
        //        var vesselTypes = await _tankRepository.GetShipType();

        //        // 6. Build response (USE entity, not list)
        //        var response = new GetDescriptionForEditResponse
        //        {
        //            Id = entity.Id,
        //            TemplateName = entity.TemplateName,
        //            VesselType = entity.VesselType,
        //            SectionName = entity.SectionName,
        //            DescriptionName = entity.Description,
        //            TankTypeId = entity.TanktypeId,
        //            Status = entity.IsActive,
        //            Templates = templates,
        //            Sections = sections,
        //            VesselTypes = vesselTypes
        //        };

        //        return ServiceResult<GetDescriptionForEditResponse>.Success(response);
        //    }


        public async Task<ServiceResult<bool>> UpdateDescriptionAsync(
    UpdateDescriptionRequest model,
    string username)
        {
            // 1️⃣ Permission check
            var permission = await _userPermissionRepository
        .GetRolePermissionByUserName(
            username,
            EnumExtensions.GetDescription(ManagePages.ManageDescription)
        );

            if (permission?.Edit != true)
                return ServiceResult<bool>.Failure("AccessDenied");

            // 2️⃣ Get existing record by ID (FIXED)
            var description = await _descriptionRepository.GetImageDescriptionById(model.Id);

            if (description == null)
                return ServiceResult<bool>.Failure("NotFound");

            // 3️⃣ Update fields (FIXED)
            description.VesselType = model.VesselType;
            description.TemplateName = model.TemplateName;
            description.SectionId = model.SectionId;
            description.Description = model.DescriptionName;
            description.IsActive = model.Status;

            //description.UpdatedBy = username;
            description.UpdatedDttm = DateTime.UtcNow;

            // 4️⃣ Save changes
            await _descriptionRepository.UpdateImageDescription(description);

            // 5️⃣ Return success
            return ServiceResult<bool>.Success(true);
        }


        public Task<ServiceResult<GetDescriptionForEditResponse>> GetDescriptionForEditAsync(int descriptionId, Guid sectionId, int tankTypeId, string username)
        {
            throw new NotImplementedException();
        }
    }

}
