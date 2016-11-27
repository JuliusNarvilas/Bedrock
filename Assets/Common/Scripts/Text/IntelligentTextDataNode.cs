using System;
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

    /// <summary>
    /// An interface to adjust final generated IntelligentText mesh.
    /// </summary>
    public interface IIntelligentTextMeshModifier
    {
        void ChangeMesh();
    }

    public class IntelligentTextDataNode
    {
        public readonly List<Bounds> Bounds;
        public readonly int Id;
        public readonly IntelligentTextDataType Type;
        public readonly string InteractorId;
        public readonly List<IntelligentTextDataNode> Children;
        public readonly List<IIntelligentTextMeshModifier> MeshModifier;

        public IntelligentTextDataNode(int i_Id)
        {
            Bounds = new List<Bounds>();
            Id = i_Id;
            Type = IntelligentTextDataType.None;
            InteractorId = null;
            Children = new List<IntelligentTextDataNode>();
            MeshModifier = new List<IIntelligentTextMeshModifier>();
        }

        protected IntelligentTextDataNode(int i_Id, string i_InteractorId, IntelligentTextDataType i_Type, List<IntelligentTextDataNode> i_Children)
        {
            Id = i_Id;
            Type = i_Type;
            InteractorId = i_InteractorId;
            Children = i_Children;
            MeshModifier = new List<IIntelligentTextMeshModifier>();
        }

        public virtual bool Merge(IntelligentTextDataNode i_Node)
        {
            return false;
        }

        public virtual void BuildText(StringBuilder i_TextAccumulator, ref IntelligentTextParser i_Parser)
        { }

        public virtual int BuildSubMesh(int i_StartVertsIndex, List<IntelligentTextMeshData> i_MeshSets, ref IntelligentTextParser i_Parser)
        {
            int size = MeshModifier.Count;
            for (int i = 0; i < size; ++i)
            {
                MeshModifier[i].ChangeMesh();
            }

            return i_StartVertsIndex;
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

        public override void BuildText(StringBuilder i_TextAccumulator, ref IntelligentTextParser i_Parser)
        {
            i_TextAccumulator.Append(IntelligentTextSettings.Instance.Localize(Text));
        }
        
        public override int BuildSubMesh(int i_StartVertsIndex, List<IntelligentTextMeshData> i_MeshSets, ref IntelligentTextParser i_Parser)
        {
            int characterCount = Text.Length;
            var subMeshData = new IntelligentTextSubMeshData {
                Trinagles = new List<int>(characterCount * 6),
                Material = i_Parser.TextSettings.font.material
            };
            for (int i = 0; i < characterCount; ++i)
            {
                int vertIndexStart = i_StartVertsIndex + i * 4;
                subMeshData.Trinagles.Add(vertIndexStart);
                subMeshData.Trinagles.Add(vertIndexStart + 1);
                subMeshData.Trinagles.Add(vertIndexStart + 2);
                subMeshData.Trinagles.Add(vertIndexStart);
                subMeshData.Trinagles.Add(vertIndexStart + 2);
                subMeshData.Trinagles.Add(vertIndexStart + 3);
            }

            i_MeshSets[0].SubMeshes.Add(subMeshData);

            int size = MeshModifier.Count;
            for (int i = 0; i < size; ++i)
            {
                MeshModifier[i].ChangeMesh();
            }
            return i_StartVertsIndex + (characterCount * 4);
        }
    }

    public class IntelligentTextDataImageNode : IntelligentTextDataNode, IIntelligentTextMeshModifier
    {
        private int m_PlaceholderLength;

        public Sprite Image;
        public IntelligentTextTransform Transform;

        public IntelligentTextDataImageNode(int i_Id, string i_InteractorId, Sprite i_Image, IntelligentTextTransform i_Transform) :
            base(i_Id, i_InteractorId, IntelligentTextDataType.Image, null)
        {
            Image = i_Image;
            Transform = i_Transform;
            MeshModifier.Add(this);
        }

        public override void BuildText(StringBuilder i_TextAccumulator, ref IntelligentTextParser i_Parser)
        {
            Vector2 imageSize = Transform.scale * i_Parser.TextSettings.fontSize;
            float estimatedPlaceholderWidth = i_Parser.TextSettings.fontSize * i_Parser.SpacePlaceholderSizePerUnit.x;
            m_PlaceholderLength = (int)(imageSize.x / estimatedPlaceholderWidth + 0.5f);
            if (m_PlaceholderLength <= 0)
            {
                m_PlaceholderLength = 1;
            }
            
            for (int i = 0; i < m_PlaceholderLength; ++i)
            {
                i_TextAccumulator.Append(IntelligentTextParser.SPACE_PLACEHOLDER_STR);
            }
        }

        public void ChangeMesh()
        {
            throw new NotImplementedException();
        }

        //TODO:
        public override int BuildSubMesh(int i_StartVertsIndex, List<IntelligentTextMeshData> i_MeshSets, ref IntelligentTextParser i_Parser)
        {
            var meshData = i_MeshSets[0];
            int vertsRemoveStart = i_StartVertsIndex + 4;
            int vertsRemoveCount = (m_PlaceholderLength - 1) * 4;
            if (vertsRemoveCount > 0)
            {
                meshData.Verts.RemoveRange(vertsRemoveStart, vertsRemoveCount);
                meshData.Colors.RemoveRange(vertsRemoveStart, vertsRemoveCount);
                meshData.Uvs.RemoveRange(vertsRemoveStart, vertsRemoveCount);
            }

            var subMeshData = new IntelligentTextSubMeshData
            {
                Trinagles = new List<int>(6),
                //Material = TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            };

            meshData.Verts[i_StartVertsIndex] = new Vector3();
            meshData.Colors.RemoveRange(i_StartVertsIndex, vertsRemoveCount);
            meshData.Uvs.RemoveRange(i_StartVertsIndex, vertsRemoveCount);

            int size = MeshModifier.Count;
            for (int i = 0; i < size; ++i)
            {
                MeshModifier[i].ChangeMesh();
            }
            return i_StartVertsIndex;
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
