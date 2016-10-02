using Common.Collections;
using Common.Grid.TerrainType;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Common.Grid
{
    public abstract class Grid2D<TGridTileData, TTerrainData> where TTerrainData : ISerializable
    {
        protected enum GridPathingState
        {
            New,
            Opened,
            Closed
        }

        /// <summary>
        /// Class for internal <see cref="Grid2D{TGridTileData, TTerrainData}"/> tile data for path finding
        /// </summary>
        protected class GridElement
        {
            /// <summary>
            /// The tile data that is accessible to users.
            /// </summary>
            public Grid2DTile<TGridTileData, TTerrainData> Tile;
            /// <summary>
            /// The approximate distance to the destination (must not be an underestimate).
            /// </summary>
            public ushort HeuristicDistance;
            /// <summary>
            /// The path cost so far.
            /// </summary>
            public ushort PathCost;
            /// <summary>
            /// The estimated path cost to the destination.
            /// </summary>
            public ushort FValue;
            /// <summary>
            /// This element's index of the path sequence.
            /// </summary>
            public ushort PathTileIndex;
            /// <summary>
            /// Current pathing inspection state.
            /// </summary>
            public GridPathingState PathingState;

            /// <summary>
            /// Initializes a new instance of the <see cref="GridElement"/> class.
            /// </summary>
            /// <param name="i_Tile">The tile data.</param>
            public GridElement(Grid2DTile<TGridTileData, TTerrainData> i_Tile)
            {
                Debug.Assert(i_Tile != null, "Tile argument invalid.");
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

        protected class GridElementFValueDescendingComparer : IComparer<GridElement>
        {
            private GridElementFValueDescendingComparer()
            { }

            public int Compare(GridElement i_A, GridElement i_B)
            {
                return i_A.FValue.CompareTo(i_B.FValue) * -1;
            }

            public static GridElementFValueDescendingComparer Instance = new GridElementFValueDescendingComparer();
        }

        public class Grid2DPath
        {
            private readonly Grid2D<TGridTileData, TTerrainData> m_Grid;
            private readonly List<GridElement> m_OpenList = new List<GridElement>();
            private readonly List<GridElement> m_ClosedList = new List<GridElement>();
            private readonly List<Grid2DTile<TGridTileData, TTerrainData>> m_PathTileList = new List<Grid2DTile<TGridTileData, TTerrainData>>();
            int m_PathCost = -1;

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
            public List<Grid2DTile<TGridTileData, TTerrainData>> Tiles
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
            public int PathCost
            {
                get { return m_PathCost; }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Grid2DPath"/> class, creating a tile sequence for quickest path between given positions.
            /// </summary>
            /// <param name="i_Grid">The i_ grid.</param>
            /// <param name="i_StartPos">The i_ start position.</param>
            /// <param name="i_EndPosition">The i_ end position.</param>
            public Grid2DPath(Grid2D<TGridTileData, TTerrainData> i_Grid, Grid2DPosition i_StartPos, Grid2DPosition i_EndPosition)
            {
                Debug.Assert(i_Grid != null, "Invalid grid argument.");
                m_Grid = i_Grid;
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
                TerrainTypeData<TTerrainData> terrainTypeData = m_Grid.TerrainTypeManager.Get(i_Element.Tile.TerrainType);
                // move terrain cost
                int terrainCost = i_Element.Tile.GetCost(i_Parent.Tile, terrainTypeData);
                if (terrainCost >= 0)
                {
                    i_Element.HeuristicDistance = m_Grid.GetHeuristicDistance(i_Element.Tile.Position, m_FinishElement.Tile.Position);
                    i_Element.PathCost = (ushort)(terrainCost + i_Parent.PathCost); //cost of the path so far
                    i_Element.FValue = (ushort)(i_Element.PathCost + i_Element.HeuristicDistance);

                    i_Element.PathingState = GridPathingState.Opened;
                    i_Element.PathTileIndex = (ushort)(i_Parent.PathTileIndex + 1);
                    m_OpenList.Add(i_Element);
                }
            }

            private bool Reopen(GridElement i_Element, GridElement i_Parent)
            {
                TerrainTypeData<TTerrainData> terrainTypeData = m_Grid.TerrainTypeManager.Get(i_Element.Tile.TerrainType);
                // move terrain cost
                int terrainCost = i_Element.Tile.GetCost(i_Parent.Tile, terrainTypeData);
                //negative cost indicates blockers
                if(terrainCost >= 0)
                {
                    int newPathCost = (terrainCost + i_Parent.PathCost);
                    if (newPathCost < i_Element.PathCost)
                    {
                        i_Element.PathCost = (ushort)newPathCost;
                        i_Element.FValue = (ushort)(newPathCost + i_Element.HeuristicDistance);
                        i_Element.PathTileIndex = (ushort)(i_Parent.PathTileIndex + 1);
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
                    m_OpenList.InsertionSort(GridElementFValueDescendingComparer.Instance);
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
                    m_PathTileList.Capacity = pathTileCount;

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
        protected readonly TerrainTypeManager<TTerrainData> m_TerrainTypeManager;

        /// <summary>
        /// Gets the terrain type manager.
        /// </summary>
        /// <value>
        /// The terrain type manager.
        /// </value>
        public TerrainTypeManager<TTerrainData> TerrainTypeManager
        {
            get { return m_TerrainTypeManager; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid2D{TGridTileData, TTerrainData}"/> class.
        /// </summary>
        public Grid2D()
        {
            m_TerrainTypeManager = new TerrainTypeManager<TTerrainData>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grid2D{TGridTileData, TTerrainData}"/> class.
        /// </summary>
        /// <param name="i_TerrainTypeManager">The terrain type manager.</param>
        public Grid2D(TerrainTypeManager<TTerrainData> i_TerrainTypeManager)
        {
            Debug.Assert(i_TerrainTypeManager != null, "Invalid TerrainTypeManager argument.");
            m_TerrainTypeManager = i_TerrainTypeManager;
        }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <param name="i_Start">The start position.</param>
        /// <param name="i_End">The end position.</param>
        /// <returns></returns>
        public Grid2DPath GetPath(Grid2DPosition i_Start, Grid2DPosition i_End)
        {
            return new Grid2DPath(this, i_Start, i_End);
        }

        public abstract Grid2DTile<TGridTileData, TTerrainData> GetTile(Grid2DPosition i_Position);

        public abstract ushort GetHeuristicDistance(Grid2DPosition i_From, Grid2DPosition i_To);

        protected abstract void GetConnected(Grid2DPosition i_Position, List<GridElement> o_ConnectedElements);

        protected abstract GridElement GetElement(Grid2DPosition i_Position);
    }
}
