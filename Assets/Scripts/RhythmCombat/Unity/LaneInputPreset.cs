using System.Collections.Generic;
using UnityEngine;

namespace RhythmCombat.Unity
{
    [CreateAssetMenu(fileName = "LaneInputPreset", menuName = "RhythmCombat/Input/Lane Input Preset")]
    public sealed class LaneInputPreset : ScriptableObject
    {
        [SerializeField] List<LaneKeyBinding> m_Bindings = new List<LaneKeyBinding>();

        public IReadOnlyList<LaneKeyBinding> Bindings => m_Bindings;
    }
}
