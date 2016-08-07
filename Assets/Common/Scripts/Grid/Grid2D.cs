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
            private List<Grid2DTile<TGridTileData, TTerrainData>> m_PathList = null;
            int m_PathCost = 0;

            List<GridElement> m_ConnectedList = new List<GridElement>();
            GridElement m_FinishElement = null;

            public Grid2DPath(Grid2D<TGridTileData, TTerrainData> i_Grid, Grid2DPosition i_StartPos, Grid2DPosition i_FinishPos)
            {
                Debug.Assert(i_Grid != null, "Invalid grid argument.");

                m_Grid = i_Grid;
                m_FinishElement = m_Grid.GetElement(i_FinishPos);

                GridElement currentElement = m_Grid.GetElement(i_StartPos);
                if(m_FinishElement != null && currentElement != null)
                {
                    bool finished = Close(currentElement);
                    while (!finished)
                    {
                        currentElement = PickNext();
                        finished = Close(currentElement);
                    }
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
                    i_Element.PathCost = terrainCost +
                        i_Parent.PathCost; //cost of the path so far

                    i_Element.FValue = i_Element.PathCost + i_Element.HeuristicDistance;

                    i_Element.PathingState = GridPathingState.Opened;
                    i_Element.PathTileIndex = i_Parent.PathTileIndex + 1;
                    m_OpenList.Add(i_Element);
                }
            }

            private bool UpdateOpened(GridElement i_Element, GridElement i_Parent)
            {
                TerrainTypeData<TTerrainData> terrainTypeData = m_Grid.GetTerrainTypeManager().Get(i_Element.Tile.TerrainType);
                // move terrain cost
                int terrainCost = i_Element.Tile.GetCost(i_Parent.Tile, terrainTypeData);

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

            private bool Close(GridElement i_Element)
            {
                i_Element.PathingState = GridPathingState.Closed;
                m_ClosedList.Add(i_Element);

                //finish reached
                if (i_Element == m_FinishElement)
                    return true;

                m_ConnectedList.Clear();
                m_Grid.GetConnected(i_Element.Tile.Position, m_ConnectedList);

                bool requireSorting = false;
                foreach(GridElement neighbourElement in m_ConnectedList)
                {
                    switch(neighbourElement.PathingState)
                    {
                        case GridPathingState.New:
                            Open(neighbourElement, i_Element);
                            requireSorting = true;
                            break;
                        case GridPathingState.Opened:
                            requireSorting = requireSorting || UpdateOpened(neighbourElement, i_Element);
                            break;
                    }
                }

                if(requireSorting)
                {
                    m_OpenList.InsertionSort(GridElementFValueDescendingComparer.Instance);
                }

                return false;
            }

            private void Finish()
            {
                if (m_FinishElement.PathTileIndex >= 0)
                {
                    m_PathCost = m_FinishElement.PathCost;

                    m_PathList = new List<Grid2DTile<TGridTileData, TTerrainData>>(m_FinishElement.PathTileIndex + 1);

                    while (m_FinishElement != null)
                    {
                        m_PathList.Insert(m_FinishElement.PathTileIndex, m_FinishElement.Tile);
                        m_FinishElement = m_FinishElement.Parent;
                    }
                }
                Clear();
            }

            private void Clear()
            {
                foreach(GridElement openListElement in m_OpenList)
                {
                    openListElement.Reset();
                }
                m_OpenList.Clear();

                foreach (GridElement closedListElement in m_ClosedList)
                {
                    closedListElement.Reset();
                }
                m_ClosedList.Clear();

                m_ConnectedList.Clear();
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

        public abstract Grid2DTile<TGridTileData, TTerrainData> GetTile(Grid2DPosition i_Position);

        public abstract int GetHeuristicDistance(Grid2DPosition from, Grid2DPosition to);

        protected abstract void GetConnected(Grid2DPosition i_Position, List<GridElement> o_ConnectedElements);

        protected abstract GridElement GetElement(Grid2DPosition i_Position);
    }
}
