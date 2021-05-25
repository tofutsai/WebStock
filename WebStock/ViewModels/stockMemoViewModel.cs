using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebStock.Models;

namespace WebStock.ViewModels
{
    public class stockMemoViewModel
    {
        public string type { get; set; }
        public string memoContent { get; set; }
        public stockMemo stockMemo { get; set; }
    }
}