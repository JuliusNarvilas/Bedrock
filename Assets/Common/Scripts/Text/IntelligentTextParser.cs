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
        private static readonly string ROOT_TAG = "root";

        public static readonly string INSERT_TAG = "insert";
        public static readonly string INSERT_DONE_TAG = "_insert";
        public static readonly string GROUP_TAG = "group";
        public static readonly string IMAGE_TAG = "image";

        public static readonly string INTERACTOR_ID_ATTRIBUTE = "interactorId";
        public static readonly string ID_ATTRIBUTE = "id";
        public static readonly string TRANSFORM_ID_ATTRIBUTE = "transformId";
        
        private const float SPACE_PLACEHOLDER_MEASURE_SIZE = 100;
        public static readonly string SPACE_PLACEHOLDER_STR = "|";
        
        private List<IntelligentTextDataNode> m_DataList;
        private TextGenerator m_TextGenerator;
        private Vector2 m_SpacePlaceholderSizePerUnit;
        private Vector2 m_SpacePlaceholderSize;
        private Mesh m_Mesh;

        public TextGenerationSettings TextSettings;

        public Vector2 SpacePlaceholderSizePerUnit { get { return m_SpacePlaceholderSizePerUnit; } }
        public Vector2 SpacePlaceholderEstimatedSize { get { return m_SpacePlaceholderSizePerUnit * TextSettings.fontSize; } }
        public Vector2 SpacePlaceholderSize { get { return m_SpacePlaceholderSize; } }
        public IntelligentTextDataNode DataRoot { get { return m_DataList[0]; } }
        public Mesh Mesh { get { return m_Mesh; } }

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

        public void BuildTextData(XmlNode i_XmlContainer, IntelligentTextDataNode i_Parent)
        {
            foreach (XmlNode node in i_XmlContainer)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Text:
                        {
                            IntelligentTextDataNode data = new IntelligentTextDataTextNode(
                                m_DataList.Count,
                                null,
                                node.InnerText
                            );

                            int lastSiblingIndex = i_Parent.Children.Count - 1;
                            //attempt to merge sibling nodes
                            if (lastSiblingIndex < 0 || !i_Parent.Children[lastSiblingIndex].Merge(data))
                            {
                                //track a new node if merging failed
                                i_Parent.Children.Add(data);
                                m_DataList.Add(data);
                            }
                        }
                        break;
                    case XmlNodeType.Element:
                        if(node.Name == INSERT_DONE_TAG)
                        {
                            BuildTextData(node, i_Parent);
                        }
                        else if (node.Name == GROUP_TAG)
                        {
                            var interactorContainer = node.Attributes[INTERACTOR_ID_ATTRIBUTE];
                            string interactorValue = interactorContainer != null ? interactorContainer.Value : null;
                            IntelligentTextDataNode data = new IntelligentTextDataGroupNode(
                                    m_DataList.Count,
                                    interactorValue
                                );
                            i_Parent.Children.Add(data);
                            m_DataList.Add(data);
                            BuildTextData(node, data);
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
                                    var interactorContainer = node.Attributes[INTERACTOR_ID_ATTRIBUTE];
                                    string interactorValue = interactorContainer != null ? interactorContainer.Value : null;
                                    IntelligentTextDataImageNode data = new IntelligentTextDataImageNode(
                                            m_DataList.Count,
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

            TextGenerationSettings tempTextSettings = new TextGenerationSettings()
            {
                color = Color.black,
                font = TextSettings.font,
                fontSize = (int)(SPACE_PLACEHOLDER_MEASURE_SIZE + 0.5),
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
                richText = false,
                scaleFactor = 1,
                textAnchor = TextAnchor.MiddleCenter,
                updateBounds = true,
                verticalOverflow = VerticalWrapMode.Overflow
            };
            //update the spacing placeholder width for selected font
            m_TextGenerator.Populate(SPACE_PLACEHOLDER_STR, tempTextSettings);
            m_SpacePlaceholderSizePerUnit = m_TextGenerator.rectExtents.size / SPACE_PLACEHOLDER_MEASURE_SIZE;
        }

        public void BuildMesh()
        {
            StringBuilder textAccumulator = new StringBuilder();
            int textDataSize = m_DataList.Count;
            for (int i = 0; i < textDataSize; ++i)
            {
                m_DataList[i].BuildText(textAccumulator, ref this);
            }
            textAccumulator.Append(SPACE_PLACEHOLDER_STR);
            string finalText = textAccumulator.ToString();
            m_TextGenerator.Populate(finalText, TextSettings);

            var generatedChars = m_TextGenerator.characters;
            float generatedSpacePlaceholderWidth = 0;
            for(int i = SPACE_PLACEHOLDER_STR.Length; i > 0; --i)
            {
                var generatedSpacePlaceholderChar = generatedChars[finalText.Length - i];
                generatedSpacePlaceholderWidth += generatedSpacePlaceholderChar.charWidth;
            }
            m_SpacePlaceholderSize = m_SpacePlaceholderSizePerUnit * (generatedSpacePlaceholderWidth / m_SpacePlaceholderSizePerUnit.x);

            List<IntelligentTextLineInfo>
            var generatedLines = m_TextGenerator.lines;
            generatedLines[0].

            IList<UIVertex> generatorVerts = m_TextGenerator.verts;
            int vertSize = generatorVerts.Count;
            var initialMeshData = new IntelligentTextMeshData
            {
                Order = 0,
                Verts = new List<Vector3>(vertSize),
                Colors = new List<Color32>(vertSize),
                Uvs = new List<Vector2>(vertSize),
                SubMeshes = new List<IntelligentTextSubMeshData> { new IntelligentTextSubMeshData { Trinagles = new List<int>(vertSize / 4 * 6) } }
            };
            for (int i = 0; i < vertSize; ++i)
            {
                initialMeshData.Verts[i] = generatorVerts[i].position;
                initialMeshData.Colors[i] = generatorVerts[i].color;
                initialMeshData.Uvs[i] = generatorVerts[i].uv0;
            }
            List<IntelligentTextMeshData> meshSets = new List<IntelligentTextMeshData> { initialMeshData };
            int currentVertexIndex = 0;

            for (int i = 0; i < textDataSize; ++i)
            {
                m_DataList[i].BuildSubMesh(textAccumulator, ref this);
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


            if (m_Mesh != null)
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



            //TODO:!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            GenerateMesh(textAccumulator.ToString());
        }

        public void Parse(string i_Text)
        {
            m_DataList.Clear();
            Setup();
            var document = new XmlDocument();
            var dataRoot = new IntelligentTextDataNode(0);
            m_DataList.Add(dataRoot);

            XmlElement xmlRoot = document.CreateElement(ROOT_TAG);
            document.AppendChild(xmlRoot);
            xmlRoot.InnerXml = i_Text;

            ReplaceInsersts(document);
            BuildTextData(xmlRoot, dataRoot);
            BuildMesh();
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
