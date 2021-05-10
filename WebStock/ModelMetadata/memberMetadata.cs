using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebStock.Models
{
    [MetadataType(typeof(memberMetadata))]
    public partial class member
    {
        public class memberMetadata
        {
            [DisplayName("帳號")]
            [Required(ErrorMessage = "請輸入帳號")]
            public string account { get; set; }

            [DisplayName("密碼")]
            [Required(ErrorMessage = "請輸入密碼")]
            public string password { get; set; }

            [DisplayName("姓名")]
            [Required(ErrorMessage = "請輸入姓名")]
            public string name { get; set; }
            public bool providerFB { get; set; }
            public bool providerGoogle { get; set; }
            public string role { get; set; }

        }
    }
    
}