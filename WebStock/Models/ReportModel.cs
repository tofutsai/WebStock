using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using static WebStock.ViewModels.ReportViewModel;

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
                                a.code LIKE @code
                                OR
                                a.company LIKE @code
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

        internal List<RSF> ReadStockFavorite(FormSearch form, int operId)
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
                               ORDER BY i.code
                               {0}";
                string pageStr = (form.options.page != 0 && form.options.itemsPerPage != 0) ? "OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY" : "";
                string strsql = string.Format(sql, pageStr);
                favorites = db.Database.SqlQuery<RSF>(strsql,
                            new SqlParameter("@operId", operId),
                            new SqlParameter("@OFFSET", ((form.options.page - 1) * form.options.itemsPerPage)),
                            new SqlParameter("@FETCH", form.options.itemsPerPage)).ToList();
            }
            return favorites;
        }

        internal string createFavoriteStock(string code, int operId)
        {
            using (var db = new WebStockEntities())
            {
                stockIndex company = db.stockIndex.Where(x => x.company == code || x.code == code).FirstOrDefault();
                if (company != null)
                    code = company.code;
                else
                    return " 新增自選失敗，請輸入正確股票代碼/名稱";
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

        internal string updateStockMemo(stockFavorite favorite, int operId)
        {
            using (var db = new WebStockEntities())
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

        internal List<RSP> ReadStockProfit(FormSearch form, int operId)
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
                            FROM stockIndex i
                            JOIN stockNow n
                            	ON i.code = n.code
                            JOIN stockProfit p
                            	ON i.code = p.code
                            WHERE p.operId = @operId
                            ORDER BY i.code";
                string pageStr = (form.options.page != 0 && form.options.itemsPerPage != 0) ? "OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY" : "";
                string strsql = string.Format(sql, pageStr);
                stockInventoryProfits = db.Database.SqlQuery<RSP>(strsql,
                                           new SqlParameter("@operId", operId),
                                           new SqlParameter("@OFFSET", ((form.options.page - 1) * form.options.itemsPerPage)),
                                           new SqlParameter("@FETCH", form.options.itemsPerPage)).ToList();
                const double sellcommision = 0.004425;
                const int percentage = 100;
                foreach (var item in stockInventoryProfits)
                {
                    item.profit = ((item.closePrice * int.Parse(item.buyShares) * (1 - sellcommision)) - int.Parse(item.buyCost, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign)).ToString("###,###");
                    item.profitPercentage = Math.Round(double.Parse(item.profit, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign) / double.Parse(item.buyCost, NumberStyles.AllowThousands) * percentage, 2);
                }

            }
            return stockInventoryProfits;
        }

        internal string createStockInventory(stockProfit profit, int operId)
        {
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
                        return "資料輸入錯誤，請重新輸入!!";
                    stockProfit.operId = operId;
                    stockProfit.code = profit.code;
                    stockProfit.buyPrice = profit.buyPrice;
                    stockProfit.buyShares = profit.buyShares;
                    stockProfit.buyCost = (profit.buyPrice * int.Parse(profit.buyShares) * buycommision).ToString("###,###");
                    stockProfit.profit = (stockNow.closePrice * int.Parse(profit.buyShares) * (1 - sellcommision) - (profit.buyPrice * int.Parse(profit.buyShares) * buycommision)).ToString("###,###");
                    stockProfit.profitPercentage = Math.Round(double.Parse(stockProfit.profit, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign) / double.Parse(stockProfit.buyCost, NumberStyles.AllowThousands) * percentage, 2);
                    db.stockProfit.Add(stockProfit);
                    db.SaveChanges();
                }
                return "新增到庫存成功!";
            }
            catch (Exception ex)
            {
                return "資料輸入錯誤，請重新輸入!!";
            }
        }

        internal string deleteStockInventory(stockProfit data, int operId)
        {
            using (var db = new WebStockEntities())
            {
                stockProfit stockProfit = db.stockProfit.Where(x => x.code == data.code && x.operId == operId && x.id == data.id).FirstOrDefault();
                if (stockProfit != null)
                {
                    db.stockProfit.Remove(stockProfit);
                    db.SaveChanges();
                    return "刪除庫存成功";
                }
                else
                    return "刪除庫存失敗";
            }
        }

        internal string addStockMemo(stockMemoView data)
        {
            if (string.IsNullOrEmpty(data.memoContent))
                return "新增失敗，請輸入memo內容";
            
            string sql = "select * from stockMemo";
            string[] stringSeparators = new string[] { "\r\n" };
            string[] codeArray = data.stockMemo.code.Split(stringSeparators, StringSplitOptions.None);
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
                        if(dr["code"].ToString() == item.code)
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
                    foreach(DataRow dr in dt.Rows)
                    {
                        if (dr["code"].ToString() == code)
                        {
                            switch (data.type)
                            {
                                case "ext1":
                                    dr["ext1"] = data.memoContent; break;
                                case "ext2":
                                    dr["ext2"] = data.memoContent; break;
                                case "ext3":
                                    dr["ext3"] = data.memoContent; break;
                                case "ext4":
                                    dr["ext4"] = data.memoContent; break;
                                case "ext5":
                                    dr["ext5"] = data.memoContent; break;
                                case "ext6":
                                    dr["ext6"] = data.memoContent; break;
                                case "ext7":
                                    dr["ext7"] = data.memoContent; break;
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
            return "新增成功";
        }
    }
}