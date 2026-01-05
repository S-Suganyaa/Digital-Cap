using DigitalCap.Core.Models.Survey;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.ViewModels
{
    public class SurveyReportViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public Report Report { get; set; }
        public List<ReportPart> ReportPartGrid { get; set; }
        public List<UpskillImageData> UnplacedImages { get; set; }
        public bool? ReportExport { get; set; } = false;
        public bool? ReportExportAll { get; set; }
        public bool? ResetTemplate { get; set; }
        public IsSynchedOnline SynchedOnline { get; set; }
        public bool IsSynched { get; set; }
        public bool IsRleased { get; set; }
        public bool IsConfiguredProject { get; set; }
    }
}
