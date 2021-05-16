using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebStock.Models;
using static WebStock.Models.CommonModel;
using WebStock.ViewModels;

namespace WebStock.Controllers
{
    public class stockStatisticsController : BaseController
    {
        CommonModel commonModel = new CommonModel();
        // GET: stockStatistics
        public ActionResult index()
        {
            return View();
        }

        
        public ActionResult stockStatistics()
        {
            List<stockSummaryStatistics> summaryStatistics = commonModel.summaryStatistics();
            stockStatisticsViewModel stockStatisticsViewModel = new stockStatisticsViewModel();
            stockStatisticsViewModel.stockSummaryStatistics = summaryStatistics;

            return View(stockStatisticsViewModel);
        }

    }
}