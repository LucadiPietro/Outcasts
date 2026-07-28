using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class CGSingleton<T> : Singleton<T> where T : MonoBehaviour
{

    protected CanvasGroup cg;

    protected override void OnAwake()
    {
        base.OnAwake();
        cg = GetComponent<CanvasGroup>();
    }

    public void Show()
    {
        cg.Show();
    }

    public void Hide()
    {
        cg.Hide();
    }

}
