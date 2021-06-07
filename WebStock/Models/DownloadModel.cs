﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using static WebStock.Models.CommonModel;
using WebStock.Models;
using System.Data;
using System.Data.SqlClient;

namespace WebStock.Models
{

    public class DownloadModel
    {
        SysModel sysModel = new SysModel();
        internal async Task<string> DownloadStock(string date)
        {

            string result = "";
            string apiDate = date.Replace("-", "");
            string jsonString = await stockAPI(apiDate);
            int rescount = 0;
            stockDataAPI stockDataAPI = new stockDataAPI();

            //JSON反序列化裝入刻好的物件，傳入controller
            stockDataAPI = JsonConvert.DeserializeObject<stockDataAPI>(jsonString);

            if (stockDataAPI.stat.Contains("OK"))
            {
                //init bulkcopy
                DataTable dt = new DataTable();
                dt.Columns.Add("id", typeof(Int64));
                dt.Columns.Add("code", typeof(string));
                dt.Columns.Add("dataDate", typeof(DateTime));
                dt.Columns.Add("shares", typeof(Int64));
                dt.Columns.Add("turnover", typeof(double));
                dt.Columns.Add("openPrice", typeof(double));
                dt.Columns.Add("highestPrice", typeof(double));
                dt.Columns.Add("lowestPrice", typeof(double));
                dt.Columns.Add("closePrice", typeof(double));
                dt.Columns.Add("spread", typeof(double));

                if (stockDataAPI.data9 == null)
                    stockDataAPI.data9 = stockDataAPI.data8;
                foreach (var item in stockDataAPI.data9)
                {
                    DataRow row = dt.NewRow();
                    if (item[5].Contains("--") || item[5] == "0.00")
                        continue;
                    //insert row data
                    row["code"] = item[0];
                    row["dataDate"] = date;
                    row["shares"] = Math.Floor(Convert.ToDouble(item[2].Replace(",", "")) / 1000);
                    row["turnover"] = Math.Round(Convert.ToDouble(item[4].Replace(",", "")) / 1000000, 4);
                    row["openPrice"] = Convert.ToDouble(item[5].Replace(",", ""));
                    row["highestPrice"] = Convert.ToDouble(item[6].Replace(",", ""));
                    row["lowestPrice"] = Convert.ToDouble(item[7].Replace(",", ""));
                    row["closePrice"] = Convert.ToDouble(item[8].Replace(",", ""));
                    
                    if (item[9].Contains("+") || item[9].Contains(" ") || item[9].Contains("X"))
                        row["spread"] = Convert.ToDouble(item[10]) * 1;
                    if (item[9].Contains("-"))
                        row["spread"] = Convert.ToDouble(item[10]) * -1;
                                         
                    dt.Rows.Add(row);
                }

                using (var db = new WebStockEntities())
                {
                    //清空資料暫存Table
                    db.Database.ExecuteSqlCommand(@"truncate table stockDataTmp");

                    //sqlBulkCopy 寫入資料Table
                    SqlConnection conn = (SqlConnection)db.Database.Connection;
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();

                    using (var sqlBulkCopy = new SqlBulkCopy((SqlConnection)db.Database.Connection))
                    {
                        sqlBulkCopy.DestinationTableName = "dbo.stockDataTmp";
                        sqlBulkCopy.WriteToServer(dt);
                    }

                    string strSql = @"INSERT INTO [dbo].[stockData]  
                                     (code,		
                                     dataDate,	
                                     shares,		
                                     turnover,	
                                     openPrice,
                                     highestPrice,
                                     lowestPrice,	
                                     closePrice,
                                     spread)
                                    
                                    SELECT 
                                     b.code, 
                                     b.dataDate,	
                                     b.shares,	
                                     b.turnover,	
                                     b.openPrice,
                                     b.highestPrice,
                                     b.lowestPrice,
                                     b.closePrice,
                                     b.spread
                                    FROM stockIndex a
                                    JOIN stockDataTmp b
                                    ON a.code = b.code
                                    LEFT JOIN stockData c
                                    ON b.code = c.code AND b.dataDate = c.dataDate
                                    WHERE a.type LIKE '上市' AND  c.code is NULL;
                                     
                                    Select @@ROWCOUNT;
                                    ";

                    rescount = db.Database.ExecuteSqlCommand(strSql);
                }

                string logtype = "DownloadStock";
                string message = $"queryDate : {date}, result : 寫入成功, count : {rescount}";
                sysLog log = sysModel.createLog(logtype, message);

                result = $"queryDate : {log.date}, result : {log.message}";
            }
            else
            {
                string logtype = "DownloadStock";
                string message = $"queryDate : {date}, result : 無資料, count : {rescount}";
                sysLog log = sysModel.createLog(logtype, message);

                result = $"queryDate : {log.date}, result : {log.message}";

            }
            return result;

        }

