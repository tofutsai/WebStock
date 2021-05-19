using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebStock.Models;

namespace WebStock.Controllers
{
    public class GetStockDataController : BaseController
    {
        // GET: GetStockData
        public ActionResult Index()
        {
            using (var db = new WebStockEntities())
            {
                var lastUpdate = db.sysConfig.FirstOrDefault();
                string stockDate = lastUpdate.stockUpdate.ToString("yyyy-MM-dd");
                ViewBag.stockLastUpdate = $"Stock Last Update {stockDate}";
                string otcDate = lastUpdate.otcUpdate.ToString("yyyy-MM-dd");
                ViewBag.otcLastUpdate = $"Otc Last Update {otcDate}";
                string nowDate = lastUpdate.nowDate.ToString("yyyy-MM-dd");
                ViewBag.nowDate = $"Now Last Update {nowDate}";
                string avgStartDate = lastUpdate.avgStartDate.ToString("yyyy-MM-dd");
                string avgEndDate = lastUpdate.avgEndDate.ToString("yyyy-MM-dd");
                ViewBag.avgDate = $"{avgStartDate} ~ {avgEndDate}";
            }
            return View();
        }

        [HttpPost]
        public async Task<string> downloadStockData()
        {
            DownloadModel downloadModel = new DownloadModel();
            SysModel sysModel = new SysModel();

            DateTime date = sysModel.getsysConfigstockUpdate();
            string totalmsg = "";
            while (date <= DateTime.Now)
            {
                string datetime = date.ToString("yyyy-MM-dd");
                string msg = await downloadModel.DownloadStock(datetime);
                totalmsg += msg + "<br/>";
                DateTime nextdate = date.AddDays(1);
                int count = sysModel.updatesysConfigstockUpdate(nextdate);
                date = nextdate;
                Thread.Sleep(5000);
            }
            return totalmsg;

        }

        [HttpPost]
        public async Task<string> downloadOtcData()
        {
            DownloadModel downloadModel = new DownloadModel();
            SysModel sysModel = new SysModel();

            DateTime date = sysModel.getsysConfigotcUpdate();
            string totalmsg = "";
            while (date <= DateTime.Now)
            {
                DateTime taiwandatetime = date.AddYears(-1911);
                string datetime = taiwandatetime.ToString("yyyy/MM/dd");
                string msg = await downloadModel.DownloadOtc(datetime);
                totalmsg += msg + "<br/>";
                DateTime nextdate = date.AddDays(1);
                int count = sysModel.updatesysConfigotcUpdate(nextdate);
                date = nextdate;
                Thread.Sleep(5000);
            }
            return totalmsg;

        }

        CommonModel commonModel = new CommonModel();

        [HttpPost]
        public ActionResult computeStockAvg()
        {
            commonModel.stockAvgStatistics();
            return RedirectToAction("Index", "GetStockData");
        }

        [HttpPost]
        public ActionResult computeStockNow()
        {
            commonModel.stockNowsStatistics();
            return RedirectToAction("Index", "GetStockData");
        }

        public ActionResult sysConfig()
        {
            using (var db = new WebStockEntities())
            {
                var sysconfig = db.sysConfig.FirstOrDefault();
                return View(sysconfig);
            }

        }

        public string sysConfigAjax(sysConfig sys)
        {
            string sql = @"UPDATE sysConfig
                           SET
                           stockUpdate = @stockUpdate, 
                           otcUpdate = @otcUpdate, 
                           nowDate = @nowDate,  
                           avgStartDate = @avgStartDate,  
                           avgEndDate = @avgEndDate 
                           WHERE id = @id;";
            using (var db = new WebStockEntities())
            {
                if (sys != null)
                {
                   int res = db.Database.ExecuteSqlCommand(sql,
                   new SqlParameter("@stockUpdate", sys.stockUpdate),
                   new SqlParameter("@otcUpdate", sys.otcUpdate),
                   new SqlParameter("@nowDate", sys.nowDate),
                   new SqlParameter("@avgStartDate", sys.avgStartDate),
                   new SqlParameter("@avgEndDate", sys.avgEndDate),
                   new SqlParameter("@id", sys.id)
                );
                    db.SaveChanges();
                    return "upDate success !";
                }
                else
                    return "upDate error !";
            }

        }
    }
}