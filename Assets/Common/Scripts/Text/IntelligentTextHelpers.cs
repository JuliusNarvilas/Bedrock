using System;
using UnityEngine;
using System.Collections.Generic;

namespace Common.Text
{
    [Serializable]
    public struct IntelligentTextLocalizationRecord
    {
        public string id;
        public string displayName;
        public string path;
    }

    [Serializable]
    public struct IntelligentTextLocalizationsContainer
    {
        public List<IntelligentTextLocalizationRecord> localizationList;
    }

    [Serializable]
    public class IntelligentTextLocalizationData
    {
        public List<IntelligentTextKeyValueRecord> textLocalizations;
        public List<IntelligentTextKeyValueRecord> inserts;
        public List<IntelligentTextStyleRecord> styles;
        public List<IntelligentTextImageRecord> images;
        public List<IntelligentTextTransform> transforms;
    }

    [Serializable]
    public struct IntelligentTextKeyValueRecord
    {
        public string id;
        public string data;
    }

    [Serializable]
    public class IntelligentTextStyleRecord
    {
        public string id;
        public string fontPath;
        public Color color;
        public float lineSpacing;
        public int fontSize;
    }

    public class IntelligentTextStyle
    {
        public Color Color;
        public Font Font;
        public int FontSize;
        public float LineSpacing;
    }

    [Serializable]
    public struct IntelligentTextImageRecord
    {
        public string id;
        public string path;
        public string sprite;
    }

    public class IntelligentTextSubMeshData
    {
        int[] Trinagles;
        Material Material;
    }

    public class IntelligentTextMeshData
    {
        int Order;
        Vector3[] Verts;
        Color32[] Colors;
        Vector2[] Uvs;

        Vector2[] Uvs2;
        Vector4[] Tangents;

        List<IntelligentTextSubMeshData> SubMeshes;
    }

    public enum IntelligentTextTransformPivot
    {
        Top,
        Center,
        Bottom
    }

    [Serializable]
    public class IntelligentTextTransform
    {
        public string id;
        public Vector2 scale;
        public Vector2 offset;
        public float rotation;
        public IntelligentTextTransformPivot pivot;
    }
}
