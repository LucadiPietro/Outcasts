using UnityEngine;

public abstract class Singleton<T> : Singleton where T : MonoBehaviour
{

    public static T instance = null;
    public bool dontDestroy = false;

    private void Awake()
    {
        if(instance == null)
        {
            T[] objs = FindObjectsOfType<T>();
            instance = objs[0];
            if(dontDestroy)DontDestroyOnLoad(instance.gameObject);
            OnAwake();
        }
        else
        {
            Destroy(this);
        }
    }

    protected virtual void OnAwake() { }

}

public abstract class Singleton : MonoBehaviour
{
    #region  Properties
    public static bool Quitting { get; private set; }
    #endregion

    #region  Methods
    private void OnApplicationQuit()
    {
        Quitting = true;
    }
    #endregion
}