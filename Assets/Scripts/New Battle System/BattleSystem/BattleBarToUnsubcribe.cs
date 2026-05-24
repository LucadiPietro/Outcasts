using UnityEngine;

public class BattleBarToUnsubcribe : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var button = other.GetComponent<BattleButton>();
        if (button == null || BattleManager.Instance == null)
        {
            return;
        }

        if (button.isResolved)
        {
            return;
        }

        button.MarkResolved();

        if (button.cell is BMButtonPrefab.Cell.Cell4 or BMButtonPrefab.Cell.Cell5 or BMButtonPrefab.Cell.Cell6)
        {
            BattleManager.Instance.DefenceRoutine(button.cell);
        }
        BattleManager.Instance.Unsubscribe(button);
        BattleManager.Instance.counter = 0;
        if (BattleManager.Instance.battleUIManager != null)
        {
            BattleManager.Instance.battleUIManager.UpdateCounter(0);
            BattleManager.Instance.battleUIManager.ShowFeedback("MISS " + button.cell, Color.red);
        }
    }

}
