using System;
using UnityEngine;

namespace Common.Text
{
    [Serializable]
    public struct HashedString
    {
        public string Text;
        public int Hash;

        public HashedString(string i_Text)
        {
            Text = i_Text;
            Hash = i_Text.GetHashCode();
        }

        public static bool operator ==(HashedString i_ObjA, HashedString i_ObjB)
        {
#if UNITY_EDITOR
            if ((i_ObjA.Text == i_ObjB.Text) != (i_ObjA.Hash == i_ObjB.Hash))
            {
                Debug.LogWarningFormat("Incorrect IntelligentTextId early match due to a hash clash ({0} == {1})", i_ObjA.Text, i_ObjB.Text);
            }
#endif
            if (i_ObjA.Hash == i_ObjB.Hash)
            {
                return i_ObjA.Text == i_ObjB.Text;
            }
            return false;
        }

        public static bool operator !=(HashedString i_ObjA, HashedString i_ObjB)
        {
            return !(i_ObjA == i_ObjB);
        }

        public override bool Equals(object obj)
        {
            HashedString hashedText = (HashedString)obj;
            return this == hashedText;
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }
    }
}
