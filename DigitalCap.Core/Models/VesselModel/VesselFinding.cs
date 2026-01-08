using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Models.VesselModel
{
    public class Failures
    {
        public string Type { get; set; }
        public string Descriptor { get; set; }
    }

    public class VesselFinding : CachedData
    {
        [JsonProperty("uid")]
        public ulong? Uid { get; set; }

        [JsonProperty("serviceType")]
        public string ServiceType { get; set; }

        [JsonProperty("serviceGroup")]
        public string ServiceGroup { get; set; }

        [JsonProperty("findingNumber")]
        public string FindingNumber { get; set; }

        [JsonProperty("findingType")]
        public string FindingType { get; set; }

        [JsonProperty("failures")]
        public Failures[] Failures { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("partUid")]
        public ulong? PartUid { get; set; }

        [JsonProperty("partName")]
        public string PartName { get; set; }

        [JsonProperty("criticality")]
        public string Criticality { get; set; }

        [JsonProperty("reportNumber")]
        public string ReportNumber { get; set; }

        [JsonProperty("associatedTask")]
        public string AssociatedTask { get; set; }

        [JsonProperty("dateCreated")]
        public DateTime? DateCreated { get; set; }

        [JsonProperty("dueDate")]
        public DateTime? DueDate { get; set; }

        [JsonProperty("dueByTask")]
        public string DueByTask { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("history")]
        public History[] History { get; set; }

        [JsonIgnore]
        public int? CriticalityValue { get; set; }
    }
}
