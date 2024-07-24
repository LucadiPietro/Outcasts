namespace Common.Dialogues
{
    using NaughtyAttributes;
    using System;
    using UnityEngine;

    [Serializable]
    public sealed class DialogueLine
    {
        [SerializeField] Speaker m_Speaker;
        public Speaker Speaker => m_Speaker;

        [SerializeField] EmotionId m_Emotion;
        public EmotionId Emotion => m_Emotion;

        [SerializeField, ResizableTextArea, AllowNesting] string m_Text;
        public string Text => m_Text;

        public Speaker.Emotion GetEmotion() => Speaker.GetEmotion(Emotion);

        public DialogueLine(Speaker speaker, EmotionId emotion, string text)
        {
            m_Text = text;
            m_Emotion = emotion;
            m_Speaker = speaker;
        }
    }

}