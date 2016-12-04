
namespace Common.Grid
{
    public class Grid2DTile<TTerrainData, TContext>
        where TTerrainData : GridTerrain<TContext>
    {
        public readonly Grid2DPosition Position;
        public readonly TTerrainData Terrain;

        public Grid2DTile(Grid2DPosition i_Position, TTerrainData i_Terrain)
        {
            Position = i_Position;
            Terrain = i_Terrain;
        }

        public virtual float GetTransitionInCost(Grid2DTile<TTerrainData, TContext> i_FromTile, TContext i_Context)
        {
            return Terrain.GetCost(i_Context);
        }

        public virtual float GetTransitionOutCost(Grid2DTile<TTerrainData, TContext> i_ToTile, TContext i_Context)
        {
            return 0.0f;
        }

        public virtual void Reset()
        { }

        public override string ToString()
        {
            return string.Format(
                "{ Position: {0} ; Terrain: {1} }",
                Position.ToString(),
                Terrain.ToString()
                );
        }
    }
}
