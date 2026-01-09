using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Models.VesselModel
{
    public class Link
    {
        [JsonProperty("rel")]
        public string Rel { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("types")]
        public List<string> Types { get; set; }
    }

    public class Certificate : CachedData
    {
        [JsonProperty("serviceType")]
        public string ServiceType { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("abbr")]
        public string Abbr { get; set; }

        [JsonProperty("term")]
        public string Term { get; set; }

        [JsonProperty("flagName")]
        public string FlagName { get; set; }

        [JsonProperty("deadweight")]
        public string Deadweight { get; set; }

        [JsonProperty("issueDate")]
        public string IssueDate { get; set; }

        [JsonProperty("expiryDate")]
        public string ExpiryDate { get; set; }

        [JsonProperty("extensionDate")]
        public string ExtensionDate { get; set; }

        [JsonProperty("placeOfIssue")]
        public string PlaceOfIssue { get; set; }

        [JsonProperty("issuedBy")]
        public string IssuedBy { get; set; }

        [JsonProperty("personName")]
        public string PersonName { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("statusDate")]
        public string StatusDate { get; set; }

        [JsonProperty("link")]
        public Link Link { get; set; }
    }
}
