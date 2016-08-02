using System;

namespace Utilities.Properties.Numerical.Data
{
    public interface INumericalPropertyData<TNumerical> : IComparable<TNumerical>
    {
        TNumerical Get();
        void Set(TNumerical i_Value);
        void Add(TNumerical i_Value);
        void Substract(TNumerical i_Value);
        void Multiply(TNumerical i_Value);
        void Divide(TNumerical i_Value);
        TNumerical AdditiveInverse();
        TNumerical MultiplicativeInverse();
        void ToZero();
        INumericalPropertyData<TNumerical> CreateZero();
    }

    public static class NumericalPropertyDataExtensions
    {
        public static void Set<T>(this INumericalPropertyData<T> i_Target, INumericalPropertyData<T> i_Value)
        {
            i_Target.Set(i_Value.Get());
        }
        public static void Add<T>(this INumericalPropertyData<T> i_Target, INumericalPropertyData<T> i_Value)
        {
            i_Target.Add(i_Value.Get());
        }
        public static void Substract<T>(this INumericalPropertyData<T> i_Target, INumericalPropertyData<T> i_Value)
        {
            i_Target.Substract(i_Value.Get());
        }
        public static void Multiply<T>(this INumericalPropertyData<T> i_Target, INumericalPropertyData<T> i_Value)
        {
            i_Target.Multiply(i_Value.Get());
        }
        public static void Divide<T>(this INumericalPropertyData<T> i_Target, INumericalPropertyData<T> i_Value)
        {
            i_Target.Divide(i_Value.Get());
        }
    }
    
}
