using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Common.Text
{
    public enum IntelligentTextDataType
    {
        None,
        Group,
        Image,
        Text
    }

    public class IntelligentTextDataNode
    {
        public Bounds Bounds;
        public readonly int Id;
        public readonly IntelligentTextDataType Type;
        public readonly string InteractorId;
        public readonly List<IntelligentTextDataNode> Children;

        public IntelligentTextDataNode(int i_Id)
        {
            Id = i_Id;
            Type = IntelligentTextDataType.None;
            InteractorId = null;
            Children = new List<IntelligentTextDataNode>();
        }

        protected IntelligentTextDataNode(int i_Id, string i_InteractorId, IntelligentTextDataType i_Type, List<IntelligentTextDataNode> i_Children)
        {
            Id = i_Id;
            Type = i_Type;
            InteractorId = i_InteractorId;
            Children = i_Children;
        }

        public virtual bool Merge(IntelligentTextDataNode i_Node)
        {
            return false;
        }
    }

    public class IntelligentTextDataTextNode : IntelligentTextDataNode
    {
        public string Text;

        public IntelligentTextDataTextNode(int i_Id, string i_InteractorId, string i_Text) :
            base(i_Id, i_InteractorId, IntelligentTextDataType.Text, null)
        {
            Text = i_Text;
        }

        public override bool Merge(IntelligentTextDataNode i_Node)
        {
            if (i_Node.Type == Type && i_Node.InteractorId == InteractorId)
            {
                var textNode = (IntelligentTextDataTextNode)i_Node;
                Text += textNode.Text;
                return true;
            }
            return false;
        }

        public 
    }

    public class IntelligentTextDataImageNode : IntelligentTextDataNode
    {
        private const float SPACE_PLACEHOLDER_REPLACE_SIZE = 10;
        private const float SPACE_PLACEHOLDER_MEASURE_SIZE = 100;
        private static readonly string SPACE_PLACEHOLDER_START = string.Format("<size={0}>", (int)SPACE_PLACEHOLDER_REPLACE_SIZE);
        private static readonly string SPACE_PLACEHOLDER_END = "</size>";
        private static readonly string SPACE_PLACEHOLDER_STR = "|";
        private static readonly string SPACE_PLACEHOLDER_MEASURE = string.Format("<size={0}>{1}</size>", (int)SPACE_PLACEHOLDER_MEASURE_SIZE, SPACE_PLACEHOLDER_STR);

        private Vector2 m_SpacePlaceholderSize;
        private string m_PlaceholderText;

        public Sprite Image;
        public IntelligentTextTransform Transform;

        public IntelligentTextDataImageNode(int i_Id, string i_InteractorId, Sprite i_Image, IntelligentTextTransform i_Transform) :
            base(i_Id, i_InteractorId, IntelligentTextDataType.Image, null)
        {
            Image = i_Image;
            Transform = i_Transform;
        }

        private void SetupSpacePlaceholder(TextGenerator i_TextGenerator, ref TextGenerationSettings i_TextSettings)
        {
            TextGenerationSettings tempTextSettings = new TextGenerationSettings()
            {
                color = Color.black,
                font = i_TextSettings.font,
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
            i_TextGenerator.Populate(SPACE_PLACEHOLDER_MEASURE, tempTextSettings);
            m_SpacePlaceholderSize = i_TextGenerator.rectExtents.size * (SPACE_PLACEHOLDER_REPLACE_SIZE / SPACE_PLACEHOLDER_MEASURE_SIZE);
        }

        public void BuildPlaceholderText(IntelligentTextTransform i_Transform, ref TextGenerationSettings i_TextSettings)
        {
            Vector2 imageSize = i_Transform.scale * i_TextSettings.fontSize;
            int placementCount = (int)(imageSize.x / m_SpacePlaceholderSize.x + 0.5f);
            if (placementCount <= 0)
            {
                placementCount = 1;
            }

            StringBuilder textAccumulator = new StringBuilder();
            textAccumulator.Append(SPACE_PLACEHOLDER_START);
            for (int i = 0; i < placementCount; ++i)
            {
                textAccumulator.Append(SPACE_PLACEHOLDER_STR);
            }
            textAccumulator.Append(SPACE_PLACEHOLDER_END);

            m_PlaceholderText = textAccumulator.ToString();
        }
    }

    public class IntelligentTextDataGroupNode : IntelligentTextDataNode
    {
        public IntelligentTextDataGroupNode(int i_Id, string i_InteractorId) :
            base(i_Id, i_InteractorId, IntelligentTextDataType.Group, new List<IntelligentTextDataNode>())
        { }

        public override bool Merge(IntelligentTextDataNode i_Node)
        {
            if (i_Node.Type == Type && i_Node.InteractorId == InteractorId)
            {
                var groupNode = (IntelligentTextDataGroupNode)i_Node;
                Children.AddRange(groupNode.Children);
                return true;
            }
            return false;
        }
    }
}
