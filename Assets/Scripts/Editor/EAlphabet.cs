#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Alphabet))]
public class EAlphabet : Editor
{

    public override void OnInspectorGUI()
    {

        var alphabet = target as Alphabet;

        DrawDefaultInspector();

        if (alphabet == null)
        {
            EditorGUILayout.LabelField("Alphabet missing.");
            return;
        }
        EditorGUIUtility.labelWidth = 160;

        if (GUILayout.Button("Initialize"))
        {
            alphabet.InitializeDictionary();
        }
    }
}
#endif
