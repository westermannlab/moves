using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager : ScriptableObject
{
    public Boost Boost;
    public Brake Brake;
    public Rain Rain;
    public Earthquake Shake;

    public bool IsItBoosting()
    {
        return Boost.IsPerformingAction();
    }

    public bool IsItBraked()
    {
        return Brake.IsPerformingAction();
    }
    
    public bool IsItRaining()
    {
        return Rain.IsPerformingAction();
    }

    public bool IsItShaking()
    {
        return Shake.IsPerformingAction();
    }
}
