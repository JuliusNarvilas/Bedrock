using Common.IO;
using System;
using UnityEngine;
using System.Collections.Generic;
using Common.Text;

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
    public struct IntelligentTextLocalizationData
    {
        public List<IntelligentTextLocalizationInsert> inserts;
        public List<IntelligentTextStyleRecord> styles;
    }

    [Serializable]
    public struct IntelligentTextLocalizationInsert
    {
        public string id;
        public string data;
    }
    
    public struct IntelligentTextLocalization
    {
        public Dictionary<string, string> Inserts;
        public Dictionary<string, IntelligentTextStyle> Styles;

        public static IntelligentTextLocalization Create()
        {
            return new IntelligentTextLocalization()
            {
                Inserts = new Dictionary<string, string>(),
                Styles = new Dictionary<string, IntelligentTextStyle>()
            };
        }

        public void Append(IntelligentTextLocalizationData i_Data)
        {
            int size = i_Data.inserts.Count;
            IntelligentTextLocalizationInsert insert;
            for (int i = 0; i < size; ++i)
            {
                insert = i_Data.inserts[i];
#if UNITY_EDITOR
                if (Inserts.ContainsKey(insert.id))
                {
                    Debug.LogWarningFormat("IntelligentText Localization overwrites existing insert entry with id: {0}", insert.id);
                }
#endif
                Inserts[insert.id] = insert.data;
            }


            size = i_Data.styles.Count;
            IntelligentTextStyleRecord styleRecord;
            for (int i = 0; i < size; ++i)
            {
                styleRecord = i_Data.styles[i];
#if UNITY_EDITOR
                if (Styles.ContainsKey(styleRecord.id))
                {
                    Debug.LogWarningFormat("IntelligentText Localization overwrites existing style entry with id: {0}", styleRecord.id);
                }
#endif
                var resource = ResourcesDB.GetByPath(styleRecord.data.fontPath);
                if (resource != null)
                {
                    var fontAsset = resource.Load<Font>();
                    if (fontAsset != null)
                    {
                        Styles[styleRecord.id] = new IntelligentTextStyle()
                        {
                            Color = styleRecord.data.color,
                            Font = fontAsset,
                            FontSize = styleRecord.data.fontSize,
                            LineSpacing = styleRecord.data.lineSpacing
                        };
                    }
                }
#if UNITY_EDITOR
                if(!Styles.ContainsKey(styleRecord.id))
                {
                    Debug.LogErrorFormat("IntelligentText Localization font not found ({0}) for style id: {1}", styleRecord.data.fontPath, styleRecord.id);
                }
#endif
            }
        }

        public void Clear()
        {
            if (Inserts != null)
            {
                Inserts.Clear();
            }
            if (Styles != null)
            {
                Styles.Clear();
            }
        }
    }

    [Serializable]
    public class IntelligentTextStyleData
    {
        public string fontPath;
        public Color color;
        public float lineSpacing;
        public int fontSize;
    }

    [Serializable]
    public struct IntelligentTextStyleRecord
    {
        public string id;
        public IntelligentTextStyleData data;
    }

    public class IntelligentTextStyle
    {
        public Color Color;
        public Font Font;
        public int FontSize;
        public float LineSpacing;
    }


    public enum IntelligentTextDataType
    {
        None,
        Group,
        Image,
        Text
    }

    public class IntelligentTextDataNode
    {
        public readonly int Id;
        public int TextStartIndex;
        //not inclusive
        public int TextEndIndex;
        public IntelligentTextDataType Type = IntelligentTextDataType.None;
        public string InteractorId;
        public readonly List<IntelligentTextDataNode> Children;
        public Bounds Bounds;

        public IntelligentTextDataNode(
            int i_Id,
            int i_TextStartIndex,
            int i_TextEndIndex,
            string i_InteractorId,
            IntelligentTextDataType i_Type = IntelligentTextDataType.None
        )
        {
            Id = i_Id;
            TextStartIndex = i_TextStartIndex;
            TextEndIndex = i_TextEndIndex;
            InteractorId = i_InteractorId;
            Children = new List<IntelligentTextDataNode>();
        }
    }












    [Serializable]
    public class IntelligentTextMaterialData
    {
        public ResourceReference FontReference;
        public Material Material;
    }

    [Serializable]
    public class IntelligentTextLocalizedMaterialData
    {
        public HashedString Language;
        public HashedString Id;
        public IntelligentTextMaterialData Data;
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
