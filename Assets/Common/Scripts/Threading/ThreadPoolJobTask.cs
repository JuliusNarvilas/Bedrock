using System;
using System.Collections.Generic;
using System.Threading;

namespace Common.Threading
{
    /// <summary>
    /// Generic Task to be run async
    /// </summary>
    public class ThreadPoolJobTask
    {
        public ThreadedTaskState State = ThreadedTaskState.InProgress;
        public Exception Exception = null;
        public readonly Action RunFunc = null;
        public readonly TimeSpan MaxRunTime;
        public readonly DateTime ExpectedRunTimestamp;
        public DateTime RunStartTimestamp = DateTime.MaxValue;

        public ThreadPoolJobTask(Action i_RunFunc, ThreadPriority i_Priority = ThreadPriority.Normal)
        {
            RunFunc = i_RunFunc;
            MaxRunTime = new TimeSpan(0, 0, 5, 0);
            ExpectedRunTimestamp = GetExpectedEndTime(i_Priority);
        }
        public ThreadPoolJobTask(Action i_RunFunc, TimeSpan i_MaxRunTime, ThreadPriority i_Priority = ThreadPriority.Normal)
        {
            RunFunc = i_RunFunc;
            MaxRunTime = i_MaxRunTime;
            ExpectedRunTimestamp = GetExpectedEndTime(i_Priority);
        }

        private static DateTime GetExpectedEndTime(ThreadPriority i_Priority)
        {
            switch (i_Priority)
            {
                case ThreadPriority.Lowest:
                    return DateTime.UtcNow.AddSeconds(4);
                case ThreadPriority.BelowNormal:
                    return DateTime.UtcNow.AddSeconds(3);
                case ThreadPriority.Normal:
                    return DateTime.UtcNow.AddSeconds(2);
                case ThreadPriority.AboveNormal:
                    return DateTime.UtcNow.AddSeconds(1);
                case ThreadPriority.Highest:
                    return DateTime.UtcNow.AddMilliseconds(500);
            }
            return DateTime.UtcNow.AddSeconds(4);
        }


        public class TerminationComparer : IComparer<ThreadPoolJobTask>
        {
            private int m_Multiplier = 1;

            private TerminationComparer(bool i_Ascending)
            {
                m_Multiplier = i_Ascending ? 1 : -1;
            }

            public int Compare(ThreadPoolJobTask i_A, ThreadPoolJobTask i_B)
            {
                return i_A.ExpectedRunTimestamp.CompareTo(i_B.ExpectedRunTimestamp) * m_Multiplier;
            }
            
            public static TerminationComparer Ascending = new TerminationComparer(true);
            public static TerminationComparer Descending = new TerminationComparer(false);
        }
    }
}
