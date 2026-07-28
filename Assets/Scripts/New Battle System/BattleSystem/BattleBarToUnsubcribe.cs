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

        BattleManager.Instance.ResolveMissedButton(button);
    }

}
