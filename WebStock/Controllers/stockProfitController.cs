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
            form.operId = UserInfo.OperId;
            stockProfitView.formSearch = form;
            stockProfitView.data = reportModel.ReadStockProfit(form);
            return View(stockProfitView);
        }

        public string createStockInventory(stockProfit profit)
        {
            profit.operId = UserInfo.OperId;
            bool status = reportModel.createStockInventory(profit);
            string msg = status ? "新增成功" : "新增失敗";
            return msg;
        }

        public string deleteStockInventory(stockProfit data)
        {
            bool status = reportModel.deleteStockInventory(data.id);
            string msg = status ? "刪除成功" : "刪除失敗";
            return msg;
        }
    }
}