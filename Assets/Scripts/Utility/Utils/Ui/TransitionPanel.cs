using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TransitionPanel : Singleton<TransitionPanel>
{
    [SerializeField] bool enabledOnStart;
    float tp = 0f;
    float time = 1f;
    Image img;
    Color c = Color.black;
    string sn;

    bool forward = true;

    private void Start()
    {
        if (enabledOnStart)
        {
            CreateImage(Color.black);
            forward = false;
            transitioning = true;
        }
    }

    private void Update()
    {
        if (!img) return;
        if (transitioning)
        {
            if (forward)
            {
                TransitionForward();
            }
            else
            {
                TransitionBackward();
            }
        }
    }

    bool transitioning = false;

    public void LoadScene(string sceneName, float t)
    {
        sn = sceneName;
        time = t;
        tp = 0f;
        CreateImage(Color.black);
        transitioning = true;
        forward = true;
    } 


    void TransitionForward()
    {
        tp += Time.deltaTime;
        c.a = Mathf.Clamp(tp / time, 0f, 1f);
        img.color = c;
        if (tp >= time)
        {
            forward = false;
            tp = 0;
            SceneManager.LoadScene(sn);
        }
    }

    void TransitionBackward()
    {
        tp += Time.deltaTime;
        c.a = Mathf.Clamp(1 - tp / time, 0f, 1f);
        img.color = c;
        if (tp >= time)
        {
            transitioning = false;
            Destroy(img.gameObject);
        }
    }


    void CreateImage(Color clr)
    {
        if (img) Destroy(img.gameObject);
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        img = new GameObject().AddComponent<Image>();
        img.rectTransform.SetParent(canvas.transform);
        img.rectTransform.SetAsLastSibling();
        img.rectTransform.anchorMax = new Vector2(1, 1);
        img.rectTransform.anchorMin = new Vector2(0, 0);

        img.rectTransform.SetLeft(0);
        img.rectTransform.SetRight(0);
        img.rectTransform.SetTop(0);
        img.rectTransform.SetBottom(0);
        c = clr;
        c.a = 0f;
        img.color = c;
    }

}
