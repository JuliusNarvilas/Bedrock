using System;
using System.Diagnostics;
using System.Threading;

namespace Common.Properties
{
    public class MutexPropertyProvider<T, TProperty> where TProperty : Property<T>
    {
        public readonly TProperty Property;
        protected readonly Mutex m_Mutex;

        public MutexPropertyProvider(TProperty i_Property)
        {
            Property = i_Property;
            m_Mutex = new Mutex();
        }

        public MutexPropertyLock GetMutexPropertyLock()
        {
            return new MutexPropertyLock(this);
        }

        public class MutexPropertyLock : IDisposable
        {
            public readonly TProperty Property;
            protected readonly Mutex m_Mutex;
            protected bool m_Disposed;

            public MutexPropertyLock(MutexPropertyProvider<T, TProperty> i_MutexProperty)
            {
                Debug.Assert(i_MutexProperty != null, "Invalid null property.");
                m_Mutex = i_MutexProperty.m_Mutex;
                m_Disposed = false;
                Property = i_MutexProperty.Property;
                m_Mutex.WaitOne();
            }

            ~MutexPropertyLock()
            {
                if (!m_Disposed)
                {
                    Dispose();
                }
            }

            public void Dispose()
            {
                if (!m_Disposed)
                {
                    m_Disposed = true;
                    m_Mutex.ReleaseMutex();
                }
            }
        }
    }

    public class MutexPropertyProvider<T> : MutexPropertyProvider<T, Property<T>>
    {
        public MutexPropertyProvider(Property<T> i_Property) : base(i_Property)
        { }
    }
}
