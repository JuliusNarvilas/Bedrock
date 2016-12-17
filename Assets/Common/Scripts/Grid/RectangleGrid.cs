using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common.Grid
{
    public class RectangleGrid<TGridTileData, TTerrainData> : Grid2D<TGridTileData, TTerrainData> where TTerrainData : ISerializable
    {
        protected readonly List<List<GridElement>> m_Tiles = new List<List<GridElement>>();
        protected bool m_AllowMoveDiagonally = true;

        public override int GetHeuristicDistance(Grid2DPosition i_From, Grid2DPosition i_To)
        {
            int xDiff = Math.Abs(i_To.X - i_From.X);
            int yDiff = Math.Abs(i_To.Y - i_From.Y);

            if (m_AllowMoveDiagonally)
            {
                int minDiff = Math.Min(xDiff, yDiff);
                int diagonalDist = (int)(Math.Sqrt(minDiff * minDiff * 2) + 0.5);
                return diagonalDist + xDiff - minDiff + yDiff - minDiff;
            }
            else
            {
                return xDiff + yDiff;
            }
        }

        public override Grid2DTile<TGridTileData, TTerrainData> GetTile(Grid2DPosition i_Position)
        {
            GridElement element = GetTile(i_Position);
            if (element != null)
            {
                return element.Tile;
            }
            return null;
        }

        protected override void GetConnected(Grid2DPosition i_Position, List<GridElement> o_ConnectedElements)
        {
            GridElement tempElement = null;
            if (i_Position.X > 0)
            {
                tempElement = GetTile(new Grid2DPosition(i_Position.X - 1, i_Position.Y));
                if (tempElement != null) o_ConnectedElements.Add(tempElement);
                if (m_AllowMoveDiagonally)
                {
                    tempElement = GetTile(new Grid2DPosition(i_Position.X - 1, i_Position.Y - 1));
                    if (tempElement != null) o_ConnectedElements.Add(tempElement);
                    tempElement = GetTile(new Grid2DPosition(i_Position.X - 1, i_Position.Y + 1));
                    if (tempElement != null) o_ConnectedElements.Add(tempElement);
                }
            }
            tempElement = GetTile(new Grid2DPosition(i_Position.X + 1, i_Position.Y));
            if (tempElement != null) o_ConnectedElements.Add(tempElement);
            if (m_AllowMoveDiagonally)
            {
                tempElement = GetTile(new Grid2DPosition(i_Position.X + 1, i_Position.Y - 1));
                if (tempElement != null) o_ConnectedElements.Add(tempElement);
                tempElement = GetTile(new Grid2DPosition(i_Position.X + 1, i_Position.Y + 1));
                if (tempElement != null) o_ConnectedElements.Add(tempElement);
            }

            if (i_Position.Y > 0)
            {
                tempElement = GetTile(new Grid2DPosition(i_Position.X, i_Position.Y - 1));
                if (tempElement != null)
                {
                    o_ConnectedElements.Add(tempElement);
                }
            }
            tempElement = GetTile(new Grid2DPosition(i_Position.X, i_Position.Y + 1));
            if (tempElement != null)
            {
                o_ConnectedElements.Add(tempElement);
            }
        }

        protected override GridElement GetTile(Grid2DPosition i_Position)
        {
            if (m_Tiles.Count > i_Position.X)
            {
                List<GridElement> rows = m_Tiles[i_Position.X];
                if (rows.Count > i_Position.Y)
                {
                    return rows[i_Position.Y];
                }
            }
            return null;
        }
    }
}
