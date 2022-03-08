using System.Collections.Generic;
using UnityEngine;

public class Assessment : ScriptableObject
{
    public AssessmentItem[] Items;
    public readonly List<AssessmentScale> ScaleList = new List<AssessmentScale>();
    public LevelData CurrentLevelData;
    
    private int _scaleCount;

    public void ResetData()
    {
        ScaleList.Clear();
    }
    
    public void AddScale(AssessmentScale scale)
    {
        ScaleList.Add(scale);
    }
}
