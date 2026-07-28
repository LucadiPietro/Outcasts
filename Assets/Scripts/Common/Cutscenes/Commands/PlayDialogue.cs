namespace Common.Cutscenes.Commands
{
    using Common.Dialogues;
    using NaughtyAttributes;
    using System;
    using System.Collections;
    using UnityEngine;

    [Serializable]
    [AddTypeMenu("Play Dialogue")]
    public sealed class PlayDialogue : ICinematicCommand
    {
        [OnValueChanged(nameof(OnDialogueChanged)), AllowNesting]
        [SerializeField] DialogueData m_Dialogue;
        [SerializeField] LineRange m_Range;
        bool IsCustom => m_Range == LineRange.Custom;

        [Dropdown(nameof(GetLines)), AllowNesting]
        [SerializeField, ShowIf(nameof(IsCustom))] int m_From;
        [Dropdown(nameof(GetLines)), AllowNesting]
        [SerializeField, ShowIf(nameof(IsCustom))] int m_To;

        [SerializeField] bool m_ShouldWaitEnd = true;
        public bool ShouldWaitEnd => m_ShouldWaitEnd;

        public void Execute()
        {
            if (IsCustom) DialogueController.instance.PlayDialogue(new DialogueSlice(m_Dialogue, m_From, m_To - m_From + 1));
            else DialogueController.instance.PlayDialogue(m_Dialogue);
        }
        public IEnumerator ExecuteAwaitable()
        {
            if (IsCustom) yield return DialogueController.instance.PlayDialogueAwaitable(new DialogueSlice(m_Dialogue, m_From, m_To - m_From + 1));
            else yield return DialogueController.instance.PlayDialogueAwaitable(m_Dialogue);
        }
        public void FastForward()
        {
            DialogueController.instance.StopCurrentDialogue();
        }

        enum LineRange
        {
            All,
            Custom,
        }
//#if UNITY_EDITOR
        #region Lines
        DropdownList<int> GetLines()
        {
            var list = new DropdownList<int>();
            if (m_Dialogue != null)
            {
                for (int i = 0; i < m_Dialogue.Lines.Count; i++)
                {
                    var line = m_Dialogue.Lines[i];
                    var preview = GetLinePreview(i);
                    list.Add(preview, i);
                }
            }
            else list.Add("<None>", -1);

            return list;
        }
        void OnDialogueChanged()
        {
            m_From = 0;
            m_To = 1;
        }
        string GetLinePreview(int lineIndex)
        {
            if (m_Dialogue == null) return "";
            var line = m_Dialogue.Lines[lineIndex];

            const int kMaxPreviewLength = 30;
            int previewLength = Mathf.Min(line.Text.Length, kMaxPreviewLength);
            string suffix = "";
            if (previewLength == kMaxPreviewLength)
            {
                previewLength -= 3;
                suffix = "...";
            }

            var previewText = line.Text.Substring(0, previewLength).Replace(" \n ", " ").Replace(" \n", " ").Replace("\n ", " ").Replace("\n", " ");
            return $"{lineIndex:D2}) {line.Speaker.Name}: \"{previewText}{suffix}\"";
        }
        #endregion
//#endif
    }
}
