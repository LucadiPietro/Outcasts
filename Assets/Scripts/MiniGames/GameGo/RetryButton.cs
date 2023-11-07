using UnityEngine;
using UnityEngine.SceneManagement;

public class RetryButton : MonoBehaviour
{
    public GameObject retry;
    public void OnMouseUp()
    {
        if (!retry.activeSelf) return;
        SceneManager.LoadScene("GameGo");
    }
}