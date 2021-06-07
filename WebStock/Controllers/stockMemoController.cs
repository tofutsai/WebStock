﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebStock.Models;
using WebStock.ViewModels;
using static WebStock.ViewModels.ReportViewModel;

namespace WebStock.Controllers
{
    public class stockMemoController : BaseController
    {
        ReportModel reportModel = new ReportModel();
        // GET: stockMemo
        public ActionResult stockMemo()
        {
            return View();
        }

        [HttpPost]
        public ActionResult stockMemo(stockMemoView data)
        {
            bool status= reportModel.addStockMemo(data.type, data.codes, data.memoContent);
            ViewBag.Message = status ? "新增成功":"新增失敗";
            return View();
        }
    }
}