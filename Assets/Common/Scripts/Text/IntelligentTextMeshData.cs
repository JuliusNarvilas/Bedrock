﻿using System.Collections.Generic;
using UnityEngine;
using Common.Collections;

namespace Common.Text
{
    public class IntelligentTextForeignItemProperties
    {
        public IntelligentTextTransformAnchor Anchor;
        public int CharIndex;
        public float Height;

        public float BottomYResult;
        public int LineIndexResult;

        public IntelligentTextForeignItemProperties(IntelligentTextTransformAnchor i_Anchor, int i_CharIndex, float i_Height)
        {
            Anchor = i_Anchor;
            CharIndex = i_CharIndex;
            Height = i_Height;
            LineIndexResult = -1;
        }
    }

    public class IntelligentTextForeignItemPropertiesSorter : Comparer<IntelligentTextForeignItemProperties>
    {
        private int m_Ascending;

        private IntelligentTextForeignItemPropertiesSorter(bool i_Ascending)
        {
            m_Ascending = i_Ascending ? 1 : -1;
        }

        public override int Compare(IntelligentTextForeignItemProperties x, IntelligentTextForeignItemProperties y)
        {
            return x.CharIndex.CompareTo(y.CharIndex) * m_Ascending;
        }

        public static readonly IntelligentTextForeignItemPropertiesSorter Ascending = new IntelligentTextForeignItemPropertiesSorter(true);
        public static readonly IntelligentTextForeignItemPropertiesSorter Descending = new IntelligentTextForeignItemPropertiesSorter(false);
    }

    public class IntelligentTextMeshData
    {
        public class Sorter : IComparer<IntelligentTextMeshData>
        {
            private int m_Ascending;

            private Sorter(bool i_Ascending)
            {
                m_Ascending = i_Ascending ? 1 : -1;
            }

            public int Compare(IntelligentTextMeshData x, IntelligentTextMeshData y)
            {
                return x.Order.CompareTo(y) * m_Ascending;
            }

            static public readonly Sorter Ascending = new Sorter(true);
            static public readonly Sorter Descending = new Sorter(false);
        }

        public int Order;
        public int TextLength;
        public List<IntelligentTextLineInfo> Lines;
        public List<Vector3> Verts;
        public List<Color32> Colors;
        public List<Vector2> Uvs;

        public List<Vector2> Uvs2;
        public List<Vector4> Tangents;

        public List<IntelligentTextSubMeshData> SubMeshes;
        public Rect ExtentRect;


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

        private void FitMeshesDownY(int i_StartLineIndex, float i_StartTopY)
        {
            const float MIN_OFFSET = 0.0005f;

            int lineCount = Lines.Count;
            for (int i = i_StartLineIndex; i < lineCount; ++i)
            {
                var line = Lines[i];
                i_StartTopY -= line.Height + line.PaddingTop;
                float offset = i_StartTopY - Verts[line.StartCharIndex * 4].y;
                i_StartTopY -= line.PaddingBottom;
                if (offset > MIN_OFFSET)
                {
                    int lineTextVertEnd = TextLength * 4;
                    if((i + 1) < lineCount)
                    {
                        lineTextVertEnd = Lines[i + 1].StartCharIndex * 4;
                    }

                    for(int j = line.StartCharIndex * 4; j < lineTextVertEnd; ++j)
                    {
                        Vector3 temp = Verts[j];
                        temp.y += offset;
                        Verts[j] = temp;
                    }
                }
            }
        }

        private void FitMeshesUpY(int i_StartLineIndex, float i_StartBottomY)
        {
            const float MIN_OFFSET = 0.0005f;
            IntelligentTextLineInfo line = null;
            if(i_StartLineIndex >= Lines.Count - 1)
            {
                line = Lines[Lines.Count - 1];
                i_StartBottomY += line.PaddingBottom;
                float offset = i_StartBottomY - Verts[line.StartCharIndex * 4].y;
                int lineTextVertEnd = TextLength * 4;
                for (int j = line.StartCharIndex * 4; j < lineTextVertEnd; ++j)
                {
                    Vector3 temp = Verts[j];
                    temp.y += offset;
                    Verts[j] = temp;
                }
                i_StartBottomY += line.Height + line.PaddingTop;
                i_StartLineIndex = Lines.Count - 2;
            }
            
            for (int i = i_StartLineIndex; i >= 0; --i)
            {
                line = Lines[i];
                i_StartBottomY += line.PaddingBottom;
                float offset = i_StartBottomY - Verts[line.StartCharIndex * 4].y;
                i_StartBottomY += line.Height + line.PaddingTop;
                if (offset > MIN_OFFSET)
                {
                    int lineTextVertEnd = Lines[i + 1].StartCharIndex * 4;
                    for (int j = line.StartCharIndex * 4; j < lineTextVertEnd; ++j)
                    {
                        Vector3 temp = Verts[j];
                        temp.y += offset;
                        Verts[j] = temp;
                    }
                }
            }
        }

