using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class Console : Singleton<Console>
{

    [SerializeField] Text text;
    CanvasGroup cg;

    static Color color = Color.white;
    protected override void OnAwake()
    {
        base.OnAwake();
        text.raycastTarget = false;
        cg = GetComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;
        cg.Hide();
    }

    static float tp = 0;
    bool fading = false;
    private void Update()
    {
        if (!fading) return;
        tp += Time.deltaTime;
        if(tp >= 2f)
        {
            cg.alpha -= Time.deltaTime;
        }
    }

    public void Error(string s)
    {
        fading = true;
        color = Color.red;
        text.color = color;
        Text(s);
    }

    public void Success(string s)
    {
        fading = true;
        color = Color.green;
        text.color = color;
        Text(s);
    }

    public void Message(string s)
    {
        fading = true;
        color = Color.white;
        text.color = color;
        Text(s);
    }

    public void Show(string s)
    {
        fading = false;
        color = Color.white;
        text.color = color;
        Text(s);
    }

    public void Close()
    {
        cg.Hide();
    }

    public void Text(string s, Color c)
    {
        color = c;
        text.color = color;
        Text(s);
    }

    void Text(string s)
    {
        cg.alpha = 1f;
        text.text = s;
        tp = 0f;
    }

}
