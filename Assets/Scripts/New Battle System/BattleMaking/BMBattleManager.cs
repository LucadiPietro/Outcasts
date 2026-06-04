using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class BMBattleManager : BMManager
{
    public BattleButton buttonPrefab;
    public List<BMButtonPrefab> laneButtonModels = new List<BMButtonPrefab>();

    public float timeToReachBar = 2;
    public bool enableSpatialMotion = true;

    [SerializeField] RectTransform noteParent;
    [SerializeField] RectTransform lineCenter;
    [SerializeField] RectTransform lineUp;
    [SerializeField] RectTransform lineDown;
    [SerializeField] float laneColumnSpacing = 520f;
    [SerializeField] Vector2 runtimeNoteSize = new Vector2(50f, 50f);
    [SerializeField] Vector2 keyboardKeycapSize = new Vector2(72f, 58f);
    [SerializeField] Color keyboardKeycapColor = new Color(0.08f, 0.09f, 0.1f, 1f);
    [SerializeField] Color keyboardKeycapTextColor = Color.white;
    [SerializeField] float keyboardKeycapFontSize = 34f;

    public override void CreateButton(BMButtonPrefab prefab)
    {
        Image image = prefab.GetComponent<Image>();
        Sprite sprite = image != null ? image.sprite : null;
        Color color = image != null ? image.color : Color.white;
        CreateButton(prefab.cell, prefab.key, sprite, color);
    }

    public BattleButton CreateButton(BMButtonPrefab.Cell cell, Keys key, Sprite sprite, Color color)
    {
        return CreateButton(cell, key, sprite, color, -1, 0d);
    }

    public BattleButton CreateButton(
        BMButtonPrefab.Cell cell,
        Keys key,
        Sprite sprite,
        Color color,
        int laneIndex,
        double hitTimeSeconds)
    {
        string cellName = cell.ToString();
        EnsureLayoutReferences();
        GameObject cellObject = TryGetConfiguredCell(cellName);

        ApplyLaneModelFallback(cell, ref key, ref sprite, ref color);

        BattleButton prefabToUse = buttonPrefab;
        bool canUseSpatialMotion =
            enableSpatialMotion &&
            laneIndex >= 0 &&
            BattleManager.Instance != null &&
            BattleManager.Instance.SpatialMotionService != null;

        Transform buttonParent = canUseSpatialMotion ? GetSpatialParent(cellObject) : GetLegacyParent(cellObject);
        var newBut = CreateButtonInstance(prefabToUse, buttonParent);
        newBut.transform.localPosition = Vector3.zero;
        newBut.buttonAction = key;
        newBut.cell = cell;

        Vector2 targetPosition = GetLaneCenterAnchoredPosition(cell);
        newBut.positionToReach = targetPosition.y + (IsTopCell(cell) ? 100f : -100f);
        newBut.barPosition = targetPosition.y;

        newBut.timeToReachBar = timeToReachBar;
        if (laneIndex >= 0)
        {
            newBut.ConfigureSpatialJudgment(laneIndex, hitTimeSeconds);
        }

        ApplyInputDisplay(newBut, sprite, color);

        if (canUseSpatialMotion)
        {
            newBut.ConfigureSpatialMotion(
                BattleManager.Instance.SpatialMotionService,
                () => BattleManager.Instance.CurrentChartTimeSeconds,
                GetLaneStartAnchoredPosition(cell),
                GetLaneCenterAnchoredPosition(cell));
        }

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SubcribeButton(newBut);
        }

        if (!newBut.gameObject.activeSelf)
        {
            newBut.gameObject.SetActive(true);
        }

        return newBut;
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
            Keys key = Keys.NONE;
            ApplyLaneModelFallback(button.cell, ref key, ref sprite, ref color);
            ApplyInputDisplay(button, sprite, color);
        }
    }

    BattleButton CreateButtonInstance(BattleButton prefabToUse, Transform parent)
    {
        if (prefabToUse != null)
        {
            return Instantiate(prefabToUse, parent);
        }

        var buttonObject = new GameObject(
            "BattleButton",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(BattleButton),
            typeof(BoxCollider2D),
            typeof(Rigidbody2D));

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = runtimeNoteSize;

        Image image = buttonObject.GetComponent<Image>();
        image.preserveAspect = true;
        image.raycastTarget = false;

        BoxCollider2D collider = buttonObject.GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = runtimeNoteSize;

        Rigidbody2D body = buttonObject.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        BattleButton button = buttonObject.GetComponent<BattleButton>();
        button.image = image;
        return button;
    }

    Transform GetSpatialParent(GameObject cellObject)
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

    Transform GetLegacyParent(GameObject cellObject)
    {
        if (cellObject != null)
        {
            return cellObject.transform;
        }

        return GetSpatialParent(cellObject);
    }

    GameObject TryGetConfiguredCell(string cellName)
    {
        if (cells != null && cells.TryGetValue(cellName, out GameObject cellObject))
        {
            return cellObject;
        }

        GameObject found = GameObject.Find(cellName);
        return found;
    }

    void ApplyLaneModelFallback(BMButtonPrefab.Cell cell, ref Keys key, ref Sprite sprite, ref Color color)
    {
        int index = (int)cell;
        if (laneButtonModels == null || index < 0 || index >= laneButtonModels.Count)
        {
            return;
        }

        BMButtonPrefab model = laneButtonModels[index];
        if (model == null)
        {
            return;
        }

        if (key == Keys.NONE)
        {
            key = model.key;
        }

        Image modelImage = model.GetComponent<Image>();
        if (modelImage != null)
        {
            if (sprite == null)
            {
                sprite = modelImage.sprite;
            }

            color = modelImage.color;
        }
    }

    void ApplyInputDisplay(BattleButton button, Sprite xboxSprite, Color xboxColor)
    {
        if (button == null)
        {
            return;
        }

        int laneIndex = button.laneIndex >= 0 ? button.laneIndex : (int)button.cell;
        BattleManager battleManagerInstance = BattleManager.Instance;
        BattleInputDisplayMode mode = battleManagerInstance != null
            ? battleManagerInstance.CurrentInputDisplayMode
            : BattleInputDisplayMode.Xbox;

        if (mode == BattleInputDisplayMode.Keyboard)
        {
            if (battleManagerInstance != null)
            {
                button.buttonAction = battleManagerInstance.GetLaneButtonAction(laneIndex);
            }

            string label = battleManagerInstance != null
                ? battleManagerInstance.GetLaneDisplayLabel(laneIndex)
                : GetKeyboardFallbackLabel(laneIndex);
            button.ApplyKeycapDisplay(label, keyboardKeycapColor, keyboardKeycapTextColor, keyboardKeycapFontSize, keyboardKeycapSize);
            return;
        }

        Keys key = battleManagerInstance != null ? battleManagerInstance.GetLaneButtonAction(laneIndex) : Keys.NONE;
        if (key != Keys.NONE)
        {
            button.buttonAction = key;
        }

        button.ApplySpriteDisplay(xboxSprite, xboxColor, runtimeNoteSize);
    }

    string GetKeyboardFallbackLabel(int laneIndex)
    {
        switch (laneIndex)
        {
            case 0:
                return "A";
            case 1:
                return "S";
            case 2:
                return "D";
            case 3:
                return "J";
            case 4:
                return "K";
            case 5:
                return "L";
            default:
                return "?";
        }
    }

    Vector2 GetLaneStartAnchoredPosition(BMButtonPrefab.Cell cell)
    {
        string cellName = cell.ToString();
        GameObject cellObject = TryGetConfiguredCell(cellName);
        RectTransform cellRect = cellObject != null ? cellObject.GetComponent<RectTransform>() : null;
        float x = cellRect != null && cells != null && cells.ContainsKey(cellName)
            ? cellRect.anchoredPosition.x
            : GetLaneX(cell);

        return new Vector2(x, GetLineY(lineCenter, 0f));
    }

    Vector2 GetLaneCenterAnchoredPosition(BMButtonPrefab.Cell cell)
    {
        string cellName = cell.ToString();
        GameObject cellObject = TryGetConfiguredCell(cellName);
        RectTransform cellRect = cellObject != null ? cellObject.GetComponent<RectTransform>() : null;
        GameObject bar = TryGetConfiguredBar(cell);
        RectTransform barRect = bar != null ? bar.GetComponent<RectTransform>() : null;

        float x = cellRect != null && cells != null && cells.ContainsKey(cellName)
            ? cellRect.anchoredPosition.x
            : GetLaneX(cell);
        float fallbackY = IsTopCell(cell) ? 460f : -460f;
        float y = barRect != null ? barRect.anchoredPosition.y : GetLineY(IsTopCell(cell) ? lineUp : lineDown, fallbackY);
        return new Vector2(x, y);
    }

    GameObject TryGetConfiguredBar(BMButtonPrefab.Cell cell)
    {
        if (bars == null || bars.Count < 2)
        {
            return null;
        }

        return IsTopCell(cell) ? bars[0] : bars[1];
    }

    void EnsureLayoutReferences()
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
                noteParent = canvas != null ? canvas.GetComponent<RectTransform>() : null;
            }
        }
    }

    RectTransform FindRectTransform(string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        return found != null ? found.GetComponent<RectTransform>() : null;
    }

    float GetLaneX(BMButtonPrefab.Cell cell)
    {
        float spacing = laneColumnSpacing;
        if (noteParent != null && noteParent.rect.width > 0f)
        {
            spacing = Mathf.Min(laneColumnSpacing, Mathf.Max(300f, noteParent.rect.width * 0.28f));
        }

        int column = (int)cell % 3;
        if (column == 0)
        {
            return -spacing;
        }

        if (column == 1)
        {
            return 0f;
        }

        return spacing;
    }

    float GetLineY(RectTransform line, float fallback)
    {
        return line != null ? line.anchoredPosition.y : fallback;
    }

    static bool IsTopCell(BMButtonPrefab.Cell cell)
    {
        return cell == BMButtonPrefab.Cell.Cell1 ||
               cell == BMButtonPrefab.Cell.Cell2 ||
               cell == BMButtonPrefab.Cell.Cell3;
    }
}
