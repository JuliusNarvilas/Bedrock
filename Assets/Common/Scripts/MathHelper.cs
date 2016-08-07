using System;
using System.Diagnostics;

namespace Common
{
    public static class MathHelper
    {
        public const float DEFAULT_FLOATING_POINT_EQUAL_ERROR_MARGIN = 0.000001f;

        public static bool EqualsWithMarginF(float i_NumberA, float i_NumberB, float i_ErrorMargin = DEFAULT_FLOATING_POINT_EQUAL_ERROR_MARGIN)
        {
            Debug.Assert(i_ErrorMargin >= 0.0f, "Error margin is less than 0.");
            return Math.Abs(i_NumberA - i_NumberB) <= i_ErrorMargin;
        }

        public static bool EqualsF(this float i_NumberA, float i_NumberB, float i_ErrorMargin = DEFAULT_FLOATING_POINT_EQUAL_ERROR_MARGIN)
        {
            return EqualsWithMarginF(i_NumberA, i_NumberB, i_ErrorMargin);
        }

        public static bool EqualsWithMargin(double i_NumberA, double i_NumberB, float i_ErrorMargin = DEFAULT_FLOATING_POINT_EQUAL_ERROR_MARGIN)
        {
            Debug.Assert(i_ErrorMargin >= 0.0, "Error margin is less than 0.");
            return Math.Abs(i_NumberA - i_NumberB) <= i_ErrorMargin;
        }

        public static bool Equals(this double i_NumberA, double i_NumberB, float i_ErrorMargin = DEFAULT_FLOATING_POINT_EQUAL_ERROR_MARGIN)
        {
            return EqualsWithMargin(i_NumberA, i_NumberB, i_ErrorMargin);
        }


        public static bool InRange(int i_Min, int i_Max, int i_Data)
        {
            return (i_Data >= i_Min) && (i_Data <= i_Max);
        }

        public static bool InRangeF(float i_Min, float i_Max, float i_Data)
        {
            return (i_Data >= i_Min) && (i_Data <= i_Max);
        }
        
        public static bool InRange(double i_Min, double i_Max, double i_Data)
        {
            return (i_Data >= i_Min) && (i_Data <= i_Max);
        }

        public static bool IsInRange(this int i_Data, int i_Min, int i_Max)
        {
            return InRange(i_Min, i_Max, i_Data);
        }

        public static bool IsInRangeF(this float i_Data, float i_Min, float i_Max)
        {
            return InRange(i_Min, i_Max, i_Data);
        }

        public static bool IsInRange(this double i_Data, double i_Min, double i_Max)
        {
            return InRange(i_Min, i_Max, i_Data);
        }
    }
}
