using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebStock.Models;
using WebStock.ViewModels;
using static WebStock.ViewModels.ReportViewModel;

namespace WebStock.Controllers
{
    public class stockProfitController : BaseController
    {
        ReportModel reportModel = new ReportModel();
        // GET: stockProfit
        public ActionResult stockProfit()
        {
            StockProfitView stockProfitView = new StockProfitView();
            //預設查詢條件
            FormSearch form = new FormSearch();
            form.options = new Options();
            form.options.page = 1;
            form.options.itemsPerPage = 9999;
            stockProfitView.formSearch = form;
            stockProfitView.data = reportModel.ReadStockProfit(form,UserInfo.OperId);
            return View(stockProfitView);
        }
                
        public string createStockInventory(stockProfit profit)
        {
            string msg = reportModel.createStockInventory(profit, UserInfo.OperId);
            return msg;
        }

        public string deleteStockInventory(stockProfit data)
        {
            string msg = reportModel.deleteStockInventory(data, UserInfo.OperId);
            return msg;
        }
    }
}