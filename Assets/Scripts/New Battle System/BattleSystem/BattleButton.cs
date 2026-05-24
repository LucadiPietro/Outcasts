using System;
using System.Collections;
using DG.Tweening;
using RhythmCombat.Domain.Chart;
using RhythmCombat.Domain.Movement;
using UnityEngine;
using UnityEngine.UI;

public class BattleButton : MonoBehaviour
{
    public float timeBeforeStart = 0.1f;
    public float timeBeforDesappear = 0.3f;
    public float timeToReachBar = 0.5f;
    public Image image;
    
    public Keys  buttonAction;

    public float barPosition;
    public float positionToReach;
    private Tween myTween;
    
    public BMButtonPrefab.Cell cell;
    public bool hasSpatialJudgmentData;
    public int laneIndex = -1;
    public double hitTimeSeconds;
    public CombatNote spatialNote;
    public bool isResolved;
    public bool hasSpatialMotionData;

    NoteMotionService spatialMotionService;
    Func<double> currentTimeProvider;
    Vector2 spatialSpawnAnchoredPosition;
    Vector2 spatialLaneCenterAnchoredPosition;

    public void ConfigureSpatialJudgment(int laneIndexValue, double hitTimeSecondsValue)
    {
        laneIndex = laneIndexValue;
        hitTimeSeconds = hitTimeSecondsValue;
        spatialNote = new CombatNote("battle-button-" + GetInstanceID(), laneIndex, hitTimeSeconds);
        hasSpatialJudgmentData = true;
    }

    public void ConfigureSpatialMotion(
        NoteMotionService motionService,
        Func<double> chartTimeProvider,
        Vector2 spawnAnchoredPosition,
        Vector2 laneCenterAnchoredPosition)
    {
        spatialMotionService = motionService;
        currentTimeProvider = chartTimeProvider;
        spatialSpawnAnchoredPosition = spawnAnchoredPosition;
        spatialLaneCenterAnchoredPosition = laneCenterAnchoredPosition;
        hasSpatialMotionData = spatialMotionService != null && currentTimeProvider != null;

        RectTransform rect = transform as RectTransform;
        if (rect != null)
        {
            rect.anchoredPosition = spatialSpawnAnchoredPosition;
        }
    }

    public void MarkResolved()
    {
        isResolved = true;

        Collider2D noteCollider = GetComponent<Collider2D>();
        if (noteCollider != null)
        {
            noteCollider.enabled = false;
        }
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(timeBeforeStart);
        if (hasSpatialMotionData && spatialNote != null)
        {
            yield return RunSpatialMotion();
            yield break;
        }

        yield return RunLegacyMotion();
    }

    IEnumerator RunLegacyMotion()
    {
        bool complete = false;
        myTween = transform.DOMoveY(barPosition, timeToReachBar).SetEase(Ease.Linear).OnComplete(() => complete = true);
        yield return new WaitUntil(() => complete || isResolved);
        KillTween();
        if (isResolved)
        {
            yield break;
        }

        complete = false;
        myTween = transform.DOMoveY(positionToReach, 0.5f).SetEase(Ease.Linear).OnComplete(() => complete = true);
        yield return new WaitUntil(() => complete || isResolved);
        if (isResolved)
        {
            yield break;
        }

        yield return new WaitForSeconds(timeBeforDesappear);
        FadeOutButton();
    }

    IEnumerator RunSpatialMotion()
    {
        RectTransform rect = transform as RectTransform;
        while (!isResolved)
        {
            NotePositionResult positionResult = spatialMotionService.GetPosition(spatialNote, currentTimeProvider());
            float progress = (float)positionResult.NormalizedProgress;
            Vector2 anchoredPosition = Vector2.LerpUnclamped(
                spatialSpawnAnchoredPosition,
                spatialLaneCenterAnchoredPosition,
                progress);

            if (rect != null)
            {
                rect.anchoredPosition = anchoredPosition;
            }
            else
            {
                transform.localPosition = new Vector3(anchoredPosition.x, anchoredPosition.y, transform.localPosition.z);
            }

            if (positionResult.ShouldDespawn)
            {
                if (BattleManager.Instance != null)
                {
                    BattleManager.Instance.ResolveMissedButton(this);
                }
                else
                {
                    MarkResolved();
                    FadeOutButton();
                }

                yield break;
            }

            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Bar"))
        {
            BattleManager.Instance.SubcribeButton(this);
        }
    }

    public void KillButton()
    {
        MarkResolved();
        KillTween();
        transform.DOScale(0, 0.3f).OnComplete((() =>
        {
            Vector3 pos = transform.position;
            pos.Set(pos.x, positionToReach, pos.z);
            transform.position = pos;
        }));
    }

    public void FadeOutButton()
    {
        KillTween();
        if (image != null)
        {
            image.DOFade(0, 0.3f);
        }
    }

    void KillTween()
    {
        if (myTween != null && myTween.IsActive())
        {
            myTween.Kill();
        }
    }
}
