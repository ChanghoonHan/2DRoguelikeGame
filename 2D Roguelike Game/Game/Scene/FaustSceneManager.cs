using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FaustSceneManager : MonoBehaviour {

    static public FaustSceneManager S;

    public string chageSceneName = "";

    private void Awake()
    {
        if (S == null)
        {
            S = this;
        }
    }

    public void SceneChange(string sceneName, float time = 0)
    {
        chageSceneName = sceneName;
        Invoke("SceneChangeInvoke", time);
    }

    private void SceneChangeInvoke()
    {
        SceneManager.LoadScene(chageSceneName);
    }
}
