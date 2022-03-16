using System.Net;
using System;
namespace LitEngine.Net
{
    public class HttpCacheObject
    {
        public string file { get; internal set; }
        public string ETag { get; internal set; }
        public string LastModified { get; internal set; }
        public string responseData { get; internal set; }
        public string Url { get; internal set; }

        public HttpCacheObject()
        {
        }

    }
}
