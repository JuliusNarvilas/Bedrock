using System.Threading;

namespace Common.Threading
{
    public class ThreadedJob
    {
        private bool m_IsDone = false;
        protected object m_Handle = new object();
        protected Thread m_Thread = null;

        public ThreadedJob()
        {
            m_Thread = new System.Threading.Thread(Run);
        }

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

        public virtual void Start()
        {
            IsDone = false;
            m_Thread.Start();
        }

        public virtual void Abort()
        {
            m_Thread.Abort();
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
