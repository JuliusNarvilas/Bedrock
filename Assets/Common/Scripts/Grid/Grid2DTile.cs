
using Common.Grid.TerrainType;
using System;
using System.Runtime.Serialization;

namespace Common.Grid
{
    public class Grid2DTile<TGridTileData, TTerrainData> where TTerrainData : ISerializable
    {
        public readonly int TerrainType;
        public readonly Grid2DPosition Position;
        public readonly TGridTileData Data;

        public Grid2DTile(int i_TerrainType, Grid2DPosition i_Position, TGridTileData i_Data)
        {
            TerrainType = i_TerrainType;
            Position = i_Position;
            Data = i_Data;
        }

        public virtual int GetCost(Grid2DTile<TGridTileData, TTerrainData> i_FromTile, TerrainTypeData<TTerrainData> i_TerrainTypeData)
        {
            return i_TerrainTypeData.Cost;
        }

        public virtual void Reset()
        { }

        public override string ToString()
        {
            return string.Format(
                "{ Position: {1} {0} Data: {2} {0}}",
                Environment.NewLine,
                Position.ToString(),
                Data.ToString()
                );
        }
    }
}
