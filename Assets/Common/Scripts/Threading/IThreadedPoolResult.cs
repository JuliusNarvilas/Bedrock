using System;

namespace Common.Threading
{
    public interface IThreadPoolResult<TResult>
    {
        ThreadedJobState State
        {
            get;
        }

        Exception GetException();

        TResult GetResult();
    }
}
