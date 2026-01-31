using DigitalCap.Core.Models.Grading;
using DigitalCap.Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DigitalCap.Core.ViewModels
{

    public class DescriptionViewModel
    {
        public bool IsActive { get; set; }
        //public List<DigitalCAP.Entities.Models.ImageDescription.ImageDescriptions> ImageDescriptions { get; set; }
        public bool Editable { get; set; }
    }

    public class ImageDescriptionViewModel
    {
        public ImageDescriptionViewModel()
        {
            Status = Active;
        }
        public int Id { get; set; }

        public int TemplateId { get; set; }

        [Display(Name = "Part Name")]
        public string TemplateName { get; set; }

        [Display(Name = "Vessel Template")]
        public string VesselType { get; set; }
        public Guid SectionId { get; set; }

        public string SectionName { get; set; }
        public string DescriptionName { get; set; }

        [HiddenInput(DisplayValue = true)]
        public bool Status { get; set; } = true;

        public int CategoryId { get; set; }

        public int TankTypeId { get; set; }

        public bool Active { get; set; }
        public bool InActive { get; set; }
        public List<GradingSection> sections { get; set; }
        //public List<DigitalCAP.Entities.Models.Grading.GradingTemplate> templates { get; set; }
        //public List<ShipType> vesselTypes { get; set; }
        public List<BreadCrumb> Breadcrumbs { get; set; } = new List<BreadCrumb>();
        public string Controller { get; set; }
        public string PostAction { get; set; }
        public string Title { get; set; }
        public string CancelUrl { get; set; }
        public string SaveText { get; set; }
        public bool NewDescription { get; set; }




    }
}










