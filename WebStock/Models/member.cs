//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebStock.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class member
    {
        public int id { get; set; }
        public string account { get; set; }
        public string password { get; set; }
        public string name { get; set; }
        public bool providerFB { get; set; }
        public bool providerGoogle { get; set; }
        public string role { get; set; }
        public bool isAdmin { get; set; }
    }
}
