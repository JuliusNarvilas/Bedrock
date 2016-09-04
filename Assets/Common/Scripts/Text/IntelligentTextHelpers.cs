using Common.IO;
using System;
using UnityEngine;

namespace Common.Text
{
    public class IntelligentTextRenderElement
    {
        //mesh
        //material
        //bounds
        //interactor
    }

    [Serializable]
    public class IntelligentTextAsset
    {
        public HashedString Id;
        public ResourceReference FileReference;
    }

    public enum IntelligentTextTransformPivot
    {
        Top,
        Center,
        Bottom
    }

    [Serializable]
    public class IntelligentTextTransformData
    {
        public Vector2 SizeScaler;
        public Vector2 PerceivedSizeScaler;
        public Vector2 Offset;
        public float Rotation;
        public IntelligentTextTransformPivot Pivot;
    }

    [Serializable]
    public class IntelligentTextTransform
    {
        public HashedString Id;
        public IntelligentTextTransformData Transform;
    }
}
