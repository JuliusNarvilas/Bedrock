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
        private Vector2 m_SpacePlaceholderSizePerUnit;
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

        private void AddDataNode()
        {

        }

        public int BuildPlaceholderText(IntelligentTextTransform i_Transform, StringBuilder i_TextAccumulator)
        {
            Vector2 imageSize = i_Transform.scale * TextSettings.fontSize;
            float imagePlaceholderWidth = SPACE_PLACEHOLDER_REPLACE_SIZE * m_SpacePlaceholderSizePerUnit.x;
            int placementCount = (int)(imageSize.x / imagePlaceholderWidth + 0.5f);
            if(placementCount <= 0)
            {
                placementCount = 1;
            }
            string imagePlaceholderForWidthStart = string.Format("<size={0}>", SPACE_PLACEHOLDER_REPLACE_SIZE);
            i_TextAccumulator.Append(imagePlaceholderForWidthStart);
            for (int i = 0; i < placementCount; ++i)
            {
                i_TextAccumulator.Append(SPACE_PLACEHOLDER_STR);
            }
            i_TextAccumulator.Append(SPACE_PLACEHOLDER_END);

            return imagePlaceholderForWidthStart.Length + SPACE_PLACEHOLDER_END.Length + (SPACE_PLACEHOLDER_STR.Length * placementCount);
        }

        public void BuildFinalText(StringBuilder i_TextAccumulator, XmlNode i_XmlContainer, IntelligentTextDataNode i_Parent, int i_CharCounter = 0)
        {
            foreach (XmlNode node in i_XmlContainer)
            {
                int startIndex = i_CharCounter;
                switch (node.NodeType)
                {
                    case XmlNodeType.Text:
                        {
                            string decodedString = HttpUtility.HtmlDecode(node.InnerText);
                            i_TextAccumulator.Append(decodedString);
                            int textLength = decodedString.Length;
                            i_CharCounter += textLength;
                            int lastIndex = i_Parent.Children.Count - 1;

                            if (lastIndex >= 0 && i_Parent.Children[lastIndex].Type == IntelligentTextDataType.Text)
                            {
                                i_Parent.Children[lastIndex].TextEndIndex += textLength;
                            }
                            else
                            {
                                IntelligentTextDataNode data = new IntelligentTextDataNode(
                                        m_DataList.Count,
                                        startIndex,
                                        i_CharCounter,
                                        null,
                                        IntelligentTextDataType.Text
                                    );

                                i_Parent.Children.Add(data);
                                m_DataList.Add(data);
                            }
                        }
                        break;
                    case XmlNodeType.Element:
                        if(node.Name == INSERT_DONE_TAG)
                        {
                            BuildFinalText(i_TextAccumulator, node, i_Parent, i_CharCounter);
                        }
                        else if (node.Name == GROUP_TAG)
                        {
                            var interactorContainer = node.Attributes[INTERACTOR_ID_ATTRIBUTE];
                            string interactorValue = interactorContainer != null ? interactorContainer.Value : null;
                            IntelligentTextDataNode data = new IntelligentTextDataNode(
                                    m_DataList.Count,
                                    startIndex,
                                    startIndex,
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
                                    i_CharCounter += BuildPlaceholderText(transform, i_TextAccumulator);

                                    var interactorContainer = node.Attributes[INTERACTOR_ID_ATTRIBUTE];
                                    string interactorValue = interactorContainer != null ? interactorContainer.Value : null;
                                    IntelligentTextDataImageNode data = new IntelligentTextDataImageNode(
                                            m_DataList.Count,
                                            startIndex,
                                            i_CharCounter,
                                            interactorValue,
                                            image,
                                            transform
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
            m_SpacePlaceholderSizePerUnit.x = m_TextGenerator.rectExtents.width / SPACE_PLACEHOLDER_MEASURE_SIZE;
            m_SpacePlaceholderSizePerUnit.y = m_TextGenerator.rectExtents.height / SPACE_PLACEHOLDER_MEASURE_SIZE;
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
            GenerateMesh(textAccumulator.ToString());

            document.RemoveAll();
        }

        public void GenerateMesh(string i_Text)
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

            int vertSize = m_TextGenerator.vertexCount;
            Vector3[] tempVerts = new Vector3[vertSize];
            Color32[] tempColours = new Color32[vertSize];
            Vector2[] tempUvs = new Vector2[vertSize];
            IList<UIVertex> generatorVerts = m_TextGenerator.verts;
            for (int i = 0; i < vertSize; ++i)
            {
                tempVerts[i] = generatorVerts[i].position;
                tempColours[i] = generatorVerts[i].color;
                tempUvs[i] = generatorVerts[i].uv0;
            }
            
            m_Mesh.vertices = tempVerts;
            m_Mesh.colors32 = tempColours;
            m_Mesh.uv = tempUvs;

            int dataNodeCount = m_DataList.Count;
            for (int i = 0; i < dataNodeCount; ++i)
            {
                var dataNode = m_DataList[i];
                switch (dataNode.Type)
                {
                    case IntelligentTextDataType.None: break;
                    case IntelligentTextDataType.Group: break;
                    case IntelligentTextDataType.Image:
                        break;
                    case IntelligentTextDataType.Text:
                        break;
                }
            }

            int characterCount = vertSize / 4;
            int[] tempIndices = new int[characterCount * 6];
            for (int i = 0; i < characterCount; ++i)
            {
                int vertIndexStart = i * 4;
                int trianglesIndexStart = i * 6;
                tempIndices[trianglesIndexStart++] = vertIndexStart;
                tempIndices[trianglesIndexStart++] = vertIndexStart + 1;
                tempIndices[trianglesIndexStart++] = vertIndexStart + 2;
                tempIndices[trianglesIndexStart++] = vertIndexStart;
                tempIndices[trianglesIndexStart++] = vertIndexStart + 2;
                tempIndices[trianglesIndexStart] = vertIndexStart + 3;
            }
            o_Mesh.triangles = tempIndices;
            //TODO: setBounds manually
            o_Mesh.RecalculateBounds();
        }

        private int[] GenerateTriangles(IntelligentTextDataImageNode i_Data)
        {
            int characterCount = i_Data.TextEndIndex - i_Data.TextStartIndex;
            int[] result = new int[characterCount * 6];
            for (int i = 0; i < characterCount; ++i)
            {
                int vertIndexStart = (i_Data.TextStartIndex + i) * 4;
                int trianglesIndexStart = (i_Data.TextStartIndex + i) * 6;
                result[trianglesIndexStart++] = vertIndexStart;
                result[trianglesIndexStart++] = vertIndexStart + 1;
                result[trianglesIndexStart++] = vertIndexStart + 2;
                result[trianglesIndexStart++] = vertIndexStart;
                result[trianglesIndexStart++] = vertIndexStart + 2;
                result[trianglesIndexStart] = vertIndexStart + 3;
            }
            return result;
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
