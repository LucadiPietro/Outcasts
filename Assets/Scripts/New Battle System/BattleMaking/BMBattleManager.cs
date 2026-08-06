using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BMBattleManager : BMManager
{
    [Header("Note Prefab")]
    public BattleButton buttonPrefab;
    public List<BMButtonPrefab> laneButtonModels = new List<BMButtonPrefab>();

    [Header("Movement")]
    public float timeToReachBar = 2f;
    public bool enableSpatialMotion = true;

    [Header("Scene References")]
    [SerializeField] RectTransform noteParent;
    [SerializeField] RectTransform lineCenter;
    [SerializeField] RectTransform lineUp;
    [SerializeField] RectTransform lineDown;

    [Header("Fallback Layout")]
    [SerializeField] float laneColumnSpacing = 520f;
    [SerializeField] float topTargetY = 467f;
    [SerializeField] float bottomTargetY = -467f;

    [Header("Note Display")]
    [SerializeField] Vector2 runtimeNoteSize = new Vector2(50f, 50f);
    [SerializeField] Vector2 keycapSize = new Vector2(72f, 58f);
    [SerializeField] Color keycapColor = new Color(0.08f, 0.09f, 0.1f, 1f);
    [SerializeField] Color keycapTextColor = Color.white;
    [SerializeField] float keycapFontSize = 34f;

    public override void CreateButton(BMButtonPrefab prefab)
    {
        if (prefab == null)
        {
            return;
        }

        Image image = prefab.GetComponent<Image>();
        Sprite sprite = image != null ? image.sprite : null;
        Color color = image != null ? image.color : Color.white;

        CreateButton(
            prefab.cell,
            Keys.NONE,
            sprite,
            color,
            (int)prefab.cell,
            BattleManager.Instance != null
                ? BattleManager.Instance.CurrentChartTimeSeconds + timeToReachBar
                : 0d);
    }

    public BattleButton CreateButton(
        BMButtonPrefab.Cell cell,
        Keys ignoredLegacyKey,
        Sprite sprite,
        Color color)
    {
        return CreateButton(cell, Keys.NONE, sprite, color, (int)cell, 0d);
    }

    public BattleButton CreateButton(
        BMButtonPrefab.Cell cell,
        Keys ignoredLegacyKey,
        Sprite sprite,
        Color color,
        int laneIndex,
        double hitTimeSeconds)
    {
        EnsureSceneReferences();

        GameObject cellObject = TryGetConfiguredCell(cell.ToString());
        ApplyLaneModelVisualFallback(cell, ref sprite, ref color);

        Transform parent = GetNoteParent(cellObject);
        BattleButton note = CreateButtonInstance(parent);

        note.transform.localPosition = Vector3.zero;
        note.transform.localScale = Vector3.one;
        note.buttonAction = Keys.NONE;
        note.cell = cell;
        note.timeToReachBar = timeToReachBar;

        Vector2 spawnPosition = GetLaneSpawnAnchoredPosition(cell);
        Vector2 targetPosition = GetLaneTargetAnchoredPosition(cell);

        note.barPosition = targetPosition.y;
        note.positionToReach = targetPosition.y + (IsAttackCell(cell) ? 100f : -100f);

        if (laneIndex >= 0)
        {
            note.ConfigureSpatialJudgment(laneIndex, hitTimeSeconds);
        }

        ApplyInputDisplay(note, sprite, color);

        if (enableSpatialMotion &&
            laneIndex >= 0 &&
            BattleManager.Instance != null &&
            BattleManager.Instance.SpatialMotionService != null)
        {
            note.ConfigureSpatialMotion(
                BattleManager.Instance.SpatialMotionService,
                () => BattleManager.Instance.CurrentChartTimeSeconds,
                spawnPosition,
                targetPosition);
        }

        BattleNoteRegistry.Register(note);

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SubcribeButton(note);
        }

        if (!note.gameObject.activeSelf)
        {
            note.gameObject.SetActive(true);
        }

        return note;
    }

    public void RefreshActiveButtonDisplays()
    {
        BattleButton[] activeButtons = FindObjectsOfType<BattleButton>();

        for (int i = 0; i < activeButtons.Length; i++)
        {
            BattleButton button = activeButtons[i];

            if (button == null || button.isResolved)
            {
                continue;
            }

            Sprite sprite = null;
            Color color = Color.white;
            ApplyLaneModelVisualFallback(button.cell, ref sprite, ref color);
            ApplyInputDisplay(button, sprite, color);
        }
    }

    public Vector2 GetLaneSpawnAnchoredPosition(BMButtonPrefab.Cell cell)
    {
        EnsureSceneReferences();

        return new Vector2(
            GetLaneX(cell),
            lineCenter != null ? lineCenter.anchoredPosition.y : 0f);
    }

    public Vector2 GetLaneTargetAnchoredPosition(BMButtonPrefab.Cell cell)
    {
        EnsureSceneReferences();

        float target = IsAttackCell(cell)
            ? (lineUp != null ? lineUp.anchoredPosition.y : topTargetY)
            : (lineDown != null ? lineDown.anchoredPosition.y : bottomTargetY);

        return new Vector2(GetLaneX(cell), target);
    }

