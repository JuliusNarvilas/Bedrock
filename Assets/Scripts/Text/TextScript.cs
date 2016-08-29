using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class TextScript : MonoBehaviour {

    [SerializeField]
    Font m_Font;

    CanvasRenderer temp;

    protected MeshRenderer m_MeshRenderer;
    protected Mesh m_Mesh;

    // Use this for initialization
    void OnEnable () {
        if (m_MeshRenderer == null)
        {
            m_MeshRenderer = GetComponent<MeshRenderer>();
        }
        
        //m_MeshRenderer.material = m_Font.material;

        GetComponent<MeshFilter>().mesh = m_Mesh = new Mesh();
        m_Mesh.name = "Text Mesh";
        m_Mesh.hideFlags = HideFlags.HideAndDontSave;

        m_MeshRenderer.sharedMaterial.mainTexture = m_Font.material.mainTexture;

        Vector3[] verts =
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0)
        };
        Vector2[] uvs =
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        int[] indices = { 0, 2, 1, 2, 3, 1 };

        m_Mesh.vertices = verts;
        m_Mesh.uv = uvs;
        m_Mesh.triangles = indices;
        m_Mesh.RecalculateBounds();
    }
	
    void OnDisable()
    {
        if (Application.isEditor)
        {
            GetComponent<MeshFilter>().mesh = null;
            DestroyImmediate(m_Mesh);
        }
    }

	// Update is called once per frame
	void Update () {
	
	}
}
