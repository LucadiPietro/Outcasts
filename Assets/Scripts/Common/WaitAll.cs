namespace Common.Cutscenes.Commands
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Pool;

    /// <summary>
    /// !!! Needs to be tested extensively before use !!!
    /// </summary>
    public sealed class WaitAll : CustomYieldInstruction
    {
        List<bool> m_IsWaiting;
        public WaitAll(IReadOnlyList<IEnumerator> enumerators)
        {
            m_IsWaiting = ListPool<bool>.Get();

            for (int i = 0; i < enumerators.Count; i++)
            {
                m_IsWaiting.Add(true);
                Coroutiner.Start(RunUntilDestroyedByUnity(enumerators[i], i));
            }
        }
        ~WaitAll()
        {
            ListPool<bool>.Release(m_IsWaiting);
        }

        public override bool keepWaiting => m_IsWaiting.Contains(true);

        IEnumerator RunUntilDestroyedByUnity(IEnumerator enumerator, int index)
        {
            while (true)
            {
                if (enumerator != null && enumerator.MoveNext())
                {
                    m_IsWaiting[index] = true;
                    yield return enumerator.Current;
                }
                else
                {
                    m_IsWaiting[index] = false;
                    break;
                }
            }
        }
    }
}
