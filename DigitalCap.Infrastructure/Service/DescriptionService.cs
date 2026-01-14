using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.ImageDescription;
using DigitalCap.Core.Interfaces.Service;
using System.Linq;
using DigitalCap.Core.Models.Grading;
using DigitalCap.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Infrastructure.Service
{
    public class DescriptionService : IDescriptionService
    {
        private readonly IDescriptionRepository _descriptionRepository;

        public DescriptionService(IDescriptionRepository descriptionRepository)
        {
            _descriptionRepository = descriptionRepository;
        }

        public async Task<List<ImageDescriptions>> GetAllAsync()
         => await _descriptionRepository.GetAllDescription();

        public async Task<ImageDescriptions> GetByIdAsync(int id)
            => await _descriptionRepository.GetImageDescriptionById(id);

        
        public async Task<ServiceResult<ImageDescriptions>> CreateAsync(ImageDescriptions model)
        {
                if (string.IsNullOrWhiteSpace(model.Description))
                return Core.Models.ServiceResult<ImageDescriptions>.Fail("Description required");

            var sections = await _descriptionRepository
           .GetDescriptionSectionNamesByTemplateNameAndVesselType(model.TemplateName, model.VesselType);

            var section = sections.FirstOrDefault(x => x.SectionName == model.SectionName);
            if (section == null)
                return ServiceResult<ImageDescriptions>.Fail("Invalid Section");

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
                return ServiceResult<ImageDescriptions>.Fail("Already exists");

            await _descriptionRepository.CreateImageDescription(model);
            return ServiceResult<ImageDescriptions>.Success(model);
        }

        public async Task<ServiceResult<ImageDescriptions>> UpdateAsync(ImageDescriptions model)
        {
            var entity = await _descriptionRepository.GetImageDescriptionById(model.Id);
            if (entity == null)
                return ServiceResult<ImageDescriptions>.Fail("Not found");

            var exists = await _descriptionRepository.CheckImageDescriptionExistsOrNot(
                        model.VesselType,
                        model.SectionName,
                        model.TemplateName,
                        model.Description);

            if (exists.Any(x => x.Id != model.Id))
                return ServiceResult<ImageDescriptions>.Fail("Duplicate");

            entity.Description = model.Description;
            entity.IsActive = model.IsActive;
            entity.UpdatedDttm = DateTime.UtcNow;

            await _descriptionRepository.UpdateImageDescription(entity);
            return ServiceResult<ImageDescriptions>.Success(model);
        }
    }

}
