using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PulsatingEffect : MonoBehaviour
{
    [SerializeField] bool expand = false;
    Image img;

    void Start()
    {
        img = GetComponent<Image>();
        startingScale = img.rectTransform.localScale;
        if (expand)
        {
            img.rectTransform.Expand();
        }
    }

    [SerializeField] float speed;
    [SerializeField] float intensity;
    float tp = 0f;
    Vector3 startingScale;
    void Update()
    {
        tp += Time.deltaTime * speed;
        float v = 1f + Mathf.Sin(tp);//varia tra 1 e -1
        img.rectTransform.localScale = startingScale + (Vector3.one * intensity * v);
    }


}
