using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiButton : UiElement
{
    private readonly int _isDownHash = Animator.StringToHash("_IsDown");

    public enum Type { None, Quit }

    public Type ButtonType;

    private Animator _animator;

    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
    }
    
    private void OnMouseDown()
    {
        Press();
    }

    private void OnMouseUp()
    {
        Release();
    }

    private void Press()
    {
        _animator.SetBool(_isDownHash, true);
    }

    private void Release()
    {
        _animator.SetBool(_isDownHash, false);
        switch (ButtonType)
        {
            case Type.Quit:
                Application.Quit();
                break;
        }
    }
}
