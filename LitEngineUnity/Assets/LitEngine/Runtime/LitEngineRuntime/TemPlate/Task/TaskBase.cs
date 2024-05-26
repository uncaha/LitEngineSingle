
namespace LitEngine.TemPlate.Task
{
    public abstract class TaskBase
    {
        public bool IsDone { get; protected set; }
        public void Start()
        {
            TaskManager.Add(this);
        }
        abstract public void Update();
        virtual public void Dispose()
        {
            TaskManager.Remove(this);
        }
    }
}
