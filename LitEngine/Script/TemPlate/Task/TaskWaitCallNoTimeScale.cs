
namespace LitEngine.TemPlate.Task
{
    public class TaskWaitCallNoTimeScale : TaskBase
    {
        private System.Action<string> mCall;
        private string mKey;
        private float mSec;
        private float mTimer;
        public TaskWaitCallNoTimeScale(float _sec, string _key = "", System.Action<string> _call = null)
        {
            mKey = _key;
            mCall = _call;
            mSec = _sec;
        }

        public override void Update()
        {
            if (IsDone) return;
            mTimer += TaskManager.unscaledDeltaTime;
            if (mTimer < mSec) return;
            IsDone = true;
            if (mCall != null)
                mCall(mKey);
        }
    }
}
