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

        public TextGenerationSettings TextSettings;
        public IntelligentTextDataNode DataRoot { get { return m_DataList[0]; } }
        public Mesh Mesh { get { return m_Mesh; } }
        
        private List<IntelligentTextDataNode> m_DataList;
        private TextGenerator m_TextGenerator;
        private float m_SpaceWidthAtSize10;
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
                            var localizationAttribute = element.Attributes["localization"];
                            var localization = localizationAttribute != null ? localizationAttribute.Value : null;
                            var newElement = i_Document.CreateElement(INSERT_DONE_TAG);

                            //TODO: !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                            newElement.InnerXml = "";
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

        public void AccumulateFinalText(StringBuilder i_TextAccumulator, XmlNode i_XmlContainer, IntelligentTextDataNode i_Parent, int i_CharCounter = 0)
        {
            foreach (XmlNode node in i_XmlContainer)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Text:
                        {
                            int startIndex = i_CharCounter;
                            i_CharCounter += node.InnerText.Length;
                            IntelligentTextDataNode data = new IntelligentTextDataNode(
                                    m_DataList.Count,
                                    startIndex,
                                    i_CharCounter,
                                    null,
                                    IntelligentTextDataType.Text
                                );
                            i_Parent.Children.Add(data);
                            m_DataList.Add(data);
                            i_TextAccumulator.Append(node.InnerText);
                            break;
                        }
                    case XmlNodeType.Element:
                        if (node.Name == GROUP_TAG)
                        {
                            var attributeContainer = node.Attributes[INTERACTOR_ID_ATTRIBUTE];
                            string attributeValue = attributeContainer != null ? attributeContainer.Value : null;
                            IntelligentTextDataNode data = new IntelligentTextDataNode(
                                    m_DataList.Count,
                                    i_CharCounter,
                                    i_CharCounter,
                                    attributeValue,
                                    IntelligentTextDataType.Group
                                );
                            i_Parent.Children.Add(data);
                            m_DataList.Add(data);
                            AccumulateFinalText(i_TextAccumulator, node, data, i_CharCounter);
                            data.TextEndIndex = m_DataList.Last().TextEndIndex;
                        }
                        else if (node.Name == IMAGE_TAG)
                        {
                            var idContainer = node.Attributes[ID_ATTRIBUTE];
                            var transformContainer = node.Attributes[TRANSFORM_ID_ATTRIBUTE];
                            if (idContainer != null && transformContainer != null)
                            {
                                //IntelligentTextSettingsManager.Instance.GetImage;
                                //IntelligentTextSettingsManager.Instance.GetTransform;
                                int startIndex = i_CharCounter;
                                i_CharCounter += (int) ((10.0f / m_SpaceWidthAtSize10) + 0.5f);
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
                        break;
                }
            }
        }

        public void Parse(string i_Text)
        {
            var document = new XmlDocument();
            var dataRoot = new IntelligentTextDataNode(0, -1, -1, null);
            if (m_TextGenerator == null)
            {
                m_DataList = new List<IntelligentTextDataNode>();
                m_TextGenerator = new TextGenerator();
            }
            m_DataList.Clear();
            m_DataList.Add(dataRoot);

            XmlElement xmlRoot = document.CreateElement(ROOT_TAG);
            document.AppendChild(xmlRoot);
            xmlRoot.InnerXml = i_Text;

            ReplaceInsersts(document);

            string test = "A<size=10> </size>AA";
            m_TextGenerator.Populate(test, TextSettings);
            m_SpaceWidthAtSize10 = m_TextGenerator.characters[0].charWidth;

            StringBuilder textAccumulator = new StringBuilder();
            AccumulateFinalText(textAccumulator, xmlRoot, dataRoot);

            UpdateMesh();

            document.RemoveAll();
        }

        public void UpdateMesh()
        {
            if(m_Mesh != null)
            {
                m_Mesh.Clear();
                UnityEngine.Object.Destroy(m_Mesh);
            }

            m_Mesh = new Mesh();
            m_Mesh.name = "Text Mesh";
            m_Mesh.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.NotEditable;

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
        }
    }
}