        public void AccommodateElements(
            TextAnchor i_TextAnchor, List<IntelligentTextForeignItemProperties> i_ForeignItems
        )
        {
            const float MIN_PADDING_INCREASE = 0.0005f;
            int foreignItemCount = i_ForeignItems.Count;
            if(foreignItemCount <= 0)
            {
                return;
            }

            i_ForeignItems.InsertionSort(IntelligentTextForeignItemPropertiesSorter.Ascending);
            //associate lines with foreign items
            int foreignItemIndex = 0;
            int lastLineIndex = Lines.Count - 1;
            for (int i = 0; i < lastLineIndex; ++i)
            {
                while(i_ForeignItems[foreignItemIndex].CharIndex < Lines[i + 1].StartCharIndex)
                {
                    i_ForeignItems[foreignItemIndex++].LineIndexResult = i;
                    if(foreignItemIndex < foreignItemCount)
                    {
                        //terminate for loop
                        i = lastLineIndex;
                        break;
                    }
                }
            }
            if(foreignItemIndex < foreignItemCount)
            {
                while (i_ForeignItems[foreignItemIndex].CharIndex < TextLength)
                {
                    i_ForeignItems[foreignItemIndex++].LineIndexResult = lastLineIndex;
                    if (foreignItemIndex < foreignItemCount)
                    {
                        break;
                    }
                }
            }

            //expand line paddings to accommodate foreign elements
            foreignItemIndex = 0;
            int lineCount = Lines.Count;
            float fullHeight = 0;
            var foreignItem = i_ForeignItems[foreignItemIndex++];
            for (int i = 0; i < lineCount; ++i)
            {
                var line = Lines[i];
                while (foreignItem.LineIndexResult == i)
                {
                    switch (foreignItem.Anchor)
                    {
                        case IntelligentTextTransformAnchor.Top:
                            {
                                float paddingIncrease = foreignItem.Height - line.Height - line.PaddingBottom;
                                if(paddingIncrease > MIN_PADDING_INCREASE)
                                {
                                    line.PaddingBottom += paddingIncrease;
                                }
                            }
                            break;
                        case IntelligentTextTransformAnchor.Center:
                            {
                                float lineHalfHeight = line.Height * 0.5f;
                                float foreignItemHalfHeight = foreignItem.Height * 0.5f;
                                float paddingIncrease = foreignItemHalfHeight - lineHalfHeight - line.PaddingTop;
                                if(paddingIncrease > MIN_PADDING_INCREASE)
                                {
                                    line.PaddingTop += paddingIncrease;
                                }
                                paddingIncrease = foreignItemHalfHeight - lineHalfHeight - line.PaddingBottom;
                                if(paddingIncrease > MIN_PADDING_INCREASE)
                                {
                                    line.PaddingBottom += paddingIncrease;
                                }
                            }
                            break;
                        case IntelligentTextTransformAnchor.Bottom:
                            {
                                float paddingIncrease = foreignItem.Height - line.Height - line.PaddingTop;
                                if(paddingIncrease > MIN_PADDING_INCREASE)
                                {
                                    line.PaddingTop += paddingIncrease;
                                }
                            }
                            break;
                    }
                    if (foreignItemIndex < foreignItemCount)
                    {
                        //terminate for loop
                        fullHeight += line.PaddingBottom + line.Height + line.PaddingTop;
                        for(++i; i < lineCount; ++i)
                        {
                            line = Lines[i];
                            fullHeight += line.PaddingBottom + line.Height + line.PaddingTop;
                        }
                        break;
                    }
                    foreignItem = i_ForeignItems[foreignItemIndex++];
                }
                fullHeight += line.PaddingBottom + line.Height + line.PaddingTop;
            }

            
            switch(i_TextAnchor)
            {
                case TextAnchor.UpperLeft:
                case TextAnchor.UpperCenter:
                case TextAnchor.UpperRight:
                    FitMeshesDownY(0, ExtentRect.height);
                    break;
                case TextAnchor.MiddleLeft:
                case TextAnchor.MiddleCenter:
                case TextAnchor.MiddleRight:
                    {
                        float halfHeightTracker = fullHeight * 0.5f;
                        int middleLineIndex = 0;
                        for (; middleLineIndex < lineCount; ++middleLineIndex)
                        {
                            var line = Lines[middleLineIndex];
                            halfHeightTracker -= line.PaddingBottom + line.Height + line.PaddingTop;
                            if (halfHeightTracker < 0)
                            {
                                break;
                            }
                        }
                        float middleLineTopY = (ExtentRect.height * 0.5f) - halfHeightTracker;
                        FitMeshesDownY(middleLineIndex, middleLineTopY);
                        FitMeshesUpY(middleLineIndex + 1, middleLineTopY);
                    }
                    break;
                case TextAnchor.LowerLeft:
                case TextAnchor.LowerCenter:
                case TextAnchor.LowerRight:
                    FitMeshesUpY(lastLineIndex, 0);
                    break;
            }
        }
    }
}
