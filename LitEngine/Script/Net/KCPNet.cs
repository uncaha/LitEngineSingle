using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
namespace LitEngine.Net
{
    public class KCPNet : NetBase<KCPNet>
    {
        #region socket属性
        static public bool IsPushPackage = false;
        protected IPEndPoint mTargetPoint;//目标地址
        protected EndPoint mRecPoint;
        protected string mServerIP;
        protected int mLocalPort = 10186;
        #endregion
        #region 初始化
        private KCPNet() : base()
        {
            mNetTag = "KCP";
        }
        #endregion

    }
}
