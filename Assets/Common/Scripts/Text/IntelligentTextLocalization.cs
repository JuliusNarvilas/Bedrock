using Common.IO;
using System.Collections.Generic;
using UnityEngine;
using Common.Collections;
using System;
using System.Text;

namespace Common.Text
{
    public class IntelligentTextLocalization
    {
        private class LocalizationPlaceholders
        {
            public List<IntelligentTextKeyValueRecord> Values;
            public const char PLACEHOLDER_OPENING = '{';
            public const char PLACEHOLDER_CLOSING = '}';

            public string Localize(string i_Text)
            {
                bool matching = false;
                int size = i_Text.Length;
                StringBuilder result = new StringBuilder(size);
                for (int i = 0; i < size; ++i)
                {
                    char letter = i_Text[i];

                    if (!matching)
                    {
                        if (letter == PLACEHOLDER_OPENING)
                        {
                            matching = true;
                        }
                        else
                        {
                            result.Append(letter);
                        }
                    }
                    else
                    {
                        if (letter == PLACEHOLDER_CLOSING)
                        {
                            i_Text.IndexOf(placeholder, indexer, placeholder.length);
                        }
                    }
                }
            }

            public void Add(IntelligentTextKeyValueRecord i_Record)
            {
                Values.Add(i_Record);
            }

            public void Add(IList<IntelligentTextKeyValueRecord> i_Records)
            {
                Values.AddRange(i_Records);
            }
        }

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
            IntelligentTextKeyValueRecord insertRecord;
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
                if (!Styles.ContainsKey(styleRecord.id))
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
            if (ImageResources != null)
            {
                int resourceCount = ImageResources.Count;
                for (int i = 0; i < resourceCount; ++i)
                {
                    ImageResources[i].Unload();
                }
                ImageResources.Clear();
            }
        }
    }
}
