using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebStock.Models;
using static WebStock.Models.CommonModel;
using WebStock.ViewModels;
using static WebStock.ViewModels.ReportViewModel;

namespace WebStock.Controllers
{
    public class stockStatisticsController : BaseController
    {
        ReportModel ReportModel = new ReportModel();
        public ActionResult stockStatistics()
        {
            StockStatisticsView stockStatisticsView = new StockStatisticsView();
            FormSearch form = new FormSearch();
            form.options = new Options();
            stockStatisticsView.formSearch = form;
            stockStatisticsView.sysConfig = new WebStockEntities().sysConfig.FirstOrDefault();
            stockStatisticsView.data = ReportModel.ReadStockStatistics();

            return View(stockStatisticsView);
        }
        [HttpPost]
        public ActionResult stockStatistics(StockStatisticsView data)
        {
            StockStatisticsView stockStatisticsView = new StockStatisticsView();
            data.formSearch.options.page = 1;
            data.formSearch.options.itemsPerPage = 9999;
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