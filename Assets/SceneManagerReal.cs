using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerReal : MonoBehaviour
{
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
}
