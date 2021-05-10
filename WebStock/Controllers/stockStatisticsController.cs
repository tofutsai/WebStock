using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebStock.Models;

namespace WebStock.Controllers
{
    public class stockStatisticsController : BaseController
    {
        CommonModel commonModel = new CommonModel();
        // GET: stockStatistics
        public ActionResult index()
        {
            return View();
        }

        
        public ActionResult stockStatistics()
        {
            List<stockAvg> stockAvgs = commonModel.stockAvgStatistics();
            return View(stockAvgs);
        }

    }
}