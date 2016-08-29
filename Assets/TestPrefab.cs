using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class TestPrefab : MonoBehaviour {

    public TestList test;
    //public List<TestListElement> test2;

    public GameObject prefab;
    public GameObject popupContainer;
    
    private Animator anim;
    private bool isClosing = false;

    //*gameobject *animator
    void Awake()
    {
        anim = GetComponent<Animator>();
    }

	// Use this for initialization
	void Start ()
    {
        OnMigratedCallback();
    }

    void OnMigratedCallback()
    {
        if (prefab != null)
        {
            //prefab
            if (!prefab.scene.IsValid())
            {
                var prefabInstance = Instantiate(prefab);
                if (popupContainer != null)
                {
                    prefabInstance.transform.SetParent(popupContainer.transform, false);
                }
            }
            else
            {
                prefab.SetActive(true);
            }
        }
    }

    // Update is called once per frame
    void Update () {
	    if(isClosing)
        {
            if(anim != null)
            {
                //anim.
            }
        }
	}
    
}
