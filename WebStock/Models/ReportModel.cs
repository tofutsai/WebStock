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
        internal List<RSID> ReadStockIndex(FormSearch f)
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
                List<RSID> rs = db.Database.SqlQuery<RSID>(sqlStr,
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
    }
}