using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Grid
{
    public struct GridTileData
    {
        public readonly int TerrainType;

        public GridTileData(int i_TerrainType)
        {
            TerrainType = i_TerrainType;
        }
    }
}
