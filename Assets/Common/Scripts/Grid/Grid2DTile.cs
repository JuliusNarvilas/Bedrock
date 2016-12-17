
namespace Common.Grid
{
    public class Grid2DTile<TTerrain, TPosition, TContext>
        where TTerrain : GridTerrain<TContext>
    {
        public readonly TPosition Position;
        public readonly TTerrain Terrain;

        public Grid2DTile(TPosition i_Position, TTerrain i_Terrain)
        {
            Position = i_Position;
            Terrain = i_Terrain;
        }

        public virtual float GetTransitionInCost(Grid2DTile<TTerrain, TPosition, TContext> i_FromTile, TContext i_Context)
        {
            return Terrain.GetCost(i_Context);
        }

        public virtual float GetTransitionOutCost(Grid2DTile<TTerrain, TPosition, TContext> i_ToTile, TContext i_Context)
        {
            return 0.0f;
        }

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
