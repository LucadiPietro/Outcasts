namespace Common.Dialogues
{
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
}
