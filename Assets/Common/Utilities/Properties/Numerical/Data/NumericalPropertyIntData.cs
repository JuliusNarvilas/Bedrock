
using System.Diagnostics;

namespace Utilities.Properties.Numerical.Data
{
    public class NumericalPropertyIntData : INumericalPropertyData<int>
    {
        protected int m_Value;

        public NumericalPropertyIntData()
        {
            m_Value = 0;
        }

        public NumericalPropertyIntData(int i_Value)
        {
            m_Value = i_Value;
        }


        public int Get()
        {
            return m_Value;
        }

        public void Set(int i_Value)
        {
            m_Value = i_Value;
        }

        public void Add(int i_Value)
        {
            Debug.Assert(
                (((m_Value + i_Value) >= m_Value) || (i_Value < 0)) &&
                (((m_Value + i_Value) < m_Value) || (i_Value >= 0)),
                "Number overflow."
                );
            m_Value += i_Value;
        }

        public void Substract(int i_Value)
        {
            Debug.Assert(
                (((m_Value + i_Value) >= m_Value) || (i_Value < 0)) &&
                (((m_Value + i_Value) < m_Value) || (i_Value >= 0)),
                "Number overflow."
                );
            m_Value -= i_Value;
        }

        public void Multiply(int i_Value)
        {
            Debug.Assert(
                (((m_Value + i_Value) >= m_Value) || (i_Value < 0)) &&
                (((m_Value + i_Value) < m_Value) || (i_Value >= 0)),
                "Number overflow."
                );
            m_Value *= i_Value;
        }

        public void Divide(int i_Value)
        {
            Debug.Assert(
                i_Value != 0,
                "Division by 0."
                );
            m_Value /= i_Value;
        }

        public INumericalPropertyData<int> CreateZero()
        {
            return new NumericalPropertyIntData(0);
        }

        public int AdditiveInverse()
        {
            return -m_Value;
        }

        public int MultiplicativeInverse()
        {
            //int values can not represent multiplicative inverse
            return 0;
        }

        public void ToZero()
        {
            m_Value = 0;
        }

        public int CompareTo(int i_Other)
        {
            return m_Value.CompareTo(i_Other);
        }
    }
}
