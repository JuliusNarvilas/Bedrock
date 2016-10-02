using System;

namespace Common
{
    public class Pair<T1, T2> : IEquatable<Pair<T1, T2>>
    {
        public readonly T1 First;
        public readonly T2 Second;

        public Pair(T1 i_First, T2 i_Second)
        {
            First = i_First;
            Second = i_Second;
        }

        public static bool operator ==(Pair<T1, T2> i_ObjA, Pair<T1, T2> i_ObjB)
        {
            return i_ObjA.First.Equals(i_ObjB.First) && i_ObjA.Second.Equals(i_ObjB.Second);
        }

        public static bool operator !=(Pair<T1, T2> i_ObjA, Pair<T1, T2> i_ObjB)
        {
            return !(i_ObjA == i_ObjB);
        }

        public override bool Equals(object i_Other)
        {
            Pair<T1, T2> otherPair = i_Other as Pair<T1, T2>;
            return this == otherPair;
        }

        public bool Equals(Pair<T1, T2> i_Other)
        {
            return this == i_Other;
        }

        public override int GetHashCode()
        {
            return MathHelper.CombineHashCodes(First.GetHashCode(), Second.GetHashCode());
        }
    }


    public struct PairStruct<T1, T2> : IEquatable<PairStruct<T1, T2>>
    {
        public readonly T1 First;
        public readonly T2 Second;

        public PairStruct(T1 i_First, T2 i_Second)
        {
            First = i_First;
            Second = i_Second;
        }

        public static bool operator ==(PairStruct<T1, T2> i_ObjA, PairStruct<T1, T2> i_ObjB)
        {
            return i_ObjA.First.Equals(i_ObjB.First) && i_ObjA.Second.Equals(i_ObjB.Second);
        }

        public static bool operator !=(PairStruct<T1, T2> i_ObjA, PairStruct<T1, T2> i_ObjB)
        {
            return !(i_ObjA == i_ObjB);
        }

        public override bool Equals(object i_Other)
        {
            PairStruct<T1, T2> otherPair = (PairStruct<T1, T2>) i_Other;
            return this == otherPair;
        }

        public bool Equals(PairStruct<T1, T2> i_Other)
        {
            return this == i_Other;
        }

        public override int GetHashCode()
        {
            return MathHelper.CombineHashCodes(First.GetHashCode(), Second.GetHashCode());
        }
    }
}
