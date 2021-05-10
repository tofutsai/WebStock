using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace WebStock.Models
{
    public class LoginModel
    {
        private WebStockEntities db = new WebStockEntities();
        CommonModel CommonModel = new CommonModel();

        internal int Register(member formData)
        {
            string strPassword = FormsAuthentication.HashPasswordForStoringInConfigFile(formData.password, "SHA1");
            formData.password = strPassword;

            db.member.Add(formData);

            return db.SaveChanges();
        }

        internal member Login(member formData)
        {
            string strPassword = FormsAuthentication.HashPasswordForStoringInConfigFile(formData.password, "SHA1");
            formData.password = strPassword;
            var member = db.member.Where(m => m.account == formData.account && m.password == formData.password).FirstOrDefault();
            return member;
        }

        internal bool LoginSetting(member member)
        {
            var payload = new CommonModel.UserInfo
            {
                OperId = member.id,
                OperAccount = member.account,
                OperName = member.name,
                OperRole = member.role,

            };

            string JWTToken = CommonModel.GetJWTToken(payload);

            //新增表單驗證用的票證
            var ticket = new FormsAuthenticationTicket(   // 登入成功，取得門票 (票證)。請自行填寫以下資訊。
                   version: 1,   //版本號（Ver.）
                   name: member.name, // ***自行放入資料（如：使用者帳號、真實名稱），將name改成id取代
                   issueDate: DateTime.Now,  // 登入成功後，核發此票證的本機日期和時間（資料格式 DateTime）
                   expiration: DateTime.Now.AddDays(1),  //  "一天"內都有效（票證到期的本機日期和時間。）
                   isPersistent: true,  // 記住我？ true or false（畫面上通常會用 CheckBox表示）

                   userData: JWTToken,   // ***自行放入資料（如：會員權限、等級、群組） 
                                         // 與票證一起存放的使用者特定資料。
                                         // 需搭配 Global.asax設定檔 - Application_AuthenticateRequest事件。
                   cookiePath: FormsAuthentication.FormsCookiePath
                   );

            var encTicket = FormsAuthentication.Encrypt(ticket); //將 Ticket 加密
            //將 Ticket 寫入 Cookie
            HttpContext.Current.Response.Cookies.Add(
                new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

            return true;
        }

        internal bool Logout()
        {
            FormsAuthentication.SignOut();
            return true;
        }
    }
}