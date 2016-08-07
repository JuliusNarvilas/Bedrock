using System;
using System.Diagnostics;

namespace Common.Grid
{
    public struct Grid2DPosition
    {
        public readonly int X;
        public readonly int Y;

        public Grid2DPosition(int i_IndexX, int i_IndexY)
        {
            Debug.Assert(i_IndexX >= 0);
            Debug.Assert(i_IndexY >= 0);

            X = i_IndexX;
            Y = i_IndexY;
        }

        public override string ToString()
        {
            return "{ X: " + X + "; Y: " + Y + "}";
        }
    }
}
