using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteObject : PoolObject
{
    protected SpriteRenderer Sr;
    
    private Color _currentColor;
    private float _currentAlpha;
    private float _alphaHolder;

    private Color _baseColor;
    private float _baseAlpha;

    private float _currentScale;
    private float _scaleHolder;

    private Color _colorHolder;
    private float _changeColorTimer;
    private int _changeColorId;

    private Vector2 _currentPosition;
    private Vector2 _positionHolder;

    protected override void Awake()
    {
        base.Awake();
        Sr = GetComponentInChildren<SpriteRenderer>();
        _currentColor = Sr.color;
        _currentAlpha = _currentColor.a;
        _baseColor = _currentColor;
        _baseAlpha = _currentAlpha;
    }

    public override void Reset()
    {
        base.Reset();
        _currentColor = _baseColor;
        _currentAlpha = _baseAlpha;
        ApplyCurrentColor();
    }

    public void SetSprite(Sprite sprite)
    {
        Sr.sprite = sprite;
    }

    public void SetColor(Color color)
    {
        _currentColor = color;
        ApplyCurrentColor();
    }

    public void ChangeColor(Color targetColor, float duration)
    {
        _changeColorId = Utility.AddOne(_changeColorId);
        StartCoroutine(ChangeColorRoutine(targetColor, duration, _changeColorId));
    }

    public void SetAlpha(float alpha)
    {
        _currentAlpha = alpha;
        ApplyCurrentColor();
    }

    public void SetScale(float scale)
    {
        _currentScale = scale;
        ApplyCurrentScale();
    }
    
    public virtual void Fade(float targetAlpha, float duration, float delay = 0f)
    {
        StartCoroutine(FadeRoutine(targetAlpha, duration, delay));
    }

    public void Scale(float targetScale, float duration, float delay = 0f)
    {
        StartCoroutine(ScaleRoutine(targetScale, duration, delay));
    }

    public void MoveDelta(Vector2 targetDeltaPosition, float duration, float delay = 0f)
    {
        _currentPosition = Tf.position;
        StartCoroutine(MoveRoutine(_currentPosition + targetDeltaPosition, duration, delay));
    }
    
    public void SetSpriteSortingOrder(int sortingOrder)
    {
        Sr.sortingOrder = sortingOrder;
    }
    
    public void FlipX(bool flipX)
    {
        Sr.flipX = flipX;
    }

    private void ApplyCurrentColor()
    {
        _currentColor.a = _currentAlpha;
        Sr.color = _currentColor;
    }

    private void ApplyCurrentScale()
    {
        Tf.localScale = _currentScale * Vector3.one;
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        var timer = 0f;
        _alphaHolder = _currentAlpha;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            _currentAlpha = Mathf.Lerp(_alphaHolder, targetAlpha, timer / duration);
            ApplyCurrentColor();
            yield return null;
        }
        yield return null;
    }

    private IEnumerator ScaleRoutine(float targetScale, float duration, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        var timer = 0f;
        _scaleHolder = _currentScale;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            _currentScale = Mathf.Lerp(_scaleHolder, targetScale, timer / duration);
            ApplyCurrentScale();
            yield return null;
        }
        yield return null;
    }
    
    private IEnumerator MoveRoutine(Vector2 targetPosition, float duration, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        var timer = 0f;
        _positionHolder = _currentPosition;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            _currentPosition = Vector2.Lerp(_positionHolder, targetPosition, timer / duration);
            SetPosition(_currentPosition);
            yield return null;
        }
        yield return null;
    }

    private IEnumerator ChangeColorRoutine(Color targetColor, float duration, int id)
    {
        _changeColorTimer = 0f;
        _colorHolder = _currentColor;
        while (_changeColorTimer < duration && _changeColorId == id)
        {
            _changeColorTimer += Time.deltaTime;
            _currentColor = Color.Lerp(_colorHolder, targetColor, _changeColorTimer / duration);
            ApplyCurrentColor();
            yield return null;
        }

        if (_changeColorId == id)
        {
            _currentColor = targetColor;
            ApplyCurrentColor();
        }

        yield return null;
    }
}
