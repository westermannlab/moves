using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AccessData : MonoBehaviour
{
    public LevelData MainMenuScene;

    private void Awake()
    {
        StartCoroutine(ReadDataRoutine());
    }

    private IEnumerator ReadDataRoutine()
    {
#if UNITY_WEBGL
        References.Io.TryToAccessFiles();
        while (!References.Io.HasBeenInitialized)
        {
            yield return null;
        }
#endif

        References.Settings.EnableCompanion(false);
        SceneManager.LoadScene(MainMenuScene.SceneName);
        yield return null;
    }
}
