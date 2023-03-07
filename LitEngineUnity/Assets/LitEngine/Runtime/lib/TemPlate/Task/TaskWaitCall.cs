using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitEngine.TemPlate.Task
{
    public class TaskWaitCall : TaskBase
    {
        private System.Action<string> mCall;
        private string mKey;
        private float mSec;
        private float mTimer;
        public TaskWaitCall(float _sec, string _key = "", System.Action<string> _call = null)
        {
            mKey = _key;
            mCall = _call;
            mSec = _sec;
        }

        public override void Update()
        {
            if (IsDone) return;
            mTimer += TaskManager.deltaTime;
            if (mTimer < mSec) return;
            IsDone = true;
            if (mCall != null)
                mCall(mKey);
        }
    }
}
