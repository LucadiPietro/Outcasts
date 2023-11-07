using UnityEngine;
using System.Collections;

public static class CanvasGroupExtensions
{


    public static void Show(this CanvasGroup cg)
    {
        cg.interactable = true;
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
    }

    public static void Show(this CanvasGroup cg, bool raycast)
    {
        cg.interactable = true;
        cg.alpha = 1f;
        cg.blocksRaycasts = raycast;
    }

    public static void Hide(this CanvasGroup cg)
    {
        cg.interactable = false;
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
    }


}
