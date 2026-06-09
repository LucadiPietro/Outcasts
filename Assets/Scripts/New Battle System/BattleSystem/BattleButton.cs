using System;
using System.Collections;
using DG.Tweening;
using RhythmCombat.Domain.Chart;
using RhythmCombat.Domain.Movement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleButton : MonoBehaviour
{
    public float timeBeforeStart = 0.1f;
    public float timeBeforDesappear = 0.3f;
    public float timeToReachBar = 0.5f;
    public Image image;

    public Keys buttonAction;

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

    bool isVisualResolved;
    bool isMissAnimationRunning;

    TextMeshProUGUI displayLabel;
    NoteMotionService spatialMotionService;
    Func<double> currentTimeProvider;
    Vector2 spatialSpawnAnchoredPosition;
    Vector2 spatialLaneCenterAnchoredPosition;

    [Header("Miss Animation")]
    [SerializeField] float missShakeDuration = 0.25f;
    [SerializeField] float missShakeStrength = 12f;
    [SerializeField] float missDisappearDuration = 0.18f;

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

        Collider2D noteCollider = GetComponent<Collider2D>();
        if (noteCollider != null)
        {
            noteCollider.enabled = false;
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

    public void ApplyKeycapDisplay(string label, Color backgroundColor, Color textColor, float fontSize, Vector2 size)
    {
        RectTransform rect = transform as RectTransform;
        if (rect != null)
        {
            rect.sizeDelta = size;
        }

        if (image != null)
        {
            image.sprite = null;
            image.color = backgroundColor;
            image.preserveAspect = false;
        }

        TextMeshProUGUI labelText = GetOrCreateDisplayLabel();
        labelText.text = label;
        labelText.color = textColor;
        labelText.fontSize = fontSize;
        labelText.enabled = true;
    }

    public void ApplySpriteDisplay(Sprite sprite, Color color, Vector2 size)
    {
        RectTransform rect = transform as RectTransform;
        if (rect != null && size.x > 0f && size.y > 0f)
        {
            rect.sizeDelta = size;
        }

        if (image != null)
        {
            image.sprite = sprite;
            image.color = color;
            image.preserveAspect = true;
        }

        if (displayLabel != null)
        {
            displayLabel.text = string.Empty;
            displayLabel.enabled = false;
        }
    }

    TextMeshProUGUI GetOrCreateDisplayLabel()
    {
        if (displayLabel != null)
        {
            return displayLabel;
        }

        Transform existing = transform.Find("InputLabel");
        if (existing != null)
        {
            displayLabel = existing.GetComponent<TextMeshProUGUI>();
            if (displayLabel != null)
            {
                return displayLabel;
            }
        }

        GameObject labelObject = new GameObject("InputLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.SetParent(transform, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        displayLabel = labelObject.GetComponent<TextMeshProUGUI>();
        displayLabel.alignment = TextAlignmentOptions.Center;
        displayLabel.fontStyle = FontStyles.Bold;
        displayLabel.raycastTarget = false;
        return displayLabel;
    }

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

        myTween = transform.DOMoveY(barPosition, timeToReachBar)
            .SetEase(Ease.Linear)
            .OnComplete(() => complete = true);

        yield return new WaitUntil(() => complete || isResolved);

        KillTween();

        if (isResolved)
        {
            yield break;
        }

        complete = false;

        myTween = transform.DOMoveY(positionToReach, 0.5f)
            .SetEase(Ease.Linear)
            .OnComplete(() => complete = true);

        yield return new WaitUntil(() => complete || isResolved);

        KillTween();

        if (isResolved)
        {
            yield break;
        }

        yield return new WaitForSeconds(timeBeforDesappear);

        if (isResolved)
        {
            yield break;
        }

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.ResolveMissedButton(this);
        }
        else
        {
            ShakeMissAndDisappear();
        }
    }

    IEnumerator RunSpatialMotion()
    {
        RectTransform rect = transform as RectTransform;

        while (!isVisualResolved)
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

            if (!isResolved && BattleManager.Instance != null)
            {
                BattleManager.Instance.TryResolveSpatialMissedButton(this);
            }

            if (positionResult.ShouldDespawn)
            {
                if (!isResolved && BattleManager.Instance != null)
                {
                    BattleManager.Instance.ResolveMissedButton(this);
                }
                else if (!isVisualResolved)
                {
                    ShakeMissAndDisappear();
                }

                yield break;
            }

            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasSpatialMotionData)
        {
            return;
        }

        if (other.gameObject.CompareTag("Bar"))
        {
            BattleManager.Instance.SubcribeButton(this);
        }
    }

    public void KillButton()
    {
        if (isVisualResolved)
        {
            return;
        }

        MarkResolved();
        isVisualResolved = true;
        KillTween();

        transform.DOKill();

        transform.DOScale(0f, 0.3f).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    public void FadeOutButton()
    {
        if (isVisualResolved)
        {
            return;
        }

        MarkResolved();
        isVisualResolved = true;
        KillTween();

        transform.DOKill();

        if (image != null)
        {
            image.DOFade(0f, 0.3f).OnComplete(() =>
            {
                Destroy(gameObject);
            });
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShakeMissAndDisappear()
    {
        if (isMissAnimationRunning)
        {
            return;
        }

        MarkResolved();
        isVisualResolved = true;
        isMissAnimationRunning = true;

        KillTween();
        transform.DOKill();

        StartCoroutine(ShakeMissAndDisappearRoutine());
    }

    IEnumerator ShakeMissAndDisappearRoutine()
    {
        RectTransform rect = transform as RectTransform;

        Vector3 originalLocalPosition = transform.localPosition;
        Vector2 originalAnchoredPosition = rect != null ? rect.anchoredPosition : Vector2.zero;

        float elapsed = 0f;

        while (elapsed < missShakeDuration)
        {
            elapsed += Time.deltaTime;

            float offsetX = UnityEngine.Random.Range(-missShakeStrength, missShakeStrength);
            float offsetY = UnityEngine.Random.Range(-missShakeStrength * 0.35f, missShakeStrength * 0.35f);

            if (rect != null)
            {
                rect.anchoredPosition = originalAnchoredPosition + new Vector2(offsetX, offsetY);
            }
            else
            {
                transform.localPosition = originalLocalPosition + new Vector3(offsetX, offsetY, 0f);
            }

            yield return null;
        }

        if (rect != null)
        {
            rect.anchoredPosition = originalAnchoredPosition;
        }
        else
        {
            transform.localPosition = originalLocalPosition;
        }

        Sequence sequence = DOTween.Sequence();

        if (image != null)
        {
            sequence.Join(image.DOFade(0f, missDisappearDuration));
        }

        if (displayLabel != null)
        {
            sequence.Join(displayLabel.DOFade(0f, missDisappearDuration));
        }

        sequence.Join(transform.DOScale(0f, missDisappearDuration));

        sequence.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    void KillTween()
    {
        if (myTween != null && myTween.IsActive())
        {
            myTween.Kill();
        }
    }
}