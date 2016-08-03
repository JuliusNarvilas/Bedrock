
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
    public class NumericalPropertyDoubleData : INumericalPropertyData<double>
    {
        protected double m_Value;

        public NumericalPropertyDoubleData()
        {
            m_Value = 0.0;
        }

        public NumericalPropertyDoubleData(double i_Value)
        {
            m_Value = i_Value;
        }


        public double Get()
        {
            return m_Value;
        }

        public void Set(double i_Value)
        {
            m_Value = i_Value;
        }

        public void Add(double i_Value)
        {
#if (NUMERICAL_PROPERTY_DATA_VALIDATION)
            double temp = m_Value + i_Value;
            Debug.Assert(
                ((temp >= m_Value) || (i_Value < 0)) &&
                ((temp < m_Value) || (i_Value >= 0)),
                "Number overflow."
                );
#endif
            m_Value += i_Value;
        }

        public void Substract(double i_Value)
        {
#if (NUMERICAL_PROPERTY_DATA_VALIDATION)
            double temp = m_Value - i_Value;
            Debug.Assert(
                ((temp >= m_Value) || (i_Value > 0)) &&
                ((temp < m_Value) || (i_Value <= 0)),
                "Number overflow."
                );
#endif
            m_Value -= i_Value;
        }

        public void Multiply(double i_Value)
        {
#if (NUMERICAL_PROPERTY_DATA_VALIDATION)
            double temp = m_Value * i_Value;
            Debug.Assert(
                ((temp >= m_Value) || (i_Value < 0)) &&
                ((temp < m_Value) || (i_Value >= 0)),
                "Number overflow."
                );
#endif
            m_Value *= i_Value;
        }

        public void Divide(double i_Value)
        {
#if (NUMERICAL_PROPERTY_DATA_VALIDATION)
            Debug.Assert(
                i_Value != 0.0,
                "Division by 0.0."
                );
#endif
            m_Value /= i_Value;
        }

        public INumericalPropertyData<double> CreateZero()
        {
            return new NumericalPropertyDoubleData(0.0);
        }

        public double AdditiveInverse()
        {
            return -m_Value;
        }

        public double MultiplicativeInverse()
        {
            return 1.0 / m_Value;
        }

        public void ToZero()
        {
            m_Value = 0.0;
        }

        public int CompareTo(double i_Other)
        {
            return m_Value.CompareTo(i_Other);
        }
    }
}
