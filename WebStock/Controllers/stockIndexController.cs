using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebStock.Models;
using static WebStock.ViewModels.ReportViewModel;

namespace WebStock.Controllers
{
    public class stockIndexController : BaseController
    {
        ReportModel ReportModel = new ReportModel();
        CommonModel commonModel = new CommonModel();
        // GET: stockIndex
        public ActionResult Index()
        {
            //List<stockIndex> stockIndexs = commonModel.getStockIndex();
            //return View(stockIndexs);

            //預設查詢條件
            FormSearch f = new FormSearch();
            f.type = "上";
            f.options = new Options();
            f.options.sortBy = new string[] { "id" };
            f.options.sortDesc = new bool[] { false };
            f.options.page = 1;
            f.options.itemsPerPage = 9999;

            StockIndexView StockIndexView = new StockIndexView();
            StockIndexView.data = ReportModel.ReadStockIndex(f);
            return View(StockIndexView);
        }
    }
}