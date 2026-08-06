using System;
using System.Collections;
using DG.Tweening;
using RhythmCombat.Domain.Chart;
using RhythmCombat.Domain.Movement;
using RhythmCombat.Domain.Timing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleButton : MonoBehaviour
{
    [Header("Legacy Timing")]
    public float timeBeforeStart = 0.1f;
    public float timeBeforDesappear = 0.3f;
    public float timeToReachBar = 0.5f;

    [Header("Main Visual")]
    public Image image;
    [SerializeField] TextMeshProUGUI displayLabel;

    [Header("Long Note Visuals - already present in prefab")]
    [SerializeField] RectTransform holdBody;
    [SerializeField] Image holdBodyImage;
    [SerializeField] RectTransform holdTail;
    [SerializeField] Image holdTailImage;

    [Header("Long Note Judgment")]
    [SerializeField] float holdReleasePerfectWindowSeconds = 0.06f;
    [SerializeField] float holdReleaseGoodWindowSeconds = 0.12f;
    [SerializeField] float holdReleaseBadWindowSeconds = 0.20f;
    [SerializeField] float holdBodyThickness = 28f;

    [Header("Miss Animation")]
    [SerializeField] float missShakeDuration = 0.25f;
    [SerializeField] float missShakeStrength = 12f;
    [SerializeField] float missDisappearDuration = 0.18f;

    public Keys buttonAction;
    public float barPosition;
    public float positionToReach;

    public BMButtonPrefab.Cell cell;
    public bool hasSpatialJudgmentData;
    public int laneIndex = -1;
    public double hitTimeSeconds;
    public CombatNote spatialNote;
    public bool isResolved;
    public bool hasSpatialMotionData;

    Tween movementTween;
    bool isVisualResolved;
    bool isMissAnimationRunning;
    bool isHoldNote;
    bool holdStarted;
    bool holdCompleted;

    double holdEndTimeSeconds;
    float holdApproachDurationSeconds = 2f;
    Color holdColor = new Color(0.9f, 0.75f, 0.2f, 0.78f);

    NoteMotionService spatialMotionService;
    Func<double> currentTimeProvider;
    Vector2 spawnAnchoredPosition;
    Vector2 targetAnchoredPosition;

    public bool IsHoldNote => isHoldNote;
    public bool HoldStarted => holdStarted;
    public double HoldEndTimeSeconds => holdEndTimeSeconds;

    public void ConfigureSpatialJudgment(
        int laneIndexValue,
        double hitTimeSecondsValue)
    {
        laneIndex = laneIndexValue;
        hitTimeSeconds = hitTimeSecondsValue;
        spatialNote = new CombatNote(
            "battle-button-" + GetInstanceID(),
            laneIndex,
            hitTimeSeconds);

        hasSpatialJudgmentData = true;
    }

    public void ConfigureSpatialMotion(
        NoteMotionService motionService,
        Func<double> chartTimeProvider,
        Vector2 spawnPosition,
        Vector2 targetPosition)
    {
        spatialMotionService = motionService;
        currentTimeProvider = chartTimeProvider;
        spawnAnchoredPosition = spawnPosition;
        targetAnchoredPosition = targetPosition;

        hasSpatialMotionData =
            spatialMotionService != null &&
            currentTimeProvider != null;

        SetAnchoredPosition(spawnAnchoredPosition);

        Collider2D noteCollider = GetComponent<Collider2D>();

        if (noteCollider != null)
        {
            noteCollider.enabled = false;
        }
    }

    public void ConfigureHold(
        double endTimeSeconds,
        Color bodyColor,
        float approachDurationSeconds)
    {
        if (endTimeSeconds <= hitTimeSeconds)
        {
            return;
        }

        isHoldNote = true;
        holdEndTimeSeconds = endTimeSeconds;
        holdApproachDurationSeconds =
            Mathf.Max(0.05f, approachDurationSeconds);

        holdColor = bodyColor;
        holdColor.a = Mathf.Min(0.82f, holdColor.a);

        if (holdBody != null)
        {
            holdBody.gameObject.SetActive(true);
        }

        if (holdTail != null)
        {
            holdTail.gameObject.SetActive(true);
        }

        if (holdBodyImage != null)
        {
            holdBodyImage.color = holdColor;
        }

        if (holdTailImage != null)
        {
            holdTailImage.color = holdColor;
        }

        UpdateHoldVisual(GetCurrentChartTime(), 0f);
    }

#if UNITY_EDITOR
    public void ConfigurePrefabVisuals(
        Image mainImage,
        TextMeshProUGUI label,
        RectTransform body,
        Image bodyImage,
        RectTransform tail,
        Image tailImage)
    {
        image = mainImage;
        displayLabel = label;
        holdBody = body;
        holdBodyImage = bodyImage;
        holdTail = tail;
        holdTailImage = tailImage;
    }
#endif

    public void MarkResolved()
    {
        isResolved = true;
        BattleNoteRegistry.Unregister(this);

        Collider2D noteCollider = GetComponent<Collider2D>();

        if (noteCollider != null)
        {
            noteCollider.enabled = false;
        }
    }

    public void ApplyKeycapDisplay(
        string label,
        Color backgroundColor,
        Color textColor,
        float fontSize,
        Vector2 size)
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

        if (displayLabel != null)
        {
            displayLabel.text = label;
            displayLabel.color = textColor;
            displayLabel.fontSize = fontSize;
            displayLabel.enabled = true;
        }
    }

    public void ApplySpriteDisplay(
        Sprite sprite,
        Color color,
        Vector2 size)
    {
        RectTransform rect = transform as RectTransform;

        if (rect != null &&
            size.x > 0f &&
            size.y > 0f)
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

    public void PlayPressFeedback(JudgmentGrade grade)
    {
        if (image == null || isVisualResolved)
        {
            return;
        }

        Color flashColor = GetGradeColor(grade);

        image.DOKill();
        transform.DOKill();

        Sequence sequence = DOTween.Sequence();
        sequence.Join(transform.DOScale(1.22f, 0.07f));
        sequence.Join(image.DOColor(flashColor, 0.07f));
        sequence.Append(transform.DOScale(1f, 0.12f));
        sequence.Join(image.DOColor(Color.white, 0.12f));
    }

    public void PlayChordPulse()
    {
        if (isVisualResolved)
        {
            return;
        }

        transform.DOKill();
        transform.localScale = Vector3.one;

        transform
            .DOScale(1.35f, 0.09f)
            .SetLoops(2, LoopType.Yoyo);
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

    IEnumerator RunSpatialMotion()
    {
        while (!isVisualResolved)
        {
            double chartTime = GetCurrentChartTime();

            if (holdStarted)
            {
                SetAnchoredPosition(targetAnchoredPosition);
                UpdateHoldVisual(chartTime, 1f);
                yield return null;
                continue;
            }

            NotePositionResult positionResult =
                spatialMotionService.GetPosition(
                    spatialNote,
                    chartTime);

            float headProgress =
                (float)positionResult.NormalizedProgress;

            SetAnchoredPosition(
                Vector2.LerpUnclamped(
                    spawnAnchoredPosition,
                    targetAnchoredPosition,
                    headProgress));

            UpdateHoldVisual(chartTime, headProgress);

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

    IEnumerator RunLegacyMotion()
    {
        bool complete = false;

        movementTween = transform
            .DOMoveY(barPosition, timeToReachBar)
            .SetEase(Ease.Linear)
            .OnComplete(() => complete = true);

        yield return new WaitUntil(
            () => complete || isResolved);

        KillMovementTween();

        if (isResolved)
        {
            yield break;
        }

        complete = false;

        movementTween = transform
            .DOMoveY(positionToReach, 0.5f)
            .SetEase(Ease.Linear)
            .OnComplete(() => complete = true);

        yield return new WaitUntil(
            () => complete || isResolved);

        KillMovementTween();

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

    public void KillButton()
    {
        if (isVisualResolved)
        {
            return;
        }

        if (isHoldNote && !holdStarted && !holdCompleted)
        {
            BeginHold();
            return;
        }

        ResolveAndScaleOut();
    }

    void BeginHold()
    {
        holdStarted = true;
        SetAnchoredPosition(targetAnchoredPosition);

        if (image != null)
        {
            image.DOKill();
            image.DOColor(
                Color.Lerp(image.color, Color.white, 0.35f),
                0.10f);
        }

        StartCoroutine(MonitorHoldRoutine());
    }

    IEnumerator MonitorHoldRoutine()
    {
        yield return null;

        BattleLaneInputRouter router =
            BattleLaneInputRouter.Instance;

        if (router == null)
        {
            FailHold();
            yield break;
        }

        while (!isResolved)
        {
            if (router.IsInputSuppressed)
            {
                yield return null;
                continue;
            }

            double chartTime = GetCurrentChartTime();
            double delta = chartTime - holdEndTimeSeconds;

            UpdateHoldVisual(chartTime, 1f);

            if (!router.IsPressed(cell))
            {
                ResolveHoldRelease(delta);
                yield break;
            }

            if (delta > holdReleaseBadWindowSeconds)
            {
                FailHold();
                yield break;
            }

            yield return null;
        }
    }

    void ResolveHoldRelease(double deltaSeconds)
    {
        double absoluteDelta = Math.Abs(deltaSeconds);
        JudgmentGrade grade;

        if (absoluteDelta <= holdReleasePerfectWindowSeconds)
        {
            grade = JudgmentGrade.Perfect;
        }
        else if (absoluteDelta <= holdReleaseGoodWindowSeconds)
        {
            grade = JudgmentGrade.Good;
        }
        else if (absoluteDelta <= holdReleaseBadWindowSeconds)
        {
            grade = JudgmentGrade.Bad;
        }
        else
        {
            grade = JudgmentGrade.Miss;
        }

        BattleVisualFeedbackController.Instance?.ShowHoldRelease(
            cell,
            grade,
            deltaSeconds,
            holdColor);

        if (grade == JudgmentGrade.Miss)
        {
            FailHold();
            return;
        }

        CompleteHold();
    }

    void CompleteHold()
    {
        if (holdCompleted || isResolved)
        {
            return;
        }

        holdCompleted = true;
        ResolveAndScaleOut();
    }

    void FailHold()
    {
        if (isResolved)
        {
            return;
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

    void UpdateHoldVisual(
        double chartTime,
        float headProgress)
    {
        if (!isHoldNote ||
            holdBody == null ||
            holdTail == null)
        {
            return;
        }

        float tailProgress = Mathf.Clamp01(
            (float)(
                1d -
                (holdEndTimeSeconds - chartTime) /
                holdApproachDurationSeconds));

        Vector2 headPosition = holdStarted
            ? targetAnchoredPosition
            : Vector2.LerpUnclamped(
                spawnAnchoredPosition,
                targetAnchoredPosition,
                headProgress);

        Vector2 tailPosition =
            Vector2.LerpUnclamped(
                spawnAnchoredPosition,
                targetAnchoredPosition,
                tailProgress);

        Vector2 localTail = tailPosition - headPosition;
        float length = localTail.magnitude;

        holdTail.anchoredPosition = localTail;
        holdBody.anchoredPosition = localTail * 0.5f;
        holdBody.sizeDelta =
            new Vector2(holdBodyThickness, length);

        if (length > 0.001f)
        {
            float angle =
                Mathf.Atan2(localTail.y, localTail.x) *
                Mathf.Rad2Deg - 90f;

            holdBody.localRotation =
                Quaternion.Euler(0f, 0f, angle);
        }
    }

    void ResolveAndScaleOut()
    {
        MarkResolved();
        isVisualResolved = true;
        KillMovementTween();
        transform.DOKill();

        transform
            .DOScale(0f, 0.25f)
            .OnComplete(() => Destroy(gameObject));
    }

    public void FadeOutButton()
    {
        if (isVisualResolved)
        {
            return;
        }

        MarkResolved();
        isVisualResolved = true;
        KillMovementTween();
        transform.DOKill();

        if (image != null)
        {
            image
                .DOFade(0f, 0.25f)
                .OnComplete(() => Destroy(gameObject));
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
        KillMovementTween();
        transform.DOKill();

        StartCoroutine(ShakeMissAndDisappearRoutine());
    }

    IEnumerator ShakeMissAndDisappearRoutine()
    {
        RectTransform rect = transform as RectTransform;
        Vector3 originalLocalPosition = transform.localPosition;
        Vector2 originalAnchoredPosition =
            rect != null
                ? rect.anchoredPosition
                : Vector2.zero;

        float elapsed = 0f;

        while (elapsed < missShakeDuration)
        {
            elapsed += Time.deltaTime;

            float offsetX = UnityEngine.Random.Range(
                -missShakeStrength,
                missShakeStrength);

            float offsetY = UnityEngine.Random.Range(
                -missShakeStrength * 0.35f,
                missShakeStrength * 0.35f);

            if (rect != null)
            {
                rect.anchoredPosition =
                    originalAnchoredPosition +
                    new Vector2(offsetX, offsetY);
            }
            else
            {
                transform.localPosition =
                    originalLocalPosition +
                    new Vector3(offsetX, offsetY, 0f);
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
            sequence.Join(
                image.DOFade(0f, missDisappearDuration));
        }

        if (displayLabel != null)
        {
            sequence.Join(
                displayLabel.DOFade(
                    0f,
                    missDisappearDuration));
        }

        sequence.Join(
            transform.DOScale(
                0f,
                missDisappearDuration));

        sequence.OnComplete(
            () => Destroy(gameObject));
    }

    void SetAnchoredPosition(Vector2 position)
    {
        RectTransform rect = transform as RectTransform;

        if (rect != null)
        {
            rect.anchoredPosition = position;
        }
        else
        {
            transform.localPosition =
                new Vector3(
                    position.x,
                    position.y,
                    transform.localPosition.z);
        }
    }

    double GetCurrentChartTime()
    {
        if (currentTimeProvider != null)
        {
            return currentTimeProvider();
        }

        return BattleManager.Instance != null
            ? BattleManager.Instance.CurrentChartTimeSeconds
            : Time.timeAsDouble;
    }

    static Color GetGradeColor(JudgmentGrade grade)
    {
        switch (grade)
        {
            case JudgmentGrade.Perfect:
                return new Color(1f, 0.9f, 0.25f, 1f);

            case JudgmentGrade.Good:
                return new Color(0.25f, 0.95f, 0.55f, 1f);

            case JudgmentGrade.Bad:
                return new Color(1f, 0.55f, 0.18f, 1f);

            default:
                return new Color(1f, 0.18f, 0.18f, 1f);
        }
    }

    void KillMovementTween()
    {
        if (movementTween != null &&
            movementTween.IsActive())
        {
            movementTween.Kill();
        }
    }

    void OnDestroy()
    {
        BattleNoteRegistry.Unregister(this);
    }
}
