using System;
using UnityEngine.UI;
using UnityEngine;

class UiUtils
{

    public static void BoundPositionToCanvasView(RectTransform mrect, Vector2 size)
    {
        Vector2 apos = mrect.anchoredPosition;
        apos.x = Mathf.Clamp(apos.x, 0, Screen.width - size.x);
        apos.y = Mathf.Clamp(apos.y, 0, Screen.height - size.y);
        mrect.anchoredPosition = apos;
    }

}
