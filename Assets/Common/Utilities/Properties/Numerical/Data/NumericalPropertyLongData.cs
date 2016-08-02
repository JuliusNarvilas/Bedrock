
using System.Diagnostics;

namespace Utilities.Properties.Numerical.Data
{
    public class NumericalPropertyLongData : INumericalPropertyData<long>
    {
        protected long m_Value;

        public NumericalPropertyLongData()
        {
            m_Value = 0;
        }

        public NumericalPropertyLongData(long i_Value)
        {
            m_Value = i_Value;
        }


        public long Get()
        {
            return m_Value;
        }

        public void Set(long i_Value)
        {
            m_Value = i_Value;
        }

        public void Add(long i_Value)
        {
            Debug.Assert(
                (((m_Value + i_Value) >= m_Value) || (i_Value < 0)) &&
                (((m_Value + i_Value) < m_Value) || (i_Value >= 0)),
                "Number overflow."
                );
            m_Value += i_Value;
        }

        public void Substract(long i_Value)
        {
            Debug.Assert(
                (((m_Value + i_Value) >= m_Value) || (i_Value < 0)) &&
                (((m_Value + i_Value) < m_Value) || (i_Value >= 0)),
                "Number overflow."
                );
            m_Value -= i_Value;
        }

        public void Multiply(long i_Value)
        {
            Debug.Assert(
                (((m_Value + i_Value) >= m_Value) || (i_Value < 0)) &&
                (((m_Value + i_Value) < m_Value) || (i_Value >= 0)),
                "Number overflow."
                );
            m_Value *= i_Value;
        }

        public void Divide(long i_Value)
        {
            Debug.Assert(
                i_Value != 0,
                "Division by 0."
                );
            m_Value /= i_Value;
        }

        public INumericalPropertyData<long> CreateZero()
        {
            return new NumericalPropertyLongData(0);
        }

        public long AdditiveInverse()
        {
            return -m_Value;
        }

        public long MultiplicativeInverse()
        {
            //long values can not represent multiplicative inverse
            return 0L;
        }

        public void ToZero()
        {
            m_Value = 0L;
        }

        public int CompareTo(long i_Other)
        {
            return m_Value.CompareTo(i_Other);
        }
    }
}
