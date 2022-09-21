using System;
using System.Security.Cryptography;
using System.Text;
namespace LitEngine.Tool
{
    public class DataUtile
    {
        /// <summary>
        /// 1970-01-01 ms
        /// </summary>
        public static long GetTimestamp(DateTime dateTime)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (dateTime.Ticks - dt1970.Ticks) / 10000;
        }
        
        /// <summary>
        /// 1970-01-01 ms
        /// </summary>
        /// <param name="pTicks">ms ticks</param>
        /// <returns>ms ticks</returns>
        public static long GetTimestamp(long pTicks)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return pTicks - dt1970.Ticks / 10000;
        }

        /// <summary>
        /// timestamp ms
        /// </summary>
        public static DateTime NewDate(long timestamp)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long t = dt1970.Ticks + timestamp * 10000;
            return new DateTime(t);
        }
        
        /// <summary>
        /// md5
        /// </summary>
        public static string GetMD5(string pStr)
        {
            byte[] data = Encoding.UTF8.GetBytes(pStr);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] OutBytes = md5.ComputeHash(data);
            StringBuilder tstr = new StringBuilder();
            for (int i = 0; i < OutBytes.Length; i++)
            {
                tstr.Append(OutBytes[i].ToString("x2"));
            }
            return tstr.ToString();
        }
    }
}