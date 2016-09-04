using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml;

namespace Common.Text
{
    [RequireComponent(typeof(MeshFilter))]
    public class IntelligentText : MonoBehaviour
    {
        public static readonly string INSERT_TAG = "insert";
        public static readonly string INSERT_TAG_XPATH = "//" + INSERT_TAG;
        public static readonly string INSERT_DONE_TAG = "_insert";
        public static readonly string GROUP_TAG = "group";
        public static readonly string IMAGE_TAG = "image";

        [SerializeField]
        protected string m_Text;

        [SerializeField]
        protected Font m_Font;

        protected Mesh m_Mesh;
        protected TextGenerator m_TextGenerator;
        protected MeshRenderer m_MeshRenderer;
        protected CanvasRenderer m_CanvasRenderer;
        protected TextGenerationSettings m_TextSettings;

        public static void ParseIntelligentText(string i_Text)
        {
            //calculate element bounds
        }

        public static void ReplaceInsersts(XmlDocument i_Container)
        {
            int recursionCount = 0;
            XmlNodeList elementList = null;
            do
            {
                ++recursionCount;
                elementList = i_Container.GetElementsByTagName(INSERT_TAG);

                foreach (XmlNode element in elementList)
                {
                    var idAttribute = element.Attributes["id"];
                    var localizationAttribute = element.Attributes["localization"];
                    if (localizationAttribute != null && idAttribute != null)
                    {
                        var id = idAttribute.Value;
                        var localization = localizationAttribute.Value;
                        if (!string.IsNullOrEmpty(localization) && !string.IsNullOrEmpty(id))
                        {
                            var newElement = i_Container.CreateElement(INSERT_DONE_TAG);
                            //TODO: !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                            newElement.InnerXml = "";
                            element.ParentNode.ReplaceChild(newElement, element);
                        }
                    }
                }
            }
            while (elementList != null && elementList.Count > 0 && recursionCount < 5);
        }

        private void Awake()
        {
            m_Mesh = new Mesh();
            m_TextGenerator = new TextGenerator();
            m_MeshRenderer = GetComponent<MeshRenderer>();
            m_CanvasRenderer = GetComponent<CanvasRenderer>();
            m_TextSettings = new TextGenerationSettings();

            /*

            System.Xml.XmlDocument doc = new XmlDocument();
            XmlElement wrapper = doc.CreateElement("wrapper");
            wrapper.InnerXml = "pre string <insert> inner <insert/> <insert>string</insert> </insert><test testAttribute='true'/> post string";
            ParseInsersts(wrapper);
            string testString = doc.ToString();
            */
        }

        private void OnEnable()
        {
            m_Mesh.name = "Text Mesh";
            //m_Mesh.hideFlags = HideFlags.HideAndDontSave;

            //m_MeshRenderer.material.mainTexture = m_Font.material.mainTexture;

            m_TextSettings.textAnchor = TextAnchor.MiddleCenter;
            m_TextSettings.color = Color.red;
            m_TextSettings.generationExtents = new Vector2(100, 100);
            m_TextSettings.pivot = new Vector2(0.5f, 0.5f); ;
            m_TextSettings.richText = true;
            m_TextSettings.font = m_Font;
            m_TextSettings.fontSize = 14;
            m_TextSettings.fontStyle = FontStyle.Normal;
            m_TextSettings.verticalOverflow = VerticalWrapMode.Overflow;
            m_TextSettings.horizontalOverflow = HorizontalWrapMode.Wrap;
            m_TextSettings.lineSpacing = 1;
            m_TextSettings.generateOutOfBounds = true;
            m_TextSettings.resizeTextForBestFit = false;
            m_TextSettings.scaleFactor = 1f;

            //UnityEngine.UI.Text uiText = GetComponentInChildren<UnityEngine.UI.Text>();
            //var settings = uiText.GetGenerationSettings(new Vector2(100, 100));

            //m_TextGenerator = uiText.cachedTextGenerator;
            //m_TextGenerator.Populate("Test", settings);
            m_TextGenerator.Populate("Test yifit tf hfi f iytufdiyt fiytuf iuygf iyufiyyu tfd ytuifdiytfd ", m_TextSettings);



            m_TextGenerator.GetMesh(m_Mesh);

            if (m_CanvasRenderer != null)
            {
                //m_Mesh.ve(m_TextGenerator.verts as List<UIVertex>);
                m_CanvasRenderer.SetMaterial(m_Font.material, null);
                m_CanvasRenderer.SetMesh(m_Mesh);
            }
            else if (m_MeshRenderer != null)
            {
                GetComponent<MeshFilter>().mesh = m_Mesh;
                m_MeshRenderer.sharedMaterial = m_Font.material;
            }

        }

        private void Update()
        {

        }
    }
}
