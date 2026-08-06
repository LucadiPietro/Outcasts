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
        [Dropdown(nameof(GetLines)), AllowNesting]
        [SerializeField, ShowIf(nameof(IsCustom))] int m_From;
        [Dropdown(nameof(GetLines)), AllowNesting]
        [SerializeField, ShowIf(nameof(IsCustom))] int m_To;
        [SerializeField] bool m_ShouldWaitEnd = true;

        enum LineRange
        {
            All,
            Custom,
        }

        public bool ShouldWaitEnd => m_ShouldWaitEnd;
        bool IsCustom => m_Range == LineRange.Custom;

        public void Execute()
        {
            if (!TryBuildDialogue(out DialogueController controller, out DialogueSlice slice))
            {
                return;
            }

            if (IsCustom)
            {
                controller.PlayDialogue(slice);
            }
            else
            {
                controller.PlayDialogue(m_Dialogue);
            }
        }

        public IEnumerator ExecuteAwaitable()
        {
            if (!TryBuildDialogue(out DialogueController controller, out DialogueSlice slice))
            {
                yield break;
            }

            if (IsCustom)
            {
                yield return controller.PlayDialogueAwaitable(slice);
            }
            else
            {
                yield return controller.PlayDialogueAwaitable(m_Dialogue);
            }
        }

        public void FastForward()
        {
            if (DialogueController.instance != null)
            {
                DialogueController.instance.StopCurrentDialogue();
            }
        }

        /// <summary>
        /// Validates the controller, asset, and custom line range.
        /// </summary>
        bool TryBuildDialogue(out DialogueController controller, out DialogueSlice slice)
        {
            controller = DialogueController.instance;
            slice = default;

            if (controller == null || m_Dialogue == null)
            {
                Debug.LogWarning("PlayDialogue skipped because its controller or dialogue asset is missing.");
                return false;
            }

            if (!IsCustom)
            {
                return true;
            }

            int lineCount = m_Dialogue.Lines.Count;
            if (lineCount <= 0)
            {
                Debug.LogWarning("PlayDialogue skipped because the selected dialogue contains no lines.");
                return false;
            }

            int from = Mathf.Clamp(m_From, 0, lineCount - 1);
            int to = Mathf.Clamp(m_To, from, lineCount - 1);
            slice = new DialogueSlice(m_Dialogue, from, to - from + 1);
            return true;
        }

        DropdownList<int> GetLines()
        {
            var list = new DropdownList<int>();
            if (m_Dialogue == null)
            {
                list.Add("<None>", -1);
                return list;
            }

            for (int i = 0; i < m_Dialogue.Lines.Count; i++)
            {
                list.Add(GetLinePreview(i), i);
            }

            return list;
        }

        void OnDialogueChanged()
        {
            m_From = 0;
            m_To = m_Dialogue != null && m_Dialogue.Lines.Count > 1 ? 1 : 0;
        }

        string GetLinePreview(int lineIndex)
        {
            if (m_Dialogue == null || lineIndex < 0 || lineIndex >= m_Dialogue.Lines.Count)
            {
                return string.Empty;
            }

            var line = m_Dialogue.Lines[lineIndex];
            string text = line.Text ?? string.Empty;
            text = text.Replace("\n", " ").Trim();

            const int maxPreviewLength = 30;
            if (text.Length > maxPreviewLength)
            {
                text = text.Substring(0, maxPreviewLength - 3) + "...";
            }

            string speaker = line.Speaker != null ? line.Speaker.Name : "Unknown";
            return $"{lineIndex:D2}) {speaker}: \"{text}\"";
        }
    }
}
