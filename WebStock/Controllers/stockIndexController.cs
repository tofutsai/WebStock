using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebStock.Models;

namespace WebStock.Controllers
{
    public class stockIndexController : Controller
    {
        CommonModel commonModel = new CommonModel();
        // GET: stockIndex
        public ActionResult Index()
        {
            List<stockIndex> stockIndexs = commonModel.getStockIndex();
            return View(stockIndexs);
        }
    }
}