#if UNITY_EDITOR
    public void ConfigureEditorReferences(
        RectTransform newNoteParent,
        RectTransform newLineCenter,
        RectTransform newLineUp,
        RectTransform newLineDown,
        BattleButton newButtonPrefab)
    {
        noteParent = newNoteParent;
        lineCenter = newLineCenter;
        lineUp = newLineUp;
        lineDown = newLineDown;
        buttonPrefab = newButtonPrefab;
        enableSpatialMotion = true;
    }
#endif

    BattleButton CreateButtonInstance(Transform parent)
    {
        if (buttonPrefab == null)
        {
            Debug.LogError(
                "BMBattleManager: BattleButton prefab non configurato. " +
                "Reimporta la patch Battle Enhanced v3 e controlla la Console.");
            return null;
        }

        return Instantiate(buttonPrefab, parent);
    }

    Transform GetNoteParent(GameObject cellObject)
    {
        if (noteParent != null)
        {
            return noteParent;
        }

        if (cellObject != null && cellObject.transform.parent != null)
        {
            return cellObject.transform.parent;
        }

        return transform;
    }

    GameObject TryGetConfiguredCell(string cellName)
    {
        if (cells != null &&
            cells.TryGetValue(cellName, out GameObject cellObject) &&
            cellObject != null)
        {
            return cellObject;
        }

        return GameObject.Find(cellName);
    }

    void ApplyLaneModelVisualFallback(
        BMButtonPrefab.Cell cell,
        ref Sprite sprite,
        ref Color color)
    {
        int index = (int)cell;

        if (laneButtonModels == null ||
            index < 0 ||
            index >= laneButtonModels.Count)
        {
            return;
        }

        BMButtonPrefab model = laneButtonModels[index];

        if (model == null)
        {
            return;
        }

        Image modelImage = model.GetComponent<Image>();

        if (modelImage == null)
        {
            return;
        }

        if (sprite == null)
        {
            sprite = modelImage.sprite;
        }

        color = modelImage.color;
    }

    void ApplyInputDisplay(
        BattleButton button,
        Sprite laneSprite,
        Color laneColor)
    {
        if (button == null)
        {
            return;
        }

        BattleLaneInputRouter router = BattleLaneInputRouter.Instance;

        if (router != null)
        {
            Sprite configuredSprite = router.GetDisplaySprite(button.cell);
            string configuredLabel = router.GetDisplayLabel(button.cell);
            Color configuredColor = router.GetDisplayColor(button.cell, laneColor);

            if (configuredSprite != null)
            {
                button.ApplySpriteDisplay(
                    configuredSprite,
                    configuredColor,
                    runtimeNoteSize);

                return;
            }

            button.ApplyKeycapDisplay(
                configuredLabel,
                keycapColor,
                keycapTextColor,
                keycapFontSize,
                keycapSize);

            return;
        }

        if (laneSprite != null)
        {
            button.ApplySpriteDisplay(laneSprite, laneColor, runtimeNoteSize);
            return;
        }

        button.ApplyKeycapDisplay(
            button.cell.ToString(),
            keycapColor,
            keycapTextColor,
            keycapFontSize,
            keycapSize);
    }

    void EnsureSceneReferences()
    {
        if (lineCenter == null)
        {
            lineCenter = FindRectTransform("LineCenter");
        }

        if (lineUp == null)
        {
            lineUp = FindRectTransform("LineUp");
        }

        if (lineDown == null)
        {
            lineDown = FindRectTransform("LineDown");
        }

        if (noteParent == null)
        {
            if (lineCenter != null && lineCenter.parent is RectTransform parentRect)
            {
                noteParent = parentRect;
            }
            else
            {
                Canvas canvas = FindObjectOfType<Canvas>();
                noteParent = canvas != null
                    ? canvas.GetComponent<RectTransform>()
                    : null;
            }
        }
    }

    RectTransform FindRectTransform(string objectName)
    {
        RectTransform[] rectTransforms =
            FindObjectsOfType<RectTransform>(true);

        for (int i = 0; i < rectTransforms.Length; i++)
        {
            RectTransform rect = rectTransforms[i];

            if (rect != null &&
                rect.gameObject.scene.IsValid() &&
                rect.name == objectName)
            {
                return rect;
            }
        }

        return null;
    }

    float GetLaneX(BMButtonPrefab.Cell cell)
    {
        GameObject cellObject = TryGetConfiguredCell(cell.ToString());
        RectTransform cellRect =
            cellObject != null
                ? cellObject.GetComponent<RectTransform>()
                : null;

        if (cellRect != null)
        {
            return cellRect.anchoredPosition.x;
        }

        int column = (int)cell % 3;

        if (column == 0)
        {
            return -laneColumnSpacing;
        }

        if (column == 1)
        {
            return 0f;
        }

        return laneColumnSpacing;
    }

    static bool IsAttackCell(BMButtonPrefab.Cell cell)
    {
        return cell == BMButtonPrefab.Cell.Cell1 ||
               cell == BMButtonPrefab.Cell.Cell2 ||
               cell == BMButtonPrefab.Cell.Cell3;
    }
}
