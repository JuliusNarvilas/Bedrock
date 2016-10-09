using Common.IO;
using System;
using UnityEngine;
using System.Collections.Generic;
using Common.Text;

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
        public List<IntelligentTextInsertRecord> inserts;
        public List<IntelligentTextStyleRecord> styles;
        public List<IntelligentTextImageRecord> images;
        public List<IntelligentTextTransform> transforms;
    }

    [Serializable]
    public struct IntelligentTextInsertRecord
    {
        public string id;
        public string data;
    }
    
    public class IntelligentTextLocalization
    {
        public Dictionary<string, string> Inserts;
        public Dictionary<string, IntelligentTextStyle> Styles;
        public Dictionary<string, Sprite> Images;
        public Dictionary<string, IntelligentTextTransform> Transforms;
        public List<ResourcesDBItem> ImageResources;

        public static IntelligentTextLocalization Create()
        {
            return new IntelligentTextLocalization()
            {
                Inserts = new Dictionary<string, string>(),
                Styles = new Dictionary<string, IntelligentTextStyle>(),
                Images = new Dictionary<string, Sprite>(),
                Transforms = new Dictionary<string, IntelligentTextTransform>(),
                ImageResources = new List<ResourcesDBItem>()
            };
        }

        public void Append(IntelligentTextLocalizationData i_Data)
        {
            int insertsCount = i_Data.inserts.Count;
            IntelligentTextInsertRecord insertRecord;
            for (int i = 0; i < insertsCount; ++i)
            {
                insertRecord = i_Data.inserts[i];
#if UNITY_EDITOR
                if (Inserts.ContainsKey(insertRecord.id))
                {
                    Debug.LogWarningFormat("IntelligentText Localization overwrites existing insert entry with id: {0}", insertRecord.id);
                }
#endif
                Inserts[insertRecord.id] = insertRecord.data;
            }


            int stylesCount = i_Data.styles.Count;
            IntelligentTextStyleRecord styleRecord;
            for (int i = 0; i < stylesCount; ++i)
            {
                styleRecord = i_Data.styles[i];
#if UNITY_EDITOR
                if (Styles.ContainsKey(styleRecord.id))
                {
                    Debug.LogWarningFormat("IntelligentText Localization overwrites existing style entry with id: {0}", styleRecord.id);
                }
#endif
                var resource = ResourcesDB.GetByPath(styleRecord.fontPath);
                if (resource != null)
                {
                    var fontAsset = resource.Load<Font>();
                    if (fontAsset != null)
                    {
                        Styles[styleRecord.id] = new IntelligentTextStyle()
                        {
                            Color = styleRecord.color,
                            Font = fontAsset,
                            FontSize = styleRecord.fontSize,
                            LineSpacing = styleRecord.lineSpacing
                        };
                    }
                }
#if UNITY_EDITOR
                if(!Styles.ContainsKey(styleRecord.id))
                {
                    Debug.LogErrorFormat("IntelligentText font not found ({0}) for style id: {1}", styleRecord.fontPath, styleRecord.id);
                }
#endif
            }

            int imagesCount = i_Data.images.Count;
            IntelligentTextImageRecord imageRecord;
            for (int i = 0; i < imagesCount; ++i)
            {
                imageRecord = i_Data.images[i];
#if UNITY_EDITOR
                if (Images.ContainsKey(imageRecord.id))
                {
                    Debug.LogWarningFormat("IntelligentText Localization overwrites existing image entry with id: {0}", imageRecord.id);
                }
#endif
                var resource = ResourcesDB.GetByPath(imageRecord.path);
                if (resource != null)
                {
                    var spriteAsset = resource.Load<Sprite>(imageRecord.sprite);
                    if (spriteAsset != null)
                    {
                        Images[imageRecord.id] = spriteAsset;
                        ImageResources.Add(resource);
                    }
                    else
                    {
                        resource.Unload();
                    }
                }
#if UNITY_EDITOR
                if (!Images.ContainsKey(imageRecord.id))
                {
                    Debug.LogErrorFormat("IntelligentText Localization image not found ({0}) for id: {1}", imageRecord.path, imageRecord.id);
                }
#endif
            }

            int transformsCount = i_Data.transforms.Count;
            IntelligentTextTransform transformRecord;
            for (int i = 0; i < transformsCount; ++i)
            {
                transformRecord = i_Data.transforms[i];
#if UNITY_EDITOR
                if (Transforms.ContainsKey(transformRecord.id))
                {
                    Debug.LogWarningFormat("IntelligentText Localization overwrites existing transform entry with id: {0}", transformRecord.id);
                }
#endif
                Transforms[transformRecord.id] = transformRecord;
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
            if (Images != null)
            {
                Images.Clear();
            }
            if (Transforms != null)
            {
                Transforms.Clear();
            }
            if(ImageResources != null)
            {
                int resourceCount = ImageResources.Count;
                for(int i = 0; i < resourceCount; ++i)
                {
                    ImageResources[i].Unload();
                }
                ImageResources.Clear();
            }
        }
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

    public class IntelligentTextDataImageNode : IntelligentTextDataNode
    {
        Sprite Image;
        IntelligentTextTransform Transform;

        public IntelligentTextDataImageNode(int i_Id, int i_TextStartIndex, int i_TextEndIndex, string i_InteractorId, Sprite i_Image, IntelligentTextTransform i_Transform) :
            base(i_Id, i_TextStartIndex, i_TextEndIndex, i_InteractorId, IntelligentTextDataType.Image)
        {
            Image = i_Image;
            Transform = i_Transform;
        }
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
