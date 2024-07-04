namespace Common.Dialogues
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [CreateAssetMenu(fileName = "SpeakerDatabase", menuName = "Common/Dialogues/SpeakerDatabase")]
    public sealed class SpeakerDatabase : ScriptableObject
    {
        static readonly string kDatabasePath = "DialogueSystem/Characters/SpeakerDatabase";

        static SpeakerDatabase s_Instance = default;
        public static SpeakerDatabase Instance
        {
            get
            {
                if (s_Instance == null) s_Instance = Resources.Load<SpeakerDatabase>(kDatabasePath);
                if (s_Instance == null) Debug.LogError($"Trying to access the SpeakerDatabase with Resources.Load(\"{kDatabasePath}\") but it's not there. Please create it");
                return s_Instance;
            }
        }

        [SerializeField] List<Speaker> m_Speakers;

        public bool TryGetSpeakerById(string speakerId, out Speaker speaker)
        {
            speaker = m_Speakers.FirstOrDefault(speaker => speaker.name == speakerId);
            return speaker != null;
        }
        public bool TryGetSpeakerByName(string speakerName, out Speaker speaker)
        {
            speaker = m_Speakers.FirstOrDefault(speaker => speaker.Name == speakerName);
            return speaker != null;
        }
    }
}
