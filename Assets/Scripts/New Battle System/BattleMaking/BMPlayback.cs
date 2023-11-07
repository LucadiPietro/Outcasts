using UnityEngine;
using UnityEngine.UI;

public class BMPlayback : BMManager
{
    public PlaybackButton buttonPrefab;
    public float timeToReachBar = 2;

    public override void CreateButton(BMButtonPrefab prefab)
    {
        var newBut =Instantiate(buttonPrefab, cells[prefab.cell.ToString()].transform);
        newBut.transform.localPosition = Vector3.zero;
        newBut.image.sprite = prefab.GetComponent<Image>().sprite;

        newBut.barPosition = prefab.cell.ToString() switch
        {
            "Cell1" => bars[0].transform.position.y,
            "Cell2" => bars[0].transform.position.y,
            "Cell3" => bars[0].transform.position.y,
            "Cell4" => bars[1].transform.position.y,
            "Cell5" => bars[1].transform.position.y,
            "Cell6" => bars[1].transform.position.y,
            _ => newBut.barPosition
        };
        
        newBut.positionToReach = prefab.cell.ToString() switch
        {
            "Cell1" => bars[0].transform.position.y + 100,
            "Cell2" => bars[0].transform.position.y + 100,
            "Cell3" => bars[0].transform.position.y + 100,
            "Cell4" => bars[1].transform.position.y - 100,
            "Cell5" => bars[1].transform.position.y - 100,
            "Cell6" => bars[1].transform.position.y - 100,
            _ => newBut.positionToReach
        };

        newBut.timeToReachBar = timeToReachBar;
    }
}
