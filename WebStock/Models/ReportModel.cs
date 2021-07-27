using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using static WebStock.ViewModels.ReportViewModel;
using static WebStock.Models.CommonModel;
using AutoMapper;

namespace WebStock.Models
{
    public class ReportModel
    {
        internal List<RSI> ReadStockIndex(FormSearch form)
        {
            using (var db = new WebStockEntities())
            {
                string sqlStr = @"
                                SELECT
                                count(1) over() as totalCount,
                                a.id,			
                                a.type,		
                                a.category,	
                                a.code,		
                                a.company,	
                                a.dataDate,
                                a.isEnable
                                FROM stockIndex a
                                WHERE
                                {0} 
                                {1} {2} 
                                {3}
                                ";

                string whereStr = "";
                if (string.IsNullOrEmpty(form.code))
                {
                    whereStr = @"
                                a.type LIKE '%'+ @type +'%'
                                ";
                }
                else
                {
                    whereStr = @"
                                a.type LIKE '%'+ @type +'%'
                                AND
                                (
                                a.code LIKE @code
                                OR
                                a.company LIKE @code
                                )
                                ";
                }

                string sortStr = "ORDER BY ";
                string sortDESC = (form.options.sortDesc != null && form.options.sortDesc[0]) ? "DESC" : "ASC";
                string pageStr = (form.options.page != 0 && form.options.itemsPerPage != 0) ? "OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY" : "";


                string sortByType = (form.options.sortBy != null && form.options.sortBy[0] != "") ? form.options.sortBy[0] : "";
                switch (sortByType)
                {
                    case "id": sortStr += "a.id"; break;
                    case "type": sortStr += "a.type"; break;
                    case "category": sortStr += "a.category"; break;
                    case "code": sortStr += "a.code"; break;
                    case "company": sortStr += "a.company"; break;
                    case "dataDate": sortStr += "a.dataDate"; break;
                    case "isEnable": sortStr += "a.isEnable"; break;
                    default: sortStr += "a.id"; break;
                }

                sqlStr = string.Format(sqlStr, whereStr, sortStr, sortDESC, pageStr);
                List<RSI> rs = db.Database.SqlQuery<RSI>(sqlStr,
                    new SqlParameter("@type", form.type),
                    new SqlParameter("@code", form.code ?? string.Empty),
                    new SqlParameter("@OFFSET", ((form.options.page - 1) * form.options.itemsPerPage)),
                    new SqlParameter("@FETCH", form.options.itemsPerPage)
                    ).ToList();
                return rs;
            }

        }


        internal bool CreateStockIndex(stockIndex stockIndex)
        {
            int status = 0;

            using (var db = new WebStockEntities())
            {
                db.stockIndex.Add(stockIndex);
                status = db.SaveChanges();
                return status > 0 ? true : false;
            }
        }

        internal bool UpdateStockIndex(stockIndex stockIndex)
        {

            int status = 0;
            using (var db = new WebStockEntities())
            {
                var d = db.stockIndex.Where(x => x.id == stockIndex.id).FirstOrDefault();
                Mapper.Initialize(cfg => cfg.CreateMap<stockIndex, stockIndex>()
                                                  .ForMember(x => x.id, opt => opt.Ignore())
                                                  );
                Mapper.Map(stockIndex, d);
                status = db.SaveChanges();
            }
            return status >= 0 ? true : false;
        }

