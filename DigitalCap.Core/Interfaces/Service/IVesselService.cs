using DigitalCap.Core.Models;
using DigitalCap.Core.Models.Survey;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Service
{
    public interface IVesselService
    {
        Task<ServiceResult<string>> CreateVesselMainData(string imoNumber, Project project);
        Task<ServiceResult<string>> CreateSurveyAudit(SurveyStatus surveyStatus, int projectId);
        Task<ServiceResult<string>> CreateStatutoryCertificate(int projectId, Report report);


    }
}
