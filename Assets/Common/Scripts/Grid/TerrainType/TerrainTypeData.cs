using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Common.Grid.TerrainType
{
    [Serializable]
    public class TerrainTypeData<TUserData> : ISerializable where TUserData : ISerializable
    {
        public readonly int Cost;
        public readonly int Index;
        public readonly string Name;
        public readonly TUserData UserData;

        public TerrainTypeData(int i_Index, int i_Cost, string i_Name, TUserData i_UserData)
        {
            Log.DebugAssert(
                i_Index.IsInRange(byte.MinValue, byte.MaxValue),
                string.Format("Invalid index argument: {0}. Expected range: [{1};{2}]", i_Index, byte.MinValue, byte.MaxValue));
            Log.DebugAssert(
                i_Cost.IsInRange(-1, sbyte.MaxValue),
                string.Format("Invalid cost argument: {0}. Expected range: [{1};{2}]", i_Cost, -1, short.MaxValue));
            Log.DebugAssert(
                !string.IsNullOrEmpty(i_Name),
                "Invalid name argument.");

            Cost = i_Cost;
            Index = i_Index;
            Name = i_Name;
            UserData = i_UserData;
        }

        protected TerrainTypeData(SerializationInfo info, StreamingContext context)
        {
            Cost = info.GetInt16("cost");
            Index = info.GetByte("index");
            Name = info.GetString("name");
            UserData = (TUserData)info.GetValue("userData", typeof(TUserData));

            Log.DebugAssert(
                Index.IsInRange(byte.MinValue, byte.MaxValue),
                string.Format("Invalid index: {0}. Expected range: [{1};{2}]", Index, byte.MinValue, byte.MaxValue));
            Log.DebugAssert(
                Cost.IsInRange(byte.MinValue, byte.MaxValue),
                string.Format("Invalid cost: {0}. Expected range: [{1};{2}]", Cost, byte.MinValue, byte.MaxValue));
            Log.DebugAssert(
                !string.IsNullOrEmpty(Name),
                "Invalid name.");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("cost", (short)Cost);
            info.AddValue("index", (byte)Index);
            info.AddValue("name", Name);
            info.AddValue("userData", UserData);
        }
    }
}
