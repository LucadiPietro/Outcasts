using RhythmCombat.Domain.Chart;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmCombat.Unity
{
    public sealed class LaneInputReader : MonoBehaviour
    {
        [SerializeField] LaneInputPreset m_Preset;

        private readonly Dictionary<int, KeyCode> _keyByLane = new Dictionary<int, KeyCode>();

        void Awake()
        {
            if (m_Preset != null)
                ApplyPreset(m_Preset);
        }

        public void ApplyPreset(LaneInputPreset preset)
        {
            if (preset == null)
                throw new ArgumentNullException(nameof(preset));

            _keyByLane.Clear();

            foreach (var binding in preset.Bindings)
            {
                if (binding == null)
                    continue;

                SetBinding(binding.laneIndex, binding.key);
            }
        }

        public void SetBinding(int laneIndex, KeyCode key)
        {
            LaneMapping.ValidateLaneIndex(laneIndex);
            _keyByLane[laneIndex] = key;
        }

        public bool GetLaneDown(int laneIndex)
        {
            return TryGetKey(laneIndex, out var key) && Input.GetKeyDown(key);
        }

        public bool GetLaneUp(int laneIndex)
        {
            return TryGetKey(laneIndex, out var key) && Input.GetKeyUp(key);
        }

        public bool TryGetKey(int laneIndex, out KeyCode key)
        {
            LaneMapping.ValidateLaneIndex(laneIndex);
            return _keyByLane.TryGetValue(laneIndex, out key);
        }
    }
}