        internal bool DeleteStockIndex(int id)
        {
            int status = 0;
            using (var db = new WebStockEntities())
            {
                var d = db.stockIndex.Where(x => x.id == id).FirstOrDefault();
                db.stockIndex.Remove(d);
                status = db.SaveChanges();
            }
            return status > 0 ? true : false;
        }
        internal List<RSD> ReadStockData(FormSearch form)
        {
            using (var db = new WebStockEntities())
            {

                string strSqlTmp = @"
                                    SELECT
                                    count(1) over() as totalCount, 
                                    a.id,			
                                    a.code,		
                                    b.company,
                                    a.dataDate,	
                                    a.shares,		
                                    a.turnover,	
                                    a.openPrice,
                                    a.highestPrice,
                                    a.lowestPrice,	
                                    a.closePrice
                                    FROM stockData a
                                    JOIN stockIndex b
                                    ON a.code = b.code
                                    WHERE
                                    (
                                    a.code LIKE @code
                                    OR
                                    b.company LIKE @code
                                    )
                                    AND
                                    (
                                    a.dataDate >= @dataDate
                                    )
                                    AND
                                    (
                                    b.isEnable = 'true'
                                    )
                                    {0} {1}
                                    {2}
                                    ";

                string sortStr = "ORDER BY ";
                string sortDESC = (form.options.sortDesc != null && form.options.sortDesc[0]) ? "DESC" : "ASC";
                string pageStr = (form.options.page != 0 && form.options.itemsPerPage != 0) ? "OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY" : "";

                string sortByType = (form.options.sortBy != null && form.options.sortBy[0] != "") ? form.options.sortBy[0] : "";
                switch (sortByType)
                {
                    case "id": sortStr += "a.id"; break;
                    case "code": sortStr += "a.code"; break;
                    case "company": sortStr += "b.company"; break;
                    case "dataDate": sortStr += "a.dataDate"; break;
                    case "shares": sortStr += "a.shares"; break;
                    case "turnover": sortStr += "a.turnover"; break;
                    case "openPrice": sortStr += "a.openPrice"; break;
                    case "highestPrice": sortStr += "a.highestPrice"; break;
                    case "lowestPrice": sortStr += "a.lowestPrice"; break;
                    case "closePrice": sortStr += "a.closePrice"; break;
                    default: sortStr += "a.dataDate"; break;
                }

                string strSql = string.Format(strSqlTmp, sortStr, sortDESC, pageStr);

                List<RSD> datas = db.Database.SqlQuery<RSD>(strSql,
                    new SqlParameter("@code", form.code ?? string.Empty),
                    new SqlParameter("@dataDate", form.dataDate),
                    new SqlParameter("@OFFSET", ((form.options.page - 1) * form.options.itemsPerPage)),
                    new SqlParameter("@FETCH", form.options.itemsPerPage)
                    ).ToList();

                if (datas.Count == 0)
                { return new List<RSD>(); }

                return datas;
            }
        }

