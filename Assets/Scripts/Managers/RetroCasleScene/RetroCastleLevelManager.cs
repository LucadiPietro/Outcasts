using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Restores the party to the correct entrance when returning to the retro castle.
/// Existing serialized field names and UnityEvents are intentionally preserved.
/// </summary>
public sealed class RetroCastleLevelManager : MonoBehaviour
{
    const string BattleReturnScene = "Warp Scene";
    const string NextAreaReturnScene = "Warp Scene 2";

    [Header("General")]
    [SerializeField] Transform brogan;
    [SerializeField] Transform malphayt;
    [SerializeField] Transform lysander;

    [Header("From Battle")]
    [SerializeField] Transform broganPos;
    [SerializeField] Transform malphaytPos;
    [SerializeField] Transform lysanderPos;
    public UnityEvent FromBattle = new UnityEvent();

    [Header("From Next Scene")]
    [SerializeField] Transform broganPos2;
    [SerializeField] Transform malphaytPos2;
    [SerializeField] Transform lysanderPos2;
    public UnityEvent FromNext = new UnityEvent();

    /// <summary>
    /// Applies the spawn state before notifying scene listeners.
    /// </summary>
    void Start()
    {
        SessionManager sessionManager = SessionManager.Instance;
        if (sessionManager == null)
        {
            Debug.LogWarning("RetroCastleLevelManager could not resolve SessionManager.", this);
            return;
        }

        switch (sessionManager.LastScene)
        {
            case BattleReturnScene:
                ApplyPartyPositions(broganPos, malphaytPos, lysanderPos);
                FromBattle?.Invoke();
                break;

            case NextAreaReturnScene:
                ApplyPartyPositions(broganPos2, malphaytPos2, lysanderPos2);
                FromNext?.Invoke();
                break;
        }
    }

    /// <summary>
    /// Moves every configured party member while reporting incomplete authoring.
    /// </summary>
    void ApplyPartyPositions(
        Transform broganSpawn,
        Transform malphaytSpawn,
        Transform lysanderSpawn)
    {
        ApplyPosition(brogan, broganSpawn, nameof(brogan));
        ApplyPosition(malphayt, malphaytSpawn, nameof(malphayt));
        ApplyPosition(lysander, lysanderSpawn, nameof(lysander));
    }

    /// <summary>
    /// Applies one spawn point without allowing a null reference to break scene start.
    /// </summary>
    void ApplyPosition(Transform character, Transform spawn, string characterName)
    {
        if (character == null || spawn == null)
        {
            Debug.LogWarning(
                $"Retro castle spawn for '{characterName}' is not fully configured.",
                this);
            return;
        }

        character.position = spawn.position;
    }
}
