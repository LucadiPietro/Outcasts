using UnityEngine.UI;
using UnityEngine;

public class BMBattleManager : BMManager
{
    public BattleButton buttonPrefab;

    public float timeToReachBar = 2;

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
        var newBut = Instantiate(buttonPrefab, cells[cell.ToString()].transform);
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

        if (!newBut.gameObject.activeSelf)
        {
            newBut.gameObject.SetActive(true);
        }

        return newBut;
    }
}
