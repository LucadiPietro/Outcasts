using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WorldTextPopup : MonoBehaviour
{

    [SerializeField] Text text;
    [SerializeField] float fadeTime = 1f;

    Vector2 dir;
    Color color;
    public void Setup(string s, Color c, Vector2 dir)
    {
        this.dir = dir;
        text.text = s;
        color = c;
        text.color = c;
        setup = true;
    }

    bool setup = false;
    float tp = 0f;
    private void Update()
    {
        if (setup)
        {
            tp += Time.deltaTime;
            transform.position += new Vector3(dir.x, dir.y, 0f) * Time.deltaTime;
            float a = 1f - tp / fadeTime;
            color.a = a;
            text.color = color;
            if (tp >= fadeTime)
            {
                setup = false;
                Destroy(gameObject);
            }
        }
    }

}
