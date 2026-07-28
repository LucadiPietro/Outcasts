namespace Common.Cutscenes.Commands
{
    using NaughtyAttributes;
    using Pathfinding;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [Serializable]
    [AddTypeMenu("Character/Stop")]
    public class StopMoving : ICinematicCommand
    {
        public bool ShouldWaitEnd => false;

        public void Execute()
        {
            NavigationHelper.Instance.StopAllCharacters();
        }

        public IEnumerator ExecuteAwaitable()
        {
            Execute();
            yield return null;
        }

        public void FastForward() { Execute(); }
    }
}