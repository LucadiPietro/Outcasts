namespace Common.Cutscenes.Commands
{
    using System;
    using System.Collections;
    using UnityEngine;

    [Serializable]
    [AddTypeMenu("PlayCutscene ")]
    public sealed class PlayCutscene : ICinematicCommand
    {
        [SerializeField] CutsceneBase m_SubCutscene;

        [SerializeField] bool m_ShouldWaitEnd = true;
        public bool ShouldWaitEnd => m_ShouldWaitEnd;

        public void Execute() => m_SubCutscene.Play();
        public IEnumerator ExecuteAwaitable()
        {
            yield return m_SubCutscene.PlayAwaitable();
        }
        public void FastForward() => m_SubCutscene.FastForward();
    }
}
