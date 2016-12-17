using Common.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Assets.Common.Scripts.Grid
{
    public class HexGrid<TGridTileData, TTerrainData> : Grid2D<TGridTileData, TTerrainData> where TTerrainData : ISerializable
    {
        public override int GetHeuristicDistance(Grid2DPosition i_From, Grid2DPosition i_To)
        {
            throw new NotImplementedException();
        }

        public override Grid2DTile<TGridTileData, TTerrainData> GetTile(Grid2DPosition i_Position)
        {
            throw new NotImplementedException();
        }

        protected override void GetConnected(Grid2DPosition i_Position, List<GridElement> o_ConnectedElements)
        {
            throw new NotImplementedException();
        }

        protected override GridElement GetTile(Grid2DPosition i_Position)
        {
            throw new NotImplementedException();
        }
    }
}
