using LitEngine.UpdateSpace;
namespace LitEngine.TemPlate.State
{
    public class StateManager
    {
        private static object lockobj = new object();
        private static StateManager sInstance = null;
        static public StateManager Instance
        {
            get
            {
                if (sInstance == null)
                {
                    lock (lockobj)
                    {
                        if (sInstance == null)
                        {
                            sInstance = new StateManager();
                        }
                    }

                }
                return sInstance;
            }
        }
        private StateManager() {
            updateObject = new UpdateObject("StateManager", new Method.Method_Action(Update), null);
            updateObject.MaxTime = 0;
            GameUpdateManager.RegUpdate(updateObject);
        }
        private UpdateObject updateObject;
        protected enum MAction
        {
            kst_none = 0,
            kst_change,
            kst_normal,
        }

        public StateBase State { get; protected set; }
        private MAction SelfAction = MAction.kst_none;
        private StateBase NextState;

        private System.Action completeDelegate;
        public static void ChangeState<T>(System.Action _complete,params object[] _objects) where T : StateBase, new()
        {
            Instance.completeDelegate = _complete;
            Instance.NextState = new T();
            Instance.NextState.SetStateData(_objects);
            Instance.SelfAction = MAction.kst_change;
            DLog.Log("Change State to " + Instance.NextState.GetType().Name);
        }

        public static void ChangeStateImmediately<T>(params object[] _objects) where T : StateBase, new()
        {
            ChangeState<T>(null,_objects);
            Instance.Change();
        }

        private void Change()
        {
            if (State != null)
            {
                State.OnQuit();
                State.Dispose();
            }
            State = NextState;
            NextState = null;
            State.OnEnter();
            if (completeDelegate != null)
                completeDelegate();
            completeDelegate = null;
            SelfAction = MAction.kst_normal;
        }
        private void Update()
        {
            UpdateAction();
        }

        protected void UpdateAction()
        {
            switch (SelfAction)
            {
                case MAction.kst_change:
                    Change();
                    break;
                case MAction.kst_normal:
                    UpdateState();
                    break;
            }
        }
        protected void UpdateState()
        {
            if (State == null) return;
            State.Update();
        }
    }
}
