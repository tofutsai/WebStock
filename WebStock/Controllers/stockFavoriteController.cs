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
    public class stockFavoriteController : BaseController
    {
        CommonModel commonModel = new CommonModel();
        // GET: stockFavorite
        public ActionResult selfFavorite()
        {
            List<stockSelfFavorite> stockfavorites = commonModel.getStockFavorite(UserInfo.OperId);
            stockFavoriteViewModel stockFavoriteViewModel = new stockFavoriteViewModel();
            stockFavoriteViewModel.favorites = stockfavorites;
            stockFavoriteViewModel.user = UserInfo;
            return View(stockFavoriteViewModel);
        }

        public string CreateFavoriteStock(string code)
        {
            string msg = commonModel.createFavoriteStock(code, UserInfo.OperId);
            return msg;
        }

        public string updateStockMemo(stockFavorite favorite)
        {
            string msg = commonModel.updateStockMemo(favorite, UserInfo.OperId);
            return msg;
        }

        public string DeleteFavoriteStock(string code)
        {
            string msg = commonModel.deleteFavoriteStock(code, UserInfo.OperId);
            return msg;
        }


    }
}