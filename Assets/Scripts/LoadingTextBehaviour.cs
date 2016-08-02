using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LoadingTextBehaviour : MonoBehaviour {

    private Text m_LoadingProcText;

	// Use this for initialization
	void Start () {
        m_LoadingProcText = gameObject.GetComponent<Text>();
        Debug.Assert(m_LoadingProcText != null, "Text not found.");
	}
	
	// Update is called once per frame
	void Update () {
        m_LoadingProcText.text = LoadingManagerBehaviour.GetLoadingProgress() + "%";
	}
}
