using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiElement : MonoBehaviour
{
    protected Transform Tf;
    protected SpriteRenderer Sr;
    
    protected Color CurrentColor;
    private float _changeAlphaTimer;
    private float _currentAlpha;
    private float _alphaHolder;
    private int _changeAlphaId;
    
    private Vector2 _basePosition;
    private Vector2 _currentOffset;
    private Vector2 _offsetHolder;
    private float _moveTimer;
    private int _moveId;
    
    protected virtual void Awake()
    {
        Tf = transform;
        Sr = GetComponentInChildren<SpriteRenderer>();
        if (Sr)
        {
            CurrentColor = Sr.color;
            _currentAlpha = CurrentColor.a;
        }
        else
        {
            CurrentColor = Color.white;
            _currentAlpha = 1f;
        }
        _basePosition = Tf.localPosition;
    }

    public void SetColor(Color color)
    {
        CurrentColor = color;
        ApplyCurrentAlpha();
    }
    
    public void SetLocalPosition(Vector2 localPosition)
    {
        _basePosition = localPosition;
        Tf.localPosition = localPosition;
    }

    public void SetScale(float scale)
    {
        Tf.localScale = scale * Vector3.one;
    }
    
    protected void ChangeAlpha(float targetAlpha, float duration)
    {
        _changeAlphaId = Utility.AddOne(_changeAlphaId);
        StartCoroutine(ChangeAlphaRoutine(targetAlpha, duration, _changeAlphaId));
    }

    protected virtual void ApplyCurrentAlpha()
    {
        CurrentColor.a = _currentAlpha;
        if (Sr)
        {
            Sr.color = CurrentColor;
        }
    }
    
    public virtual void Activate(float duration = 0.125f)
    {
        ChangeAlpha(1f, duration);
    }

    public virtual void Deactivate(float duration = 0.125f)
    {
        ChangeAlpha(0f, duration);
    }
    
    public void Move(Vector2 offset, float duration, float delay = 0f)
    {
        _moveId = Utility.AddOne(_moveId);
        StartCoroutine(MoveRoutine(offset, duration, delay, _moveId));
    }

    private IEnumerator ChangeAlphaRoutine(float targetAlpha, float duration, int id)
    {
        _changeAlphaTimer = 0f;
        _alphaHolder = _currentAlpha;
        
        while (_changeAlphaTimer < duration && _changeAlphaId == id)
        {
            _changeAlphaTimer += Time.unscaledDeltaTime;
            _currentAlpha = Mathf.Lerp(_alphaHolder, targetAlpha, _changeAlphaTimer / duration);
            ApplyCurrentAlpha();
            yield return null;
        }

        if (_changeAlphaId == id)
        {
            _currentAlpha = targetAlpha;
            ApplyCurrentAlpha();
        }
        
        yield return null;
    }
    
    private IEnumerator MoveRoutine(Vector2 targetOffset, float duration, float delay, int id)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        _moveTimer = 0f;
        _offsetHolder = _currentOffset;
        while (_moveTimer < duration && _moveId == id)
        {
            _moveTimer += Time.deltaTime;
            _currentOffset = Vector2.Lerp(_offsetHolder, targetOffset, _moveTimer / duration);
            Tf.localPosition = _basePosition + _currentOffset;
            yield return null;
        }

        if (_moveId == id)
        {
            _currentOffset = targetOffset;
            Tf.localPosition = _basePosition + _currentOffset;
        }
        
        yield return null;
    }
}
