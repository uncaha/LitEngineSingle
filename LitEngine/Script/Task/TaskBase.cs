using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LitEngine
{
    namespace LitTask
    {
        public abstract class TaskBase : IDisposable
        {
            public TaskBase()
            {

            }
            ~TaskBase()
            {
                Dispose(false);
            }

            protected bool mDisposed = false;
            public void Dispose()
            {
                Dispose(true);
                System.GC.SuppressFinalize(this);
            }

            protected void Dispose(bool _disposing)
            {
                if (mDisposed)
                    return;
                mDisposed = true;
                DisposeGC();
            }

            abstract protected void DisposeGC();

            virtual protected void Update()
            {
                if (!IsDone) return;
            }
            abstract public bool IsDone { get;}
        }
    }
}
