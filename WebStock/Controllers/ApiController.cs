using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebStock.Models;
using static WebStock.ViewModels.ApiViewModel;
using static WebStock.ViewModels.ReportViewModel;

namespace WebStock.Controllers
{
    public class ApiController : BaseApiController
    {
        ReportModel ReportModel = new ReportModel();

        [HttpPost]
        public JsonResult ReadStockIndex(FormSearch form)
        {
            List<RSI> data = ReportModel.ReadStockIndex(form);
            return Json(new Results<List<RSI>>
            {
                Success = data.Count() > 0 ? true : false,
                Message = data.Count() > 0 ? "查詢成功!" : "查詢失敗!",
                Data = data,
                TotalCount = data.Count() > 0 ? data.FirstOrDefault().totalCount : 0
            });
        }

        [HttpPost]
        public JsonResult ReadStockData(FormSearch form)
        {
            List<RSD> data = ReportModel.ReadStockData(form);
            return Json(new Results<List<RSD>>
            {
                Success = data.Count() > 0 ? true : false,
                Message = data.Count() > 0 ? "查詢成功!" : "查詢失敗!",
                Data = data,
                TotalCount = data.Count() > 0 ? data.FirstOrDefault().totalCount : 0
            });
        }

        [HttpPost]
        public JsonResult ReadStockStatistics(FormSearch form)
        {
            List<RSS> data = ReportModel.ReadStockStatistics(form);
            return Json(new Results<List<RSS>>
            {
                Success = data.Count() > 0 ? true : false,
                Message = data.Count() > 0 ? "查詢成功!" : "查詢失敗!",
                Data = data,
                TotalCount = data.Count() > 0 ? data.FirstOrDefault().totalCount : 0
            });
        }

        [HttpPost]
        public JsonResult ReadStockFavorite(FormSearch form)
        {
            List<RSF> data = ReportModel.ReadStockFavorite(form, UserInfo.OperId);
            return Json(new Results<List<RSF>>
            {
                Success = data.Count() > 0 ? true : false,
                Message = data.Count() > 0 ? "查詢成功!" : "查詢失敗!",
                Data = data,
                TotalCount = data.Count() > 0 ? data.FirstOrDefault().totalCount : 0
            });
        }

        [HttpPost]
        public JsonResult ReadStockProfit(FormSearch form)
        {
            List<RSP> data = ReportModel.ReadStockProfit(form, UserInfo.OperId);
            return Json(new Results<List<RSP>>
            {
                Success = data.Count() > 0 ? true : false,
                Message = data.Count() > 0 ? "查詢成功!" : "查詢失敗!",
                Data = data,
                TotalCount = data.Count() > 0 ? data.FirstOrDefault().totalCount : 0
            });
        }
    }
}