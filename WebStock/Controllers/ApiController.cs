using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebStock.Models;
using static WebStock.Models.CommonModel;
using static WebStock.ViewModels.ApiViewModel;
using static WebStock.ViewModels.ReportViewModel;

namespace WebStock.Controllers
{
    public class ApiController : BaseApiController
    {
        ReportModel ReportModel = new ReportModel();

        [HttpPost]
        public JsonResult ReadStockIndex(FormSearch f)
        {
            List<RSI> data = ReportModel.ReadStockIndex(f);
            return Json(new Results<List<RSI>>
            {
                Success = data.Count() > 0 ? true : false,
                Message = data.Count() > 0 ? "查詢成功!" : "查詢失敗!",
                Data = data,
                TotalCount = data.Count() > 0 ? data.FirstOrDefault().totalCount : 0
            });
        }

        [HttpPost]
        public JsonResult ReadStockData(FormSearch f)
        {
            List<RSD> data = ReportModel.ReadStockData(f);
            return Json(new Results<List<RSD>>
            {
                Success = data.Count() > 0 ? true : false,
                Message = data.Count() > 0 ? "查詢成功!" : "查詢失敗!",
                Data = data,
                TotalCount = data.Count() > 0 ? data.FirstOrDefault().totalCount : 0
            });
        }
    }
}