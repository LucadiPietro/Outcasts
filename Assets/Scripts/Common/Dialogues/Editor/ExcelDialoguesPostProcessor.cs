namespace Common.Dialogues.Editor
{
    using ExcelDataReader;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Path = System.IO.Path;

    class ExcelDialoguesPostProcessor : AssetPostprocessor
    {
        #region SafeMode
        [MenuItem("Outcasts/Dialogues/Excel automatic convertion/Safe mode ON")]
        static void SafeModeOn() => UseSafeMode = true;
        [MenuItem("Outcasts/Dialogues/Excel automatic convertion/Safe mode OFF")]
        static void SafeModeOff() => UseSafeMode = false;

        static readonly string kPlayerPrefsPath = "ExcelDialoguesPostProcessor_SafeMode";
        static bool UseSafeMode
        {
            get => PlayerPrefs.GetInt(kPlayerPrefsPath, 1) == 1;
            set => PlayerPrefs.SetInt(kPlayerPrefsPath, value ? 1 : 0);
        }
        #endregion

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            foreach (string path in importedAssets)
            {
                // Skipping creation of this asset because it wasn't actually created but moved, it's going to be dealt with later
                if (movedAssets.Contains(path)) continue;

                var info = new PathInfo(path);

                // If the newly imported file is an excel in the controlled directory, we create the DialogueData asset
                if (info.IsExcel && info.IsInsideCorrectDirectory) CreateAsset(info.OriginalPath, info.ScriptableObjectPath);
            }

            foreach (string path in deletedAssets)
            {
                // Skipping deletion of this asset because it wasn't actually deleted but moved, it's going to be dealt with later
                if (movedFromAssetPaths.Contains(path)) continue;

                var info = new PathInfo(path);

                // If the excel file was deleted by the controlled folder, we can:
                // - Remove the matching DialogueData (if SafeMode = OFF)
                // - Give a warning of the left-over (if SafeMode = ON)
                if (info.IsExcel && info.IsInsideCorrectDirectory)
                {
                    if (UseSafeMode) Debug.LogWarning($"Excel file {info.OriginalPath} has been deleted but it left over the {nameof(DialogueData)} asset at {info.ScriptableObjectPath}; If you want to automatically remove dialogues when a excel is deleted, you can turn SafeMode off from the menu 'Outcasts/Dialogues/Outcasts/Dialogues/Excel automatic convertion'");
                    else
                    {
                        Debug.LogWarning($"Excel file '{info.OriginalPath}' has been deleted and the corresponding {nameof(DialogueData)} asset at {info.ScriptableObjectPath} was deleted as a result; If you don't want to automatically remove dialogues when a excel is deleted, you can turn SafeMode ON from the menu 'Outcasts/Dialogues/Outcasts/Dialogues/Excel automatic convertion");
                        AssetDatabase.DeleteAsset(info.ScriptableObjectPath);
                    }
                }
            }

            for (int i = 0; i < movedAssets.Length; i++)
            {
                string from = movedFromAssetPaths[i];
                string to = movedAssets[i];

                var fromInfo = new PathInfo(from);
                var toInfo = new PathInfo(to);
                if (fromInfo.IsExcel && fromInfo.IsInsideCorrectDirectory)
                {
                    // Excel asset was renamed inside the same folder
                    if (toInfo.IsExcel && toInfo.IsInsideCorrectDirectory) AssetDatabase.MoveAsset(fromInfo.ScriptableObjectPath, toInfo.ScriptableObjectPath);

                    // If the excel file was moved from inside to outside of the controlled folder, we can:
                    // - Remove the matching DialogueData (if SafeMode = OFF)
                    // - Give a warning of the left-over (if SafeMode = ON)
                    else
                    {
                        if (UseSafeMode) Debug.LogWarning($"Excel file '{fromInfo.OriginalPath}' has been moved out of the 'Dialogues/Excel' folder to '{toInfo.OriginalPath}' but it left over the {nameof(DialogueData)} asset at {fromInfo.ScriptableObjectPath}; If you want to automatically remove dialogues when a excel is moved out of the controlled folder, you can turn SafeMode OFF from the menu 'Outcasts/Dialogues/Outcasts/Dialogues/Excel automatic convertion");
                        else
                        {
                            Debug.LogWarning($"Excel file '{fromInfo.OriginalPath}' has been moved out of the 'Dialogues/Excel' folder to '{toInfo.OriginalPath}' and the corresponding {nameof(DialogueData)} asset at {fromInfo.ScriptableObjectPath} was deleted as a result; If you don't want to automatically remove dialogues when a excel is moved out of the controlled folder, you can turn SafeMode ON from the menu 'Outcasts/Dialogues/Outcasts/Dialogues/Excel automatic convertion");
                            AssetDatabase.DeleteAsset(fromInfo.ScriptableObjectPath);
                        }
                    }
                }
                else
                {
                    // If the excel file was moved from outside to inside of the controlled folder, we create the related DialogueData asset
                    if (toInfo.IsExcel && toInfo.IsInsideCorrectDirectory) CreateAsset(toInfo.OriginalPath, toInfo.ScriptableObjectPath);
                }
            }
        }

        static void CreateAsset(string excelPath, string newAssetPath)
        {
            var database = SpeakerDatabase.Instance;
            if (database == null)
            {
                Debug.LogError($"The SpeakerDatabase wasn't found while processing {excelPath}. Please create one and reimport the dialogues");
                return;
            }

            var lines = new List<DialogueLine>();
            using (var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    int lineIndex = 0;
                    while (reader.Read())
                    {
                        var characterName = reader.GetString(0);
                        if (string.IsNullOrEmpty(characterName)) break;

                        var emotionId = EmotionIdExtensions.FromString(reader.GetString(1));
                        var text = reader.GetString(2);

                        if (!database.TryGetSpeakerById(characterName, out var speaker))
                        {
                            Debug.LogError($"Dialogue at {excelPath}; Error at line {lineIndex}; Couldn't find character with name {characterName}");
                        }

                        var line = new DialogueLine(speaker, emotionId, text);
                        lines.Add(line);

                        lineIndex++;
                    }
                }
            }
            var newDialogue = DialogueData.Create(lines, newAssetPath);
            if (newDialogue != null) Debug.Log($"Dialogue at {newAssetPath} successfully created from excel file at {excelPath}");
        }

        static void CreateAssetOld(string excelPath, string newAssetPath)
        {
            var database = SpeakerDatabase.Instance;
            if (database == null)
            {
                Debug.LogError($"The SpeakerDatabase wasn't found while processing {excelPath}. Please create one and reimport the dialogues");
                return;
            }

            var lines = new List<DialogueLine>();
            using (var stream = File.Open(excelPath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    int lineIndex = 0;
                    while (reader.Read())
                    {
                        var characterId = reader.GetString(0);
                        var parts = characterId.Split('_');
                        if (parts.Length != 2)
                        {
                            Debug.LogError($"Dialogue at {excelPath}; Error at line {lineIndex}; CharacterId: {characterId} should contain exactly one underscore '_'");
                            continue;
                        }

                        var characterName = parts[0];
                        if (characterName.Length > 1) characterName = char.ToUpperInvariant(characterName[0]) + parts[0].Substring(1).ToLowerInvariant();
                        else characterName = char.ToUpperInvariant(characterName[0]).ToString();

                        var characterEmotionName = parts[1];
                        var emotionId = EmotionIdExtensions.FromString(characterEmotionName);

                        var text = reader.GetString(1);

                        if (!database.TryGetSpeakerByName(characterName, out var speaker))
                        {
                            Debug.LogError($"Dialogue at {excelPath}; Error at line {lineIndex}; CharacterId: {characterId}; Couldn't find character with name {characterName}");
                        }

                        var line = new DialogueLine(speaker, emotionId, text);
                        lines.Add(line);

                        lineIndex++;
                    }
                }
            }
            var newDialogue = DialogueData.Create(lines, newAssetPath);
            if (newDialogue != null) Debug.Log($"Dialogue at {newAssetPath} successfully created from excel file at {excelPath}");
        }

        /// <summary>
        /// Utility class to store all the information needed for matching the excel file with the ScriptableObject
        /// </summary>
        class PathInfo
        {
            string m_OriginalPath;
            public string OriginalPath => m_OriginalPath;

            string m_ScriptableObjectPath;
            public string ScriptableObjectPath => m_ScriptableObjectPath;

            string m_FileNameWithoutExtension;
            string m_Extension;
            public string Extension => m_Extension;
            public bool IsExcel => Extension == ".xlsx";
            bool m_IsValid;
            public bool IsInsideCorrectDirectory => m_IsValid;

            public PathInfo(string path)
            {
                m_OriginalPath = path;

                m_Extension = Path.GetExtension(path);

                m_FileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);

                var directory = Path.GetDirectoryName(path).Replace('\\', '/');
                m_IsValid = directory.EndsWith("Dialogues/Excel");

                var parentDirectory = Path.GetDirectoryName(directory).Replace('\\', '/');
                m_ScriptableObjectPath = $"{parentDirectory}/{m_FileNameWithoutExtension}.asset";
            }
        }
    }
}
