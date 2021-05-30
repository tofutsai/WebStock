using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using WebStock.Models;
using static WebStock.Models.CommonModel;

namespace WebStock
{
    public class MvcApplication : System.Web.HttpApplication
    {
        CommonModel CommonModel = new CommonModel();
        public CommonModel.UserInfo UserInfo { get; set; }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            //取得 ASP.NET 使用者
            var user = HttpContext.Current.User;
            if (user != null)
            {
                //是否通過驗證
                if (user?.Identity?.IsAuthenticated == true)
                {
                    //取得 FormsIdentity
                    var identity = (FormsIdentity)user.Identity;

                    //取得 FormsAuthenticationTicket
                    var UserData = identity.Ticket.UserData;
                    UserInfo = CommonModel.DecodeJWTTokenMVC(UserData);

                    string[] roles = UserInfo.OperRole.Split(new char[] { ',' });

                    Context.User = new GenericPrincipal(Context.User.Identity, roles);
                }

            }
        }
    }
}
