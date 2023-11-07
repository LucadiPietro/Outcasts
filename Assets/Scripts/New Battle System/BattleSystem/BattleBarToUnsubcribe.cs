using UnityEngine;

public class BattleBarToUnsubcribe : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var button = other.GetComponent<BattleButton>();
        if (button.cell is BMButtonPrefab.Cell.Cell4 or BMButtonPrefab.Cell.Cell5 or BMButtonPrefab.Cell.Cell6)
        {
            BattleManager.Instance.DefenceRoutine(button.cell);
        }
        BattleManager.Instance.Unsubscribe(button);
        BattleManager.Instance.counter = 0;
        BattleManager.Instance.battleUIManager.UpdateCounter(0);
    }

}
