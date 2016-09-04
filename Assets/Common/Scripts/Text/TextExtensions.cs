using System.Collections.Generic;
using UnityEngine;

namespace Common.Text
{
    public static class TextExtensions
    {
        static public void GetMesh(this TextGenerator i_Generator, Mesh o_Mesh)
        {
            if (o_Mesh == null)
                return;

            int vertSize = i_Generator.vertexCount;
            Vector3[] tempVerts = new Vector3[vertSize];
            Color32[] tempColours = new Color32[vertSize];
            Vector2[] tempUvs = new Vector2[vertSize];
            IList<UIVertex> generatorVerts = i_Generator.verts;
            for (int i = 0; i < vertSize; ++i)
            {
                tempVerts[i] = generatorVerts[i].position;
                tempColours[i] = generatorVerts[i].color;
                tempUvs[i] = generatorVerts[i].uv0;
            }
            o_Mesh.vertices = tempVerts;
            o_Mesh.colors32 = tempColours;
            o_Mesh.uv = tempUvs;

            int characterCount = vertSize / 4;
            int[] tempIndices = new int[characterCount * 6];
            for(int i = 0; i < characterCount; ++i)
            {
                int vertIndexStart = i * 4;
                int trianglesIndexStart = i * 6;
                tempIndices[trianglesIndexStart++] = vertIndexStart;
                tempIndices[trianglesIndexStart++] = vertIndexStart + 1;
                tempIndices[trianglesIndexStart++] = vertIndexStart + 2;
                tempIndices[trianglesIndexStart++] = vertIndexStart;
                tempIndices[trianglesIndexStart++] = vertIndexStart + 2;
                tempIndices[trianglesIndexStart] = vertIndexStart + 3;
            }
            o_Mesh.triangles = tempIndices;
            //TODO: setBounds manually
            o_Mesh.RecalculateBounds();
        }
    }
}
