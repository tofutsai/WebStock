using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebStock.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            string name = UserInfo.OperName;
            ViewBag.Message = "Hello " + name;
            return View();
        }

       
    }
}