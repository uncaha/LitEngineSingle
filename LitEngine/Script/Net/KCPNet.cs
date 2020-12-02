using System;
namespace LitEngine.Net
{
    public class KCPNet : NetBase<KCPNet>
    {
        #region 初始化
        private KCPNet() : base()
        {
            mNetTag = "KCP";
        }
        #endregion

    }
}
