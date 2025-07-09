namespace LemonGames.Editor
{
    using UnityEditor;
    using UnityEngine;

    public static class MenuItems
    {
        [MenuItem("Outcasts/Open save folder")]
        static void ShowSavedData()
        {
            EditorUtility.OpenWithDefaultApp(Application.persistentDataPath);
        }
    }
}

