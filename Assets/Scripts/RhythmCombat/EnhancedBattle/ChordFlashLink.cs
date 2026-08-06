using UnityEngine;
using UnityEngine.UI;

public class ChordFlashLink : MonoBehaviour
{
    [SerializeField] RectTransform rectTransform;
    [SerializeField] Image image;
    [SerializeField] CanvasGroup canvasGroup;

    BattleButton firstNote;
    BattleButton secondNote;
    RectTransform coordinateSpace;
    Color linkColor;
    float linkThickness;
    float pulseDuration;
    float steadyAlpha;
    double pulseStartTime;
    bool attached;

#if UNITY_EDITOR
    public void ConfigureEditor(
        RectTransform newRectTransform,
        Image newImage,
        CanvasGroup newCanvasGroup)
    {
        rectTransform = newRectTransform;
        image = newImage;
        canvasGroup = newCanvasGroup;
    }
#endif

    public void Attach(
        BattleButton first,
        BattleButton second,
        RectTransform newCoordinateSpace,
        Color color,
        float thickness,
        float newPulseDuration,
        float newSteadyAlpha)
    {
        Detach();

        if (first == null ||
            second == null ||
            newCoordinateSpace == null ||
            rectTransform == null ||
            image == null ||
            canvasGroup == null)
        {
            return;
        }

        firstNote = first;
        secondNote = second;
        coordinateSpace = newCoordinateSpace;
        linkColor = color;
        linkThickness = Mathf.Max(1f, thickness);
        pulseDuration = Mathf.Max(0.01f, newPulseDuration);
        steadyAlpha = Mathf.Clamp01(newSteadyAlpha);
        pulseStartTime = Time.unscaledTimeAsDouble;
        attached = true;

        image.color = linkColor;
        canvasGroup.alpha = 1f;
        rectTransform.localScale = Vector3.one;
        gameObject.SetActive(true);
        UpdateGeometry();
    }

    public void Detach()
    {
        attached = false;
        firstNote = null;
        secondNote = null;
        coordinateSpace = null;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    void LateUpdate()
    {
        if (!attached)
        {
            return;
        }

        if (!IsNoteVisible(firstNote) ||
            !IsNoteVisible(secondNote) ||
            coordinateSpace == null)
        {
            Detach();
            return;
        }

        UpdateGeometry();
        UpdateSpawnPulse();
    }

    static bool IsNoteVisible(BattleButton note)
    {
        return note != null &&
               !note.isResolved &&
               note.gameObject.activeInHierarchy;
    }

    void UpdateGeometry()
    {
        RectTransform firstRect =
            firstNote != null
                ? firstNote.transform as RectTransform
                : null;

        RectTransform secondRect =
            secondNote != null
                ? secondNote.transform as RectTransform
                : null;

        if (firstRect == null || secondRect == null)
        {
            return;
        }

        Vector2 firstLocal =
            coordinateSpace.InverseTransformPoint(
                firstRect.TransformPoint(Vector3.zero));

        Vector2 secondLocal =
            coordinateSpace.InverseTransformPoint(
                secondRect.TransformPoint(Vector3.zero));

        Vector2 delta = secondLocal - firstLocal;
        float length = delta.magnitude;
        float angle =
            Mathf.Atan2(delta.y, delta.x) *
            Mathf.Rad2Deg;

        rectTransform.anchoredPosition =
            (firstLocal + secondLocal) * 0.5f;

        rectTransform.sizeDelta =
            new Vector2(length, linkThickness);

        rectTransform.localRotation =
            Quaternion.Euler(0f, 0f, angle);
    }

    void UpdateSpawnPulse()
    {
        float normalizedTime = Mathf.Clamp01(
            (float)(
                (Time.unscaledTimeAsDouble - pulseStartTime) /
                pulseDuration));

        float pulseScale;

        if (normalizedTime < 0.45f)
        {
            pulseScale = Mathf.Lerp(
                1f,
                2.15f,
                normalizedTime / 0.45f);
        }
        else
        {
            pulseScale = Mathf.Lerp(
                2.15f,
                1f,
                (normalizedTime - 0.45f) / 0.55f);
        }

        rectTransform.localScale =
            new Vector3(1f, pulseScale, 1f);

        canvasGroup.alpha = Mathf.Lerp(
            1f,
            steadyAlpha,
            normalizedTime);
    }

    void OnDisable()
    {
        attached = false;
    }
}
