using System;
using System.Diagnostics;
using System.Threading;

namespace Common.Threading
{
    public enum ThreadedJobState
    {
        InProgress,
        Aborted,
        Succeeded,
        Errored
    }

    public class ThreadedJob<TResult>
    {
        protected readonly Thread m_Thread;
        protected Func<TResult> m_RunFunc = null;
        protected ThreadedJobState m_State = ThreadedJobState.InProgress;
        protected TResult m_Result = default(TResult);
        protected Exception m_Exception = null;

        public ThreadedJob(Func<TResult> i_Function)
        {
            Log.DebugAssert(i_Function != null, "invalid Function argument.");
            m_RunFunc = i_Function;

            m_Thread = new Thread(Run);
            m_Thread.Start();
        }

        public ThreadedJobState State
        {
            get { return m_State; }
        }

        public Exception GetException()
        {
            return m_Exception;
        }

        public TResult GetResult()
        {
            if (m_State == ThreadedJobState.InProgress)
            {
                m_Thread.Join();
            }
            return m_Result;
        }

        public virtual void Abort()
        {
            m_Thread.Abort();
            m_State = ThreadedJobState.Aborted;
        }

        protected virtual void Run()
        {
            try
            {
                m_Result = m_RunFunc();
                m_State = ThreadedJobState.Succeeded;
            }
            catch(Exception e)
            {
                m_Exception = e;
                m_State = ThreadedJobState.Errored;
            }
        }
    }
}
