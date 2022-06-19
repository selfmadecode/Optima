using Optima.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Utilities
{
    public static class DateRangeExtensions
    {
        public static TimeBoundSearchVm SetDateRange(this TimeBoundSearchVm model)
        {
            if (model == null)
                return model;

            switch (model.TimeRange)
            {
                case Models.Enums.DateRangeQueryType.Today:
                    {
                        if (model.FromDate == null)
                            model.FromDate = DateTime.Now.Date;
                        break;
                    }
                case Models.Enums.DateRangeQueryType.Week:
                    {
                        if (model.FromDate == null)
                            model.FromDate = DateTime.Now.Date.AddDays(-7);
                        break;
                    }
                case Models.Enums.DateRangeQueryType.Month:
                    {
                        if (model.FromDate == null)
                            model.FromDate = DateTime.Now.Date.AddMonths(-1);
                        break;
                    }
                case Models.Enums.DateRangeQueryType.Quarter:
                    {
                        if (model.FromDate == null)
                            model.FromDate = DateTime.Now.Date.AddMonths(-3);
                        break;
                    }
                case Models.Enums.DateRangeQueryType.BiAnnual:
                    {
                        if (model.FromDate == null)
                            model.FromDate = DateTime.Now.Date.AddMonths(-6);
                        break;
                    }
                case Models.Enums.DateRangeQueryType.Annual:
                    {
                        if (model.FromDate == null)
                            model.FromDate = DateTime.Now.Date.AddMonths(-12);
                        break;
                    }
            }

            return model;
        }


      
    }

    public class TimeBoundSearchVm
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public DateRangeQueryType TimeRange { get; set; }
    }
}
