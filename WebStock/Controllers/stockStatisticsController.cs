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
              
        public ActionResult stockStatistics()
        {
            List<stockSummaryStatistics> summaryStatistics = commonModel.summaryStatistics();
            stockStatisticsViewModel stockStatisticsViewModel = new stockStatisticsViewModel();
            stockStatisticsViewModel.stockSummaryStatistics = summaryStatistics;

            return View(stockStatisticsViewModel);
        }
        [HttpPost]
        public ActionResult stockStatistics(stockStatisticsViewModel viewModel)
        {
            List<stockSummaryStatistics> summaryStatistics = commonModel.summaryStatistics(viewModel);
            stockStatisticsViewModel stockStatisticsViewModel = new stockStatisticsViewModel();
            stockStatisticsViewModel.stockSummaryStatistics = summaryStatistics;

            return View(stockStatisticsViewModel);
        }

        
        public string stockFavorite(string code)
        {
            string result = commonModel.addFavorite(code, UserInfo.OperId);
            return result;
        }
    }
}