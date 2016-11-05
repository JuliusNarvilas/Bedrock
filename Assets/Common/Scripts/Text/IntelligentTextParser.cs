using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using UnityEngine;
using Common.Collections;

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
        private List<Material> m_Materials;

        public TextGenerationSettings TextSettings;
        public Rect Rectangle;

        public Vector2 SpacePlaceholderSizePerUnit { get { return m_SpacePlaceholderSizePerUnit; } }
        public Vector2 SpacePlaceholderEstimatedSize { get { return m_SpacePlaceholderSizePerUnit * TextSettings.fontSize; } }
        public Vector2 SpacePlaceholderSize { get { return m_SpacePlaceholderSize; } }
        public IntelligentTextDataNode DataRoot { get { return m_DataList[0]; } }
        public Mesh Mesh { get { return m_Mesh; } }
        public List<Material> Materials { get { return m_Materials; } }

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

        public void RebuildMesh()
        {
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


            var generatedLines = m_TextGenerator.lines;
            int lineCount = generatedLines.Count;
            IList<UIVertex> generatorVerts = m_TextGenerator.verts;
            int vertSize = generatorVerts.Count;
            IntelligentTextMeshData initialMeshData = new IntelligentTextMeshData
            {
                Order = 0,
                TextLength = finalText.Length,
                Lines = new List<IntelligentTextLineInfo>(lineCount),
                Verts = new List<Vector3>(vertSize),
                Colors = new List<Color32>(vertSize),
                Uvs = new List<Vector2>(vertSize),
                SubMeshes = new List<IntelligentTextSubMeshData> {
                    new IntelligentTextSubMeshData {
                        Trinagles = new List<int>(vertSize / 4 * 6),
                        Material = TextSettings.font.material
                    }
                },
                ExtentRect = Rectangle
            };
            for(int i = 0; i < lineCount; ++i)
            {
                var line = new IntelligentTextLineInfo() {
                    Height = generatedLines[i].height,
                    StartCharIndex = generatedLines[i].startCharIdx
                };
                initialMeshData.Lines.Add(line);
            }
            for (int i = 0; i < vertSize; ++i)
            {
                initialMeshData.Verts.Add(generatorVerts[i].position);
                initialMeshData.Colors.Add(generatorVerts[i].color);
                initialMeshData.Uvs.Add(generatorVerts[i].uv0);
            }
            List<IntelligentTextMeshData> meshDataList = new List<IntelligentTextMeshData>() { initialMeshData };


            int currentVertexIndex = 0;
            for (int i = 0; i < textDataSize; ++i)
            {
                currentVertexIndex = m_DataList[i].BuildSubMesh(currentVertexIndex, meshDataList, ref this);
            }

            meshDataList.InsertionSort(IntelligentTextMeshData.Sorter.Ascending);

            var combinedData = meshDataList[0];
            for (int i = 1; i < meshDataList.Count; ++i)
            {
                var tempMeshData = meshDataList[i];

                //adjust the indices to work with the merged vertex data
                int combinedDataVertCount = combinedData.Verts.Count;
                int subMeshCount = tempMeshData.SubMeshes.Count;
                for(int j = 0; j < subMeshCount; ++j)
                {
                    var subMesh = tempMeshData.SubMeshes[i];
                    int indicesCount = subMesh.Trinagles.Count;
                    for (int k = 0; k < indicesCount; ++k)
                    {
                        subMesh.Trinagles[k] += combinedDataVertCount;
                    }
                }

                combinedData.Verts.AddRange(tempMeshData.Verts);
                combinedData.Colors.AddRange(tempMeshData.Colors);
                combinedData.Uvs.AddRange(tempMeshData.Uvs);
                combinedData.SubMeshes.AddRange(tempMeshData.SubMeshes);
            }

            m_Mesh.vertices = combinedData.Verts.ToArray();
            m_Mesh.colors32 = combinedData.Colors.ToArray();
            m_Mesh.uv = combinedData.Uvs.ToArray();
            int combinedSubMeshCount = combinedData.SubMeshes.Count;
            m_Mesh.subMeshCount = combinedSubMeshCount;
            if(m_Materials == null)
            {
                m_Materials = new List<Material>(combinedSubMeshCount);
            }
            else
            {
                m_Materials.Clear();
            }
            for(int i = 0; i < combinedSubMeshCount; ++i)
            {
                m_Mesh.SetTriangles(combinedData.SubMeshes[i].Trinagles, i);
                m_Materials.Add(combinedData.SubMeshes[i].Material);
            }

            //m_Mesh.RecalculateBounds();
            //TODO: update data bounds after inserted image adjustment
        }

        public void Parse(string i_Text)
        {
            Setup();
            m_DataList.Clear();
            var document = new XmlDocument();
            var dataRoot = new IntelligentTextDataNode(0);
            m_DataList.Add(dataRoot);

            XmlElement xmlRoot = document.CreateElement(ROOT_TAG);
            document.AppendChild(xmlRoot);
            xmlRoot.InnerXml = i_Text;

            ReplaceInsersts(document);
            BuildTextData(xmlRoot, dataRoot);

            RebuildMesh();
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
