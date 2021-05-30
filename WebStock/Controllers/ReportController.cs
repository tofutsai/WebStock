using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebStock.Controllers
{
    public class ReportController : BaseController
    {
        // GET: Report
        public ActionResult Report()
        {
            return View();
        }
    }
}