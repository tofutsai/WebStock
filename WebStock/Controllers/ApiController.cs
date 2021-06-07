using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebStock.Models;
using static WebStock.ViewModels.ApiViewModel;
using static WebStock.ViewModels.ReportViewModel;
using static WebStock.Models.CommonModel;

namespace WebStock.Controllers
{
    public class ApiController : BaseApiController
    {
        ReportModel reportModel = new ReportModel();
        LoginModel loginModel = new LoginModel();
        [HttpPost]
        public JsonResult ReadStockIndex(FormSearch form)
        {
            bool status = true;
            bool check = true;
            List<RSI> data = null;
            string msg = "";


            if (string.IsNullOrEmpty(form.type))
            {
                check = false;
            }

            if (check)
            {
                data = reportModel.ReadStockIndex(form);
                status = data.Count() > 0 ? true : false;
                msg = status ? "查詢成功!" : "查詢失敗!";
            }
            else
            {
                status = false;
                msg = "資料輸入錯誤!";
            }

            return Json(new Results<List<RSI>>
            {
                Success = status,
                Message = msg,
                Data = data,
                TotalCount = status ? data.FirstOrDefault().totalCount : 0
            });
        }

        [HttpPost]
        public JsonResult ReadStockData(FormSearch form)
        {
            bool status = true;
            bool check = true;
            List<RSD> data = null;
            string msg = "";


            if (string.IsNullOrEmpty(form.code))
            {
                check = false;
            }
            if (form.dataDate == DateTime.MinValue)
            {
                check = false;
            }

            if (check)
            {
                data = reportModel.ReadStockData(form);
                status = data.Count() > 0 ? true : false;
                msg = status ? "查詢成功!" : "查詢失敗!";
            }
            else
            {
                status = false;
                msg = "資料輸入錯誤!";
            }

            return Json(new Results<List<RSD>>
            {
                Success = status,
                Message = msg,
                Data = data,
                TotalCount = status ? data.FirstOrDefault().totalCount : 0
            });
        }

        [HttpPost]
        public JsonResult ReadStockStatistics(FormSearch form)
        {
            bool status = true;
            bool check = true;
            List<RSS> data = null;
            string msg = "";

            if (string.IsNullOrEmpty(form.type))
            { check = false; }


            if (check)
            {
                data = reportModel.ReadStockStatistics(form);
                status = data.Count() > 0 ? true : false;
                msg = status ? "查詢成功!" : "查詢失敗!";
            }
            else
            {
                status = false;
                msg = "資料輸入錯誤!";
            }

            return Json(new Results<List<RSS>>
            {
                Success = status,
                Message = msg,
                Data = data,
                TotalCount = status ? data.FirstOrDefault().totalCount : 0
            });
        }

        [HttpPost]
        public JsonResult ReadStockFavorite(FormSearch form)
        {
            bool status = true;
            bool check = true;
            List<RSF> data = null;
            string msg = "";

            if (form.operId == 0)
            { check = false; }


            if (check)
            {
                data = reportModel.ReadStockFavorite(form);
                status = data.Count() > 0 ? true : false;
                msg = status ? "查詢成功!" : "查詢失敗!";
            }
            else
            {
                status = false;
                msg = "資料輸入錯誤!";
            }

            return Json(new Results<List<RSF>>
            {
                Success = status,
                Message = msg,
                Data = data,
                TotalCount = status ? data.FirstOrDefault().totalCount : 0
            });
        }

        [HttpPost]
        public JsonResult ReadStockProfit(FormSearch form)
        {
            bool status = true;
            bool check = true;
            List<RSP> data = null;
            string msg = "";

            if (form.operId == 0)
            { check = false; }


            if (check)
            {
                data = reportModel.ReadStockProfit(form);
                status = data.Count() > 0 ? true : false;
                msg = status ? "查詢成功!" : "查詢失敗!";
            }
            else
            {
                status = false;
                msg = "資料輸入錯誤!";
            }

            return Json(new Results<List<RSP>>
            {
                Success = status,
                Message = msg,
                Data = data,
                TotalCount = status ? data.FirstOrDefault().totalCount : 0
            });
        }


        [HttpPost]
        public JsonResult ReadSysConfig()
        {
            bool status = true;
            List<RSC> data = null;
            string msg = "";

            data = reportModel.getsysConfig();
            status = data.Count() > 0 ? true : false;
            msg = status ? "查詢成功!" : "查詢失敗!";

            return Json(new Results<List<RSC>>
            {
                Success = status,
                Message = msg,
                Data = data,
                TotalCount = status ? data.FirstOrDefault().totalCount : 0
            });

        }

