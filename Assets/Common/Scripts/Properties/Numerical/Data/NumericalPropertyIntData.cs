
#if (NUMERICAL_PROPERTY_DATA_VALIDATION_OFF)
#undef NUMERICAL_PROPERTY_DATA_VALIDATION

#elif (!NUMERICAL_PROPERTY_DATA_VALIDATION)

#if (DEBUG)
#define NUMERICAL_PROPERTY_DATA_VALIDATION
#endif

#endif

using System.Diagnostics;

namespace Common.Properties.Numerical.Data
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
#if (NUMERICAL_PROPERTY_DATA_VALIDATION)
            int temp = m_Value + i_Value;
            Debug.Assert(
                ((temp >= m_Value) || (i_Value < 0)) &&
                ((temp < m_Value) || (i_Value >= 0)),
                "Number overflow."
                );
#endif
            m_Value += i_Value;
        }

        public void Substract(int i_Value)
        {
#if (NUMERICAL_PROPERTY_DATA_VALIDATION)
            int temp = m_Value - i_Value;
            Debug.Assert(
                ((temp >= m_Value) || (i_Value > 0)) &&
                ((temp < m_Value) || (i_Value >= 0)),
                "Number overflow."
                );
#endif
            m_Value -= i_Value;
        }

        public void Multiply(int i_Value)
        {
#if (NUMERICAL_PROPERTY_DATA_VALIDATION)
            int temp = m_Value * i_Value;
            Debug.Assert(
                ((temp >= m_Value) || (i_Value < 0)) &&
                ((temp < m_Value) || (i_Value >= 0)),
                "Number overflow."
                );
#endif
            m_Value *= i_Value;
        }

        public void Divide(int i_Value)
        {
#if (NUMERICAL_PROPERTY_DATA_VALIDATION)
            Debug.Assert(
                i_Value != 0,
                "Division by 0."
                );
#endif
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
