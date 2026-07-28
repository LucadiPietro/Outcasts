namespace Common.Dialogues
{
    using NaughtyAttributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// A single character than can speak using the DialogueSystem.
    /// It has a Name, an optional Description, and a list of Emotion (facial expressions).
    /// It also has a DefaultEmotion as a fallback for when a specific emotion is missing 
    /// </summary>
    [CreateAssetMenu(fileName = "Speaker", menuName = "Common/Dialogues/Speaker")]
    public sealed class Speaker : ScriptableObject
    {
        [SerializeField] string m_CharacterName;
        public string Name => m_CharacterName;

        [SerializeField, ResizableTextArea] string m_Description;
        public string Description => m_Description;

        [SerializeField, Tooltip("This is the default expression of the character, used when a more specific emotion is not defined")] Emotion m_DefaultEmotion;
        [SerializeField, Tooltip("List of all the possible emotions the character can show")] List<Emotion> m_Emotions;

        /// <summary>
        /// Get Emotion's data from its ID
        /// </summary>
        public Emotion GetEmotion(EmotionId emotionId)
        {
            if (m_DefaultEmotion.Id == emotionId) return m_DefaultEmotion;

            var emotion = m_Emotions.FirstOrDefault(emotion => emotion.Id == emotionId);
            if (emotion != null) return emotion;
            else
            {
                Debug.LogError($"Emotion with id {emotionId} was not found on speaker {Name}... falling back to the DefaultEmotion");
                return m_DefaultEmotion;
            }
        }

        /// <summary>
        /// Facial Expression of a character in the dialogue system.
        /// It has a Name, an Image for the character face, and a Color used as a background for the character name
        /// </summary>
        [Serializable]
        public class Emotion
        {
            [SerializeField] EmotionId m_Id = EmotionId.Normal;
            public EmotionId Id => m_Id;

            [SerializeField] string m_EmotionName = "Normal";
            public string Name => m_EmotionName;

            [SerializeField] Sprite m_Image;
            public Sprite Image => m_Image;

            [SerializeField] Color m_Color;
            public Color Color => m_Color;

            [SerializeField] AudioClip[] m_SpeechSfx;
            public AudioClip[] SpeechSfx => m_SpeechSfx;

            public Emotion(EmotionId id, string name, Sprite image, Color color, IEnumerable<AudioClip> clips)
            {
                m_Id = id;
                m_EmotionName = name;
                m_Image = image;
                m_Color = color;
                m_SpeechSfx = clips.ToArray();
            }
        }

#if UNITY_EDITOR
        public static Speaker Create(string name, string description, Emotion defaultEmotion, List<Emotion> emotions, string path)
        {
            var speaker = AssetDatabase.LoadAssetAtPath<Speaker>(path);
            bool isNew = speaker == null;

            if (isNew) speaker = ScriptableObject.CreateInstance<Speaker>();

            speaker.m_CharacterName = name;
            speaker.m_Description = description;
            speaker.m_DefaultEmotion = defaultEmotion;
            speaker.m_Emotions = new List<Emotion>(emotions);

            if (isNew) AssetDatabase.CreateAsset(speaker, path);
            else
            {
                EditorUtility.SetDirty(speaker);
                AssetDatabase.SaveAssetIfDirty(speaker);
            }

            AssetDatabase.Refresh();
            return speaker;
        }
#endif
    }

    public enum EmotionId
    {
        Normal,
        Doubt,
        Anger,
        Irritation,
        FacePalm,
        Surprise,
    }

    // TODO: This is temporary, remove after standardizing Excel file inputs
    public static class EmotionIdExtensions
    {
        public static EmotionId FromString(string emotionIdString)
        {
            if (Enum.TryParse<EmotionId>(emotionIdString, out var emotionId)) return emotionId;

            switch (emotionIdString)
            {
                case "DUBBIO": return EmotionId.Doubt;
                case "SORPRESA": return EmotionId.Surprise;
                case "IRRITATO": return EmotionId.Irritation;
                default:
                    Debug.LogError($"Unknown {nameof(EmotionId)} value '{emotionIdString}'. Falling back to {EmotionId.Normal}");
                    return EmotionId.Normal;
            }
        }
    }
}
