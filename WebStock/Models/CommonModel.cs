using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebStock.Models
{
    public class CommonModel
    {
        public class stockDataAPI
        {
            public string stat { get; set; }
            public string date { get; set; }
            public string title { get; set; }
            public List<string> fields9 { get; set; }
            public List<List<string>> data8 { get; set; }
            public List<List<string>> data9 { get; set; }
            public List<string> notes { get; set; }
        }


        public class OtcDataAPI
        {
            public string reportDate { get; set; }
            public string reportTitle { get; set; }
            public string iTotalRecords { get; set; }
            public string iTotalDisplayRecords { get; set; }
            public List<List<string>> aaData { get; set; }
        }

        public class UserInfo
        {
            public int OperId { get; set; }
            public string OperAccount { get; set; }
            public string OperName { get; set; }
            public string OperRole { get; set; }
        }

        public class stockStatistics : stockAvg
        {
            public string dateYear { get; set; }
        }  

        public string GetJWTToken(object payload)
        {
            string jsonString = JsonConvert.SerializeObject(payload);
            return jsonString;

        }
        internal UserInfo DecodeJWTToken(string jwtToken)
        {
            UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(jwtToken);
            return userInfo;
        }

        internal List<stockAvg> stockAvgStatistics()
        {
            
            List<stockAvg> stockAvgs = new List<stockAvg>();
            List<stockStatistics> stockStatistics = new List<stockStatistics>();
            using(var db = new WebStockEntities())
            {
                string sql = @"select s.code,year(dataDate) as dataYear, round(avg(closeprice),2) as avgPrice, MAX(closeprice) as highestPrice, MIN(closeprice) as lowestPrice, round(avg(shares), 2) as avgShares, round(avg(turnover), 2) as avgTurnover 
                             from stockData s where dataDate between '2016-01-01' and '2020-12-31' group by year(dataDate), s.code order by s.code, dataYear";
                stockStatistics = db.Database.SqlQuery<stockStatistics>(sql).ToList();
                foreach (var item in stockStatistics)
                {
                    stockAvg stockAvg = new stockAvg();
                    stockAvg.code = item.code;
                    stockAvg.avgPrice = item.avgPrice;
                    stockAvg.highestPrice = item.highestPrice;
                    stockAvg.lowestPrice = item.lowestPrice;
                    stockAvg.avgShares = item.avgShares;
                    stockAvg.avgTurnover = item.avgTurnover;
                    stockAvgs.Add(stockAvg);
                }

            }

            return stockAvgs;
        }
    }
}