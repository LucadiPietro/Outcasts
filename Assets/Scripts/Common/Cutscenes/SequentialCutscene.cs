namespace Common.Cutscenes
{
    using Common.Cutscenes.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Executes small cinematic commands in deterministic serialized order.
    /// Long scenes should use this component as an orchestrator of smaller
    /// SequentialCutscene components through PlayCutscene commands.
    /// </summary>
    public sealed class SequentialCutscene : CutsceneBase
    {
        [SerializeReference, SubclassSelector]
        List<ICinematicCommand> m_Commands = new List<ICinematicCommand>();

        [SerializeField] string m_CinematicKey = string.Empty;

        int m_Index;

        public int CommandCount => m_Commands?.Count ?? 0;
        public int CurrentCommandIndex => m_Index;

        protected override string kPrefKey =>
            string.IsNullOrWhiteSpace(m_CinematicKey) ? base.kPrefKey : m_CinematicKey;

        protected override IEnumerator Sequence()
        {
            m_Index = 0;

            if (m_Commands == null)
            {
                yield break;
            }

            while (m_Index < m_Commands.Count)
            {
                int commandIndex = m_Index;
                ICinematicCommand command = m_Commands[m_Index++];

                if (command == null)
                {
                    Debug.LogWarning(
                        $"{name}: skipped a null cutscene command at index {commandIndex}.",
                        this);
                    continue;
                }

                yield return ExecuteCommandSafely(command, commandIndex);
            }
        }

        public override void FastForward()
        {
            CancelPlayback(invokeFinished: false);

            if (m_Commands != null)
            {
                for (int i = 0; i < m_Commands.Count; i++)
                {
                    ICinematicCommand command = m_Commands[i];
                    if (command == null)
                    {
                        continue;
                    }

                    try
                    {
                        command.FastForward();
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError(
                            $"{name}: fast-forward failed at command {i} ({command.GetType().Name}).",
                            this);
                        Debug.LogException(exception, this);
                    }
                }
            }

            CompleteImmediately();
        }

        IEnumerator ExecuteCommandSafely(ICinematicCommand command, int commandIndex)
        {
            if (!command.ShouldWaitEnd)
            {
                try
                {
                    command.Execute();
                }
                catch (Exception exception)
                {
                    LogCommandException(command, commandIndex, exception);
                }

                yield break;
            }

            IEnumerator routine;

            try
            {
                routine = command.ExecuteAwaitable();
            }
            catch (Exception exception)
            {
                LogCommandException(command, commandIndex, exception);
                yield break;
            }

            if (routine == null)
            {
                yield break;
            }

            try
            {
                while (true)
                {
                    bool hasNext;

                    try
                    {
                        hasNext = routine.MoveNext();
                    }
                    catch (Exception exception)
                    {
                        LogCommandException(command, commandIndex, exception);
                        yield break;
                    }

                    if (!hasNext)
                    {
                        yield break;
                    }

                    yield return routine.Current;
                }
            }
            finally
            {
                (routine as IDisposable)?.Dispose();
            }
        }

        void LogCommandException(
            ICinematicCommand command,
            int commandIndex,
            Exception exception)
        {
            Debug.LogError(
                $"{name}: command {commandIndex} ({command.GetType().Name}) failed and was skipped.",
                this);
            Debug.LogException(exception, this);
        }
    }
}