        [HttpPost]
        public async Task<JsonResult> DownloadStockData()
        {
            bool status = true;
            string msg = "";

            try
            {
                DownloadModel downloadModel = new DownloadModel();
                SysModel sysModel = new SysModel();

                DateTime date = sysModel.getsysConfigstockUpdate();

                int count = 0;
                while (date <= DateTime.Now)
                {
                    string datetime = date.ToString("yyyy-MM-dd");
                    string s = await downloadModel.DownloadStock(datetime);
                    msg += s + "<br/>";
                    DateTime nextdate = date.AddDays(1);
                    count = sysModel.updatesysConfigstockUpdate(nextdate);
                    date = nextdate;
                    Thread.Sleep(5000);
                }
                status = true;
            }
            catch (Exception e)
            {
                status = false;
                msg += e.Message;
            }
            return Json(new Results<DBNull>
            {
                Success = status,
                Message = msg,
                Data = null,
                TotalCount = status ? 1 : 0
            });
        }

        [HttpPost]
        public async Task<JsonResult> DownloadOtcData()
        {
            bool status = true;
            string msg = "";
            try
            {
                DownloadModel downloadModel = new DownloadModel();
                SysModel sysModel = new SysModel();

                DateTime date = sysModel.getsysConfigotcUpdate();

                int count = 0;
                while (date <= DateTime.Now)
                {
                    DateTime taiwandatetime = date.AddYears(-1911);
                    string datetime = taiwandatetime.ToString("yyyy/MM/dd");
                    string s = await downloadModel.DownloadOtc(datetime);
                    msg += s + "<br/>";
                    DateTime nextdate = date.AddDays(1);
                    count = sysModel.updatesysConfigotcUpdate(nextdate);
                    date = nextdate;
                    Thread.Sleep(5000);
                }
            }
            catch (Exception e)
            {
                status = false;
                msg += e.Message;
            }

            return Json(new Results<DBNull>
            {
                Success = status,
                Message = msg,
                Data = null,
                TotalCount = status ? 1 : 0
            });

        }

        [HttpPost]
        public JsonResult ComputeStockAvg()
        {
            bool status = true;
            string msg = "";

            status = reportModel.stockAvgStatistics();
            msg = status ? "計算成功!" : "計算失敗!";

            return Json(new Results<DBNull>
            {
                Success = status,
                Message = msg,
                Data = null,
                TotalCount = status ? 1 : 0
            });
        }

        [HttpPost]
        public JsonResult ComputeStockNow()
        {
            bool status = true;
            string msg = "";

            status = reportModel.stockNowsStatistics();
            msg = status ? "計算成功!" : "計算失敗!";

            return Json(new Results<DBNull>
            {
                Success = status,
                Message = msg,
                Data = null,
                TotalCount = status ? 1 : 0
            });
        }

        [HttpPost]
        public JsonResult StockMemo(string type, string codes, string memoContent)
        {
            bool status = true;
            bool check = true;
            string msg = "";


            if (string.IsNullOrEmpty(type))
            {
                check = false;
            }
            if (string.IsNullOrEmpty(codes))
            {
                check = false;
            }

            if (check)
            {
                status = reportModel.addStockMemo(type, codes, memoContent);
                msg = status ? "更新成功!" : "更新失敗!";
            }
            else
            {
                status = false;
                msg = "資料輸入錯誤!";

            }

            return Json(new Results<DBNull>
            {
                Success = status,
                Message = msg,
                Data = null,
                TotalCount = status ? 1 : 0
            });
        }


        [HttpPost]
        public JsonResult EditSysConfig(sysConfig sys)
        {
            bool status = true;
            bool check = true;
            string msg = "";

            if (sys.stockUpdate == DateTime.MinValue)
            {
                check = false;
            }
            if (sys.otcUpdate == DateTime.MinValue)
            {
                check = false;
            }
            if (sys.nowDate == DateTime.MinValue)
            {
                check = false;
            }
            if (sys.avgStartDate == DateTime.MinValue)
            {
                check = false;
            }
            if (sys.avgEndDate == DateTime.MinValue)
            {
                check = false;
            }

            if (check)
            {
                status = reportModel.EditSysConfig(sys);
                msg = status ? "更新成功!" : "更新失敗!";
            }
            else
            {
                status = false;
                msg = "資料輸入錯誤!";
            }
            return Json(new Results<DBNull>
            {
                Success = status,
                Message = msg,
                Data = null,
                TotalCount = status ? 1 : 0
            });
        }

        public JsonResult CreateStockFavorite(stockFavorite form)
        {
            bool status = true;
            bool check = true;
            string msg = "";

            if (form.operId == 0)
            {
                check = false;
            }
            if (string.IsNullOrEmpty(form.code))
            {
                check = false;
            }

            if (check)
            {
                string s = reportModel.createFavoriteStock(form.code, form.operId);
                status = s != "02" ? true : false;
                msg = s != "02" ? s != "03" ? "新增成功!" : "無須新增!" : "新增失敗!";
            }
            else
            {
                status = false;
                msg = "資料輸入錯誤!";
            }

            return Json(new Results<DBNull>
            {
                Success = status,
                Message = msg,
                Data = null,
                TotalCount = status ? 1 : 0
            });
        }

