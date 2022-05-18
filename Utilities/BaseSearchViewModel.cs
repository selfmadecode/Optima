using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AzureRays.Shared.ViewModels
{
    public class BaseSearchViewModel
    {
        /// <summary>
        /// Gets or sets the index of the page.
        /// </summary>
        /// <value>The index of the page.</value>
        [Range(1, int.MaxValue, ErrorMessage = "PageIndex must be greater than 0")]
        public int PageIndex { get; set; } = 1;

        public int PageTotal { get; set; }
        /// <summary>
        /// Gets the page skip.
        /// </summary>
        /// <value>The page skip.</value>
        public int PageSkip => (PageIndex - 1) * PageSize;
        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>The size of the page.</value>
        [Range(1, int.MaxValue, ErrorMessage = "PageSize must be greater than 0")]

        public int PageSize { get; set; } = 20;

        public string Filter { get; set; }

        public string  Keyword { get; set; }

        public DateFilter DateFilter { get; set; }

    }
  
    public class DateFilter 
    {     
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
    
}