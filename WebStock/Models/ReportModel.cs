using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using static WebStock.ViewModels.ReportViewModel;

namespace WebStock.Models
{
    public class ReportModel
    {
        internal List<RSI> ReadStockIndex(FormSearch f)
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
                if (string.IsNullOrEmpty(f.code))
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
                string sortDESC = (f.options.sortDesc != null && f.options.sortDesc[0]) ? "DESC" : "ASC";
                string pageStr = "OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY";


                string sortByType = (f.options.sortBy != null && f.options.sortBy[0] != "") ? f.options.sortBy[0] : "";
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
                    new SqlParameter("@type", f.type),
                    new SqlParameter("@code", f.code ?? string.Empty),
                    new SqlParameter("@OFFSET", ((f.options.page - 1) * f.options.itemsPerPage)),
                    new SqlParameter("@FETCH", f.options.itemsPerPage)
                    ).ToList();
                return rs;
            }

        }

        internal List<RSD> ReadStockData(FormSearch f)
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
                string sortDESC = (f.options.sortDesc != null && f.options.sortDesc[0]) ? "DESC" : "ASC";
                string pageStr = "OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY";

                string sortByType = (f.options.sortBy != null && f.options.sortBy[0] != "") ? f.options.sortBy[0] : "";
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
                    new SqlParameter("@code", f.code ?? string.Empty),
                    new SqlParameter("@dataDate", f.dataDate),
                    new SqlParameter("@OFFSET", ((f.options.page - 1) * f.options.itemsPerPage)),
                    new SqlParameter("@FETCH", f.options.itemsPerPage)
                    ).ToList();

                if (datas.Count == 0)
                { return new List<RSD>(); }

                return datas;
            }
        }

        internal List<RSS> ReadStockStatistics()
        {
            List<RSS> stockSummaryStatistics = new List<RSS>();
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
                stockSummaryStatistics = db.Database.SqlQuery<RSS>(sql).ToList();
            }
            return stockSummaryStatistics;
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
    }
}