using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebStock.ViewModels
{
    public class ApiViewModel
    {
        public class Results<T>
        {
            /// 回傳訊息
            public string Message { get; set; }

            /// 是否成功
            public bool Success { get; set; }

            /// 狀態碼
            public string Code { get; set; }

            /// 資料內容
            private T data;
            public virtual T Data
            {
                get
                {
                    return data;
                }
                set
                {
                    data = value;
                }
            }
            /// 總筆數
            public int? TotalCount;

        }
    }
}