using System.Collections.Generic;

namespace Common.Grid
{
    public abstract class Grid2D<TTile, TTerrain, TPosition, TContext>
        where TTile : Grid2DTile<TTerrain, TPosition, TContext>
        where TTerrain : GridTerrain<TContext>
    {
        protected readonly object m_PathingLockHandle = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid2D{TGridTileData, TTerrainData}"/> class.
        /// </summary>
        public Grid2D()
        { }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <param name="i_Start">The start position.</param>
        /// <param name="i_End">The end position.</param>
        /// <returns></returns>
        public Grid2DPath<TTerrain, TPosition, TContext> GetPath(TPosition i_Start, TPosition i_End, TContext i_Context)
        {
            return new Grid2DPath<TTerrain, TPosition, TContext>(this, i_Start, i_End, i_Context);
        }

        public abstract TTile GetTile(TPosition i_Position);

        public abstract int GetHeuristicDistance(TPosition i_From, TPosition i_To);

        protected abstract void GetConnected(TPosition i_Position, List<TTile> o_ConnectedElements);
    }
}
