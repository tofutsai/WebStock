using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using JWT.Serializers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using WebStock.ViewModels;
using static WebStock.ViewModels.ReportViewModel;

namespace WebStock.Models
{
    public class CommonModel
    {
        public class UserInfo
        {
            public string JWToken { get; set; }
            public int OperId { get; set; }
            public string OperAccount { get; set; }
            public string OperName { get; set; }
            public string OperRole { get; set; }
            public bool OperIsAdmin { get; set; }
        }

        //public static string TokenSecretKey = ConfigurationManager.AppSettings["TokenSecretKey"].ToString();
        public static string TokenSecretKey = "HELLOKITTY";
        public static string EncodeJWTToken(object payload)
        {

            var secret = TokenSecretKey;

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(payload, secret);
            //Console.WriteLine(token);

            return token;
        }

        public static UserInfo DecodeJWTToken(string jwtToken)
        {

            try
            {
                IJsonSerializer serializer = new JsonNetSerializer();
                IDateTimeProvider provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);
                UserInfo dd = decoder.DecodeToObject<UserInfo>(jwtToken, TokenSecretKey, true);

                return dd;
            }
            catch (TokenExpiredException)
            {
                Console.WriteLine("Token has expired");
                return null;
            }
            catch (SignatureVerificationException)
            {
                Console.WriteLine("Token has invalid signature");
                return null;
            }

        }

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

        public class stockStatistics : stockAvg
        {
            public string dateYear { get; set; }
        }

        public class stockNowStatistics : stockNow
        {
            public double highestPrice { get; set; }
            public double lowestPrice { get; set; }
        }

        public class stockSummaryStatistics : stockAvg
        {
            public string type { get; set; }
            public string category { get; set; }
            public string company { get; set; }
            public double position { get; set; }
            public double closePrice { get; set; }
            public DateTime dataDate { get; set; }
            public string memo { get; set; }
        }
               
        public string GetJWTToken(object payload)
        {
            string jsonString = JsonConvert.SerializeObject(payload);
            return jsonString;

        }
        internal UserInfo DecodeJWTTokenMVC(string jwtToken)
        {
            UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(jwtToken);
            return userInfo;
        }

        internal List<stockAvg> stockAvgStatistics()
        {
            List<stockAvg> stockAvgs = new List<stockAvg>();
            List<stockStatistics> stockStatistics = new List<stockStatistics>();
            using (var db = new WebStockEntities())
            {
                var sys = db.sysConfig.FirstOrDefault();
                string truncatesql = "truncate table stockAvg";
                db.Database.ExecuteSqlCommand(truncatesql);
                db.SaveChanges();
                string sql = $"select s.code,year(dataDate) as dataYear, round(avg(closeprice),2) as avgPrice, MAX(closeprice) as highestPrice, MIN(closeprice) as lowestPrice, round(avg(shares), 2) as avgShares, round(avg(turnover), 2) as avgTurnover " +
                             $"from stockData s where dataDate between '{sys.avgStartDate.ToShortDateString()}' and '{sys.avgEndDate.ToShortDateString()}' group by year(dataDate), s.code order by s.code, dataYear";
                stockStatistics = db.Database.SqlQuery<stockStatistics>(sql).ToList();

                foreach (var item in db.stockIndex.ToList())
                {
                    double fiveYearsAvgPrice = 0;
                    double fiveYearsAvghighestPrice = 0;
                    double fiveYearsAvglowestPrice = 0;
                    int fiveYearsAvgShares = 0;
                    double fiveYearsAvgTurnOver = 0;
                    var singleCodeStock = stockStatistics.Where(x => x.code == item.code);
                    if (singleCodeStock.Count() == 0)
                        continue;
                    foreach (var data in singleCodeStock)
                    {
                        fiveYearsAvgPrice += data.avgPrice;
                        fiveYearsAvghighestPrice += data.highestPrice;
                        fiveYearsAvglowestPrice += data.lowestPrice;
                        fiveYearsAvgShares += data.avgShares;
                        fiveYearsAvgTurnOver += data.avgTurnover;
                    }
                    fiveYearsAvgPrice = Math.Round(fiveYearsAvgPrice / singleCodeStock.Count(), 2);
                    fiveYearsAvghighestPrice = Math.Round(fiveYearsAvghighestPrice / singleCodeStock.Count(), 2);
                    fiveYearsAvglowestPrice = Math.Round(fiveYearsAvglowestPrice / singleCodeStock.Count(), 2);
                    fiveYearsAvgShares = fiveYearsAvgShares / singleCodeStock.Count();
                    fiveYearsAvgTurnOver = Math.Round(fiveYearsAvgTurnOver / singleCodeStock.Count(), 2);
                    stockAvg stockAvg = new stockAvg();
                    stockAvg.code = item.code;
                    stockAvg.avgPrice = fiveYearsAvgPrice;
                    stockAvg.highestPrice = fiveYearsAvghighestPrice;
                    stockAvg.lowestPrice = fiveYearsAvglowestPrice;
                    stockAvg.avgShares = fiveYearsAvgShares;
                    stockAvg.avgTurnover = fiveYearsAvgTurnOver;

                    stockAvgs.Add(stockAvg);
                    db.stockAvg.Add(stockAvg);
                    db.SaveChanges();
                }
            }
            return stockAvgs;
        }

