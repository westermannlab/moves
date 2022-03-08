using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiCheckmark : UiElement
{
    protected override void Awake()
    {
        base.Awake();
        Deactivate(0f);
    }
}
