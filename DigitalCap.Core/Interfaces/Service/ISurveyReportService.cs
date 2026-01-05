using DigitalCap.Core.Models.Survey;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Interfaces.Service
{
    public interface ISurveyReportService
    {
        Task<Report> GetReport(int projectId, int templateSectionId, List<string> sectionIds);       
    }
}
