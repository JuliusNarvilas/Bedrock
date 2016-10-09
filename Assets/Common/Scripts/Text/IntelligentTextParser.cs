using RestSharp.Contrib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace Common.Text
{
    public struct IntelligentTextParser : IDisposable
    {
        public static readonly string INSERT_TAG = "insert";
        public static readonly string INSERT_DONE_TAG = "_insert";
        public static readonly string GROUP_TAG = "group";
        public static readonly string IMAGE_TAG = "image";

        public static readonly string INTERACTOR_ID_ATTRIBUTE = "interactorId";
        public static readonly string ID_ATTRIBUTE = "id";
        public static readonly string TRANSFORM_ID_ATTRIBUTE = "transformId";

        private static readonly string ROOT_TAG = "root";
        private const float SPACE_PLACEHOLDER_REPLACE_SIZE = 10;
        private const float SPACE_PLACEHOLDER_MEASURE_SIZE = 100;
        private static readonly string SPACE_PLACEHOLDER_START = string.Format("<size={0}>", (int)SPACE_PLACEHOLDER_MEASURE_SIZE);
        private static readonly string SPACE_PLACEHOLDER_END = "</size>";
        private static readonly string SPACE_PLACEHOLDER_STR = "|";

        public TextGenerationSettings TextSettings;
        public IntelligentTextDataNode DataRoot { get { return m_DataList[0]; } }
        public Mesh Mesh { get { return m_Mesh; } }
        
        private List<IntelligentTextDataNode> m_DataList;
        private TextGenerator m_TextGenerator;
        private Vector2 m_SpacePlaceholderSize;
        private Mesh m_Mesh;

        private void ReplaceInsersts(XmlDocument i_Document)
        {
            int recursionCount = 0;
            XmlNodeList elementList = null;
            do
            {
                ++recursionCount;
                elementList = i_Document.GetElementsByTagName(INSERT_TAG);

                int size = elementList.Count;
                XmlNode element;
                for(int i = 0; i < size; ++i)
                {
                    element = elementList[i];
                    var idAttribute = element.Attributes["id"];
                    if (idAttribute != null)
                    {
                        var id = idAttribute.Value;
                        if (!string.IsNullOrEmpty(id))
                        {
                            var newElement = i_Document.CreateElement(INSERT_DONE_TAG);
                            string insertValue = IntelligentTextSettings.Instance.GetInsert(id);
                            newElement.InnerXml = insertValue;
                            element.ParentNode.ReplaceChild(newElement, element);
                        }
                    }
                }
            }
            while (elementList.Count > 0 && recursionCount < 5);
        }

        private void ConstructIntelligentTextElements()
        {

        }

        public void BuildFinalText(StringBuilder i_TextAccumulator, XmlNode i_XmlContainer, IntelligentTextDataNode i_Parent, int i_CharCounter = 0)
        {
            foreach (XmlNode node in i_XmlContainer)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Text:
                        {
                            int startIndex = i_CharCounter;
                            string decodedString = HttpUtility.HtmlDecode(node.InnerText);
                            i_TextAccumulator.Append(decodedString);
                            int textLength = node.InnerText.Length;
                            i_CharCounter += textLength;
                            int lastIndex = i_Parent.Children.Count - 1;

                            if (lastIndex >= 0 && i_Parent.Children[lastIndex].Type == IntelligentTextDataType.Text)
                            {
                                i_Parent.Children[lastIndex].TextEndIndex += textLength;
                                continue;
                            }

                            IntelligentTextDataNode data = new IntelligentTextDataNode(
                                    m_DataList.Count,
                                    startIndex,
                                    i_CharCounter,
                                    null,
                                    IntelligentTextDataType.Text
                                );

                            i_Parent.Children.Add(data);
                            m_DataList.Add(data);
                            break;
                        }
                    case XmlNodeType.Element:
                        if(node.Name == INSERT_DONE_TAG)
                        {
                            BuildFinalText(i_TextAccumulator, node, i_Parent, i_CharCounter);
                        }
                        else if (node.Name == GROUP_TAG)
                        {
                            var attributeContainer = node.Attributes[INTERACTOR_ID_ATTRIBUTE];
                            string interactorValue = attributeContainer != null ? attributeContainer.Value : null;
                            IntelligentTextDataNode data = new IntelligentTextDataNode(
                                    m_DataList.Count,
                                    i_CharCounter,
                                    i_CharCounter,
                                    interactorValue,
                                    IntelligentTextDataType.Group
                                );
                            i_Parent.Children.Add(data);
                            m_DataList.Add(data);
                            BuildFinalText(i_TextAccumulator, node, data, i_CharCounter);
                            data.TextEndIndex = m_DataList.Last().TextEndIndex;
                        }
                        else if (node.Name == IMAGE_TAG)
                        {
                            var idContainer = node.Attributes[ID_ATTRIBUTE];
                            var transformContainer = node.Attributes[TRANSFORM_ID_ATTRIBUTE];
                            if (idContainer != null && transformContainer != null)
                            {
                                Sprite image = IntelligentTextSettings.Instance.GetImage(idContainer.Value);
                                IntelligentTextTransform transform = IntelligentTextSettings.Instance.GetTransform(transformContainer.Value);
                                if (image != null && transform != null)
                                {
                                    int startIndex = i_CharCounter;
                                    Vector2 imageSize = transform.scale * TextSettings.fontSize;
                                    int imagePlaceholderHighScale = (int)(imageSize.y / m_SpacePlaceholderSize.y + 0.5f);
                                    float imagePlaceholderLargeWidth = imagePlaceholderHighScale * m_SpacePlaceholderSize.x;
                                    imageSize.x -= imagePlaceholderLargeWidth;

                                    string imagePlaceholderForHeightStart = string.Format("<size={0}>", imagePlaceholderHighScale);
                                    int largePlacementLength = imagePlaceholderForHeightStart.Length + SPACE_PLACEHOLDER_STR.Length + SPACE_PLACEHOLDER_END.Length;
                                    i_TextAccumulator.Append(imagePlaceholderForHeightStart);
                                    i_TextAccumulator.Append(SPACE_PLACEHOLDER_STR);
                                    i_TextAccumulator.Append(SPACE_PLACEHOLDER_END);
                                    i_CharCounter += largePlacementLength;

                                    bool fitLargeEnding = imageSize.x >= imagePlaceholderLargeWidth;
                                    if (fitLargeEnding)
                                    {
                                        imageSize.x -= imagePlaceholderLargeWidth;
                                    }

                                    float imagePlaceholderSmallWidth = SPACE_PLACEHOLDER_REPLACE_SIZE * m_SpacePlaceholderSize.x;
                                    int smallPlacementCount = (int)(imageSize.x / imagePlaceholderSmallWidth + 0.5f);
                                    if (smallPlacementCount > 0)
                                    {
                                        string imagePlaceholderForWidthStart = string.Format("<size={0}>", SPACE_PLACEHOLDER_REPLACE_SIZE);
                                        i_TextAccumulator.Append(imagePlaceholderForWidthStart);
                                        i_CharCounter += imagePlaceholderForWidthStart.Length + SPACE_PLACEHOLDER_END.Length + (SPACE_PLACEHOLDER_STR.Length + smallPlacementCount);

                                        for (int i = 0; i < smallPlacementCount; ++i)
                                        {
                                            i_TextAccumulator.Append(SPACE_PLACEHOLDER_STR);
                                        }
                                        i_TextAccumulator.Append(SPACE_PLACEHOLDER_END);
                                    }

                                    if(fitLargeEnding)
                                    {
                                        i_TextAccumulator.Append(imagePlaceholderForHeightStart);
                                        i_TextAccumulator.Append(SPACE_PLACEHOLDER_STR);
                                        i_TextAccumulator.Append(SPACE_PLACEHOLDER_END);
                                        i_CharCounter += largePlacementLength;
                                    }

                                    var interactorContainer = node.Attributes[INTERACTOR_ID_ATTRIBUTE];
                                    string interactorValue = interactorContainer != null ? interactorContainer.Value : null;
                                    IntelligentTextDataNode data = new IntelligentTextDataNode(
                                            m_DataList.Count,
                                            startIndex,
                                            i_CharCounter,
                                            interactorValue,
                                            IntelligentTextDataType.Image
                                        );
                                    i_Parent.Children.Add(data);
                                    m_DataList.Add(data);
                                }
                            }
                        }
                        break;
                }
            }
        }

        private void Setup()
        {
            if (m_TextGenerator == null)
            {
                m_DataList = new List<IntelligentTextDataNode>();
                m_TextGenerator = new TextGenerator();
            }
            m_DataList.Clear();
            TextGenerationSettings tempTextSettings = new TextGenerationSettings()
            {
                color = Color.black,
                font = TextSettings.font,
                fontSize = 10,
                lineSpacing = 1,
                alignByGeometry = false,
                fontStyle = FontStyle.Normal,
                generateOutOfBounds = true,
                generationExtents = new Vector2(100, 100),
                horizontalOverflow = HorizontalWrapMode.Overflow,
                pivot = new Vector2(0.5f, 0.5f),
                resizeTextForBestFit = false,
                resizeTextMaxSize = 600,
                resizeTextMinSize = 6,
                richText = true,
                scaleFactor = 1,
                textAnchor = TextAnchor.MiddleCenter,
                updateBounds = true,
                verticalOverflow = VerticalWrapMode.Overflow
            };
            //update the spacing placeholder width for selected font
            string spacePlaceholderTestStr = string.Format("{0}{1}{2}", SPACE_PLACEHOLDER_START, SPACE_PLACEHOLDER_STR, SPACE_PLACEHOLDER_END);
            m_TextGenerator.Populate(spacePlaceholderTestStr, tempTextSettings);
            m_SpacePlaceholderSize.x = m_TextGenerator.rectExtents.width / SPACE_PLACEHOLDER_MEASURE_SIZE;
            m_SpacePlaceholderSize.y = m_TextGenerator.rectExtents.height / SPACE_PLACEHOLDER_MEASURE_SIZE;
        }

        public void Parse(string i_Text)
        {
            Setup();
            var document = new XmlDocument();
            var dataRoot = new IntelligentTextDataNode(0, -1, -1, null);
            m_DataList.Add(dataRoot);

            XmlElement xmlRoot = document.CreateElement(ROOT_TAG);
            document.AppendChild(xmlRoot);
            xmlRoot.InnerXml = i_Text;

            ReplaceInsersts(document);
            StringBuilder textAccumulator = new StringBuilder();
            BuildFinalText(textAccumulator, xmlRoot, dataRoot);
            UpdateMesh(textAccumulator.ToString());

            document.RemoveAll();
        }

        public void UpdateMesh(string i_Text)
        {
            if(m_Mesh != null)
            {
                m_Mesh.Clear();
#if UNITY_EDITOR
                UnityEngine.Object.DestroyImmediate(m_Mesh);
#else
                UnityEngine.Object.Destroy(m_Mesh);
#endif
            }
            m_Mesh = new Mesh();
            m_Mesh.name = "Text Mesh";
            m_Mesh.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.NotEditable;

            m_TextGenerator.Populate(i_Text, TextSettings);
            m_TextGenerator.GetMesh(m_Mesh);
        }

        public void Dispose()
        {
            if (m_Mesh != null)
            {
                m_Mesh.Clear();
                UnityEngine.Object.Destroy(m_Mesh);
            }
            m_Mesh = null;
            using (m_TextGenerator)
            { }
        }
    }
}
