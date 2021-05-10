using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebStock.Models
{
    public class SysModel
    {
        public sysLog createLog(string type, string message)
        {
            using(var db = new WebStockEntities())
            {
                sysLog log = new sysLog();
                log.date = DateTime.Now;
                log.type = type;
                log.message = message;
                db.sysLog.Add(log);
                db.SaveChanges();
                return log;
            }
            
        }

        public DateTime getsysConfigstockUpdate()
        {
            using(var db = new WebStockEntities())
            {
                DateTime stockdate = db.sysConfig.Select(m => m.stockUpdate).FirstOrDefault();
                return stockdate;
            }
        }
        public DateTime getsysConfigotcUpdate()
        {
            using (var db = new WebStockEntities())
            {
                DateTime otcdate = db.sysConfig.Select(m => m.otcUpdate).FirstOrDefault();
                return otcdate;
            }
        }

        public int updatesysConfigstockUpdate(DateTime nextdate)
        {
            using (var db = new WebStockEntities())
            {
                var res = db.sysConfig.Where(x => x.id == 1).FirstOrDefault();
                res.stockUpdate = nextdate;
                
                return db.SaveChanges();
            }
        }
        public int updatesysConfigotcUpdate(DateTime nextdate)
        {

            using (var db = new WebStockEntities())
            {
                var res = db.sysConfig.Where(x => x.id == 1).FirstOrDefault();
                res.otcUpdate = nextdate;

                return db.SaveChanges();
            }
        }
    }
}