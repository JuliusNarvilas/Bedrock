
using System.Diagnostics;

namespace Utilities.Properties.Numerical.Data
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
            Debug.Assert(
                (((m_Value + i_Value) >= m_Value) || (i_Value < 0)) &&
                (((m_Value + i_Value) < m_Value) || (i_Value >= 0)),
                "Number overflow."
                );
            m_Value += i_Value;
        }

        public void Substract(float i_Value)
        {
            Debug.Assert(
                (((m_Value + i_Value) >= m_Value) || (i_Value < 0)) &&
                (((m_Value + i_Value) < m_Value) || (i_Value >= 0)),
                "Number overflow."
                );
            m_Value -= i_Value;
        }

        public void Multiply(float i_Value)
        {
            Debug.Assert(
                (((m_Value + i_Value) >= m_Value) || (i_Value < 0)) &&
                (((m_Value + i_Value) < m_Value) || (i_Value >= 0)),
                "Number overflow."
                );
            m_Value *= i_Value;
        }

        public void Divide(float i_Value)
        {
            Debug.Assert(
                i_Value != 0.0f,
                "Division by 0.0f."
                );
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
