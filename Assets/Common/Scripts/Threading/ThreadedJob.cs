using System.Threading;

namespace Common.Threading
{
    public class ThreadedJob
    {
        private bool m_IsDone = false;
        protected object m_Handle = new object();
        protected Thread m_Thread = null;

        public ThreadedJob()
        { }

        public bool IsDone
        {
            get
            {
                bool tmp;
                lock (m_Handle)
                {
                    tmp = m_IsDone;
                }
                return tmp;
            }
            protected set
            {
                lock (m_Handle)
                {
                    m_IsDone = value;
                }
            }
        }

        public void ThreadPoolCallback(object i_State)
        {
            Start(false);
        }

        public virtual void Start(bool i_Threaded = true)
        {
            if (!IsDone)
            {
                Abort();
            }
            IsDone = false;
            if (i_Threaded)
            {
                m_Thread = new Thread(Run);
                m_Thread.Start();
            }
            else
            {
                Run();
            }
        }

        public virtual void Abort()
        {
            if (m_Thread != null)
            {
                m_Thread.Abort();
            }
            IsDone = true;
        }

        protected virtual void ThreadFunction() { }

        protected virtual void Run()
        {
            ThreadFunction();
            IsDone = true;
        }
    }
}
