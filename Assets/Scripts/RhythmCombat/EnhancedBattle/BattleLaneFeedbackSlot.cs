using DG.Tweening;
using TMPro;
using UnityEngine;

public class BattleLaneFeedbackSlot : MonoBehaviour
{
    [SerializeField] BMButtonPrefab.Cell cell;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] LanePressFeedbackGraphic ringGraphic;
    [SerializeField] TextMeshProUGUI label;

    Sequence sequence;

    public BMButtonPrefab.Cell Cell => cell;

#if UNITY_EDITOR
    public void ConfigureEditor(
        BMButtonPrefab.Cell newCell,
        CanvasGroup newCanvasGroup,
        LanePressFeedbackGraphic newRing,
        TextMeshProUGUI newLabel)
    {
        cell = newCell;
        canvasGroup = newCanvasGroup;
        ringGraphic = newRing;
        label = newLabel;
    }
#endif

    public void Show(
        string text,
        Color color,
        float duration)
    {
        if (canvasGroup == null)
        {
            return;
        }

        if (sequence != null && sequence.IsActive())
        {
            sequence.Kill();
        }

        transform.DOKill();
        transform.localScale = Vector3.one * 0.72f;

        canvasGroup.alpha = 1f;

        if (ringGraphic != null)
        {
            ringGraphic.SetFeedbackColor(color);
        }

        if (label != null)
        {
            label.text = text;
            label.color = color;
        }

        sequence = DOTween.Sequence();
        sequence.Join(
            transform.DOScale(1.18f, 0.10f));
        sequence.Append(
            transform.DOScale(1f, 0.10f));
        sequence.AppendInterval(
            Mathf.Max(0.05f, duration - 0.30f));
        sequence.Append(
            canvasGroup.DOFade(0f, 0.18f));
    }
}
