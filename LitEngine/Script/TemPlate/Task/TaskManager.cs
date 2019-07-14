using System.Collections.Generic;
using LitEngine.UpdateSpace;
using UnityEngine;
namespace LitEngine.TemPlate.Task
{
    public class TaskManager
    {
        public static float MaxStep = 0f;
        private static object lockobj = new object();
        private static TaskManager sInstance = null;
        static public TaskManager Instance
        {
            get
            {
                if (sInstance == null)
                {
                    lock (lockobj)
                    {
                        if (sInstance == null)
                        {
                            sInstance = new TaskManager();
                        }
                    }

                }
                return sInstance;
            }
        }

        List<TaskBase> taskList = new List<TaskBase>();
        int taskCount = 0;
        static public float deltaTime { get; private set; }
        static public float unscaledDeltaTime { get; private set; }
        private TaskManager()
        {
            UpdateObject tobj = new UpdateObject("TaskManager", new Method.Method_Action(Update), null);
            tobj.MaxTime = MaxStep;
            GameUpdateManager.RegUpdate(tobj);
        }

        private void Update()
        {
            unscaledDeltaTime = MaxStep > Time.unscaledDeltaTime ? MaxStep : Time.unscaledDeltaTime;
            deltaTime = MaxStep > Time.deltaTime ? MaxStep : Time.deltaTime;
            deltaTime *= Time.timeScale;
            if (taskCount == 0) return;
            int i = taskCount - 1;
            for (; i >= 0; i--)
            {
                TaskBase curTask = taskList[i];
                curTask.Update();
                if (curTask.IsDone)
                    curTask.Dispose();
            }
        }

        public static void Add(TaskBase _task)
        {
            if (Instance.taskList.Contains(_task)) return;
            Instance.taskList.Add(_task);
            Instance.taskCount = Instance.taskList.Count;
        }

        public static void Remove(TaskBase newTask)
        {
            if (!Instance.taskList.Contains(newTask)) return;
            Instance.taskList.Remove(newTask);
            Instance.taskCount = Instance.taskList.Count;
        }
    }
}
