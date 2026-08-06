namespace Common.Cutscenes.Commands
{
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Executes another cutscene as a reusable sub-sequence.
    /// </summary>
    [Serializable]
    [AddTypeMenu("Cutscene/Play Sub-Cutscene")]
    public sealed class PlayCutscene : ICinematicCommand
    {
        [SerializeField] CutsceneBase m_SubCutscene;
        [SerializeField] bool m_ShouldWaitEnd = true;

        public bool ShouldWaitEnd => m_ShouldWaitEnd;

        public void Execute()
        {
            if (!TryGetCutscene(out CutsceneBase cutscene))
            {
                return;
            }

            cutscene.Play();
        }

        public IEnumerator ExecuteAwaitable()
        {
            if (!TryGetCutscene(out CutsceneBase cutscene))
            {
                yield break;
            }

            bool completedNormally = false;

            try
            {
                yield return cutscene.PlayAwaitable();
                completedNormally = true;
            }
            finally
            {
                // If the parent sequence is cancelled, do not leave a child
                // cutscene running independently in the scene.
                if (!completedNormally && cutscene.IsPlaying)
                {
                    cutscene.Cancel();
                }
            }
        }

        public void FastForward()
        {
            if (TryGetCutscene(out CutsceneBase cutscene))
            {
                cutscene.FastForward();
            }
        }

        bool TryGetCutscene(out CutsceneBase cutscene)
        {
            cutscene = m_SubCutscene;

            if (cutscene != null)
            {
                return true;
            }

            Debug.LogError("PlayCutscene has no sub-cutscene assigned.");
            return false;
        }
    }
}
