using System.Collections.Generic;
using System.Diagnostics;

namespace Common.Threading
{
    public class ThreadPool
    {
        private class ThreadPoolJob : ThreadedJob
        {
            ThreadPool m_ThreadPool = null;
            ThreadedJob m_JobToRun = null;
            public ThreadPoolJob (ThreadPool i_ThreadPool)
            {
                Debug.Assert(i_ThreadPool != null, "Invalid thread pool argument");
                m_ThreadPool = i_ThreadPool;
            }

            protected override void ThreadFunction()
            {
                lock (m_ThreadPool.m_ThreadPoolHandle)
                {
                    int pendingJobIndex = m_ThreadPool.m_Jobs.Count - 1;
                    if (pendingJobIndex >= 0)
                    {
                        m_JobToRun = m_ThreadPool.m_Jobs[pendingJobIndex];
                        m_ThreadPool.m_Jobs.RemoveAt(pendingJobIndex);
                    }
                }

                m_JobToRun.Start(false);
            }
        }

        private object m_ThreadPoolHandle = new object();
        private List<ThreadedJob> m_Jobs;
        private List<ThreadPoolJob> m_ThreadList = new List<ThreadPoolJob>();

        public void AddJob(ThreadedJob i_Job)
        {
            bool launchThread = false;
            lock (m_ThreadPoolHandle)
            {
                if((m_Jobs.Count <= 0) || (m_Jobs.Count >= 5))
                {
                    launchThread = true;
                }
                m_Jobs.Add(i_Job);
            }
            if(launchThread)
            {
                foreach(ThreadPoolJob threadJob in m_ThreadList)
                {
                    if(threadJob.IsDone)
                    {
                        threadJob.Start();
                    }
                }
            }
        }
    }
}
