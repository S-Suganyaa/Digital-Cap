using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Repository
{
    public interface IReportPartRepository
    {
        Task<bool> CreateProjectReportTemplate(int vesselTypeId, string userName, int projectId, bool imoExists = false, int copyprojectId = 0);

    }
}
