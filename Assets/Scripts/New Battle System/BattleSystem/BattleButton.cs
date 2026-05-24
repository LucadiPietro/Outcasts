using System.Collections;
using DG.Tweening;
using RhythmCombat.Domain.Chart;
using UnityEngine;
using UnityEngine.UI;

public class BattleButton : MonoBehaviour
{
    public float timeBeforeStart = 0.1f;
    public float timeBeforDesappear = 0.3f;
    public float timeToReachBar = 0.5f;
    public Image image;
    
    public Keys  buttonAction;

    public float barPosition;
    public float positionToReach;
    private Tween myTween;
    
    public BMButtonPrefab.Cell cell;
    public bool hasSpatialJudgmentData;
    public int laneIndex = -1;
    public double hitTimeSeconds;
    public CombatNote spatialNote;
    public bool isResolved;

    public void ConfigureSpatialJudgment(int laneIndexValue, double hitTimeSecondsValue)
    {
        laneIndex = laneIndexValue;
        hitTimeSeconds = hitTimeSecondsValue;
        spatialNote = new CombatNote("battle-button-" + GetInstanceID(), laneIndex, hitTimeSeconds);
        hasSpatialJudgmentData = true;
    }

    public void MarkResolved()
    {
        isResolved = true;

        Collider2D noteCollider = GetComponent<Collider2D>();
        if (noteCollider != null)
        {
            noteCollider.enabled = false;
        }
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(timeBeforeStart);
        bool complete = false;
        myTween = transform.DOMoveY(barPosition, timeToReachBar).SetEase(Ease.Linear).OnComplete(() => complete = true);
        yield return new WaitUntil(() => complete);
        myTween.Kill();
        myTween = transform.DOMoveY(positionToReach, 0.5f).SetEase(Ease.Linear).OnComplete(() => complete = true);
        yield return new WaitUntil(() => complete);
        yield return new WaitForSeconds(timeBeforDesappear);
        image.DOFade(0, 0.3f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Bar"))
        {
            BattleManager.Instance.SubcribeButton(this);
        }
    }

    public void KillButton()
    {
        MarkResolved();
        myTween.Kill();
        transform.DOScale(0, 0.3f).OnComplete((() =>
        {
            Vector3 pos = transform.position;
            pos.Set(pos.x, positionToReach, pos.z);
            transform.position = pos;
        }));
    }
}
