using System.Diagnostics;

namespace Common.Grid
{
    public struct Grid2DPosition
    {
        public readonly ushort X;
        public readonly ushort Y;

        public Grid2DPosition(int i_IndexX = 0, int i_IndexY = 0)
        {
            Log.DebugAssert(
                MathHelper.InRange(0, ushort.MaxValue, i_IndexX),
                "X index out of range"
                );
            Log.DebugAssert(
                MathHelper.InRange(0, ushort.MaxValue, i_IndexY),
                "Y index out of range"
                );

            X = (ushort)i_IndexX;
            Y = (ushort)i_IndexY;
        }

        public override string ToString()
        {
            return string.Format("{ X: {0}; Y: {1}}", X, Y);
        }
    }
}
