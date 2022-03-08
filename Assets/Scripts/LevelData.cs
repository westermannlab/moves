using UnityEngine;
#if (UNITY_EDITOR)
using UnityEditor;
#endif
using UnityEngine.SceneManagement;

public class LevelData : ScriptableObject
{
    public enum Type { Default, Tutorial, Menu }
    
#if (UNITY_EDITOR)
    public SceneAsset Scene;
#endif
    public string SceneName;
    public Type LevelType;
}
