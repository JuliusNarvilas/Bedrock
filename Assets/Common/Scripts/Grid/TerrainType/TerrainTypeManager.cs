using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Common.Grid.TerrainType
{
    public class TerrainTypeManager<TUserData> where TUserData : ISerializable
    {
        private List<TerrainTypeData<TUserData>> m_TerrainTypes = new List<TerrainTypeData<TUserData>>();

        public void Register(TerrainTypeData<TUserData> i_TerrainTypeData)
        {
            Debug.Assert(i_TerrainTypeData != null, "invalid terrain type data argument.");

            for(int i = m_TerrainTypes.Count; i < i_TerrainTypeData.Index; ++i)
            {
                m_TerrainTypes.Add(null);
            }
            m_TerrainTypes.Add(i_TerrainTypeData);
        }

        public TerrainTypeData<TUserData> Get(int i_Index)
        {
            Debug.Assert(i_Index >= 0, string.Format("Invalid index argument: {0}", i_Index));

            if(m_TerrainTypes.Count > i_Index)
            {
                return m_TerrainTypes[i_Index];
            }

            return null;
        }
    }
}
