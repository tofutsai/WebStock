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
            form.operId = UserInfo.OperId;
            stockFavoriteView.formSearch = form;
            stockFavoriteView.data = reportModel.ReadStockFavorite(form);
            return View(stockFavoriteView);
        }

        public string CreateFavoriteStock(string code)
        {
            string msg = reportModel.createFavoriteStock(code, UserInfo.OperId);
            msg = msg != "02" ? msg != "03" ? "新增成功" : "無須新增" : "新增失敗";
            return msg;
        }

        public string UpdateFavoriteStockMemo(stockFavorite favorite)
        {
            bool status = reportModel.updateFavoriteStockMemo(favorite);
            string msg = status ? "自行備註編輯成功" : "自行備註編輯失敗"; 
            return msg;
        }

        public string DeleteFavoriteStock(int id)
        {
            bool status = reportModel.deleteFavoriteStock(id);
            string msg = status ? "刪除成功" : "刪除失敗";
            return msg;
        }


    }
}