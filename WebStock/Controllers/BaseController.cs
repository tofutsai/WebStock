using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebStock.Models;

namespace WebStock.Controllers
{
    public class BaseController : Controller
    {
        CommonModel CommonModel = new CommonModel();
        public CommonModel.UserInfo UserInfo = new CommonModel.UserInfo();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //取得 ASP.NET 使用者
            var user = System.Web.HttpContext.Current.User;

            //是否通過驗證
            if (user?.Identity?.IsAuthenticated == true)
            {
                //取得 FormsIdentity
                var identity = (FormsIdentity)user.Identity;

                //取得 FormsAuthenticationTicket
                var ticket = identity.Ticket;

                //將 Ticket 內的 UserData 解析回 User 物件
                UserInfo = CommonModel.DecodeJWTTokenMVC(ticket.UserData);

            }
        }
    }
}