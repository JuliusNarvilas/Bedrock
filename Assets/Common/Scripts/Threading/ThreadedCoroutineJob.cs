using System.Collections;

namespace Common.Threading
{
    public class ThreadedCoroutineJob : ThreadedJob
    {
        public override void Start()
        {
            if(!IsDone)
            {
                Abort();
            }
            IsDone = false;
            OnStart();
            m_Thread.Start();
        }

        public virtual void OnStart()
        { }

        public virtual void OnThreadedStart()
        { }

        public virtual void OnThreadedFinish()
        { }

        public virtual void OnFinish()
        { }

        public virtual void Update()
        { }

        public IEnumerator RunCoroutine()
        {
            while (!IsDone)
            {
                Update();
                yield return null;
            }
            OnFinish();
        }

        protected override void Run()
        {
            OnThreadedStart();
            ThreadFunction();
            OnThreadedFinish();
            IsDone = true;
        }
    }
}
