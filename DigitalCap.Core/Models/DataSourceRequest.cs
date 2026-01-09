using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalCap.Core.Models
{
    public class DataSourceRequest
    {
        //
        // Summary:
        //     The current page.
        public int Page { get; set; }

        //
        // Summary:
        //     The page size.
        public int PageSize { get; set; }

        //
        // Summary:
        //     The sorting information for the data.
    ///    public IList<SortDescriptor> Sorts { get; set; }

        //
        // Summary:
        //     The filtering information for the data.
      ////  public IList<IFilterDescriptor> Filters { get; set; }

        //
        // Summary:
        //     The grouping information for the data.
     ////   public IList<GroupDescriptor> Groups { get; set; }

        //
        // Summary:
        //     The data aggregation.
     /////   public IList<AggregateDescriptor> Aggregates { get; set; }

        //
        // Summary:
        //     Indicates whether group paging is enabled.
        public bool GroupPaging { get; set; }

        //
        // Summary:
        //     Indicates whether subgroup count should be included
        public bool IncludeSubGroupCount { get; set; }

        //
        // Summary:
        //     The current skip.
        public int Skip { get; set; }

        //
        // Summary:
        //     The current take.
        public int Take { get; set; }

        public DataSourceRequest()
        {
            Page = 1;
     ///       Aggregates = new List<AggregateDescriptor>();
        }
    }
}
