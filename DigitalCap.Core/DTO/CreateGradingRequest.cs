using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DigitalCap.Core.DTO
{
    public class CreateGradingRequest
    {
        public string VesselType { get; set; }        
        public string TemplateName { get; set; }        
        public string SectionName { get; set; }        
        public string GradingName { get; set; }
        public bool Status { get; set; }
        public bool RequiredInReport { get; set; }
    }

}
