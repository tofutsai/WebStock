using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebStock.Models;

namespace WebStock.Controllers
{
    public class LoginController : BaseController
    {
        LoginModel LoginModel = new LoginModel();
        //註冊一開始顯示頁面
        public ActionResult Index()
        {
            //判斷使用者是否已經過登入驗證
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            else
                return View();
        }

        public ActionResult Register()
        {
            //判斷使用者是否已經過登入驗證
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            else
                return View();
        }
        [HttpPost]
        public ActionResult Register(member formData)
        {
            //判斷使用者是否已經過登入驗證
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            else
            {
                LoginModel.Register(formData);
                return RedirectToAction("Login", "Login");
            }
        }

        // GET: Login
        public ActionResult Login()
        {
            //判斷使用者是否已經過登入驗證
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            else
                return View();
            
        }

        [HttpPost]
        public ActionResult Login(member formData, FormCollection form)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            else
            {
                member member = LoginModel.Login(formData);
                if (member != null)
                {
                    LoginModel.LoginSetting(member);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.Clear();
                    ModelState.AddModelError("Password", "您輸入的帳號或密碼錯誤，請重新輸入 !");
                    return View();
                }
                    
            }
            
        }

        //登出Action
        [Authorize]//設定此Action須登入
        public ActionResult Logout()
        {
            //使用者登出
            LoginModel.Logout();
            //重新導向至登入Action
            return RedirectToAction("Login");
        }
    }
}