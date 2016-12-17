using System.Collections.Generic;

namespace Common.Grid
{
    /// <summary>
    /// Class for reprisenting <see cref="Grid2D{TTerrainData, TContext}"/> tile pathing data
    /// </summary>
    public class GridPathElement<TTerrain, TPosition, TContext> where TTerrain : GridTerrain<TContext>
    {
        /// <summary>
        /// The tile data that is accessible to users.
        /// </summary>
        public Grid2DTile<TTerrain, TPosition, TContext> Tile;
        /// <summary>
        /// The approximate distance to the destination (must not be an underestimate).
        /// </summary>
        public int HeuristicDistance;
        /// <summary>
        /// The path cost so far.
        /// </summary>
        public float PathCost;
        /// <summary>
        /// The estimated path cost to the destination.
        /// </summary>
        public float FValue;
        /// <summary>
        /// Current pathing inspection state.
        /// </summary>
        public GridPathingState PathingState = GridPathingState.New;

        /// <summary>
        /// Initializes a new instance of the <see cref="GridElement"/> class.
        /// </summary>
        public GridPathElement()
        {}

        /// <summary>
        /// Method for cleaning up pathing data back to default state.
        /// </summary>
        public void Reset()
        {
            HeuristicDistance = 0;
            PathCost = 0;
            FValue = 0;
            PathingState = GridPathingState.New;
        }

        protected class FValueComparer : IComparer<GridPathElement<TTerrain, TPosition, TContext>>
        {
            private int m_Modifier;

            private FValueComparer(bool i_Ascending)
            {
                m_Modifier = i_Ascending ? 1 : -1;
            }

            public int Compare(GridPathElement<TTerrain, TPosition, TContext> i_A, GridPathElement<TTerrain, TPosition, TContext> i_B)
            {
                return i_A.FValue.CompareTo(i_B.FValue) * m_Modifier;
            }

            public static FValueComparer Ascending = new FValueComparer(true);
            public static FValueComparer Descending = new FValueComparer(false);
        }
    }
}
