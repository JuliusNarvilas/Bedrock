using System.Collections.Generic;
using UnityEngine;

namespace Common.Text
{
    public class IntelligentTextMeshData
    {
        public int Order;
        public int TextLength;
        public List<IntelligentTextLineInfo> Lines;
        public List<Vector3> Verts;
        public List<Color32> Colors;
        public List<Vector2> Uvs;

        public List<Vector2> Uvs2;
        public List<Vector4> Tangents;

        public List<IntelligentTextSubMeshData> SubMeshes;


        public void RemoveChars(int i_CharStartIndex, int i_Count)
        {
            int charDeletionEndIndex = i_CharStartIndex + i_Count;
            int lastLineIndex = Lines.Count - 1;
            for (int i = 0; i < lastLineIndex; ++i)
            {
                int lineEndCharIndex = Lines[i + 1].StartCharIndex;
                //if deletion star has been reached
                if (lineEndCharIndex > i_CharStartIndex)
                {
                    //if deletion in progress
                    if (i_CharStartIndex < charDeletionEndIndex)
                    {
                        int lineDeletionEnd = Mathf.Min(charDeletionEndIndex, lineEndCharIndex);
                        int deletionCount = lineDeletionEnd - i_CharStartIndex;
                        charDeletionEndIndex -= deletionCount;
                        TextLength -= deletionCount;

                        for (int j = i + 1; j < Lines.Count; ++j)
                        {
                            Lines[j].StartCharIndex -= deletionCount;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //deletion in last line if necessary
            if (i_CharStartIndex < charDeletionEndIndex)
            {
                int lastDeletionEnd = Mathf.Min(charDeletionEndIndex, TextLength);
                int deletionCount = lastDeletionEnd - i_CharStartIndex;
                TextLength -= deletionCount;
            }

            //Clear empty lines
            int lineEnd = TextLength;
            for (int i = Lines.Count - 1; i >= 0; --i)
            {
                int lineStartIndex = Lines[i].StartCharIndex;
                if (lineStartIndex >= lineEnd)
                {
                    Lines.RemoveAt(i);
                }
                else
                {
                    lineEnd = lineStartIndex;
                }
            }

            //remove mesh data
            int meshDataDeleteStartIndex = i_CharStartIndex * 4;
            int meshDataDeleteCount = i_Count * 4;

            Verts.RemoveRange(meshDataDeleteStartIndex, meshDataDeleteCount);
            Colors.RemoveRange(meshDataDeleteStartIndex, meshDataDeleteCount);
            Uvs.RemoveRange(meshDataDeleteStartIndex, meshDataDeleteCount);

            Uvs2.RemoveRange(meshDataDeleteStartIndex, meshDataDeleteCount);
            Tangents.RemoveRange(meshDataDeleteStartIndex, meshDataDeleteCount);
        }

        public bool AccommodateHeight(
            TextAnchor i_TextAnchor, IntelligentTextTransformAnchor i_ItemAnchor, int i_CharIndex, float i_Height,
            out float o_StartTop, out float o_Height
        )
        {
            int matchedLineIndex = -1;
            int linesLastLineIndex = Lines.Count - 1;
            for (int i = 0; i < linesLastLineIndex; ++i)
            {
                if (i_CharIndex < Lines[i + 1].StartCharIndex)
                {
                    matchedLineIndex = i;
                    break;
                }
            }
            if (matchedLineIndex < 0 && i_CharIndex < TextLength)
            {
                matchedLineIndex = linesLastLineIndex;
            }
            if (matchedLineIndex < 0)
            {
                matchedLineIndex = linesLastLineIndex;
                o_StartTop = 0;
                o_Height = 0;
                return false;
            }

            //TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            return true;
        }
    }
}
