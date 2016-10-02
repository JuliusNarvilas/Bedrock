using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml;
using System.Collections;

namespace Common.Text
{
    [RequireComponent(typeof(RectTransform))]
    public class IntelligentText : MonoBehaviour
    {
        protected enum RenderMode
        {
            ConvasRenderer,
            MeshRenderer,
            Unknown,
            Invalid
        }

        [SerializeField]
        [TextArea]
        protected string m_Text;
        [SerializeField]
        protected string m_StyleId;
        [SerializeField]
        protected bool m_GenerateOutOfBounds;
        [SerializeField]
        protected HorizontalWrapMode m_HorizontalOverflow;
        [SerializeField]
        protected bool m_BestFit;
        [SerializeField]
        protected float m_ScaleFactor = 1;
        [SerializeField]
        protected TextAnchor m_Anchor = TextAnchor.MiddleCenter;
        [SerializeField]
        protected VerticalWrapMode m_VerticalOverflow;

        
        public string Style
        {
            get { return m_StyleId; }
            set
            {
                m_StyleId = value;
                Refresh();
            }
        }

        
        protected RenderMode m_RenderMode = RenderMode.Unknown;
        protected IntelligentTextParser m_Parser = new IntelligentTextParser();

        public string Text
        {
            get { return m_Text; }
            set
            {
                m_Text = value;
                UpdateText();
            }
        }

        private void InitialiseRenderMode()
        {
            m_RenderMode = RenderMode.Unknown;
            if (GetComponent<CanvasRenderer>() != null)
            {
                m_RenderMode = RenderMode.ConvasRenderer;
            }
            else
            {
                if (GetComponent<MeshRenderer>() != null)
                {
                    m_RenderMode = RenderMode.MeshRenderer;
                    var meshFilter = GetComponent<MeshFilter>();
                    if (meshFilter == null)
                    {
                        meshFilter = gameObject.AddComponent<MeshFilter>();
                    }
                    meshFilter.sharedMesh = null;
                    meshFilter.hideFlags = HideFlags.None;
                }
            }

            switch (m_RenderMode)
            {
                case RenderMode.ConvasRenderer:
                case RenderMode.MeshRenderer:
                    break;
                case RenderMode.Unknown:
                    m_RenderMode = RenderMode.ConvasRenderer;
                    gameObject.AddComponent<CanvasRenderer>();
                    break;
            }
        }

        public void UpdateText()
        {
            if(m_RenderMode == RenderMode.Unknown || m_RenderMode == RenderMode.Invalid)
            {
                InitialiseRenderMode();
            }

            m_Parser.Parse(m_Text);
            
            switch (m_RenderMode)
            {
                case RenderMode.ConvasRenderer:
                    {
                        var canvasRenderer = GetComponent<CanvasRenderer>();
                        canvasRenderer.SetMaterial(m_Parser.TextSettings.font.material, null);
                        canvasRenderer.SetMesh(m_Parser.Mesh);
                    }
                    break;
                case RenderMode.MeshRenderer:
                    {
                        var meshFilter = GetComponent<MeshFilter>();
                        var meshRenderer = GetComponent<MeshRenderer>();
                        meshFilter.sharedMesh = m_Parser.Mesh;
                        meshRenderer.sharedMaterial = m_Parser.TextSettings.font.material;
                    }
                    break;
            }
        }

        public void Refresh()
        {
            IntelligentTextStyle style = IntelligentTextSettingsManager.GetStyle(m_StyleId);
            if (style != null)
            {
                var transform = GetComponent<RectTransform>();

                m_Parser.TextSettings.color = style.Color;
                m_Parser.TextSettings.font = style.Font;
                m_Parser.TextSettings.fontSize = style.FontSize;
                m_Parser.TextSettings.lineSpacing = style.LineSpacing;

                m_Parser.TextSettings.alignByGeometry = false;
                m_Parser.TextSettings.fontStyle = FontStyle.Normal;
                m_Parser.TextSettings.generateOutOfBounds = true;// m_GenerateOutOfBounds;
                m_Parser.TextSettings.generationExtents = new Vector2(transform.rect.width, transform.rect.height);
                m_Parser.TextSettings.horizontalOverflow = m_HorizontalOverflow;
                m_Parser.TextSettings.pivot = new Vector2(0.5f, 0.5f);
                m_Parser.TextSettings.resizeTextForBestFit = m_BestFit;
                m_Parser.TextSettings.resizeTextMaxSize = 600;
                m_Parser.TextSettings.resizeTextMinSize = 6;
                m_Parser.TextSettings.richText = true;
                m_Parser.TextSettings.scaleFactor = m_ScaleFactor;
                m_Parser.TextSettings.textAnchor = m_Anchor;
                m_Parser.TextSettings.updateBounds = true;
                m_Parser.TextSettings.verticalOverflow = m_VerticalOverflow;

                UpdateText();
            }
            else
            {
                Debug.LogErrorFormat("IntelligentText Style not found for id: {0}", m_StyleId);
            }

        }

        private void OnValidate()
        {
            StartCoroutine(AwaitRefresh());
        }

        private IEnumerator AwaitRefresh()
        {
            while (IntelligentTextSettingsManager.Instance == null)
            {
                yield return null;
            }
            //force re-initialize
            m_RenderMode = RenderMode.Unknown;
            Refresh();
        }

        private void OnEnable()
        {
            IntelligentTextSettingsManager.RegisterText(this);
        }
        
        private void OnDisable()
        {
            IntelligentTextSettingsManager.UnregisterText(this);
        }

        private void OnDistroy()
        {
            m_Parser.Dispose();
        }
    }
}
