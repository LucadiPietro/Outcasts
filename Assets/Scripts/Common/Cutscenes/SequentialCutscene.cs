namespace Common.Cutscenes
{
    using Common.Cutscenes.Commands;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class SequentialCutscene : CutsceneBase
    {
        [SerializeReference, SubclassSelector] List<ICinematicCommand> m_Commands = default;
        [SerializeField] string m_CinematicKey = default;
        protected override string kPrefKey => m_CinematicKey;

        int m_Index = 0;
        protected override IEnumerator Sequence()
        {
            m_Index = 0;
            foreach (var command in m_Commands)
            {
                if (command.ShouldWaitEnd) yield return StartCoroutine(command.ExecuteAwaitable());
                else command.Execute();
                m_Index++;
            }
        }
        public override void FastForward()
        {
            StopPlaying();
            for (int i = 0; i < m_Commands.Count; i++)
            {
                var command = m_Commands[i];
                command.FastForward();
            }
        }
    }
}