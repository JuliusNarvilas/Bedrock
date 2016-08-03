
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
    public class NumericalPropertyFloatData : INumericalPropertyData<float>
    {
        protected float m_Value;

        public NumericalPropertyFloatData()
        {
            m_Value = 0.0f;
        }

        public NumericalPropertyFloatData(float i_Value)
        {
            m_Value = i_Value;
        }


        public float Get()
        {
            return m_Value;
        }

        public void Set(float i_Value)
        {
            m_Value = i_Value;
        }

        public void Add(float i_Value)
        {
#if (NUMERICAL_PROPERTY_DATA_VALIDATION)
            float temp = m_Value + i_Value;
            Debug.Assert(
                ((temp >= m_Value) || (i_Value < 0)) &&
                ((temp < m_Value) || (i_Value >= 0)),
                "Number overflow."
                );
#endif
            m_Value += i_Value;
        }

        public void Substract(float i_Value)
        {
#if (NUMERICAL_PROPERTY_DATA_VALIDATION)
            float temp = m_Value - i_Value;
            Debug.Assert(
                ((temp >= m_Value) || (i_Value > 0)) &&
                ((temp < m_Value) || (i_Value <= 0)),
                "Number overflow."
                );
#endif
            m_Value -= i_Value;
        }

        public void Multiply(float i_Value)
        {
#if (NUMERICAL_PROPERTY_DATA_VALIDATION)
            float temp = m_Value * i_Value;
            Debug.Assert(
                ((temp >= m_Value) || (i_Value < 0)) &&
                ((temp < m_Value) || (i_Value >= 0)),
                "Number overflow."
                );
#endif
            m_Value *= i_Value;
        }

        public void Divide(float i_Value)
        {
#if (NUMERICAL_PROPERTY_DATA_VALIDATION)
            Debug.Assert(
                i_Value != 0.0f,
                "Division by 0.0f."
                );
#endif
            m_Value /= i_Value;
        }

        public INumericalPropertyData<float> CreateZero()
        {
            return new NumericalPropertyFloatData(0.0f);
        }

        public float AdditiveInverse()
        {
            return -m_Value;
        }

        public float MultiplicativeInverse()
        {
            return 1.0f / m_Value;
        }

        public void ToZero()
        {
            m_Value = 0.0f;
        }

        public int CompareTo(float i_Other)
        {
            return m_Value.CompareTo(i_Other);
        }
    }
}
