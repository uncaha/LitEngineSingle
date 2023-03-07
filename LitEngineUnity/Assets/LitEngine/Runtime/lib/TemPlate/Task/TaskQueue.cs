using System.Collections.Generic;
namespace LitEngine.TemPlate.Task
{
    public class TaskQueue : TaskBase
    {
        private List<TaskBase> mQueue;
        private int mTaskCount = 0;
        public TaskQueue(params TaskBase[] newTasks)
        {
            if (newTasks != null)
            {
                mQueue = new List<TaskBase>();
                mQueue.AddRange(newTasks);
                mQueue.Reverse();
                mTaskCount = mQueue.Count;
            }
            if (mTaskCount == 0)
                IsDone = true;

        }

        static public void RunQueue( params TaskBase[] newTasks)
        {
            TaskQueue tque = new TaskQueue(newTasks);
            tque.Start();
        }

        public override void Update()
        {
            if (IsDone) return;

            int i = mTaskCount - 1;
            for (; i >= 0; i--)
            {
                mQueue[i].Update();
                if (mQueue[i].IsDone)
                {
                    mQueue.RemoveAt(i);
                    mTaskCount--;
                    continue;
                }
                break;
            }

            if (mTaskCount == 0)
            {
                IsDone = true;
            }
        }
    }
}
