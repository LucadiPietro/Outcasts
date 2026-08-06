using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-10000)]
public class SphericalBattleSceneLoader : MonoBehaviour
{
    [SerializeField] string battleSceneName = "Battle";
    [SerializeField] int applyLayoutDelayFrames = 2;

    IEnumerator Start()
    {
        Scene battleScene = SceneManager.GetSceneByName(battleSceneName);
        if (!battleScene.isLoaded)
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(battleSceneName, LoadSceneMode.Additive);
            if (loadOperation == null)
            {
                Debug.LogError(
                    "SphericalBattleSceneLoader: impossibile caricare '" + battleSceneName +
                    "'. Verifica che Battle sia presente nelle Build Settings.");
                yield break;
            }

            yield return loadOperation;
            battleScene = SceneManager.GetSceneByName(battleSceneName);
        }

        for (int i = 0; i < Mathf.Max(1, applyLayoutDelayFrames); i++)
        {
            yield return null;
        }

        SphericalBattleLayout.ApplyToCurrentBattle();

        if (battleScene.IsValid() && battleScene.isLoaded)
        {
            SceneManager.SetActiveScene(battleScene);
        }
    }
}
