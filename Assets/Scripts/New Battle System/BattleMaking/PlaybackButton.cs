using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlaybackButton : MonoBehaviour
{
    public float timeBeforeStart = 0.1f;
    public float timeBeforDesappear = 0.3f;
    public float timeToReachBar = 0.5f;
    public Image image;

    public float positionToReach;

    public float barPosition;
    private Tween myTween;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(timeBeforeStart);
        bool complete = false;
        myTween = transform.DOMoveY(barPosition, timeToReachBar).SetEase(Ease.Linear).OnComplete(() => complete = true);
        yield return new WaitUntil(() => complete);
        myTween.Kill();
        myTween = transform.DOMoveY(positionToReach, 0.5f).SetEase(Ease.Linear).OnComplete(() => complete = true);
        yield return new WaitUntil(() => complete);
        yield return new WaitForSeconds(timeBeforDesappear);
        image.DOFade(0, 0.3f).OnComplete(() => Destroy(gameObject));
    }
}
