using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static WebStock.Models.CommonModel;

namespace WebStock.ViewModels
{
    public class stockStatisticsViewModel
    {
        public List<stockSummaryStatistics> stockSummaryStatistics { get; set; }
    }
}