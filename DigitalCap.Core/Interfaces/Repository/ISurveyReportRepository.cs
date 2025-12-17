using DigitalCap.Core.Models.Survey;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Repository
{
    public interface ISurveyReportRepository
    {
        Task<SurveyStatus> MapSurveyStatus(string classNumber, int imo);
        Task<Report> MapStatuatoryCertificates(string classNumber, Report report);
    }
}
