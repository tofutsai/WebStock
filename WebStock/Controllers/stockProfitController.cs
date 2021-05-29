using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebStock.Models;
using WebStock.ViewModels;
using static WebStock.Models.CommonModel;

namespace WebStock.Controllers
{
    public class stockProfitController : BaseController
    {
        CommonModel commonModel = new CommonModel();
        // GET: stockProfit
        public ActionResult stockProfit()
        {
            List<stockInventoryProfit> stockInventoryProfits = commonModel.getStockProfit(UserInfo.OperId);
            stockProfitViewModel stockProfitViewModel = new stockProfitViewModel();
            stockProfitViewModel.stockInventoryProfits = stockInventoryProfits;
            stockProfitViewModel.user = UserInfo;
            return View(stockProfitViewModel);
        }

        
        public string createStockInventory(stockProfit profit)
        {
            string msg = commonModel.createStockInventory(profit, UserInfo.OperId);
            return msg;
        }

        public string deleteStockInventory(string code)
        {
            string msg = commonModel.deleteStockInventory(code, UserInfo.OperId);
            return msg;
        }
    }
}