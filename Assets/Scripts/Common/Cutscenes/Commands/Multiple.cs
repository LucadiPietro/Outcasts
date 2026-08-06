namespace Common.Cutscenes.Commands
{
    using Common;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Runs a group of commands in parallel and waits until all awaitable children end.
    /// </summary>
    [Serializable]
    [AddTypeMenu("Multiple")]
    public sealed class Multiple : ICinematicCommand
    {
        [SerializeReference, SubclassSelector]
        List<ICinematicCommand> m_Commands = new List<ICinematicCommand>();

        [SerializeField] bool m_ShouldWaitEnd = true;

        public bool ShouldWaitEnd => m_ShouldWaitEnd;

        public void Execute()
        {
            if (m_Commands == null)
            {
                return;
            }

            for (int i = 0; i < m_Commands.Count; i++)
            {
                ICinematicCommand command = m_Commands[i];
                if (command != null)
                {
                    command.Execute();
                }
            }
        }

        public IEnumerator ExecuteAwaitable()
        {
            if (m_Commands == null || m_Commands.Count == 0)
            {
                yield break;
            }

            var routines = new List<Coroutine>(m_Commands.Count);
            int remaining = 0;

            try
            {
                for (int i = 0; i < m_Commands.Count; i++)
                {
                    ICinematicCommand command = m_Commands[i];
                    if (command == null)
                    {
                        continue;
                    }

                    remaining++;
                    Coroutine routine = Coroutiner.Start(
                        RunChild(command, () => remaining--));
                    if (routine != null)
                    {
                        routines.Add(routine);
                    }
                    else
                    {
                        remaining--;
                    }
                }

                while (remaining > 0)
                {
                    yield return null;
                }
            }
            finally
            {
                for (int i = 0; i < routines.Count; i++)
                {
                    if (routines[i] != null)
                    {
                        Coroutiner.Stop(routines[i]);
                    }
                }
            }
        }

        public void FastForward()
        {
            if (m_Commands == null)
            {
                return;
            }

            for (int i = 0; i < m_Commands.Count; i++)
            {
                m_Commands[i]?.FastForward();
            }
        }

        /// <summary>
        /// Executes one child and always releases the group wait counter.
        /// </summary>
        static IEnumerator RunChild(ICinematicCommand command, Action completed)
        {
            try
            {
                if (command.ShouldWaitEnd)
                {
                    IEnumerator routine = command.ExecuteAwaitable();
                    if (routine != null)
                    {
                        yield return routine;
                    }
                }
                else
                {
                    command.Execute();
                }
            }
            finally
            {
                completed?.Invoke();
            }
        }
    }
}
