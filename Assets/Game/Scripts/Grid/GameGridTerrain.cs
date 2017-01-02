
using Common.Grid;

namespace Game.Grid
{
    public class GameGridTerrain : GridTerrain<GameGridChangeContext>
    {
        public readonly float Cost;

        public GameGridTerrain(int i_Id, string i_Name, float i_Cost) : base(i_Id, i_Name)
        {
            Cost = i_Cost;
        }

        public override float GetCost(GameGridChangeContext i_Context)
        {
            return Cost;
        }
    }
}
