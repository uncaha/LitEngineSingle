
namespace LitEngine.TemPlate.Task
{
    public class TaskCallBack : TaskBase
    {
        private System.Action mCall;
        public TaskCallBack(System.Action _call)
        {
            mCall = _call;
        }
        public override void Update()
        {
            if (IsDone) return;
            if (mCall != null)
                mCall();
            IsDone = true;
        }
    }
}
