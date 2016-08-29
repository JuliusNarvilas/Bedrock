using Common.IO;
using System;
using UnityEngine;

namespace Common.Text
{
    [Serializable]
    public struct IntelligentTextId
    {
        public string Name;
        public int Hash;

        public IntelligentTextId(string i_Id)
        {
            Name = i_Id;
            Hash = i_Id.GetHashCode();
        }

        public static bool operator ==(IntelligentTextId i_ObjA, IntelligentTextId i_ObjB)
        {
#if UNITY_EDITOR
            if ((i_ObjA.Name == i_ObjB.Name) != (i_ObjA.Hash == i_ObjB.Hash))
            {
                Debug.LogWarningFormat("Incorrect IntelligentTextId early match due to a hash clash ({0} == {1})", i_ObjA.Name, i_ObjB.Name);
            }
#endif
            if (i_ObjA.Hash == i_ObjB.Hash)
            {
                return i_ObjA.Name == i_ObjB.Name;
            }
            return false;
        }

        public static bool operator !=(IntelligentTextId i_ObjA, IntelligentTextId i_ObjB)
        {
            return !(i_ObjA == i_ObjB);
        }

        public override bool Equals(object obj)
        {
            IntelligentTextId id = (IntelligentTextId) obj;
            return this == id;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    [Serializable]
    public class IntelligentTextAsset
    {
        public IntelligentTextId Id;
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
        public IntelligentTextId Id;
        public IntelligentTextTransformData Transform;
    }
}
