
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
    /// 
    /// </summary>
    /// <typeparam name="TNumerical">The type of the numerical.</typeparam>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <typeparam name="TModifierReader">The type of the modifier reader.</typeparam>
    /// <seealso cref="Common.Properties.Numerical.ExhaustibleNumericalProperty{TNumerical, TContext, TModifierReader}" />
    public class ObservableExhaustibleNumericalProperty<TNumerical, TContext, TModifierReader> : ExhaustibleNumericalProperty<TNumerical, TContext, TModifierReader> where TModifierReader : INumericalPropertyModifierReader<TNumerical> where TNumerical : struct
    {
        public event NumericalPropertyEventHandler<TNumerical, TContext, TModifierReader> ChangeSubscription;

        public ObservableExhaustibleNumericalProperty(INumericalPropertyData<TNumerical> i_Value) : base(i_Value)
        { }

        protected override void UpdateInternal(ENumericalPropertyChangeType i_ChangeTypeMask, TContext i_Context)
        {
            if ((m_Modifiers.Count > 0) || (ChangeSubscription != null))
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
                FireChangeEvent(ref eventData);
            }
            else
            {
                UpdateExhaustableModifiedValue();
            }
        }

        protected void FireChangeEvent(ref NumericalPropertyChangeEventStruct<TNumerical, TContext, TModifierReader> i_EventData)
        {
#if (NUMERICAL_PROPERTY_LOGGING)
            Debug.Log("Numerical property change event fired.");
#endif
            if (ChangeSubscription != null)
            {
                ChangeSubscription(ref i_EventData);
            }
        }
    }
}
