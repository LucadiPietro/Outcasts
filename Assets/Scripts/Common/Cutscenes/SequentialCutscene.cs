namespace Common.Cutscenes
{
    using Common.Cutscenes.Commands;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SequentialCutscene : CutsceneBase
    {
        [SerializeReference, SubclassSelector] List<ICinematicCommand> m_Commands = default;
        [SerializeField] string m_CinematicKey = default;
        protected override string kPrefKey => m_CinematicKey;

        protected override IEnumerator Sequence()
        {
            foreach (var command in m_Commands)
            {
                if (command.ShouldWaitEnd) yield return StartCoroutine(command.ExecuteAwaitable());
                else command.Execute();
            }
        }
    }
}