        internal List<RSS> ReadStockStatistics(FormSearch form)
        {
            using (var db = new WebStockEntities())
            {
                string strSqlTmp = @"
                                    SELECT
                                    count(1) over() as totalCount, 
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

                if (string.IsNullOrEmpty(form.code))
                {
                    strSqlTmp += @" WHERE i.type LIKE '%'+ @type +'%' AND a.avgShares >= @shares AND n.position <= @position AND
                                    n.closePrice >= @closePrice {0} {1} {2} ";

                    string sortStr = "ORDER BY ";
                    string sortDESC = (form.options.sortDesc != null && form.options.sortDesc[0]) ? "DESC" : "ASC";
                    string pageStr = (form.options.page != 0 && form.options.itemsPerPage != 0) ? "OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY" : "";

                    string sortByType = (form.options.sortBy != null && form.options.sortBy[0] != "") ? form.options.sortBy[0] : "";
                    switch (sortByType)
                    {
                        case "type": sortStr += "i.type"; break;
                        case "code": sortStr += "i.code"; break;
                        case "company": sortStr += "i.company"; break;
                        case "position": sortStr += "n.position"; break;
                        case "category": sortStr += "i.category"; break;
                        case "avgShares": sortStr += "a.avgShares"; break;
                        case "avgTurnover": sortStr += "a.avgTurnover"; break;
                        case "highestPrice": sortStr += "a.highestPrice"; break;
                        case "lowestPrice": sortStr += "a.lowestPrice"; break;
                        case "closePrice": sortStr += "n.closePrice"; break;
                        default: sortStr += "i.dataDate"; break;
                    }

                    string strSql = string.Format(strSqlTmp, sortStr, sortDESC, pageStr);

                    List<RSS> datas = db.Database.SqlQuery<RSS>(strSql,
                        new SqlParameter("@type", form.type),
                        new SqlParameter("@shares", form.shares),
                        new SqlParameter("@position", form.position2),
                        new SqlParameter("@closePrice", form.closePrice),
                        new SqlParameter("@OFFSET", ((form.options.page - 1) * form.options.itemsPerPage)),
                        new SqlParameter("@FETCH", form.options.itemsPerPage)
                        ).ToList();

                    if (datas.Count == 0)
                    { return new List<RSS>(); }

                    return datas;
                }
                else
                {
                    strSqlTmp += @" WHERE ( a.code LIKE @code OR i.company LIKE @code )";
                    List<RSS> datas = db.Database.SqlQuery<RSS>(strSqlTmp,
                        new SqlParameter("@code", form.code)).ToList();

                    if (datas.Count == 0)
                    { return new List<RSS>(); }

                    return datas;

                }
            }

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

        internal List<RSF> ReadStockFavorite(FormSearch form)
        {
            List<RSF> favorites = new List<RSF>();
            using (var db = new WebStockEntities())
            {
                string sql = @"SELECT
                                count(1) over() as totalCount,
                                i.type
                               ,i.category
                               ,i.code 
                               ,i.company
                               ,f.id
                               ,n.closePrice 
                               ,n.position
                               ,ISNULL(
                               	m.ext1 + ',' +
                               	m.ext2 + ',' +
                               	m.ext3 + ',' +
                               	m.ext4 + ',' +
                               	m.ext5 + ',' +
                               	m.ext6 + ',' +
                               	m.ext7
                               	, '') AS memo
                               ,f.memo AS selfmemo
                               FROM stockIndex i
                               JOIN stockNow n
                               	ON i.code = n.code
                               JOIN stockFavorite f
                               	ON i.code = f.code
                               LEFT JOIN stockMemo m
                               ON f.code = m.code
                               WHERE f.operId = @operId
                               {0} {1} {2}";
                string sortStr = "ORDER BY ";
                string sortDESC = (form.options.sortDesc != null && form.options.sortDesc[0]) ? "DESC" : "ASC";
                string pageStr = (form.options.page != 0 && form.options.itemsPerPage != 0) ? "OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY" : "";
                string sortByType = (form.options.sortBy != null && form.options.sortBy[0] != "") ? form.options.sortBy[0] : "";
                switch (sortByType)
                {
                    case "type": sortStr += "i.type"; break;
                    case "code": sortStr += "i.code"; break;
                    case "company": sortStr += "i.company"; break;
                    case "position": sortStr += "n.position"; break;
                    case "category": sortStr += "i.category"; break;
                    case "closePrice": sortStr += "n.closePrice"; break;
                    default: sortStr += "i.code"; break;
                }
                string strsql = string.Format(sql, sortStr, sortDESC, pageStr);
                favorites = db.Database.SqlQuery<RSF>(strsql,
                            new SqlParameter("@operId", form.operId),
                            new SqlParameter("@OFFSET", ((form.options.page - 1) * form.options.itemsPerPage)),
                            new SqlParameter("@FETCH", form.options.itemsPerPage)).ToList();
            }
            return favorites;
        }


        internal string createFavoriteStock(string code, int operId)
        {
            int status = 0;

            using (var db = new WebStockEntities())
            {
                var check = db.stockIndex.Where(x => (x.code == code || x.company == code) && x.isEnable == true).FirstOrDefault();
                if (check != null)
                {
                    var r = db.stockFavorite.Where(x => x.operId == operId && x.code == check.code).FirstOrDefault();

                    if (r == null)
                    {
                        stockFavorite data = new stockFavorite();
                        data.operId = operId;
                        data.code = check.code;
                        db.stockFavorite.Add(data);
                        status = db.SaveChanges();
                        return status > 0 ? "01" : "02";
                    }
                    else
                    {
                        //已存在無須新增
                        return "03";
                    }
                }
                else
                {
                    return "02";
                }
            }
        }

        internal bool updateFavoriteStockMemo(stockFavorite favorite)
        {

            int status = 0;
            using (var db = new WebStockEntities())
            {
                var d = db.stockFavorite.Where(x => x.id == favorite.id).FirstOrDefault();
                Mapper.Initialize(cfg => cfg.CreateMap<stockFavorite, stockFavorite>()
                                                  .ForMember(x => x.id, opt => opt.Ignore())
                                                  .ForMember(x => x.operId, opt => opt.Ignore())
                                                  .ForMember(x => x.code, opt => opt.Ignore())
                                                  );
                Mapper.Map(favorite, d);
                status = db.SaveChanges();
            }
            return status >= 0 ? true : false;
        }

        internal bool deleteFavoriteStock(int id)
        {
            int status = 0;
            using (var db = new WebStockEntities())
            {
                var d = db.stockFavorite.Where(x => x.id == id).FirstOrDefault();
                db.stockFavorite.Remove(d);
                status = db.SaveChanges();
            }
            return status > 0 ? true : false;
        }

        internal List<RSP> ReadStockProfit(FormSearch form)
        {
            List<RSP> stockInventoryProfits = new List<RSP>();
            using (var db = new WebStockEntities())
            {
                string sql = @"SELECT
                                count(1) over() as totalCount,
                                p.id
                               ,i.code
                               ,i.company
                               ,n.position
                               ,n.closePrice
                               ,p.buyPrice
                               ,p.buyShares
                               ,p.buyCost
                               ,p.profit
                               ,p.profitPercentage
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
                            JOIN stockNow n
                            	ON i.code = n.code
                            JOIN stockProfit p
                            	ON i.code = p.code
                            LEFT JOIN stockMemo m
                               ON p.code = m.code
                            WHERE p.operId = @operId
                            {0} {1} {2}";
                string sortStr = "ORDER BY ";
                string sortDESC = (form.options.sortDesc != null && form.options.sortDesc[0]) ? "DESC" : "ASC";
                string pageStr = (form.options.page != 0 && form.options.itemsPerPage != 0) ? "OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY" : "";
                string sortByType = (form.options.sortBy != null && form.options.sortBy[0] != "") ? form.options.sortBy[0] : "";
                switch (sortByType)
                {
                    case "code": sortStr += "i.code"; break;
                    case "company": sortStr += "i.company"; break;
                    case "position": sortStr += "n.position"; break;
                    case "closePrice": sortStr += "n.closePrice"; break;
                    case "buyPrice": sortStr += "p.buyPrice"; break;
                    case "buyShares": sortStr += "p.buyShares"; break;
                    case "buyCost": sortStr += "p.buyCost"; break;
                    case "profit": sortStr += "p.profit"; break;
                    case "profitPercentage": sortStr += "p.profitPercentage"; break;
                    default: sortStr += "i.code"; break;
                }
                string strsql = string.Format(sql, sortStr, sortDESC, pageStr);
                stockInventoryProfits = db.Database.SqlQuery<RSP>(strsql,
                                           new SqlParameter("@operId", form.operId),
                                           new SqlParameter("@OFFSET", ((form.options.page - 1) * form.options.itemsPerPage)),
                                           new SqlParameter("@FETCH", form.options.itemsPerPage)).ToList();
                const double sellcommision = 0.004425;
                const int percentage = 100;
                foreach (var item in stockInventoryProfits)
                {
                    item.profit = ((item.closePrice * item.buyShares * (1 - sellcommision)) - item.buyCost);
                    item.profitPercentage = Math.Round(item.profit / item.buyCost * percentage, 2);
                }

            }
            return stockInventoryProfits;
        }

        internal bool createStockInventory(stockProfit profit)
        {
            bool status = false;
            try
            {
                using (var db = new WebStockEntities())
                {
                    stockIndex company = db.stockIndex.Where(x => x.company == profit.code).FirstOrDefault();
                    if (company != null)
                        profit.code = company.code;
                    const double buycommision = 1.001425;
                    const double sellcommision = 0.004425;
                    const int percentage = 100;
                    stockProfit stockProfit = new stockProfit();
                    stockNow stockNow = db.stockNow.Where(x => x.code == profit.code).FirstOrDefault();
                    if (stockNow == null)
                        return status;
                    stockProfit.operId = profit.operId;
                    stockProfit.code = profit.code;
                    stockProfit.buyPrice = profit.buyPrice;
                    stockProfit.buyShares = profit.buyShares;
                    stockProfit.buyCost = (profit.buyPrice * profit.buyShares * buycommision);
                    stockProfit.profit = (stockNow.closePrice * profit.buyShares * (1 - sellcommision) - (profit.buyPrice * profit.buyShares * buycommision));
                    stockProfit.profitPercentage = Math.Round((stockProfit.profit / stockProfit.buyCost) * percentage, 2);
                    db.stockProfit.Add(stockProfit);
                    db.SaveChanges();
                }
                status = true;
                return status;
            }
            catch (Exception ex)
            {
                return status;
            }
        }

        internal bool deleteStockInventory(int id)
        {
            int status = 0;
            using (var db = new WebStockEntities())
            {
                var d = db.stockProfit.Where(x => x.id == id).FirstOrDefault();
                db.stockProfit.Remove(d);
                status = db.SaveChanges();
            }
            return status > 0 ? true : false;
        }

        internal bool addStockMemo(string type, string codes, string memoContent)
        {
            bool status = false;
            try
            {
                string sql = "select * from stockMemo";
                string[] stringSeparators = new string[] { "\r\n", "\n" };
                string[] codeArray = codes.Split(stringSeparators, StringSplitOptions.None);
                using (var db = new WebStockEntities())
                {
                    List<stockMemo> memos = db.Database.SqlQuery<stockMemo>(sql).ToList();

                    //buckCopy Init
                    DataTable dt = new DataTable();
                    dt.Columns.Add("id", typeof(Int64));
                    dt.Columns.Add("code", typeof(string));
                    dt.Columns.Add("ext1", typeof(string));
                    dt.Columns.Add("ext2", typeof(string));
                    dt.Columns.Add("ext3", typeof(string));
                    dt.Columns.Add("ext4", typeof(string));
                    dt.Columns.Add("ext5", typeof(string));
                    dt.Columns.Add("ext6", typeof(string));
                    dt.Columns.Add("ext7", typeof(string));
                    dt.Columns.Add("ext8", typeof(string));
                    dt.Columns.Add("ext9", typeof(string));
                    dt.Columns.Add("ext10", typeof(string));
                    dt.Columns.Add("ext11", typeof(string));
                    dt.Columns.Add("ext12", typeof(string));
                    dt.Columns.Add("ext13", typeof(string));
                    dt.Columns.Add("ext14", typeof(string));
                    dt.Columns.Add("ext15", typeof(string));
                    dt.Columns.Add("ext16", typeof(string));
                    foreach (var item in db.stockIndex.ToList())
                    {
                        DataRow row = dt.NewRow();
                        row["code"] = item.code;
                        row["ext1"] = "";
                        row["ext2"] = "";
                        row["ext3"] = "";
                        row["ext4"] = "";
                        row["ext5"] = "";
                        row["ext6"] = "";
                        row["ext7"] = "";
                        row["ext8"] = "";
                        row["ext9"] = "";
                        row["ext10"] = "";
                        row["ext11"] = "";
                        row["ext12"] = "";
                        row["ext13"] = "";
                        row["ext14"] = "";
                        row["ext15"] = "";
                        row["ext16"] = "";
                        dt.Rows.Add(row);
                    }
                    db.Database.ExecuteSqlCommand(@"truncate table stockMemo");

                    foreach (DataRow dr in dt.Rows)
                    {
                        foreach (var item in memos)
                        {
                            if (dr["code"].ToString() == item.code)
                            {
                                dr["ext1"] = item.ext1;
                                dr["ext2"] = item.ext2;
                                dr["ext3"] = item.ext3;
                                dr["ext4"] = item.ext4;
                                dr["ext5"] = item.ext5;
                                dr["ext6"] = item.ext6;
                                dr["ext7"] = item.ext7;
                                dr["ext8"] = item.ext8;
                                dr["ext9"] = item.ext9;
                                dr["ext10"] = item.ext10;
                                dr["ext11"] = item.ext11;
                                dr["ext12"] = item.ext12;
                                dr["ext13"] = item.ext13;
                                dr["ext14"] = item.ext14;
                                dr["ext15"] = item.ext15;
                                dr["ext16"] = item.ext16;
                            }
                        }
                    }

                    foreach (var code in codeArray)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            if (dr["code"].ToString() == code)
                            {
                                switch (type)
                                {
                                    case "ext1":
                                        dr["ext1"] = memoContent; break;
                                    case "ext2":
                                        dr["ext2"] = memoContent; break;
                                    case "ext3":
                                        dr["ext3"] = memoContent; break;
                                    case "ext4":
                                        dr["ext4"] = memoContent; break;
                                    case "ext5":
                                        dr["ext5"] = memoContent; break;
                                    case "ext6":
                                        dr["ext6"] = memoContent; break;
                                    case "ext7":
                                        dr["ext7"] = memoContent; break;
                                    default: break;
                                }
                            }
                        }
                    }
                    //sqlBulkCopy 寫入資料Table
                    SqlConnection conn = (SqlConnection)db.Database.Connection;
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();

                    using (var sqlBulkCopy = new SqlBulkCopy((SqlConnection)db.Database.Connection))
                    {
                        sqlBulkCopy.DestinationTableName = "dbo.stockMemo";
                        sqlBulkCopy.WriteToServer(dt);
                    }

                }
                status = true;
                return status;
            }
            catch (Exception ex)
            {
                return status;
            }

        }

