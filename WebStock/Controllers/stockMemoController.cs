using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebStock.Models;
using WebStock.ViewModels;

namespace WebStock.Controllers
{
    public class stockMemoController : BaseController
    {
        CommonModel commonModel = new CommonModel();
        // GET: stockMemo
        public ActionResult stockMemo()
        {
            return View();
        }

        [HttpPost]
        public ActionResult stockMemo(stockMemoViewModel data)
        {
            string result = commonModel.addStockMemo(data);
            ViewBag.Message = result;
            return View();
        }
    }
}