namespace Outcasts.Battle
{
    using UnityEngine;

    public sealed class ScoreView : MonoBehaviour
    {
        int m_Ghosts, m_Misses, m_Goods, m_Greats, m_Perfects = 0;

        void OnEnable()
        {
            BattleManager.Instance.NoteConsumed += UpdateScore;
        }
        void OnDisable()
        {
            if(BattleManager.Instance != null) BattleManager.Instance.NoteConsumed -= UpdateScore;
        }

        void UpdateScore(Note noteConsumed, NoteResult result)
        {
            switch (result)
            {
                case NoteResult.Ghost:
                    m_Ghosts++;
                    break;
                case NoteResult.Miss:
                    m_Misses++;
                    break;
                case NoteResult.Good:
                    m_Goods++;
                    break;
                case NoteResult.Great:
                    m_Greats++;
                    break;
                case NoteResult.Perfect:
                    m_Perfects++;
                    break;
            }
            UpdateScore();
        }

        void UpdateScore()
        {
#if UNITY_EDITOR
            void UpdateScoreInConsole()
            {
                // Reflection is needed because unity doesn't expose a way to clear the console from code
                static void ClearEditorConsole()
                {
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.SceneView));

                    System.Type type = assembly.GetType("UnityEditor.LogEntries");
                    System.Reflection.MethodInfo method = type.GetMethod("Clear");
                    method.Invoke(new object(), null);
                }

                //ClearEditorConsole();
                Debug.Log($"Perfects: {m_Perfects}");
                Debug.Log($"Greats: {m_Greats}");
                Debug.Log($"Goods: {m_Goods}");
                Debug.Log($"Misses: {m_Misses}");
                Debug.Log($"Ghosts: {m_Ghosts}");
                Debug.Log($"----------------------------------------");
            }
            UpdateScoreInConsole();
#endif
        }
    }
}
