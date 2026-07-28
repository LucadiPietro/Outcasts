namespace Common.Cutscenes.Commands
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Pool;

    [Serializable]
    [AddTypeMenu("Multiple")]
    public sealed class Multiple : ICinematicCommand
    {
        public bool ShouldWaitEnd => m_ShouldWaitEnd;
        public void Execute()
        {
            foreach (var command in m_Commands) command.Execute();
        }
        public IEnumerator ExecuteAwaitable()
        {
            var enumerables = ListPool<IEnumerator>.Get();
            foreach (var command in m_Commands) enumerables.Add(command.ExecuteAwaitable());
            yield return new WaitAll(enumerables);
            ListPool<IEnumerator>.Release(enumerables);
        }
        public void FastForward()
        {
            foreach (var command in m_Commands) command.FastForward();
        }

        [SerializeReference, SubclassSelector] List<ICinematicCommand> m_Commands = default;
        [SerializeField] bool m_ShouldWaitEnd = true;
    }
}
