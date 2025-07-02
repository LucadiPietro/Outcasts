using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerReal : MonoBehaviour
{
    bool lastSceneCalled = false;

    public void LoadSceneByName(string name)
    {
        SessionManager.Instance.SetLastScene(SceneManager.GetActiveScene().name);
        SceneManager.LoadScene(name);
    }
    
    public void LoadSceneByIndex(int index)
    {
        SessionManager.Instance.SetLastScene(SceneManager.GetActiveScene().name);
        SceneManager.LoadScene(index);
    }

    public void LoadLastScene()
    {
        if (lastSceneCalled) return;

        lastSceneCalled = true;
        string name = FindObjectOfType<SessionManager>().LastScene;
        SessionManager.Instance.SetLastScene(SceneManager.GetActiveScene().name);
        SceneManager.LoadScene(name);
    }
}
