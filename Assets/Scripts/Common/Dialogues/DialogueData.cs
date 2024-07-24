namespace Common.Dialogues
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Dialogue", menuName = "Common/Dialogues/Dialogue")]
    public sealed class DialogueData : ScriptableObject
    {
        [SerializeField] List<DialogueLine> m_Lines;
        public IReadOnlyList<DialogueLine> Lines => m_Lines;

#if UNITY_EDITOR
        public static DialogueData Create(List<DialogueLine> lines, string path)
        {
            var dialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
            if (dialogue == null)
            {
                dialogue = ScriptableObject.CreateInstance<DialogueData>();
                dialogue.m_Lines = new List<DialogueLine>(lines);
                AssetDatabase.CreateAsset(dialogue, path);
            }
            else
            {
                dialogue.m_Lines = new List<DialogueLine>(lines);
                EditorUtility.SetDirty(dialogue);
                AssetDatabase.SaveAssetIfDirty(dialogue);
            }

            AssetDatabase.Refresh();
            return dialogue;
        }
#endif
    }

    [Serializable]
    public struct DialogueSlice
    {
        [SerializeField] DialogueData m_Dialogue;
        public DialogueData Dialogue => m_Dialogue;

        [SerializeField] int m_StartLineIndex;
        public int StartLineIndex => m_StartLineIndex;

        [SerializeField] int m_LineCount;
        public int LineCount => m_LineCount;

        public DialogueSlice(DialogueData dialogue) : this(dialogue, -1, -1) { }
        public DialogueSlice(DialogueData dialogue, int startIndex) : this(dialogue, startIndex, -1) { }
        public DialogueSlice(DialogueData dialogue, int startIndex, int lineCount)
        {
            m_Dialogue = dialogue;
            m_StartLineIndex = Mathf.Clamp(startIndex, 0, m_Dialogue.Lines.Count - 1);
            int maxLineCount = m_Dialogue.Lines.Count - m_StartLineIndex;
            m_LineCount = (lineCount < 0) ? maxLineCount : Mathf.Min(lineCount, maxLineCount);
        }

        public static implicit operator DialogueSlice(DialogueData dialogue) => new DialogueSlice(dialogue);
    }
}
