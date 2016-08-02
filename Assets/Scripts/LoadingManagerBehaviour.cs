using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadingManagerBehaviour : MonoBehaviour {
    
    public string DefaultSceneName = string.Empty;
    static private string s_LoadingSceneName = null;
    static private AsyncOperation s_SceneLoadingResult = null;

    static public string GetLoadingSceneName()
    {
        return s_LoadingSceneName;
    }

    static public void SetLoadingSceneName(string i_LoadingStringName)
    {
        s_LoadingSceneName = i_LoadingStringName;
    }

    static public float GetLoadingProgress()
    {
        if(s_SceneLoadingResult != null)
        {
            return s_SceneLoadingResult.progress;
        }
        return 0.0f;
    }

	// Use this for initialization
	void Start () {
        Debug.Assert(!string.IsNullOrEmpty(DefaultSceneName), "Default loading scene is empty.");
        Debug.Assert(!string.IsNullOrEmpty(s_LoadingSceneName), "Loading scene is empty.");

        if(string.IsNullOrEmpty(s_LoadingSceneName))
        {
            s_LoadingSceneName = DefaultSceneName;
        }

        s_SceneLoadingResult = SceneManager.LoadSceneAsync(s_LoadingSceneName, LoadSceneMode.Additive);
        if (s_SceneLoadingResult != null)
        {
            s_SceneLoadingResult.allowSceneActivation = true;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if(s_SceneLoadingResult != null && s_SceneLoadingResult.isDone)
        {
            Scene nextScene = SceneManager.GetSceneByName(s_LoadingSceneName);
            if (nextScene != null)
            {
                Scene activeScene = SceneManager.GetActiveScene();
                SceneManager.SetActiveScene(nextScene);
                SceneManager.UnloadScene(activeScene.buildIndex);
                s_LoadingSceneName = null;
                s_SceneLoadingResult = null;
            }
        }
    }
}
