using DigitalCap.Core.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DigitalCap.Core.Models
{
    public class AllProjectsGridExport
    {
        public int? ID { get; set; }

        [Required(ErrorMessage = "Please enter Project Name")]
        [Display(Name = "Project Name")]
        public string Name { get; set; }

        [RegularExpression(@"[1-9]\d{6}", ErrorMessage = "Project ID must be 7 digits.")]
        [Display(Name = "Project ID")]
        public int? PIDNumber { get; set; }

        [Required(ErrorMessage = "Please enter CAP Coordinator")]
        [Display(Name = "CAP Coordinator")]
        public string CapRegion { get; set; }

        [Required(ErrorMessage = "Please enter CAP Scope")]
        [Display(Name = "CAP Scope")]
        public string CapScope { get; set; }

        [Display(Name = "Other")]
        public string CapScopeOther { get; set; }

        [Required(ErrorMessage = "Please enter CAP Type")]
        [EnumDataType(typeof(CapType))]
        [Display(Name = "CAP Type")]
        public CapType CapType { get; set; }

        [Display(Name = "Planned Drydock Location")]
        public int? DrydockLocation { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{mm-dd-yyyy}")]
        [Display(Name = DisplayNames.PlannedDrydock.Start)]
        public DateTime? DrydockStart { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{mm-dd-yyyy}")]
        [Display(Name = DisplayNames.PlannedDrydock.End)]
        public DateTime? DrydockEnd { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{mm-dd-yyyy}")]
        [Display(Name = "Potential Drydock Date")]
        public DateTime? PotentialDrydockDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{mm-dd-yyyy}")]
        [Display(Name = DisplayNames.SurveyDates.FirstVisit)]
        public DateTime? SurveyFirstVisit { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{mm-dd-yyyy}")]
        [Display(Name = DisplayNames.SurveyDates.LastVisit)]
        public DateTime? SurveyLastVisit { get; set; }

        public int? ProjectStatus { get; set; }

        [Display(Name = "Project Priority")]
        public int? ProjectPriority { get; set; }

        public bool SpecialHull { get; set; } = true;

        [Required]
        public bool Fatigue { get; set; } = true;

        [RegularExpression(@"\d\d?", ErrorMessage = "Expense Markup must be one or two digits.")]
        public int? ExpenseMarkup { get; set; }

        [DataType(DataType.Currency)]
        public decimal? SurveyDayrateClient { get; set; }

        [DataType(DataType.Currency)]
        public decimal? SurveyDayrateAbs { get; set; }

        [DataType(DataType.Currency)]
        public decimal? SedRate1 { get; set; }

        [DataType(DataType.Currency)]
        public decimal? SedRate2 { get; set; }

        [Required(ErrorMessage = "Please enter CAP Contract Value")]
        [DataType(DataType.Currency)]
        [Display(Name = "CAP Contract Value")]
        public decimal? CapContractValue { get; set; }

        [Required(ErrorMessage = "Please enter Agreement / Bid Number")]
        [RegularExpression(@"[a-zA-Z0-9]+", ErrorMessage = "Bid number must be alphanumeric.")]
        [Display(Name = "Agreement / Bid Number")]
        public string AgreementNumber { get; set; }

        [Required(ErrorMessage = "Please enter Proposal Rev. No.")]
        [RegularExpression(@"\d\d?", ErrorMessage = "Proposal Rev. must be one or two digits.")]
        [Display(Name = "Proposal Rev. No.")]
        public int? ProposalRev { get; set; }

        [Required(ErrorMessage = "Please enter Agreement Owner")]
        [Display(Name = "Agreement Owner")]
        public string AgreementOwner { get; set; }

        [Required(ErrorMessage = "Please enter Agreement Submitted Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{mm-dd-yyyy}")]
        [Display(Name = DisplayNames.Agreement.Date)]
        public DateTime? AgreementDate { get; set; }

        [Required(ErrorMessage = "Please enter Agreement Signed Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{mm-dd-yyyy}")]
        [Display(Name = DisplayNames.Agreement.SignedDate)]
        public DateTime? AgreementSignedDate { get; set; }

        public string CertificateGrade { get; set; }

        public string ProjectComments { get; set; }

        [Required(ErrorMessage = "Please enter Company Name")]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Please enter Company Address")]
        [Display(Name = "Company Address")]
        public string CompanyAddress { get; set; }

        [EmailAddress]
        [Display(Name = "Company Billing Email")]
        public string CompanyBillingEmail { get; set; }

        [Url]
        [Display(Name = "Company Billing System URL")]
        public string CompanyBillingSystemUrl { get; set; }

        [Required]
        [Display(Name = "Same as Contract Client?")]
        public bool BillSameAsCapClient { get; set; } = true;

        [Display(Name = DisplayNames.BillToCompany.Name)]
        public string BillToCompanyName { get; set; }

        [Display(Name = DisplayNames.BillToCompany.Address)]
        public string BillToCompanyAddress { get; set; }

        [EmailAddress]
        [Display(Name = DisplayNames.BillToCompany.Email)]
        public string BillToCompanyEmail { get; set; }

        [Url]
        [Display(Name = DisplayNames.BillToCompany.BillingUrl)]
        public string BillToCompanyBillingUrl { get; set; }

        [Display(Name = "Point of Contact Name")]
        public string PocName { get; set; }

        [EmailAddress]
        [Display(Name = "Point of Contact Email")]
        public string PocEmail { get; set; }

        [Phone]
        [Display(Name = "Point of Contact Phone")]
        public string PocPhone { get; set; }

        [Required(ErrorMessage = "Please enter IMO Number")]
        [Display(Name = "IMO Number")]
        public int? IMO { get; set; }

        [Required(ErrorMessage = "Please enter Vessel Name")]
        [Display(Name = "Vessel Name")]
        public string VesselName { get; set; }

        [Required(ErrorMessage = "Please enter Ship Type")]
        [EnumDataType(typeof(ShipType))]
        [Display(Name = "Ship Type")]
        public ShipType ShipType { get; set; }

        [Required(ErrorMessage = "Please enter Client Profile")]
        [Display(Name = "Client Profile")]
        public Guid? ClientProfileId { get; set; }

        [Display(Name = "Contract Client Profile")]
        public Guid? BillToClientProfileId { get; set; }

        [RegularExpression(@"\d{6}", ErrorMessage = "WCN must be six digits.")]
        [Display(Name = "WCN Number")]
        public string WCN { get; set; }

        [RegularExpression(@"\d{6}", ErrorMessage = "WCN must be six digits.")]
        [Display(Name = "WCN Number")]
        public string BillToWCN { get; set; }

        [Required(ErrorMessage = "Please enter Classification")]
        [Display(Name = "Classification")]
        public string[] ClassSociety { get; set; }

        [Display(Name = "Other")]
        public string ClassSocietyOther { get; set; }

        [Display(Name = "ABS Class Number")]
        public string AbsClassID { get; set; }

        [Required]
        [Display(Name = "Does a Contract Sister Exist?")]
        public bool SisterVessel { get; set; } = false;

        [Display(Name = DisplayNames.SisterVessel.ImoNumber)]
        public string SisterVesselImoNumber { get; set; }

        [Display(Name = DisplayNames.SisterVessel.Name)]
        public string SisterVesselName { get; set; }

        public string Builder { get; set; }

        [Display(Name = "Hull Number")]
        public string HullNumber { get; set; }

        [RegularExpression(@"[2-9]|1[0-2]?", ErrorMessage = "Month must be between 1 and 12.")]
        [Display(Name = "Month Built")]
        public int? MonthBuilt { get; set; }

        [RegularExpression(@"\d{4}", ErrorMessage = "Year must have 4 digits.")]
        [Display(Name = "Year Built")]
        public int? YearBuilt { get; set; }

        [Display(Name = "Length Overall")]
        public decimal? LengthOverall { get; set; }

        [RegularExpression(@"OPP-\d{10}", ErrorMessage = "Opportunity Number must be ten digits.")]
        [Display(Name = "CRM Opportunity No.")]
        public string CRMOpportunityNumber { get; set; }

        public string ProjectStatusName { get; set; }
        public string DrydockLocationName { get; set; }

        [Display(Name = "CAP Lead")]
        public string LeadSurveyor { get; set; }

        [Display(Name = "Lead Surveyor EID")]
        public string LeadSurveyorEID { get; set; }

        [Display(Name = "Assigned CAP Team")]
        public string SecondSurveyor { get; set; }

        [Display(Name = "Second Surveyor EID")]
        public string SecondSurveyorEID { get; set; }

        [Display(Name = "Third Surveyor")]
        public string ThirdSurveyor { get; set; }

        [Display(Name = "Third Surveyor EID")]
        public string ThirdSurveyorEID { get; set; }

        [Display(Name = "Fourth Surveyor")]
        public string FourthSurveyor { get; set; }

        [Display(Name = "Fourth Surveyor EID")]
        public string FourthSurveyorEID { get; set; }

        [Required]
        [Display(Name = "Add Additional Surveyors?")]
        public bool AdditionalSurveyors { get; set; } = false;

        public DateTime? CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public string StatusChangedBy { get; set; }
        public DateTimeOffset? StatusChangedDate { get; set; }

        public string LastModifiedBy { get; set; }

        public Grades? FatigueGrade { get; set; }
        public Grades? RenewalGrade { get; set; }
        public Grades? GaugingGrade { get; set; }
        public Grades? MaterialGrade { get; set; }
        public Grades? StructuralGrade { get; set; }
        public Grades? HullGirderStrength { get; set; }
        public FinalGrades? FinalGrade { get; set; }
        public DateTime? CAPCertificateIssuanceDate { get; set; }
        public DateTime? CAP1CertificateIssuanceDate { get; set; }
        public string CAP1FinalGrade { get; set; }
        public DateTime? ClassReportDate { get; set; }
        public string MostRecentProjectComment { get; set; }

        private static class DisplayNames
        {
            public static class SisterVessel
            {
                public const string ImoNumber = "Sister IMO Number";
                public const string Name = "Sister Vessel Name";
            }

            public static class BillToCompany
            {
                public const string Name = "Company Name";
                public const string Address = "Company Address";
                public const string Email = "Company Billing Email";
                public const string BillingUrl = "Company Billing System URL";
            }

            public static class PlannedDrydock
            {
                public const string Start = "Start";
                public const string End = "End";
            }

            public static class SurveyDates
            {
                public const string FirstVisit = "First Visit Date";
                public const string LastVisit = "Last Visit Date";
            }

            public static class Agreement
            {
                public const string Date = "Agreement Submitted Date";
                public const string SignedDate = "Agreement Signed Date";
            }
        }
    }
}
