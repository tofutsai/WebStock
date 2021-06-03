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
            //預設查詢條件
            FormSearch form = new FormSearch();
            form.type = "上";
            form.options = new Options();
            form.options.sortBy = new string[] { "id" };
            form.options.sortDesc = new bool[] { false };
            form.options.page = 1;
            form.options.itemsPerPage = 9999;

            StockIndexView StockIndexView = new StockIndexView();
            StockIndexView.data = ReportModel.ReadStockIndex(form);
            return View(StockIndexView);
        }
    }
}