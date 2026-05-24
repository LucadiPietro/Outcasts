using UnityEngine.UI;
using UnityEngine;

public class BMBattleManager : BMManager
{
    public BattleButton buttonPrefab;

    public float timeToReachBar = 2;
    public bool enableSpatialMotion = true;

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
        GameObject cellObject = cells[cellName];
        bool canUseSpatialMotion =
            enableSpatialMotion &&
            laneIndex >= 0 &&
            BattleManager.Instance != null &&
            BattleManager.Instance.SpatialMotionService != null &&
            cellObject.transform.parent != null;

        Transform buttonParent = canUseSpatialMotion ? cellObject.transform.parent : cellObject.transform;
        var newBut = Instantiate(buttonPrefab, buttonParent);
        newBut.transform.localPosition = Vector3.zero;
        if (sprite != null)
        {
            newBut.image.sprite = sprite;
        }

        newBut.image.color = color;
        newBut.buttonAction = key;
        newBut.cell = cell;

        newBut.positionToReach = cell.ToString() switch
        {
            "Cell1" => bars[0].transform.position.y + 100,
            "Cell2" => bars[0].transform.position.y + 100,
            "Cell3" => bars[0].transform.position.y + 100,
            "Cell4" => bars[1].transform.position.y - 100,
            "Cell5" => bars[1].transform.position.y - 100,
            "Cell6" => bars[1].transform.position.y - 100,
            _ => newBut.positionToReach
        };
        
        newBut.barPosition = cell.ToString() switch
        {
            "Cell1" => bars[0].transform.position.y,
            "Cell2" => bars[0].transform.position.y,
            "Cell3" => bars[0].transform.position.y,
            "Cell4" => bars[1].transform.position.y,
            "Cell5" => bars[1].transform.position.y,
            "Cell6" => bars[1].transform.position.y,
            _ => newBut.barPosition
        };

        newBut.timeToReachBar = timeToReachBar;
        if (laneIndex >= 0)
        {
            newBut.ConfigureSpatialJudgment(laneIndex, hitTimeSeconds);
        }

        if (canUseSpatialMotion)
        {
            newBut.ConfigureSpatialMotion(
                BattleManager.Instance.SpatialMotionService,
                () => BattleManager.Instance.CurrentChartTimeSeconds,
                Vector2.zero,
                GetLaneCenterAnchoredPosition(cell));
        }

        if (!newBut.gameObject.activeSelf)
        {
            newBut.gameObject.SetActive(true);
        }

        return newBut;
    }

    Vector2 GetLaneCenterAnchoredPosition(BMButtonPrefab.Cell cell)
    {
        string cellName = cell.ToString();
        RectTransform cellRect = cells[cellName].GetComponent<RectTransform>();
        GameObject bar = IsTopCell(cell) ? bars[0] : bars[1];
        RectTransform barRect = bar.GetComponent<RectTransform>();

        float x = cellRect != null ? cellRect.anchoredPosition.x : cells[cellName].transform.localPosition.x;
        float y = barRect != null ? barRect.anchoredPosition.y : bar.transform.localPosition.y;
        return new Vector2(x, y);
    }

    static bool IsTopCell(BMButtonPrefab.Cell cell)
    {
        return cell == BMButtonPrefab.Cell.Cell1 ||
               cell == BMButtonPrefab.Cell.Cell2 ||
               cell == BMButtonPrefab.Cell.Cell3;
    }
}
