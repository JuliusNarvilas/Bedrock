using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public static class MapGenerator
    {
        public static void Generate(GameObject i_Tile)
        {
            Renderer renderer = i_Tile.GetComponent<Renderer>();
            Vector3 tileSize = renderer.bounds.size;

            Vector2 MapSpace = new Vector2(5, 5);


        }
    }
}
