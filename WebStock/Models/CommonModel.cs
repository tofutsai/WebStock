﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WebStock.ViewModels;

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

        public class stockSelfFavorite : stockSummaryStatistics
        {
            
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

        internal List<stockSummaryStatistics> summaryStatistics()
        {
            List<stockSummaryStatistics> stockSummaryStatistics = new List<stockSummaryStatistics>();
            using (var db = new WebStockEntities())
            {
                string sql = @"SELECT
                          	  i.type
                             ,i.category
                             ,i.code 
                             ,i.company 
                             ,a.avgShares 
                             ,a.avgTurnover 
                             ,a.highestPrice 
                             ,a.lowestPrice 
                             ,n.closePrice 
                             ,n.position
                             ,n.dataDate
                             ,ISNULL(
	                          m.ext1 + ',' +
	                          m.ext2 + ',' +
	                          m.ext3 + ',' +
	                          m.ext4 + ',' +
	                          m.ext5 + ',' +
	                          m.ext6 + ',' +
	                          m.ext7
	                          , '') AS memo
                          FROM stockIndex i
                          JOIN stockAvg a
                          	ON i.code = a.code
                          JOIN stockNow n
                          	ON i.code = n.code
                          LEFT JOIN stockMemo m
	                        ON m.code = i.code
                          ORDER BY i.category";
                stockSummaryStatistics = db.Database.SqlQuery<stockSummaryStatistics>(sql).ToList();

            }
            return stockSummaryStatistics;
        }

        internal List<stockSummaryStatistics> summaryStatistics(stockStatisticsViewModel viewModel)
        {
            List<stockSummaryStatistics> stockSummaryStatistics = new List<stockSummaryStatistics>();
            using (var db = new WebStockEntities())
            {
                string sql = @"SELECT
                          	  i.type
                             ,i.category
                             ,i.code 
                             ,i.company 
                             ,a.avgShares 
                             ,a.avgTurnover 
                             ,a.highestPrice 
                             ,a.lowestPrice 
                             ,n.closePrice 
                             ,n.position
                             ,n.dataDate
                             ,ISNULL(
	                          m.ext1 + ',' +
	                          m.ext2 + ',' +
	                          m.ext3 + ',' +
	                          m.ext4 + ',' +
	                          m.ext5 + ',' +
	                          m.ext6 + ',' +
	                          m.ext7
	                          , '') AS memo
                          FROM stockIndex i
                          JOIN stockAvg a
                          	ON i.code = a.code
                          JOIN stockNow n
                          	ON i.code = n.code
                          LEFT JOIN stockMemo m
	                        ON m.code = i.code";
                if (string.IsNullOrEmpty(viewModel.searchCode))
                {
                    string strsql = sql + @" WHERE i.type LIKE '%'+ @type +'%' AND a.avgShares >= @shares AND n.position <= @position AND n.closePrice >= @closePrice " +
                        string.Format("ORDER BY {0} {1}", viewModel.searchSort, viewModel.searchAorD);
                    stockSummaryStatistics = db.Database.SqlQuery<stockSummaryStatistics>(strsql,
                                                new SqlParameter("@type", viewModel.searchType),
                                                new SqlParameter("@shares", viewModel.searchShares),
                                                new SqlParameter("@position", viewModel.stockNowStatistics.position),
                                                new SqlParameter("@closePrice", viewModel.searchClosePrice)).ToList();
                }
                else
                {
                    string strsql = sql + " WHERE i.code = @code";
                    stockSummaryStatistics = db.Database.SqlQuery<stockSummaryStatistics>(strsql,
                                                new SqlParameter("@code", viewModel.searchCode)).ToList();
                }
            }
            return stockSummaryStatistics;
        }

        internal string addFavorite(string code, int OperId)
        {
            using (var db = new WebStockEntities())
            {
                stockFavorite favorite = new stockFavorite();
                favorite.operId = OperId;
                favorite.code = code;
                db.stockFavorite.Add(favorite);
                db.SaveChanges();

                return "success";
            }
        }
        internal List<stockSelfFavorite> getStockFavorite(int operId)
        {
            List<stockSelfFavorite> favorites = new List<stockSelfFavorite>();
            using (var db = new WebStockEntities())
            {
                string sql = @"SELECT
                               i.type
                              ,i.category
                              ,i.code 
                              ,i.company
                              ,n.closePrice 
                              ,n.position
                              FROM stockIndex i
                              JOIN stockNow n
                              	ON i.code = n.code
                              JOIN stockFavorite f
                              	ON i.code = f.code
                              WHERE f.operId = @operId";
                favorites = db.Database.SqlQuery<stockSelfFavorite>(sql,
                            new SqlParameter("@operId", operId)).ToList();
            }
            return favorites;
        }

        internal string createFavoriteStock(string code, int operId)
        {
            using (var db = new WebStockEntities())
            {
                stockFavorite stockFavorite = db.stockFavorite.Where(x => x.code == code && x.operId == operId).FirstOrDefault();
                if (stockFavorite != null)
                    return " 自選已存在無須新增!";
                else
                {
                    string sql = @"INSERT INTO stockFavorite(operId, code)
                               VALUES (@operId, @code)";
                    db.Database.ExecuteSqlCommand(sql,
                        new SqlParameter("@operId", operId),
                        new SqlParameter("@code", code));
                }
                
            }
            return " 新增自選成功";
        }

        internal string deleteFavoriteStock(string code, int operId)
        {
            using (var db = new WebStockEntities())
            {
                string sql = @"DELETE FROM stockFavorite
                           WHERE operId = @operId AND code = @code";
                db.Database.ExecuteSqlCommand(sql,
                    new SqlParameter("@operId", operId),
                    new SqlParameter("@code", code));
            }
            return "刪除成功 : " + code;
        }


        internal string updateStockMemo(stockFavorite favorite, int operId)
        {
            using(var db = new WebStockEntities())
            {
                var stockFavorite = db.stockFavorite.Where(x => x.operId == operId && x.code == favorite.code).FirstOrDefault();
                if (stockFavorite != null)
                {
                    stockFavorite.memo = favorite.memo;
                    db.SaveChanges();
                    return " 自行備註編輯成功!";
                }
                else
                    return " 自行備註編輯失敗";
                    
            }
            throw new NotImplementedException();
        }

        internal string addStockMemo(stockMemoViewModel data)
        {
            if (string.IsNullOrEmpty(data.memoContent))
                return "新增失敗，請輸入memo內容";
            string sqlCreate = @"INSERT INTO stockMemo ( code, ext1, ext2, ext3, ext4, ext5, 
                                 ext6, ext7, ext8, ext9, ext10, ext11, ext12, ext13, ext14, ext15, ext16) VALUES (
                                 @code, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '')";

            string[] stringSeparators = new string[] { "\r\n" };
            string[] codeArray = data.stockMemo.code.Split(stringSeparators, StringSplitOptions.None);
            using (var db = new WebStockEntities())
            {
                foreach (var code in codeArray)
                {
                    stockMemo stockMemo = db.stockMemo.Where(x => x.code == code).FirstOrDefault();
                    if (stockMemo != null)
                    {
                        switch (data.type)
                        {
                            case "ext1":
                                stockMemo.ext1 = data.memoContent; break;
                            case "ext2":
                                stockMemo.ext2 = data.memoContent; break;
                            case "ext3":
                                stockMemo.ext3 = data.memoContent; break;
                            case "ext4":
                                stockMemo.ext4 = data.memoContent; break;
                            case "ext5":
                                stockMemo.ext5 = data.memoContent; break;
                            case "ext6":
                                stockMemo.ext6 = data.memoContent; break;
                            case "ext7":
                                stockMemo.ext7 = data.memoContent; break;
                            default: break;
                        }
                        db.SaveChanges();
                    }
                    else
                    {
                        int result = db.Database.ExecuteSqlCommand(sqlCreate,
                            new SqlParameter("@code", code));
                        stockMemo memo = db.stockMemo.Where(x => x.code == code).FirstOrDefault();
                        switch (data.type)
                        {
                            case "ext1":
                                memo.ext1 = data.memoContent; break;
                            case "ext2":
                                memo.ext2 = data.memoContent; break;
                            case "ext3":
                                memo.ext3 = data.memoContent; break;
                            case "ext4":
                                memo.ext4 = data.memoContent; break;
                            case "ext5":
                                memo.ext5 = data.memoContent; break;
                            case "ext6":
                                memo.ext6 = data.memoContent; break;
                            case "ext7":
                                memo.ext7 = data.memoContent; break;
                            default: break;
                        }
                        db.SaveChanges();
                    }

                }
            }
            return "新增成功";
        }
    }
}