        internal List<stockNow> stockNowsStatistics()
        {
            List<stockNow> stockNows = new List<stockNow>();
            List<stockNowStatistics> stockStatisticsNows = new List<stockNowStatistics>();
            using (var db = new WebStockEntities())
            {
                string truncatesql = "truncate table stockNow";
                db.Database.ExecuteSqlCommand(truncatesql);
                db.SaveChanges();
                string sql = @"SELECT
                              	 i.code AS code
                                 ,dt.dataDate AS dataDate
                                 ,dt.closePrice AS closePrice
                                 ,a.highestPrice AS highestPrice
                                 ,a.lowestPrice AS lowestPrice
                              FROM stockIndex i
                              JOIN stockDataTmp dt
                              	ON i.code = dt.code
                              JOIN stockAvg a
                              	ON i.code = a.code
                              UNION
                              SELECT
                              	 i.code AS code
                                 ,dto.dataDate AS dataDate
                                 ,dto.closePrice AS closePrice
                                 ,a.highestPrice AS highestPrice
                                 ,a.lowestPrice AS lowestPrice
                              FROM stockIndex i
                              JOIN stockDataTmpOtc dto
                              	ON i.code = dto.code
                              JOIN stockAvg a
                              	ON i.code = a.code
                              ORDER BY i.code";

                stockStatisticsNows = db.Database.SqlQuery<stockNowStatistics>(sql).ToList();

                foreach (var item in db.stockIndex.ToList())
                {
                    var stockStatisticsNow = stockStatisticsNows.Where(x => x.code == item.code);
                    if (stockStatisticsNow.Count() == 0)
                        continue;
                    foreach (var data in stockStatisticsNow)
                    {
                        stockNow stockNow = new stockNow();
                        stockNow.code = data.code;
                        stockNow.dataDate = data.dataDate;
                        stockNow.closePrice = data.closePrice;
                        stockNow.position = Math.Round((data.closePrice - data.lowestPrice) / (data.highestPrice - data.lowestPrice), 2);
                        stockNows.Add(stockNow);
                        db.stockNow.Add(stockNow);
                        db.SaveChanges();
                    }
                }
                var sysConfig = db.sysConfig.FirstOrDefault();
                var stocknow = db.stockNow.Where(x => x.id == 1).FirstOrDefault();
                sysConfig.nowDate = stocknow.dataDate;
                db.SaveChanges();
            }
            return stockNows;
        }
               
        internal string sysConfigUpdate(sysConfig sys)
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