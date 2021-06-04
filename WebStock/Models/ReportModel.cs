using System;
using System.Collections.Generic;
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
                string pageStr = "OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY";


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
                string pageStr = "OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY";

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
                    string sortDESCbool = form.options.sortDescBool ? "DESC" : "ASC";
                    string pageStr = "OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY";

                    string sortByStrType = form.options.sortByStr != null ? form.options.sortByStr : "";
                    switch (sortByStrType)
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

                    string strSql = string.Format(strSqlTmp, sortStr, sortDESCbool, pageStr);

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

        internal List<RSF> ReadStockFavorite(FormSearch form,int operId)
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
                               OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY";
                favorites = db.Database.SqlQuery<RSF>(sql,
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
                            ORDER BY i.code
                            OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY";
                stockInventoryProfits = db.Database.SqlQuery<RSP>(sql,
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