using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebStock.Models;
using static WebStock.Models.CommonModel;

namespace WebStock.ViewModels
{
    public class stockProfitViewModel
    {
        public UserInfo user { get; set; }
        public List<stockInventoryProfit> stockInventoryProfits { get; set; }
    }
}