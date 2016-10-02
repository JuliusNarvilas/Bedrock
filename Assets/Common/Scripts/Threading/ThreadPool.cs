using Common.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Common.Threading
{
    public enum ThreadPoolTaskPriority
    {
        VeryHigh,
        High,
        Medium,
        Low
    }

    public class ThreadPool : IDisposable
    {
        private class ThreadPoolJobTask
        {
            public object Result = null;
            public ThreadedJobState State = ThreadedJobState.InProgress;
            public Exception Exception = null;
            public readonly Func<object> RunFunc = null;
            public readonly TimeSpan MaxRunTime;
            public readonly DateTime ExpectedRunTimestamp;
            public DateTime RunTimestamp = DateTime.MaxValue;

            public ThreadPoolJobTask(Func<object> i_RunFunc, ThreadPoolTaskPriority i_Priority = ThreadPoolTaskPriority.Medium)
            {
                RunFunc = i_RunFunc;
                MaxRunTime = new TimeSpan(0, 0, 5, 0);
                switch(i_Priority)
                {
                    case ThreadPoolTaskPriority.VeryHigh:
                        ExpectedRunTimestamp = DateTime.UtcNow.AddMilliseconds(500);
                        break;
                    case ThreadPoolTaskPriority.High:
                        ExpectedRunTimestamp = DateTime.UtcNow.AddSeconds(1);
                        break;
                    case ThreadPoolTaskPriority.Medium:
                        ExpectedRunTimestamp = DateTime.UtcNow.AddSeconds(2);
                        break;
                    case ThreadPoolTaskPriority.Low:
                        ExpectedRunTimestamp = DateTime.UtcNow.AddSeconds(3);
                        break;
                }
            }
            public ThreadPoolJobTask(Func<object> i_RunFunc, TimeSpan i_MaxRunTime, ThreadPoolTaskPriority i_Priority = ThreadPoolTaskPriority.Medium)
            {
                RunFunc = i_RunFunc;
                MaxRunTime = i_MaxRunTime;
                switch (i_Priority)
                {
                    case ThreadPoolTaskPriority.VeryHigh:
                        ExpectedRunTimestamp = DateTime.UtcNow.AddMilliseconds(500);
                        break;
                    case ThreadPoolTaskPriority.High:
                        ExpectedRunTimestamp = DateTime.UtcNow.AddSeconds(1);
                        break;
                    case ThreadPoolTaskPriority.Medium:
                        ExpectedRunTimestamp = DateTime.UtcNow.AddSeconds(2);
                        break;
                    case ThreadPoolTaskPriority.Low:
                        ExpectedRunTimestamp = DateTime.UtcNow.AddSeconds(3);
                        break;
                }
            }
        }

        private class ThreadPoolJobTaskDescendingComparer : IComparer<ThreadPoolJobTask>
        {
            private ThreadPoolJobTaskDescendingComparer()
            { }

            public int Compare(ThreadPoolJobTask i_A, ThreadPoolJobTask i_B)
            {
                return i_A.ExpectedRunTimestamp.CompareTo(i_B.ExpectedRunTimestamp) * -1;
            }

            public static ThreadPoolJobTaskDescendingComparer Innstance = new ThreadPoolJobTaskDescendingComparer();
        }

        private class ThreadPoolResult<TResult> : IThreadPoolResult<TResult>
        {
            private readonly ThreadPoolJobTask m_JobLink;

            public ThreadPoolResult(ThreadPoolJobTask i_JobLink)
            {
                m_JobLink = i_JobLink;
            }

            public ThreadedJobState State
            {
                get { return m_JobLink.State; }
            }

            public Exception GetException()
            {
                return m_JobLink.Exception;
            }

            public TResult GetResult()
            {
                while (m_JobLink.State == ThreadedJobState.InProgress)
                {
                    Thread.Sleep(10);
                }
                return (TResult)m_JobLink.Result;
            }
        }

        private class ThreadPoolJob
        {
            public readonly object ThreadTaskChangeHandle = new object();
            private ThreadPoolJobTask m_Task = null;
            private readonly ThreadPool m_ThreadPool;
            private bool m_Active = true;
            private readonly Thread m_Thread;

            public ThreadPoolJobTask Task
            {
                get { return m_Task; }
            }

            public ThreadPoolJob(ThreadPool i_ThreadPool)
            {
                Debug.Assert(i_ThreadPool != null, "Invalid thread pool argument");
                m_ThreadPool = i_ThreadPool;

                m_Thread = new Thread(Run);
                m_Thread.Start();
            }

            public bool Active
            {
                get { return m_Active; }
            }
            
            public void Close()
            {
                m_Active = false;
            }

            public void Abort()
            {
                m_Active = false;
                try
                {
                    m_Thread.Abort();
                    if (m_Task != null)
                    {
                        m_Task.State = ThreadedJobState.Aborted;
                    }
                }
                catch
                {
                    //do nothing
                }
                m_Task = null;
            }

            private void Run()
            {
                while (m_Active)
                {
                    if (m_ThreadPool.m_Tasks.Count > 0)
                    {
                        lock (m_ThreadPool.m_ThreadPoolTaskListChangeHandle)
                        {
                            lock (ThreadTaskChangeHandle)
                            {
                                int lastIndex = m_ThreadPool.m_Tasks.Count - 1;
                                if (lastIndex >= 0)
                                {
                                    m_Task = m_ThreadPool.m_Tasks[lastIndex];
                                    m_ThreadPool.m_Tasks.RemoveAt(lastIndex);
                                }
                            }
                        }
                    }
                    if (m_Task != null)
                    {
                        try
                        {
                            m_Task.RunTimestamp = DateTime.UtcNow;
                            m_Task.Result = m_Task.RunFunc();
                            m_Task.State = ThreadedJobState.Succeeded;
                        }
                        catch (Exception e)
                        {
                            m_Task.Exception = e;
                            m_Task.State = ThreadedJobState.Errored;
                        }
                        finally
                        {
                            lock (ThreadTaskChangeHandle)
                            {
                                m_Task = null;
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
        }

        private readonly object m_ThreadPoolTaskListChangeHandle = new object();
        private readonly Thread m_ManagerThread;
        private readonly List<ThreadPoolJobTask> m_Tasks = new List<ThreadPoolJobTask>(5);
        private readonly List<ThreadPoolJob> m_ActiveThreadList = new List<ThreadPoolJob>();
        private readonly List<ThreadPoolJob> m_ClosingThreadList = new List<ThreadPoolJob>();
        private bool m_Active = true;

        public ThreadPool()
        {
            m_ManagerThread = new Thread(ManagerRun);
            m_ManagerThread.Name = "ThreadPool Manager";
            m_ManagerThread.Priority = ThreadPriority.Lowest;
            m_ActiveThreadList.Add(new ThreadPoolJob(this));
        }

        public IThreadPoolResult<TResult> AddJob<TResult>(Func<TResult> i_Job)
        {
            Func<object> castedFunc = () => { return i_Job(); };
            ThreadPoolJobTask task = new ThreadPoolJobTask(castedFunc);
            lock (m_ThreadPoolTaskListChangeHandle)
            {
                m_Tasks.Add(task);
                m_Tasks.InsertionSort(ThreadPoolJobTaskDescendingComparer.Innstance);
            }
            return new ThreadPoolResult<TResult>(task);
        }

        private void ManagerRun()
        {
            while(m_Active)
            {
                int newThreadCount = GetNewThreadCount();
                int requiredExtraJobs = MaintainActiveThreadList(newThreadCount);
                for (int i = 0; i < requiredExtraJobs; ++i)
                {
                    m_ActiveThreadList.Add(new ThreadPoolJob(this));
                }
                MaintainClosingThreadList();
                
                Thread.Sleep(200);
            }

            //force close
            int size = m_ActiveThreadList.Count;
            for( int i = 0; i < size; ++i)
            {
                m_ActiveThreadList[i].Abort();
            }
            m_ActiveThreadList.Clear();

            size = m_ClosingThreadList.Count;
            for (int i = 0; i < size; ++i)
            {
                m_ClosingThreadList[i].Abort();
            }
            m_ClosingThreadList.Clear();
        }


        /// <summary>
        /// Gets the ideal estimated thread count for pending tasks.
        /// </summary>
        /// <returns></returns>
        private int GetNewThreadCount()
        {
            int lateTaskCount = 0;
            lock (m_ThreadPoolTaskListChangeHandle)
            {
                DateTime now = DateTime.UtcNow;
                int size = m_ClosingThreadList.Count;
                ThreadPoolJobTask task;
                for (int i = 0; i < size; ++i)
                {
                    task = m_ClosingThreadList[i].Task;
                    if (task != null)
                    {
                        if ((task.ExpectedRunTimestamp - now).TotalMilliseconds > 0)
                        {
                            ++lateTaskCount;
                        }
                    }
                }
            }
            lateTaskCount /= 3;
            return lateTaskCount <= 0 ? 1 : lateTaskCount;
        }

        private int MaintainActiveThreadList(int i_RequiredReadyJobs)
        {
            int ExtraThreadCount = i_RequiredReadyJobs;
            DateTime now = DateTime.UtcNow;
            for(int i = m_ActiveThreadList.Count - 1; i >= 0; --i)
            {
                ThreadPoolJob threadJob = m_ActiveThreadList[i];
                lock (threadJob.ThreadTaskChangeHandle)
                {
                    if (threadJob.Task == null)
                    {
                        if(ExtraThreadCount > 0)
                        {
                            --ExtraThreadCount;
                        }
                        else
                        {
                            threadJob.Close();
                            m_ActiveThreadList.RemoveAt(i);
                            m_ClosingThreadList.Add(threadJob);
                        }
                    }
                    else if((threadJob.Task.RunTimestamp + threadJob.Task.MaxRunTime - now).TotalMilliseconds > 0)
                    {
                        threadJob.Abort();
                        m_ActiveThreadList.RemoveAt(i);
                    }
                }
            }
            return ExtraThreadCount;
        }

        private void MaintainClosingThreadList()
        {
            DateTime now = DateTime.UtcNow;
            for (int i = m_ClosingThreadList.Count - 1; i >= 0; --i)
            {
                ThreadPoolJob threadJob = m_ClosingThreadList[i];
                if (threadJob.Task == null)
                {
                    m_ClosingThreadList.RemoveAt(i);
                }
                else if ((threadJob.Task.RunTimestamp + threadJob.Task.MaxRunTime - now).TotalMilliseconds > 0)
                {
                    threadJob.Abort();
                    m_ClosingThreadList.RemoveAt(i);
                }
            }
        }

        public void Dispose()
        {
            m_Active = false;
        }
    }
}
