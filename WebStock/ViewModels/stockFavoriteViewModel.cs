using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebStock.Models;
using static WebStock.Models.CommonModel;

namespace WebStock.ViewModels
{
    public class stockFavoriteViewModel
    {
        public UserInfo user { get; set; }
        public stockFavorite stockFavorite { get; set; }
        public List<stockSelfFavorite> favorites { get; set; }
    }
}