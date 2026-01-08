using DigitalCap.Core.Models.ImageDescription;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Repository
{
    public interface IDescriptionRepository
    {
        Task<bool> CreateProjectImageDescription(int projectId, string vesseltype);
        Task<List<ImageDescriptions>> GetImageDescriptionsByProjectId(int projectId);
    }
}
