using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebStock.Models;
using WebStock.ViewModels;
using static WebStock.Models.CommonModel;
using static WebStock.ViewModels.ReportViewModel;

namespace WebStock.Controllers
{
    public class GetStockDataController : BaseController
    {
        ReportModel ReportModel = new ReportModel();
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
            string msg = commonModel.sysConfigUpdate(sys);
            return msg;
        }

        public ActionResult getSingleStock()
        {
            //List<getSingleStock> getSingleStocks = new List<getSingleStock>();
            //getSingleStockViewModel stockViewModel = new getSingleStockViewModel();
            //stockViewModel.getSingleStock = getSingleStocks;

            //return View(stockViewModel);

            StockDataView StockDataView = new StockDataView();
            FormSearch f = new FormSearch();
            f.dataDate = DateTime.UtcNow.AddHours(8).AddMonths(-1);
            StockDataView.formSearch = f;
            StockDataView.data = new List<RSD>();
            return View(StockDataView);
        }

        [HttpPost]
        public ActionResult getSingleStock(StockDataView d)
        {
            //List<getSingleStock> getSingleStocks = commonModel.getSingleStockData(data);
            //getSingleStockViewModel stockViewModel = new getSingleStockViewModel();
            //stockViewModel.getSingleStock = getSingleStocks;
            //return View(stockViewModel);

            //預設查詢條件
            FormSearch f = new FormSearch();
            f = d.formSearch;
            f.options = new Options();
            f.options.sortBy = new string[] { "dataDate" };
            f.options.sortDesc = new bool[] { true };
            f.options.page = 1;
            f.options.itemsPerPage = 9999;

            StockDataView StockDataView = new StockDataView();
            StockDataView.data = ReportModel.ReadStockData(f);
            return View(StockDataView);
        }
    }
}