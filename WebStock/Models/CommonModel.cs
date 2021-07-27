using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using JWT.Serializers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using WebStock.ViewModels;
using static WebStock.ViewModels.ReportViewModel;

namespace WebStock.Models
{
    public class CommonModel
    {
        public class UserInfo
        {
            public string JWToken { get; set; }
            public int OperId { get; set; }
            public string OperAccount { get; set; }
            public string OperName { get; set; }
            public string OperRole { get; set; }
            public bool OperIsAdmin { get; set; }
        }

        //public static string TokenSecretKey = ConfigurationManager.AppSettings["TokenSecretKey"].ToString();
        public static string TokenSecretKey = "HELLOKITTY";
        public static string EncodeJWTToken(object payload)
        {

            var secret = TokenSecretKey;

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(payload, secret);
            //Console.WriteLine(token);

            return token;
        }

        public static UserInfo DecodeJWTToken(string jwtToken)
        {

            try
            {
                IJsonSerializer serializer = new JsonNetSerializer();
                IDateTimeProvider provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);
                UserInfo dd = decoder.DecodeToObject<UserInfo>(jwtToken, TokenSecretKey, true);

                return dd;
            }
            catch (TokenExpiredException)
            {
                Console.WriteLine("Token has expired");
                return null;
            }
            catch (SignatureVerificationException)
            {
                Console.WriteLine("Token has invalid signature");
                return null;
            }

        }

        public class stockDataAPI
        {
            public string stat { get; set; }
            public string date { get; set; }
            public string title { get; set; }
            public List<string> fields9 { get; set; }
            public List<List<string>> data8 { get; set; }
            public List<List<string>> data9 { get; set; }
            public List<string> notes { get; set; }
        }

        public class OtcDataAPI
        {
            public string reportDate { get; set; }
            public string reportTitle { get; set; }
            public string iTotalRecords { get; set; }
            public string iTotalDisplayRecords { get; set; }
            public List<List<string>> aaData { get; set; }
        }

        public class stockStatistics : stockAvg
        {
            public int dataYear { get; set; }
        }

        public class stockNowStatistics : stockNow
        {
            public double highestPrice { get; set; }
            public double lowestPrice { get; set; }
        }

        public class stockSummaryStatistics : stockAvg
        {
            public string type { get; set; }
            public string category { get; set; }
            public string company { get; set; }
            public double position { get; set; }
            public double closePrice { get; set; }
            public DateTime dataDate { get; set; }
            public string memo { get; set; }
        }
               
        public string GetJWTToken(object payload)
        {
            string jsonString = JsonConvert.SerializeObject(payload);
            return jsonString;

        }
        internal UserInfo DecodeJWTTokenMVC(string jwtToken)
        {
            UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(jwtToken);
            return userInfo;
        }

        
        
    }
}