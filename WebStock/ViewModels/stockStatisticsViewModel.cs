using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static WebStock.Models.CommonModel;

namespace WebStock.ViewModels
{
    public class stockStatisticsViewModel
    {
        public int searchOper { get; set; }
        public string searchType { get; set; }
        public string searchCode { get; set; }
        public int searchShares { get; set; }
        public double searchClosingPrice { get; set; }
        public List<stockSummaryStatistics> stockSummaryStatistics { get; set; }
    }
}