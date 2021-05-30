using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebStock.Models;
using static WebStock.ViewModels.ApiViewModel;
using static WebStock.Models.CommonModel;

namespace WebStock.Controllers
{
    public class BaseApiController : Controller
    {
        public UserInfo UserInfo { get; set; }
        public bool firstStatus { get; set; }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            var headder = filterContext.HttpContext.Request.Headers["Content-Language"];

            if (!string.IsNullOrEmpty(headder))
            {
                UserInfo = DecodeJWTToken(headder) as UserInfo;

            }
            else
            {
                //throw new Exception("Authorization Access denied");
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            var e = filterContext.Exception;
            var inner = "";
            var inner2 = "";

            if (e.InnerException != null)
            {
                inner = e.InnerException.Message;

                if (e.InnerException.InnerException != null)
                    inner2 = e.InnerException.InnerException.Message;
            }
            filterContext.Result = new JsonResult
            {
                Data = new Results<DBNull>
                {
                    Success = false,
                    Message = string.Format("{0}\n{1}\n{2}", e.Message, inner, inner2),
                    Code = "500"
                }
            };
            filterContext.ExceptionHandled = true;
        }
    }
}