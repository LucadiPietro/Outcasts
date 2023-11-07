using UnityEngine;
using System.Collections.Generic;

public static class TransformExtensions 
{

    public static void ClearChildren(this Transform t)
    {
        for(int x = t.childCount - 1; x >= 0; x--)
        {
            Object.Destroy(t.GetChild(x).gameObject);
        }
    }

    public static void SetLeft(this RectTransform rt, float left)
    {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    public static void SetRight(this RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    public static void SetTop(this RectTransform rt, float top)
    {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
    }

    public static void SetBottom(this RectTransform rt, float bottom)
    {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
    }

    public static void Expand(this RectTransform rectTransform)
    {
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.SetLeft(0);
        rectTransform.SetRight(0);
        rectTransform.SetTop(0);
        rectTransform.SetBottom(0);
    }

    public static void SetCorners(this RectTransform rectTransform, Vector3[] worldCorners)
    {
        Rect canvasSize = GameObject.FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect;
        rectTransform.SetLeft(worldCorners[0].x);
        rectTransform.SetRight(canvasSize.width - worldCorners[2].x);
        rectTransform.SetTop(canvasSize.height - worldCorners[2].y);
        rectTransform.SetBottom(worldCorners[0].y);
    }

    public static List<Transform> Children(this Transform t)
    {
        List<Transform> res = new List<Transform>();
        for(int x = 0; x<t.childCount; x++)
        {
            res.Add(t.GetChild(x));
        }
        return res;
    }

    public static float Distance(this Transform t, Transform o)
    {
        return t.position.Distance(o.position);
    }
    public static float Distance(this Transform t, Vector2 p)
    {
        return t.position.Distance(p);

    }

}
