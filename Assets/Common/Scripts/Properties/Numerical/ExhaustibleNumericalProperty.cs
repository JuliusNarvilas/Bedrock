
#if (NUMERICAL_PROPERTY_LOGGING_OFF)
#undef NUMERICAL_PROPERTY_LOGGING

#elif (!NUMERICAL_PROPERTY_LOGGING)

#if (DEBUG)
#define NUMERICAL_PROPERTY_LOGGING
#endif

#endif

using Common.Properties.Numerical.Data;
using UnityEngine;

namespace Common.Properties.Numerical
{
    /// <summary>
    /// An implementation of numerical property for representing values that can be depleated (like health).
    /// </summary>
    /// <remarks>
    /// This property can be depleated by an amount within the range [0; base value + modifiers].
    /// Overall depletion amount can not influence the property into the negative value or be less than 0.
    /// Even if base value + modifiers result in a negative number, the depletion could only amount to 0.
    /// </remarks>
    /// <typeparam name="TNumerical">Numerical property type.</typeparam>
    /// <typeparam name="TContext">Change context type.</typeparam>
    /// <typeparam name="TModifierReader">Reader interface for a modifier.</typeparam>
    /// <seealso cref="Common.Properties.Numerical.NumericalProperty{TNumerical, TContext, TModifierReader}" />
    public class ExhaustibleNumericalProperty<TNumerical, TContext, TModifierReader> :
        NumericalProperty<TNumerical, TContext, TModifierReader>
        where TModifierReader : INumericalPropertyModifierReader<TNumerical>
            where TNumerical : struct
    {
        /// <summary>
        /// The depletion amount tracker.
        /// </summary>
        protected TNumerical m_Depletion;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExhaustibleNumericalProperty{TNumerical, TContext, TModifierReader}"/> class.
        /// </summary>
        /// <param name="i_Value">Initial value.</param>
        public ExhaustibleNumericalProperty(INumericalPropertyData<TNumerical> i_Value) : base(i_Value)
        {
            m_Depletion = m_DataZero.Get();
        }

        public TNumerical GetDepletion()
        {
            return m_Depletion;
        }

        public TNumerical GetMax()
        {
            m_DataZero.Set(m_BaseValue);
            m_DataZero.Add(m_FinalModifier);
            TNumerical max = m_DataZero.Get();
            m_DataZero.ToZero();
            return max;
        }

        public void Deplete(TNumerical i_Depletion, TContext i_Context = default(TContext))
        {
            m_DataZero.Set(m_Depletion);
            m_DataZero.Add(i_Depletion);
            m_Depletion = m_DataZero.Get();
            m_DataZero.ToZero();

            ENumericalPropertyChangeType changeTypeMask = ENumericalPropertyChangeType.Deplete;
            UpdateInternal(changeTypeMask, i_Context);
        }

        public void Restore(TNumerical i_Restore, TContext i_Context = default(TContext))
        {
            m_DataZero.Set(i_Restore);
            m_DataZero.AdditiveInverse();
            Deplete(m_DataZero.Get(), i_Context);
        }

        protected void UpdateExhaustableModifiedValue()
        {
            //clamp depletion to be greater or equal to 0
            int zeroCompareDepletion = m_DataZero.CompareTo(m_Depletion);
            if(zeroCompareDepletion > 0)
            {
                m_Depletion = m_DataZero.Get();
            }
            TNumerical zero = m_DataZero.Get();
            m_DataZero.Set(m_BaseValue);
            m_DataZero.Add(m_FinalModifier);
            //clamp depletion to end at max
            int maxCompareDepleation = m_DataZero.CompareTo(m_Depletion);
            if(maxCompareDepleation < 0)
            {
                //clamp depletion to 0 if max is negative
                int maxCompareZero = m_DataZero.CompareTo(zero);
                m_Depletion = (maxCompareZero <= 0) ? zero : m_DataZero.Get();
            }
            m_DataZero.Substract(m_Depletion);
            m_Value = m_DataZero.Get();
            m_DataZero.ToZero();

#if (NUMERICAL_PROPERTY_LOGGING)
            Debug.Log(string.Format("Numerical property value updated to {0}: {1} + {2} - {3}.", m_Value, m_BaseValue, m_FinalModifier, m_Depletion));
#endif
        }

        protected override void UpdateInternal(ENumericalPropertyChangeType i_ChangeTypeMask, TContext i_Context)
        {
            if ((m_Modifiers.Count > 0))
            {
                if (!m_Updating)
                {
                    m_Updating = true;
                }
                else
                {
                    i_ChangeTypeMask |= ENumericalPropertyChangeType.NestedUpdate;
                }

                NumericalPropertyChangeEventStruct<TNumerical, TContext, TModifierReader> eventData =
                    new NumericalPropertyChangeEventStruct<TNumerical, TContext, TModifierReader>(
                        this,
                        i_ChangeTypeMask,
                        i_Context
                        );

                UpdateModifiers(ref eventData);
                m_Depletion = eventData.NewDepletion;
                UpdateExhaustableModifiedValue();

                if ((i_ChangeTypeMask & ENumericalPropertyChangeType.NestedUpdate) == ENumericalPropertyChangeType.None)
                {
                    m_Updating = false;
                }
            }
            else
            {
                UpdateExhaustableModifiedValue();
            }
        }
    }
}
