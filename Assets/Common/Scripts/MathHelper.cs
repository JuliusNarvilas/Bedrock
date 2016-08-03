using System;
using System.Diagnostics;

namespace Common
{
    public static class MathHelper
    {
        public const float DEFAULT_FLOATING_POINT_EQUAL_ERROR_MARGIN = 0.00001f;

        public static bool EqualsF(float i_NumberA, float i_NumberB, float i_ErrorMargin = DEFAULT_FLOATING_POINT_EQUAL_ERROR_MARGIN)
        {
            Debug.Assert(i_ErrorMargin >= 0.0f, "Error margin is less than 0.");
            return Math.Abs(i_NumberA - i_NumberB) <= i_ErrorMargin;
        }

        public static bool EqualsWithMarginF(this float i_NumberA, float i_NumberB, float i_ErrorMargin = DEFAULT_FLOATING_POINT_EQUAL_ERROR_MARGIN)
        {
            return EqualsF(i_NumberA, i_NumberB, i_ErrorMargin);
        }

        public static bool Equals(double i_NumberA, double i_NumberB, float i_ErrorMargin = DEFAULT_FLOATING_POINT_EQUAL_ERROR_MARGIN)
        {
            Debug.Assert(i_ErrorMargin >= 0.0, "Error margin is less than 0.");
            return Math.Abs(i_NumberA - i_NumberB) <= i_ErrorMargin;
        }

        public static bool EqualsWithMargin(this double i_NumberA, double i_NumberB, float i_ErrorMargin = DEFAULT_FLOATING_POINT_EQUAL_ERROR_MARGIN)
        {
            return Equals(i_NumberA, i_NumberB, i_ErrorMargin);
        }

    }
}
