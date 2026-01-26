using DigitalCap.Core.DTO;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.Grading;
using DigitalCap.Core.Models.ImageDescription;
using DigitalCap.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DigitalCap.Core.Interfaces.Service
{
    public interface IDescriptionService
    {
        Task<List<ImageDescriptions>> GetAllAsync();
        Task<ImageDescriptions> GetByIdAsync(int id);
        Task<ServiceResult<ImageDescriptions>> CreateAsync(ImageDescriptions model);
        Task<ServiceResult<ImageDescriptions>> UpdateAsync(ImageDescriptions model);
        Task<ServiceResult<List<GradingSection>>> GetSectionNamesByTemplateNameAndVesselTypeAsync(string templateName, string vesselType);

        Task<ServiceResult<bool>> EditDescriptionAsync(ImageDescriptionViewModel model);
        Task<ServiceResult<bool>> EditDescriptionByIdAsync(int id, string username);

        Task<ServiceResult<bool>> AddNewDescriptionAsync(ImageDescriptionViewModel model);
        Task<ServiceResult<bool>> SetManageDescriptionFiltersAsync(ManageDescriptionFilterRequest request);
        Task<ServiceResult<ManageDescriptionResponse>> ManageDescriptionAsync(string username, bool isActive, int descriptionRestoreFilter, int searchDescriptionRestoreFilter);
        Task<ServiceResult<List<string>>> GetDistinctVesselTypesAsync();

    }
}
