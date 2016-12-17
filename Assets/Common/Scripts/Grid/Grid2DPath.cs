using System.Collections.Generic;

namespace Common.Grid
{
    public enum GridPathingState
    {
        New,
        Opened,
        Closed
    }

    public class GridPathResult<TTile, TTerrain, TPosition, TContext>
        where TTile : Grid2DTile<TTerrain, TPosition, TContext>
        where TTerrain : GridTerrain<TContext>
    {
        public readonly float PathCost;
        public readonly List<TTile> Path;

        public readonly TPosition StartPosition;
        public readonly TPosition EndPosition;

        public GridPathResult(float i_PathCost, List<TTile> i_Path, TPosition i_Start, TPosition i_End)
        {
            PathCost = i_PathCost;
            Path = i_Path;
            StartPosition = i_Start;
            EndPosition = i_End;
        }
    }

    public class Grid2DPath<TTile, TTerrain, TPosition, TContext>
        where TTile : Grid2DTile<TTerrain, TPosition, TContext>
        where TTerrain : GridTerrain<TContext>
    {
        private readonly Grid2D<TTile, TTerrain, TPosition, TContext> m_Grid;
        private readonly List<GridPathElement<TTerrain, TPosition, TContext>> m_ElementPool;
        private int m_NewElementIndex;
        private readonly List<GridPathElement<TTerrain, TPosition, TContext>> m_OpenList = new List<GridPathElement<TTerrain, TPosition, TContext>>();
        private readonly List<GridPathElement<TTerrain, TPosition, TContext>> m_ClosedList = new List<GridPathElement<TTerrain, TPosition, TContext>>();
        private readonly List<TTile> m_PathTileList = new List<TTile>();
        private TContext m_Context;
        private float m_PathCost = -1.0f;

        List<GridPathElement<TTerrain, TPosition, TContext>> m_ConnectedList = new List<GridPathElement<TTerrain, TPosition, TContext>>();
        GridPathElement<TTerrain, TPosition, TContext> m_FinishElement;

        /// <summary>
        /// Gets the path grid tiles from starting to finishing locations.
        /// </summary>
        /// <remarks>
        /// Returns an empty list on unreachable paths.
        /// </remarks>
        /// <value>
        /// Path grid tiles.
        /// </value>
        public List<TTile> Tiles
        {
            get { return m_PathTileList; }
        }

        /// <summary>
        /// Gets the path cost.
        /// </summary>
        /// <remarks>
        /// When path is not valid, the path cost is -1.
        /// </remarks>
        /// <value>
        /// The path cost.
        /// </value>
        public float PathCost
        {
            get { return m_PathCost; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid2DPath"/> class, creating a tile sequence for quickest path between given positions.
        /// </summary>
        /// <param name="i_Grid">The i_ grid.</param>
        /// <param name="i_StartPos">The i_ start position.</param>
        /// <param name="i_EndPosition">The i_ end position.</param>
        public Grid2DPath(Grid2D<TTile, TTerrain, TPosition, TContext> i_Grid, int i_InitialCapacity)
        {
            Log.DebugAssert(i_Grid != null, "{0}: Invalid grid argument in constructor.", GetType().ToString());
            m_Grid = i_Grid;

            m_ElementPool = new List<GridPathElement<TTerrain, TPosition, TContext>>(i_InitialCapacity);
        }

        private GridPathElement<TTerrain, TPosition, TContext> GetNewElement()
        {
            if(m_ElementPool.Count == m_NewElementIndex)
            {
                m_ElementPool.Add(new GridPathElement<TTerrain, TPosition, TContext>());
            }
            return m_ElementPool[m_NewElementIndex];
        }

        private GridPathElement<TTerrain, TPosition, TContext> GetNewElement()
        {

        }

        public void FindPath(TPosition i_StartPos, TPosition i_EndPosition, TContext i_Context)
        {
            m_Context = i_Context;
            m_FinishElement = m_Grid.GetElement(i_EndPosition);

            //Only one grid path can be computed at a time because of the pathing data cached in the grid elements.
            lock (m_Grid.m_PathingLockHandle)
            {
                //if finish exists
                if (m_FinishElement != null)
                {
                    GridPathElement<TTerrain, TContext> startElement = m_Grid.GetElement(i_StartPos);
                    GridPathElement<TTerrain, TContext> currentElement = startElement;
                    //while a path option exists and finish not reached
                    while ((currentElement != null) && (currentElement != m_FinishElement))
                    {
                        Close(currentElement);
                        OpenNeighbours(currentElement);
                        currentElement = PickNext();
                    }
                    //Close the finishing tile if it was opened (it was reached) or it is also the starting point
                    if ((startElement == m_FinishElement) || (m_FinishElement.PathingState == GridPathingState.Opened))
                    {
                        Close(m_FinishElement);
                    }
                }

                Finish();
            }
        }

        public GridPathResult<TTile, TTerrain, TPosition, TContext> SaveResult()
        {
            int pathLength = m_PathTileList.Count;
            List<TTile> pathCopy = new List<TTile>(pathLength);
            for(int i = 0; i < pathLength; ++i)
            {
                pathCopy.Add(m_PathTileList[i]);
            }
            return new GridPathResult<TTile, TTerrain, TPosition, TContext>(m_PathCost, m_PathTileList,);
        }

        private void Open(GridPathElement<TTerrain, TContext> i_Element, GridPathElement<TTerrain, TContext> i_Parent)
        {
            TTerrain terrainTypeData = i_Element.Tile.Terrain;
            // move terrain cost
            float terrainCost = i_Parent.Tile.GetTransitionOutCost(i_Element.Tile, m_Context);
            terrainCost += i_Element.Tile.GetTransitionInCost(i_Parent.Tile, m_Context);
            if (terrainCost >= 0.0f)
            {
                i_Element.HeuristicDistance = m_Grid.GetHeuristicDistance(i_Element.Tile.Position, m_FinishElement.Tile.Position);
                i_Element.PathCost = terrainCost + i_Parent.PathCost; //cost of the path so far
                i_Element.FValue = i_Element.PathCost + i_Element.HeuristicDistance;

                i_Element.PathingState = GridPathingState.Opened;
                i_Element.PathTileIndex = (i_Parent.PathTileIndex + 1);
                m_OpenList.Add(i_Element);
            }
        }

        private bool Reopen(GridPathElement<TTerrain, TContext> i_Element, GridPathElement<TTerrain, TContext> i_Parent)
        {
            float terrainCost = i_Parent.Tile.GetTransitionOutCost(i_Element.Tile, m_Context);
            terrainCost += i_Element.Tile.GetTransitionInCost(i_Parent.Tile, m_Context);
            //negative cost indicates blockers
            if (terrainCost >= 0)
            {
                float newPathCost = (terrainCost + i_Parent.PathCost);
                if (newPathCost < i_Element.PathCost)
                {
                    i_Element.PathCost = newPathCost;
                    i_Element.FValue = newPathCost + i_Element.HeuristicDistance;
                    i_Element.PathTileIndex = i_Parent.PathTileIndex + 1;
                    return true;
                }
            }
            return false;
        }

        private void Close(GridPathElement<TTerrain, TContext> i_Element)
        {
            int closedListSize = m_ClosedList.Count;
            //clean tiles no longer in use
            if (i_Element.PathTileIndex > closedListSize)
            {
                for (int i = i_Element.PathTileIndex; i < closedListSize; ++i)
                {
                    m_ClosedList[i].Reset();
                }
                m_ClosedList.RemoveRange(i_Element.PathTileIndex, closedListSize - i_Element.PathTileIndex);
            }
            i_Element.PathingState = GridPathingState.Closed;
            m_ClosedList.Add(i_Element);
        }

        private void OpenNeighbours(GridPathElement<TTerrain, TContext> i_Element)
        {
            m_Grid.GetConnected(i_Element.Tile.Position, m_ConnectedList);

            int openListSizeBefore = m_OpenList.Count;
            bool oldElementChanged = false;

            int size = m_ConnectedList.Count;
            GridPathElement<TTerrain, TContext> neighbourElement;
            for (var i = 0; i < size; ++i)
            {
                neighbourElement = m_ConnectedList[i];
                switch (neighbourElement.PathingState)
                {
                    case GridPathingState.New:
                        Open(neighbourElement, i_Element);
                        break;
                    case GridPathingState.Opened:
                        bool changed = Reopen(neighbourElement, i_Element);
                        oldElementChanged = oldElementChanged || changed;
                        break;
                }
            }
            if (oldElementChanged || (openListSizeBefore > m_ClosedList.Count))
            {
                m_OpenList.InsertionSort(GridElementFValueComparer.Descending);
            }
            m_ConnectedList.Clear();
        }

        private void Finish()
        {
            int pathTileCount = m_ClosedList.Count;
            //if finish exists and was reached
            if ((m_FinishElement != null) && (m_FinishElement.PathingState == GridPathingState.Closed))
            {
                m_PathCost = m_FinishElement.PathCost;
                if (m_PathTileList.Capacity < pathTileCount)
                {
                    m_PathTileList.Capacity = pathTileCount;
                }

                for (int i = 0; i < pathTileCount; ++i)
                {
                    GridElement currentElement = m_ClosedList[i];
                    m_PathTileList.Add(currentElement.Tile);
                    //cleaning up the closed list element at the same time
                    currentElement.Reset();
                }
            }
            else
            {
                m_PathCost = -1;
                //clean closed list
                for (int i = 0; i < pathTileCount; ++i)
                {
                    m_ClosedList[i].Reset();
                }
            }
            //cleanup
            m_ClosedList.Clear();
            int openListElementCount = m_OpenList.Count;
            for (int i = 0; i < openListElementCount; ++i)
            {
                m_OpenList[i].Reset();
            }
            m_OpenList.Clear();
        }

        private GridElement PickNext()
        {
            int lastIndex = m_OpenList.Count - 1;
            GridElement pick = null;
            if (lastIndex >= 0)
            {
                pick = m_OpenList[lastIndex];
                m_OpenList.RemoveAt(lastIndex);
            }

            return pick;
        }
    }
}
