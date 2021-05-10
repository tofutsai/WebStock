using System;
using System.Collections.Generic;
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
                totalmsg += msg+"<br/>";
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
    }
}