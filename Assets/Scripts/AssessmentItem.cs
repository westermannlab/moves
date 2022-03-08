using UnityEngine;

public class AssessmentItem : ScriptableObject
{
    [TextArea(2, 10)]
    public string Text;
    public string[] LevelCaptions;
    public string Identifier;

    public int Value;

    public override string ToString()
    {
        return string.Concat(Identifier, ": ", Value);
    }
}
