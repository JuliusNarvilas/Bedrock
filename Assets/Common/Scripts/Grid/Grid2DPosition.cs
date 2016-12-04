
namespace Common.Grid
{
    public struct Grid2DPosition
    {
        public readonly int X;
        public readonly int Y;

        public Grid2DPosition(int i_IndexX = 0, int i_IndexY = 0)
        {
            X = i_IndexX;
            Y = i_IndexY;
        }

        public override string ToString()
        {
            return string.Format("{ X: {0}; Y: {1}}", X, Y);
        }
    }
}
