using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebStock.Models;
using static WebStock.ViewModels.ReportViewModel;

namespace WebStock.Controllers
{
    public class stockStatisticsController : BaseController
    {
        ReportModel ReportModel = new ReportModel();
        public ActionResult stockStatistics()
        {
            StockStatisticsView stockStatisticsView = new StockStatisticsView();
            //預設查詢條件
            FormSearch form = new FormSearch();
            form.type = "上";
            form.shares = 1;
            form.closePrice = 0.0;
            form.position2 = 10;
            form.options.sortBy = new string[] { "position" };
            form.options.sortDesc = new bool[] { false };
            stockStatisticsView.formSearch = form;
            stockStatisticsView.sysConfig = new WebStockEntities().sysConfig.FirstOrDefault();
            stockStatisticsView.data = ReportModel.ReadStockStatistics(form);

            return View(stockStatisticsView);
        }
        [HttpPost]
        public ActionResult stockStatistics(StockStatisticsView data)
        {
            StockStatisticsView stockStatisticsView = new StockStatisticsView();
            data.formSearch.options.sortBy = new string[] { data.formSearch.options.sortByStr };
            data.formSearch.options.sortDesc = new bool[] { data.formSearch.options.sortDescBool };
            stockStatisticsView.sysConfig = new WebStockEntities().sysConfig.FirstOrDefault();
            stockStatisticsView.data = ReportModel.ReadStockStatistics(data.formSearch);

            return View(stockStatisticsView);
        }

        
        public string stockFavorite(string code)
        {
            string result = ReportModel.addFavorite(code, UserInfo.OperId);
            return result;
        }
    }
}