using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteOutline : MonoBehaviour
{

    SpriteRenderer sr;
    [SerializeField] Color outlineColor;
    [SerializeField] float thickness;

    MaterialPropertyBlock block;
    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        Sprite s = transform.parent.GetComponent<SpriteRenderer>().sprite;
        //sr.sprite = s;

        block = new MaterialPropertyBlock();
        block.SetColor("_OutlineColor", outlineColor);
        block.SetFloat("_OutlineThickness", thickness);
        block.SetTexture("_MainTex", s.texture);
        sr.SetPropertyBlock(block);

        sr.enabled = false;
    }

    public void Enable()
    {
        sr.enabled = true;
    }

    public void Enable(Color c)
    {
        if (!sr) return;
        sr.enabled = true;
        sr.GetPropertyBlock(block);
        block.SetColor("_OutlineColor", c);
        sr.SetPropertyBlock(block);
    }

    public void Disable()
    {
        if (!sr) return;
        sr.enabled = false;
    }

}
