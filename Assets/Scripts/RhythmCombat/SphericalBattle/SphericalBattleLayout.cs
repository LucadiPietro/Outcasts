using UnityEngine;

/// <summary>
/// Compatibility shim for the obsolete spherical-layout patch.
/// The current Battle Enhanced system keeps the original upper/lower lane layout,
/// so this component intentionally performs no layout changes.
/// </summary>
[AddComponentMenu("")]
public sealed class SphericalBattleLayout : MonoBehaviour
{
    private void Awake()
    {
        enabled = false;
    }

    /// <summary>
    /// Retained only so obsolete scene/editor references continue to compile.
    /// </summary>
    public static SphericalBattleLayout ApplyToCurrentBattle()
    {
        Debug.LogWarning(
            "SphericalBattleLayout is obsolete and disabled. " +
            "Use Assets/Scenes/Battle_Enhanced.unity.");

        return null;
    }

    /// <summary>
    /// Retained only for compatibility with older serialized calls.
    /// </summary>
    public void ApplyLayoutNow()
    {
        // Intentionally empty.
    }
}
