using Common.Collections;
using System.Collections.Generic;

namespace Common.Grid
{
    public abstract class Grid2D<TTerrain, TContext> where TTerrain : GridTerrain<TContext>
    {
        protected enum GridPathingState
        {
            New,
            Opened,
            Closed
        }

        /// <summary>
        /// Class for internal <see cref="Grid2D{TTerrainData, TContext}"/> tile data for path finding
        /// </summary>
        protected class GridElement
        {
            /// <summary>
            /// The tile data that is accessible to users.
            /// </summary>
            public Grid2DTile<TTerrain, TContext> Tile;
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
            /// This element's index of the path sequence.
            /// </summary>
            public int PathTileIndex;
            /// <summary>
            /// Current pathing inspection state.
            /// </summary>
            public GridPathingState PathingState;

            /// <summary>
            /// Initializes a new instance of the <see cref="GridElement"/> class.
            /// </summary>
            /// <param name="i_Tile">The tile data.</param>
            public GridElement(Grid2DTile<TTerrain, TContext> i_Tile)
            {
                Log.DebugAssert(i_Tile != null, "Tile argument invalid.");
                Tile = i_Tile;
                Reset();
            }

            /// <summary>
            /// Method for cleaning up pathing data back to default state.
            /// </summary>
            public void Reset()
            {
                Tile.Reset();
                HeuristicDistance = 0;
                PathCost = 0;
                FValue = 0;
                PathTileIndex = 0;
                PathingState = GridPathingState.New;
            }
        }

        protected class GridElementFValueComparer : IComparer<GridElement>
        {
            private int m_Modifier;

            private GridElementFValueComparer(bool i_Ascending)
            {
                m_Modifier = i_Ascending ? 1 : -1;
            }

            public int Compare(GridElement i_A, GridElement i_B)
            {
                return i_A.FValue.CompareTo(i_B.FValue) * m_Modifier;
            }

            public static GridElementFValueComparer Ascending = new GridElementFValueComparer(true);
            public static GridElementFValueComparer Descending = new GridElementFValueComparer(false);
        }

        public class Grid2DPath
        {
            private readonly Grid2D<TTerrain, TContext> m_Grid;
            private readonly List<GridElement> m_OpenList = new List<GridElement>();
            private readonly List<GridElement> m_ClosedList = new List<GridElement>();
            private readonly List<Grid2DTile<TTerrain, TContext>> m_PathTileList = new List<Grid2DTile<TTerrain, TContext>>();
            private readonly TContext m_Context;
            private float m_PathCost = -1.0f;

            List<GridElement> m_ConnectedList = new List<GridElement>();
            GridElement m_FinishElement = null;

            /// <summary>
            /// Gets the path grid tiles from starting to finishing locations.
            /// </summary>
            /// <remarks>
            /// Returns an empty list on unreachable paths.
            /// </remarks>
            /// <value>
            /// Path grid tiles.
            /// </value>
            public List<Grid2DTile<TTerrain, TContext>> Tiles
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
            public Grid2DPath(Grid2D<TTerrain, TContext> i_Grid, Grid2DPosition i_StartPos, Grid2DPosition i_EndPosition, TContext i_Context)
            {
                Log.DebugAssert(i_Grid != null, "Invalid grid argument.");
                m_Grid = i_Grid;
                m_Context = i_Context;
                m_FinishElement = m_Grid.GetElement(i_EndPosition);

                //Only one grid path can be computed at a time because of the pathing data cached in the grid elements.
                lock (m_Grid.m_PathingLockHandle)
                {
                    //if finish exists
                    if (m_FinishElement != null)
                    {
                        GridElement startElement = m_Grid.GetElement(i_StartPos);
                        GridElement currentElement = startElement;
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

            private void Open(GridElement i_Element, GridElement i_Parent)
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

            private bool Reopen(GridElement i_Element, GridElement i_Parent)
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

            private void Close(GridElement i_Element)
            {
                int closedListSize = m_ClosedList.Count;
                //clean tiles no longer in use
                if(i_Element.PathTileIndex > closedListSize)
                {
                    for(int i = i_Element.PathTileIndex; i < closedListSize; ++i)
                    {
                        m_ClosedList[i].Reset();
                    }
                    m_ClosedList.RemoveRange(i_Element.PathTileIndex, closedListSize - i_Element.PathTileIndex);
                }
                i_Element.PathingState = GridPathingState.Closed;
                m_ClosedList.Add(i_Element);
            }

            private void OpenNeighbours(GridElement i_Element)
            {
                m_Grid.GetConnected(i_Element.Tile.Position, m_ConnectedList);

                int openListSizeBefore = m_OpenList.Count;
                bool oldElementChanged = false;

                int size = m_ConnectedList.Count;
                GridElement neighbourElement;
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
                if(oldElementChanged || (openListSizeBefore > m_ClosedList.Count))
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

                    for(int i = 0; i < pathTileCount; ++i)
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
                for(int i = 0; i < openListElementCount; ++i)
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
        

        protected readonly object m_PathingLockHandle = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid2D{TGridTileData, TTerrainData}"/> class.
        /// </summary>
        public Grid2D()
        {
        }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <param name="i_Start">The start position.</param>
        /// <param name="i_End">The end position.</param>
        /// <returns></returns>
        public Grid2DPath GetPath(Grid2DPosition i_Start, Grid2DPosition i_End, TContext i_Context)
        {
            return new Grid2DPath(this, i_Start, i_End, i_Context);
        }

        public abstract Grid2DTile<TTerrain, TContext> GetTile(Grid2DPosition i_Position);

        public abstract int GetHeuristicDistance(Grid2DPosition i_From, Grid2DPosition i_To);

        protected abstract void GetConnected(Grid2DPosition i_Position, List<GridElement> o_ConnectedElements);

        protected abstract GridElement GetElement(Grid2DPosition i_Position);
    }
}
