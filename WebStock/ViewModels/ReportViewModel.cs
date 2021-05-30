using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebStock.Models;

namespace WebStock.ViewModels
{
    public class ReportViewModel
    {
        public class FormSearch
        {
            public int operId { get; set; }
            public string type { get; set; }
            public string code { get; set; }
            public int shares { get; set; }
            public double closePrice { get; set; }
            public double position1 { get; set; }
            public double position2 { get; set; }
            public DateTime dataDate { get; set; }
            public Options options { get; set; }
        }
        public class Options
        {
            public bool[] sortDesc { get; set; }
            public string[] sortBy { get; set; }
            public bool sortDescBool { get; set; }
            public string sortByStr { get; set; }
            public int page { get; set; }
            public int itemsPerPage { get; set; }

        }

        public class StockIndexView
        {
            public FormSearch formSearch { get; set; }
            public List<RSID> data { get; set; }

        }
        public class RSID : stockIndex
        {
            public int totalCount { get; set; }
        }

        public class StockDataView
        {
            public FormSearch formSearch { get; set; }
            public List<RSD> data { get; set; }
        }

        public class RSD : stockData
        {
            public string company { get; set; }
            public int totalCount { get; set; }
        }

    }
}