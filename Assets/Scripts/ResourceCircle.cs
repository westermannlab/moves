using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCircle : MonoBehaviour
{
    public Sprite[] StateSprites;
    
    public SpriteRenderer Sr;
    public SpriteRenderer BackgroundSr;
    public SpriteRenderer BacklightSr;

    private float _currentFillState;

    private Color _circleColor = Color.white;
    private Color _backgroundColor = Color.white;
    private Color _defaultColor;
    private float _fadeTimer;
    private float _flashTimer;
    private float _currentAlpha;
    private float _alphaHolder;
    private int _fadeId;

    private void Awake()
    {
        _circleColor = Sr.color;
        _defaultColor = _circleColor;
        _currentFillState = 1f;
        ApplyAlpha();
    }

    public void SetColor(Color color)
    {
        _circleColor = color;
        BacklightSr.color = color;
        ApplyAlpha();
    }

    public void ResetColor()
    {
        _circleColor = _defaultColor;
        ApplyAlpha();
    }
    
    public void UpdateState(float fillState)
    {
        if (Mathf.Approximately(_currentFillState, 1f) && fillState < 1f)
        {
            Fade(1f, 0.25f);
        }
        else if (_currentFillState < 1f && Mathf.Approximately(fillState, 1f))
        {
            Fade(0f, 0.5f);
        }
        
        _currentFillState = fillState;
        Sr.sprite = StateSprites[Mathf.FloorToInt(Mathf.Clamp01(1f -_currentFillState) * (StateSprites.Length - 1))];

        if (Mathf.Approximately(_currentFillState, 0f))
        {
            Flash(1f);
        }
    }

    public void Deactivate()
    {
        _currentFillState = 1f;
        Fade(0f, 0.25f);
    }

    private void Fade(float targetAlpha, float duration)
    {
        _fadeId = Utility.AddOne(_fadeId);
        StartCoroutine(FadeRoutine(targetAlpha, duration, _fadeId));
    }

    private void Flash(float speed)
    {
        _fadeId = Utility.AddOne(_fadeId);
        StartCoroutine(FlashRoutine(speed, _fadeId));
    }

    private void ApplyAlpha()
    {
        _circleColor.a = _currentAlpha;
        _backgroundColor.a = _currentAlpha;
        Sr.color = _circleColor;
        BackgroundSr.color = _backgroundColor;
        BacklightSr.color = _circleColor;

    }
    
    private IEnumerator FadeRoutine(float targetAlpha, float duration, int id)
    {
        _fadeTimer = 0f;
        _alphaHolder = _currentAlpha;
        while (_fadeTimer < duration && _fadeId == id)
        {
            _fadeTimer += Time.deltaTime;
            _currentAlpha = Mathf.Lerp(_alphaHolder, targetAlpha, _fadeTimer / duration);
            ApplyAlpha();
            yield return null;
        }

        if (_fadeId == id)
        {
            _currentAlpha = targetAlpha;
            ApplyAlpha();
        }

        yield return null;
    }

    private IEnumerator FlashRoutine(float speed, int id)
    {
        _flashTimer = 0f;
        while (Mathf.Approximately(_currentFillState, 0f) && _fadeId == id)
        {
            while (_flashTimer < 1f / speed && _fadeId == id)
            {
                _flashTimer += Time.deltaTime;
                _currentAlpha = 1f - Mathf.PingPong(_flashTimer, 0.5f);
                ApplyAlpha();
                yield return null;
            }

            _flashTimer = 0f;
            yield return null;
        }
        yield return null;
    }
}