        internal bool stockAvgStatistics()
        {
            bool status = false;
            try
            {
                List<stockStatistics> stockStatistics = new List<stockStatistics>();
                using (var db = new WebStockEntities())
                {
                    //buckCopy Init
                    DataTable dt = new DataTable();
                    dt.Columns.Add("id", typeof(Int64));
                    dt.Columns.Add("code", typeof(string));
                    dt.Columns.Add("avgPrice", typeof(double));
                    dt.Columns.Add("highestPrice", typeof(double));
                    dt.Columns.Add("lowestPrice", typeof(double));
                    dt.Columns.Add("avgShares", typeof(Int64));
                    dt.Columns.Add("avgTurnover", typeof(double));

                    var sys = db.sysConfig.FirstOrDefault();

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
                        var singleCodeStock = stockStatistics.Where(x => x.code == item.code).AsEnumerable();
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

                        //BuckCopy寫入結果
                        DataRow row = dt.NewRow();
                        row["code"] = item.code;
                        row["avgPrice"] = fiveYearsAvgPrice;
                        row["highestPrice"] = fiveYearsAvghighestPrice;
                        row["lowestPrice"] = fiveYearsAvglowestPrice;
                        row["avgShares"] = fiveYearsAvgShares;
                        row["avgTurnover"] = fiveYearsAvgTurnOver;
                        dt.Rows.Add(row);
                    }
                    //清空資料Table
                    db.Database.ExecuteSqlCommand(@"truncate table stockAvg");

                    //sqlBulkCopy 寫入資料Table
                    SqlConnection conn = (SqlConnection)db.Database.Connection;
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();

                    using (var sqlBulkCopy = new SqlBulkCopy((SqlConnection)db.Database.Connection))
                    {
                        sqlBulkCopy.DestinationTableName = "dbo.stockAvg";
                        sqlBulkCopy.WriteToServer(dt);
                        status = true;
                    }
                }
                return status;
            }
            catch (Exception ex)
            {
                return status;
            }

        }

        internal bool stockNowsStatistics()
        {
            bool status = false;
            try
            {
                List<stockNowStatistics> stockStatisticsNows = new List<stockNowStatistics>();
                using (var db = new WebStockEntities())
                {

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

                    //buckCopy Init
                    DataTable dt = new DataTable();
                    dt.Columns.Add("id", typeof(Int64));
                    dt.Columns.Add("code", typeof(string));
                    dt.Columns.Add("closePrice", typeof(double));
                    dt.Columns.Add("position", typeof(double));
                    dt.Columns.Add("dataDate", typeof(DateTime));


                    foreach (var item in db.stockIndex.ToList())
                    {
                        var stockStatisticsNow = stockStatisticsNows.Where(x => x.code == item.code).AsEnumerable();
                        if (stockStatisticsNow.Count() == 0)
                            continue;
                        foreach (var data in stockStatisticsNow)
                        {
                            //BuckCopy寫入結果
                            DataRow row = dt.NewRow();
                            row["code"] = data.code;
                            row["closePrice"] = data.closePrice;
                            row["position"] = Math.Round((data.closePrice - data.lowestPrice) / (data.highestPrice - data.lowestPrice), 2);
                            row["dataDate"] = data.dataDate;
                            dt.Rows.Add(row);
                        }
                    }
                    //再foreach尚未更新前的stockNow list ，找出code不存在本次list，補充寫入上去dt
                    foreach (var item in db.stockNow.ToList())
                    {
                        var stockStatisticsNow = stockStatisticsNows.Where(x => x.code == item.code).AsEnumerable();
                        if (stockStatisticsNow.Count() == 0)
                        {
                            DataRow row = dt.NewRow();
                            row["code"] = item.code;
                            row["closePrice"] = item.closePrice;
                            row["position"] = item.position;
                            row["dataDate"] = item.dataDate;
                            dt.Rows.Add(row);
                        }
                    }

                    db.Database.ExecuteSqlCommand(@"truncate table stockNow");

                    //sqlBulkCopy 寫入資料Table
                    SqlConnection conn = (SqlConnection)db.Database.Connection;
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();

                    using (var sqlBulkCopy = new SqlBulkCopy((SqlConnection)db.Database.Connection))
                    {
                        sqlBulkCopy.DestinationTableName = "dbo.stockNow";
                        sqlBulkCopy.WriteToServer(dt);
                    }

                    var sysConfig = db.sysConfig.FirstOrDefault();
                    var stocknow = db.stockNow.Where(x => x.id == 1).FirstOrDefault();
                    sysConfig.nowDate = stocknow.dataDate;
                    db.SaveChanges();
                    status = true;
                }
                return status;
            }
            catch (Exception ex)
            {
                return status;
            }

        }
        internal List<RSC> ReadsysConfig()
        {
            List<RSC> data = new List<RSC>();
            using (var db = new WebStockEntities())
            {
                string sql = @"SELECT
                               COUNT(1) OVER () AS totalCount
                               ,*
                               FROM sysConfig";
                data = db.Database.SqlQuery<RSC>(sql).ToList();
            }
            return data;
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

        internal bool EditSysConfig(sysConfig sys)
        {
            using (var db = new WebStockEntities())
            {
                sysConfig d = db.sysConfig.Where(x => x.id == 1).FirstOrDefault();
                Mapper.Initialize(cfg => cfg.CreateMap<sysConfig, sysConfig>()
                                                     .ForMember(x => x.id, opt => opt.Ignore())
                                                     );
                Mapper.Map(sys, d);
                int r = db.SaveChanges();
                return r >= 0 ? true : false;
            }
        }

        internal List<RSL> ReadsysLog(FormSearch form)
        {
            List<RSL> data = new List<RSL>();
            string datefrom = form.dataDate.ToString("yyyy-MM-dd");
            string datenow = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");
            using (var db = new WebStockEntities())
            {
                string sql = @"SELECT
                               COUNT(1) OVER () AS totalCount
                               ,*
                               FROM sysLog AS s
                               WHERE s.date BETWEEN @datefrom AND @datenow
                               ORDER BY s.id DESC {0}"
                               ;
                string pageStr = (form.options.page != 0 && form.options.itemsPerPage != 0) ? "OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY" : "";
                string strsql = string.Format(sql, pageStr);
                data = db.Database.SqlQuery<RSL>(strsql,
                    new SqlParameter("@datefrom", datefrom),
                    new SqlParameter("@datenow", datenow),
                    new SqlParameter("@OFFSET", ((form.options.page - 1) * form.options.itemsPerPage)),
                    new SqlParameter("@FETCH", form.options.itemsPerPage)
                    ).ToList();
            }

            return data;
        }
    }
}