        internal async Task<string> DownloadOtc(string date)
        {
            using (var db = new WebStockEntities())
            {
                string result = "";
                string apiDate = date.Substring(1, date.Length - 1);
                string jsonString = await OtcAPI(apiDate);
                DateTime datetime = Convert.ToDateTime(date);
                DateTime westdatetime = datetime.AddYears(1911);
                int rescount = 0;
                OtcDataAPI OtcDataAPI = new OtcDataAPI();

                //JSON反序列化裝入刻好的物件，傳入controller
                OtcDataAPI = JsonConvert.DeserializeObject<OtcDataAPI>(jsonString);

                if (OtcDataAPI.iTotalRecords != "0")
                {
                    stockDataTmpOtc OtcData = new stockDataTmpOtc();
                    
                    //init bulkcopy
                    DataTable dt = new DataTable();
                    dt.Columns.Add("id", typeof(Int64));
                    dt.Columns.Add("code", typeof(string));
                    dt.Columns.Add("dataDate", typeof(DateTime));
                    dt.Columns.Add("shares", typeof(Int64));
                    dt.Columns.Add("turnover", typeof(double));
                    dt.Columns.Add("openPrice", typeof(double));
                    dt.Columns.Add("highestPrice", typeof(double));
                    dt.Columns.Add("lowestPrice", typeof(double));
                    dt.Columns.Add("closePrice", typeof(double));
                    dt.Columns.Add("spread", typeof(double));

                    foreach (var item in OtcDataAPI.aaData)
                    {
                        DataRow row = dt.NewRow();
                        if (item[2].Contains("---") || item[3].Contains("---"))
                            continue;
                        row["code"] = item[0];
                        row["dataDate"] = Convert.ToDateTime(westdatetime);
                        row["shares"] = Convert.ToInt32(item[8].Replace(",", "")) / 1000;
                        row["turnover"] = Math.Round(Convert.ToDouble(item[9].Replace(",", "")) / 1000000, 3);
                        row["openPrice"] = Convert.ToDouble(item[4].Replace(",", ""));
                        row["highestPrice"] = Convert.ToDouble(item[5].Replace(",", ""));
                        row["lowestPrice"] = Convert.ToDouble(item[6].Replace(",", ""));
                        row["closePrice"] = Convert.ToDouble(item[2].Replace(",", ""));
                        if (item[3].Contains("+") || item[3] == "0.00 ")
                        {
                            if (item[3].Contains("#"))
                            {
                                item[3] = "0";
                                row["spread"] = Convert.ToDouble(item[3]);
                            }
                            else
                                row["spread"] = Convert.ToDouble(item[3]);
                        }
                        else if (item[3].Contains("-"))
                            row["spread"] = Convert.ToDouble(item[3].Replace("-", "")) * -1;
                        else if (item[3].Contains("除"))
                        {
                            item[3] = "0";
                            row["spread"] = Convert.ToDouble(item[3]);
                        }
                        else
                            row["spread"] = Convert.ToDouble(item[3]);

                        dt.Rows.Add(row);
                    }

                    //清空資料暫存Table
                    db.Database.ExecuteSqlCommand(@"truncate table stockDataTmpOtc");

                    //sqlBulkCopy 寫入資料Table
                    SqlConnection conn = (SqlConnection)db.Database.Connection;
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();

                    using (var sqlBulkCopy = new SqlBulkCopy((SqlConnection)db.Database.Connection))
                    {
                        sqlBulkCopy.DestinationTableName = "dbo.stockDataTmpOtc";
                        sqlBulkCopy.WriteToServer(dt);
                    }

                    string strSql = @"insert into stockData (
                                code,
                                dataDate,
                                shares,
                                turnover,
                                openPrice,
                                highestPrice,
                                lowestPrice,
                                closePrice,
                                spread
                                )
                                
                                select 
                                
                                b.code,
                                b.dataDate,
                                b.shares,
                                b.turnover,
                                b.openPrice,
                                b.highestPrice,
                                b.lowestPrice,
                                b.closePrice,
                                b.spread
                                
                                from stockIndex a
                                join stockDataTmpOtc b
                                on a.code = b.code
                                left join stockData c
                                on b.code = c.code and b.dataDate = c.dataDate
                                where a.type LIKE '上櫃' and c.code is NULL;
                                select @@ROWCOUNT
                                ";

                    rescount = db.Database.ExecuteSqlCommand(strSql);
                    string logtype = "DownloadOtc";
                    string message = $"queryDate : {date}, result : 寫入成功, count : {rescount}";
                    sysLog log = sysModel.createLog(logtype, message);

                    result = $"queryDate : {log.date}, result : {log.message}";
                }
                else
                {
                    string logtype = "DownloadOtc";
                    string message = $"queryDate : {date}, result : 無資料, count : {rescount}";
                    sysLog log = sysModel.createLog(logtype, message);

                    result = $"queryDate : {log.date}, result : {log.message}";
                }

                return result;


            }
        }

        internal async Task<string> stockAPI(string date)
        {
            try
            {
                //WebClient API 設定寫法 
                string url = "https://www.twse.com.tw/exchangeReport/MI_INDEX?response=json&date=" + date + "&type=ALLBUT0999&_=1602315728894";
                string response = "";
                WebClient client = new WebClient();
                // 指定 WebClient 的 Content-Type header
                client.Headers.Add("Content-Type", "application/json;charset=utf-8");

                //連證交所取得股價資料(JSON方式回傳)
                response = await client.DownloadStringTaskAsync(url);

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }

        internal async Task<string> OtcAPI(string date)
        {
            try
            {
                //WebClient API 設定寫法 
                string url = "https://www.tpex.org.tw/web/stock/aftertrading/daily_close_quotes/stk_quote_result.php?l=zh-tw&d=" + date + "&_=1617462572985";
                string response = "";
                WebClient client = new WebClient();
                // 指定 WebClient 的 Content-Type header
                client.Headers.Add("Content-Type", "application/json;charset=utf-8");

                //連證交所取得股價資料(JSON方式回傳)
                response = await client.DownloadStringTaskAsync(url);

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }
    }
}