﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System;

namespace LitEngine.Net.Http
{
    
    public sealed class HttpNet : HttpManager<HttpNet>
    {
        public HttpNet()
        {
            Tag = "HttpNet";
        }
    }

}
