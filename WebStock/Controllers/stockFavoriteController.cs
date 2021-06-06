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
    public class stockFavoriteController : BaseController
    {
        ReportModel reportModel = new ReportModel();
        // GET: stockFavorite
        public ActionResult selfFavorite()
        {
            StockFavoriteView stockFavoriteView = new StockFavoriteView();
            FormSearch form = new FormSearch();
            stockFavoriteView.formSearch = form;
            stockFavoriteView.data = reportModel.ReadStockFavorite(form,UserInfo.OperId);
            return View(stockFavoriteView);
        }

        public string CreateFavoriteStock(string code)
        {
            string msg = reportModel.createFavoriteStock(code, UserInfo.OperId);
            return msg;
        }

        public string UpdateStockMemo(stockFavorite favorite)
        {
            string msg = reportModel.updateStockMemo(favorite, UserInfo.OperId);
            return msg;
        }

        public string DeleteFavoriteStock(string code)
        {
            string msg = reportModel.deleteFavoriteStock(code, UserInfo.OperId);
            return msg;
        }


    }
}