        public JsonResult EditStockFavorite(stockFavorite form)
        {
            bool status = true;
            bool check = true;
            string msg = "";

            if (form.id == 0)
            {
                check = false;
            }


            if (check)
            {
                status = reportModel.updateFavoriteStockMemo(form);
                msg = status ? "更新成功!" : "更新失敗!";
            }
            else
            {
                status = false;
                msg = "資料輸入錯誤!";
            }

            return Json(new Results<DBNull>
            {
                Success = status,
                Message = msg,
                Data = null,
                TotalCount = status ? 1 : 0
            });
        }

        public JsonResult DeleteStockFavorite(stockFavorite form)
        {
            bool status = true;
            bool check = true;
            string msg = "";

            if (form.id == 0)
            {
                check = false;
            }


            if (check)
            {
                status = reportModel.deleteFavoriteStock(form.id);
                msg = status ? "刪除成功!" : "刪除失敗!";
            }
            else
            {
                status = false;
                msg = "資料輸入錯誤!";
            }

            return Json(new Results<DBNull>
            {
                Success = status,
                Message = msg,
                Data = null,
                TotalCount = status ? 1 : 0
            });
        }

        public JsonResult CreateStockInventory(stockProfit form)
        {
            bool status = true;
            bool check = true;
            string msg = "";

            if (form.operId == 0)
            {
                check = false;
            }
            if (string.IsNullOrEmpty(form.code))
            {
                check = false;
            }
            if (form.buyShares == 0)
            {
                check = false;
            }
            if (form.buyPrice == 0)
            {
                check = false;
            }
            if (check)
            {
                status = reportModel.createStockInventory(form);

                msg = status ? "新增成功!" : "新增失敗!";
            }
            else
            {
                status = false;
                msg = "資料輸入錯誤!";
            }

            return Json(new Results<DBNull>
            {
                Success = status,
                Message = msg,
                Data = null,
                TotalCount = status ? 1 : 0
            });
        }
        public JsonResult DeleteStockInventory(stockFavorite form)
        {
            bool status = true;
            bool check = true;
            string msg = "";

            if (form.id == 0)
            {
                check = false;
            }


            if (check)
            {
                status = reportModel.deleteStockInventory(form.id);
                msg = status ? "刪除成功!" : "刪除失敗!";
            }
            else
            {
                status = false;
                msg = "資料輸入錯誤!";
            }

            return Json(new Results<DBNull>
            {
                Success = status,
                Message = msg,
                Data = null,
                TotalCount = status ? 1 : 0
            });
        }



        [HttpPost]
        public JsonResult Login(member form)
        {
            bool status = true;
            bool check = true;
            UserInfo data = null;
            string msg = "";


            if (string.IsNullOrEmpty(form.account))
            {
                check = false;
            }
            if (string.IsNullOrEmpty(form.password))
            {
                check = false;
            }
            if (check)
            {
                member member = loginModel.Login(form);
                if (member != null)
                {
                    var payload = new UserInfo
                    {
                        OperId = form.id,
                        OperAccount = form.account,
                        OperName = form.name
                    };

                    string JWTToken = EncodeJWTToken(payload);

                    data.OperId = member.id;
                    data.OperAccount = member.account;
                    data.OperName = member.name;
                    data.OperIsAdmin = member.isAdmin;
                    data.JWToken = JWTToken;

                }

                status = data != null ? true : false;
                msg = status ? "登入成功!" : "登入失敗!";
            }
            else
            {
                status = false;
                msg = "資料輸入錯誤!";
            }

            return Json(new Results<UserInfo>
            {
                Success = status,
                Message = msg,
                Data = data,
                TotalCount = status ? 1 : 0
            });
        }
        public JsonResult Register(member form)
        {
            bool status = true;
            bool check = true;
            string msg = "";

            if (string.IsNullOrEmpty(form.account))
            {
                check = false;
            }
            if (string.IsNullOrEmpty(form.password))
            {
                check = false;
            }
            if (string.IsNullOrEmpty(form.name))
            {
                check = false;
            }

            if (check)
            {
                string s = loginModel.Register(form);
                status = s != "02" ? true : false;
                msg = s != "02" ? s != "03" ? "新增成功!" : "無須新增!" : "新增失敗!";
            }
            else
            {
                status = false;
                msg = "資料輸入錯誤!";
            }

            return Json(new Results<DBNull>
            {
                Success = status,
                Message = msg,
                Data = null,
                TotalCount = status ? 1 : 0
            });
        }
    }
}