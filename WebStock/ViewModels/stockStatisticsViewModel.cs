using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebStock.Models;
using static WebStock.Models.CommonModel;

namespace WebStock.ViewModels
{
    public class stockStatisticsViewModel
    {
        public int searchOper { get; set; }
        public string searchType { get; set; }
        public string searchCode { get; set; }
        public int searchShares { get; set; }
        public double searchClosePrice { get; set; }
        public string searchSort { get; set; }
        public string searchAorD { get; set; }
        public stockNowStatistics stockNowStatistics { get; set; }
        public sysConfig sysConfig { get; set; }
        public List<stockSummaryStatistics> stockSummaryStatistics { get; set; }
    }
}