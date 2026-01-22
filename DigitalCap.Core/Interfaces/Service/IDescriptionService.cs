using DigitalCap.Core.Models;
using DigitalCap.Core.Models.ImageDescription;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Service
{
    public interface IDescriptionService
    {
        Task<List<ImageDescriptions>> GetAllAsync();
        Task<ImageDescriptions> GetByIdAsync(int id);
        Task<ServiceResult<ImageDescriptions>> CreateAsync(ImageDescriptions model);
        Task<ServiceResult<ImageDescriptions>> UpdateAsync(ImageDescriptions model);
    }
}
