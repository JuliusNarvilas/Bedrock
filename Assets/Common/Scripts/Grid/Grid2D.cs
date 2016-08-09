using Common.Collections;
using Common.Grid.TerrainType;
using Common.Threading;
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

        protected class GridElement
        {
            public Grid2DTile<TGridTileData, TTerrainData> Tile;
            public int HeuristicDistance;
            public int PathCost;
            public int FValue;
            public int PathTileIndex;
            public GridPathingState PathingState;
            public GridElement Parent;

            public GridElement(Grid2DTile<TGridTileData, TTerrainData> i_Tile)
            {
                Debug.Assert(i_Tile != null, "Tile argument invalid.");
                Tile = i_Tile;
                Reset();
            }

            public void Reset()
            {
                HeuristicDistance = 0;
                PathCost = 0;
                FValue = 0;
                PathingState = GridPathingState.New;
                PathTileIndex = -1;
                Parent = null;
                Tile.Reset();
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
            private Grid2D<TGridTileData, TTerrainData> m_Grid = null;
            private List<GridElement> m_OpenList = new List<GridElement>();
            private List<GridElement> m_ClosedList = new List<GridElement>();
            private List<Grid2DTile<TGridTileData, TTerrainData>> m_PathTileList = new List<Grid2DTile<TGridTileData, TTerrainData>>();
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


            public Grid2DPath(Grid2D<TGridTileData, TTerrainData> i_Grid, Grid2DPosition i_StartPos, Grid2DPosition i_FinishPos)
            {
                Debug.Assert(i_Grid != null, "Invalid grid argument.");
                m_Grid = i_Grid;
                m_FinishElement = m_Grid.GetElement(i_FinishPos);

                //if finish exists
                if(m_FinishElement != null)
                {
                    GridElement currentElement = m_Grid.GetElement(i_StartPos);
                    //while a path option exists and finish not reached
                    while ((currentElement != null) && (currentElement != m_FinishElement))
                    {
                        Close(currentElement);
                        OpenNeighbours(currentElement);
                        currentElement = PickNext();
                    }
                    Close(m_FinishElement);
                }

                Finish();
            }

            private void Open(GridElement i_Element, GridElement i_Parent)
            {
                TerrainTypeData<TTerrainData> terrainTypeData = m_Grid.GetTerrainTypeManager().Get(i_Element.Tile.TerrainType);
                // move terrain cost
                int terrainCost = i_Element.Tile.GetCost(i_Parent.Tile, terrainTypeData);
                if (terrainCost >= 0)
                {
                    i_Element.Parent = i_Parent;
                    i_Element.HeuristicDistance = m_Grid.GetHeuristicDistance(i_Element.Tile.Position, m_FinishElement.Tile.Position);
                    i_Element.PathCost = terrainCost + i_Parent.PathCost; //cost of the path so far
                    i_Element.FValue = i_Element.PathCost + i_Element.HeuristicDistance;

                    i_Element.PathingState = GridPathingState.Opened;
                    i_Element.PathTileIndex = i_Parent.PathTileIndex + 1;
                    m_OpenList.Add(i_Element);
                }
            }

            private bool Reopen(GridElement i_Element, GridElement i_Parent)
            {
                TerrainTypeData<TTerrainData> terrainTypeData = m_Grid.GetTerrainTypeManager().Get(i_Element.Tile.TerrainType);
                // move terrain cost
                int terrainCost = i_Element.Tile.GetCost(i_Parent.Tile, terrainTypeData);
                //negative cost indicates blockers
                if(terrainCost >= 0)
                {
                    int newPathCost = terrainCost + i_Parent.PathCost;
                    if (newPathCost < i_Element.PathCost)
                    {
                        i_Element.Parent = i_Parent;
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
                //clean tiles no longer in use
                if(i_Element.PathTileIndex >= m_ClosedList.Count)
                {
                    for(int i = i_Element.PathTileIndex; i < m_ClosedList.Count; ++i)
                    {
                        m_ClosedList[i].Reset();
                    }
                    m_ClosedList.RemoveRange(i_Element.PathTileIndex, m_ClosedList.Count - i_Element.PathTileIndex);
                }
                i_Element.PathingState = GridPathingState.Closed;
                m_ClosedList.Add(i_Element);
            }

            private void OpenNeighbours(GridElement i_Element)
            {
                m_Grid.GetConnected(i_Element.Tile.Position, m_ConnectedList);

                bool requireSorting = false;
                foreach (GridElement neighbourElement in m_ConnectedList)
                {
                    switch (neighbourElement.PathingState)
                    {
                        case GridPathingState.New:
                            Open(neighbourElement, i_Element);
                            requireSorting = true;
                            break;
                        case GridPathingState.Opened:
                            requireSorting = requireSorting || Reopen(neighbourElement, i_Element);
                            break;
                    }
                }
                if(requireSorting)
                {
                    m_OpenList.InsertionSort(GridElementFValueDescendingComparer.Instance);
                }
                m_ConnectedList.Clear();
            }

            private void Finish()
            {
                int pathTileCount = m_ClosedList.Count;
                //if finish exists and was reached
                if ((m_FinishElement != null) && (m_FinishElement.PathTileIndex >= 0))
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

        public class Grid2DPathThreadedJob : ThreadedJob
        {
            private Grid2DPosition m_StartPos;
            private Grid2DPosition m_FinishPos;
            private Grid2D<TGridTileData, TTerrainData> m_Grid;
            public Grid2DPath Path { get; private set; }

            public Grid2DPathThreadedJob(Grid2D<TGridTileData, TTerrainData> i_Grid, Grid2DPosition i_StartPos, Grid2DPosition i_FinishPos)
            {
                m_Grid = i_Grid;
                m_StartPos = i_StartPos;
                m_FinishPos = i_FinishPos;
                Path = null;
            }

            protected override void ThreadFunction()
            {
                Path = new Grid2DPath(m_Grid, m_StartPos, m_FinishPos);
            }
        }


        protected readonly TerrainTypeManager<TTerrainData> m_TerrainTypeManager;

        public Grid2D()
        {
            m_TerrainTypeManager = new TerrainTypeManager<TTerrainData>();
        }

        public Grid2D(TerrainTypeManager<TTerrainData> i_TerrainTypeManager)
        {
            Debug.Assert(i_TerrainTypeManager != null, "Invalid null argument.");
            m_TerrainTypeManager = i_TerrainTypeManager;
        }

        public TerrainTypeManager<TTerrainData> GetTerrainTypeManager()
        {
            return m_TerrainTypeManager;
        }

        public Grid2DPath GetPath(Grid2DPosition i_Start, Grid2DPosition i_Finish)
        {
            return new Grid2DPath(this, i_Start, i_Finish);
        }

        public Grid2DPathThreadedJob GetPathJob(Grid2DPosition i_Start, Grid2DPosition i_Finish)
        {
            return new Grid2DPathThreadedJob(this, i_Start, i_Finish);
        }

        public abstract Grid2DTile<TGridTileData, TTerrainData> GetTile(Grid2DPosition i_Position);

        public abstract int GetHeuristicDistance(Grid2DPosition from, Grid2DPosition to);

        protected abstract void GetConnected(Grid2DPosition i_Position, List<GridElement> o_ConnectedElements);

        protected abstract GridElement GetElement(Grid2DPosition i_Position);
    }
}
