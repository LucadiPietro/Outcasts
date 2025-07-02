namespace Common.Cutscenes.Commands
{
    using NaughtyAttributes;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [Serializable]
    [AddTypeMenu("Note")]
    public class CutsceneNote : ICinematicCommand
    {
        [SerializeField][TextArea(0, 5)] string note;

        public bool ShouldWaitEnd => false;

        public void Execute() {}

        public IEnumerator ExecuteAwaitable()
        {
            yield return null;
        }

        public void FastForward() { Execute(); }
    }
}