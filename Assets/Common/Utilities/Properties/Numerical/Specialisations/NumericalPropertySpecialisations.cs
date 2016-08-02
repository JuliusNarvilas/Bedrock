﻿using Utilities.Properties.Numerical.Data;

namespace Utilities.Properties.Numerical.Specialisations
{

    public class NumericalPropertyInt<TContext, TModifierReader> : NumericalProperty<int, TContext, TModifierReader> where TModifierReader : INumericalPropertyModifierReader<int>
    {
        public NumericalPropertyInt() : base(new NumericalPropertyIntData(0))
        { }
        public NumericalPropertyInt(int i_Value) : base(new NumericalPropertyIntData(i_Value))
        { }
    }
    public class NumericalPropertyInt<TContext> : NumericalPropertyInt<TContext, INumericalPropertyModifierReader<int>>
    { }

    public class NumericalPropertyFloat<TContext, TModifierReader> : NumericalProperty<float, TContext, TModifierReader> where TModifierReader : INumericalPropertyModifierReader<float>
    {
        public NumericalPropertyFloat() : base(new NumericalPropertyFloatData(0.0f))
        { }
        public NumericalPropertyFloat(int i_Value) : base(new NumericalPropertyFloatData(i_Value))
        { }
    }
    public class NumericalPropertyFloat<TContext> : NumericalPropertyFloat<TContext, INumericalPropertyModifierReader<float>>
    { }

    public class NumericalPropertyLong<TContext, TModifierReader> : NumericalProperty<long, TContext, TModifierReader> where TModifierReader : INumericalPropertyModifierReader<long>
    {
        public NumericalPropertyLong() : base(new NumericalPropertyLongData(0))
        { }
        public NumericalPropertyLong(long i_Value) : base(new NumericalPropertyLongData(i_Value))
        { }
    }
    public class NumericalPropertyLong<TContext> : NumericalPropertyLong<TContext, INumericalPropertyModifierReader<long>>
    { }

    public class NumericalPropertyDouble<TContext, TModifierReader> : NumericalProperty<double, TContext, TModifierReader> where TModifierReader : INumericalPropertyModifierReader<double>
    {
        public NumericalPropertyDouble() : base(new NumericalPropertyDoubleData(0.0))
        { }
        public NumericalPropertyDouble(double i_Value) : base(new NumericalPropertyDoubleData(i_Value))
        { }
    }
    public class NumericalPropertyDouble<TContext> : NumericalPropertyDouble<TContext, INumericalPropertyModifierReader<double>>
    { }

}
