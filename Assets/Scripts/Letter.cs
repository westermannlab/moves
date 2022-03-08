using System.Collections;
using UnityEngine;

public class Letter : SpriteObject
{
    public enum NegativeColorMode
    {
        Negative,
        Positive
    };
    
    private Color _positiveColor;
    private Color _negativeColor;
    private Vector2 _originalPos;
    private Vector3 _localPos;
    private bool _isActive;
    private bool _isOnHold;
    private float _size = 1;
    private char _character;
    
    // Color
    private Color _previousColor;
    private int _colorChangeId;
    private float _colorChangeTimer;

    // Fade
    private Color _currentLetterColor;
    private Color _fadeColorHolder;
    private Color _preFadeColorHolder;
    private float _fadeTimer;
    private int _fadeId;

    private Color _alphaOverrideColor;
    private float _alphaOverride;
    private float _alphaOverrideTimer;
    private float _alphaOverrideHolder;
    private int _alphaOverrideId;
    
    // Move
    private Vector2 _currentLocalPosition;
    private Vector2 _fromPositionHolder;
    private float _moveTimer;
    private int _moveId;
    
    // Scale
    private float _scaleTimer;
    private bool _scalingEnabled;
    
    // Rotate
    private Quaternion _fromRotation;
    private Quaternion _toRotation;
    private float _rotateTimer;
    
    // Shake
    private Vector2 _shakeOffset;
    private int _xPixelOffset;
    private int _yPixelOffset;

    public override void Reset()
    {
        Sr.sprite = null;
        Sr.sortingOrder = 2;
        _size = 1;
        _positiveColor = Color.white;
        _negativeColor = Color.clear;
        _currentLetterColor = _positiveColor;
        _alphaOverride = 1f;
        _scalingEnabled = true;
        ApplyCurrentColor();
        Tf.localScale = Vector3.one;
        Tf.localRotation = Quaternion.Euler(Vector3.zero);
        base.Reset();
    }

    public bool IsNewLine()
    {
        return _character.Equals('\n');
    }

    public bool IsSpace()
    {
        return _character.Equals(' ');
    }

    public void SetCharacter (char character, Alphabet.Font font = Alphabet.Font.Dos)
    {
        _character = character;
        Sr.sprite = References.Alphabet.GetSprite(_character, font);
    }

    public void SetCharacter (string character, Alphabet.Font font = Alphabet.Font.Dos)
    {
        SetCharacter(character[0], font);
    }

    public void Move(Vector2 direction)
    {
        SetPosition((Vector2)Tf.position + direction);
    }

    public float GetCharacterWidth()
    {
        return Sr.sprite != null ? Sr.sprite.rect.size.x * _size : 0f;
    }

    public float GetCharacterHeight()
    {
        return Sr.sprite.rect.size.y * _size;
    }

    public void SetPosition (Vector2 position)
    {
        Tf.position = position;
        _localPos = Tf.localPosition;
        _localPos.z = 0;
        Tf.localPosition = _localPos;
        _originalPos = _localPos;
    }

    public override void SetLocalPosition(Vector2 localPosition)
    {
        base.SetLocalPosition(localPosition);
        _originalPos = localPosition;
    }

    public Vector2 GetLocalPosition()
    {
        return Tf.localPosition;
    }

    public void SetColor (Color color, NegativeColorMode negativeColorMode = NegativeColorMode.Negative)
    {
        _positiveColor = color;
        _currentLetterColor = _positiveColor;
        ApplyCurrentColor();
        switch (negativeColorMode)
        {
            case NegativeColorMode.Negative:
                _negativeColor = new Color(1.0f - color.r, 1.0f - color.g, 1.0f - color.b, 1.0f);
                break;
            case NegativeColorMode.Positive:
                _negativeColor = _positiveColor;
                break;
        }

    }

    public void SetActive (bool active)
    {
        _isActive = active;
    }

    public void SetLayer (int layer)
    {
        gameObject.layer = layer;
    }

    public void ChangeColor(Color toColor, float duration, float delay = 0f)
    {
        _colorChangeId = Utility.AddOne(_colorChangeId);
        StartCoroutine(ChangeColorRoutine(toColor, duration, delay, _colorChangeId));
    }

    public void ChangeColorTwice(Color toColor, float duration, float delay, Color toSecondColor, float secondDuration, float secondDelay)
    {
        _colorChangeId = Utility.AddOne(_colorChangeId);
        StartCoroutine(ChangeColorRoutine(toColor, duration, delay, _colorChangeId));
        StartCoroutine(ChangeColorRoutine(toSecondColor, secondDuration, secondDelay, _colorChangeId));
    }

    public override void Fade (float toAlpha, float duration, float delay = 0f)
    {
        // no base call
        _fadeId = Utility.AddOne(_fadeId);
        StartCoroutine(FadeRoutine(toAlpha, duration, delay, _fadeId));
    }

    public void OverrideAlpha(float toAlpha, float duration)
    {
        _alphaOverrideId = Utility.AddOne(_alphaOverrideId);
        StartCoroutine(OverrideAlphaRoutine(toAlpha, duration, _alphaOverrideId));
    }

    public void Fade (float fromAlpha, float toAlpha, float duration, float delay = 0)
    {
        StartCoroutine(PreFadeRoutine(fromAlpha, toAlpha, duration, delay));
    }

    public void SetAlpha (float alpha)
    {
        _currentLetterColor.a = alpha;
        ApplyCurrentColor();
    }

    public void Scale(float fromScale, float toScale, float duration)
    {
        if (_isOnHold) return;
        StartCoroutine(ScaleRoutine(fromScale, toScale, duration));
    }

    public void Shake (int intensity = 1)
    {
        StartCoroutine(ShakeRoutine(intensity));
    }

    public void Rotate (float angle, float duration)
    {
        // rotates the letter by [angle]° over [duration] seconds.
        if (Mathf.Approximately(angle, 0f))
        {
            angle = Random.Range(-180, 180);
        }
        StartCoroutine(RotateRoutine(Tf.localRotation.z, angle, duration));
    }

    public void RotateIntoPlace (float duration)
    {
        float offsetAngle = Random.Range(-135, 135);
        StartCoroutine(RotateRoutine(offsetAngle, 0f, duration));
    }

    public void SetSize (float letterSize)
    {
        _size = letterSize;
        Tf.localScale = Vector3.one * _size;
    }

    private void ApplyCurrentColor()
    {
        _alphaOverrideColor = _currentLetterColor;
        _alphaOverrideColor.a *= _alphaOverride;
        Sr.color = _alphaOverrideColor;
    }

    public void MoveTo (Vector2 position, float duration, float delay = 0f)
    {
        _moveId = Utility.AddOne(_moveId);
        StartCoroutine(MoveRoutine(position, duration, delay, _moveId));
    }

    public void SetOnHold()
    {
        _isOnHold = true;
        Sr.enabled = false;
    }

    public void Release ()
    {
        _isOnHold = false;
        Sr.enabled = true;
        Scale(2f, 1f, 0.5f);
    }

    public void EnableScaling(bool enable)
    {
        _scalingEnabled = enable;
    }

    private IEnumerator ChangeColorRoutine(Color toColor, float duration, float delay, int id)
    {
        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(delay);
        }
        _colorChangeTimer = 0f;
        _previousColor = _currentLetterColor;
        while (_colorChangeTimer < duration && _colorChangeId == id)
        {
            _colorChangeTimer += Time.unscaledDeltaTime;
            _currentLetterColor = Color.Lerp(_previousColor, toColor, _colorChangeTimer / duration);
            ApplyCurrentColor();
            yield return null;
        }
        if (_colorChangeId == id)
        {
            _currentLetterColor = toColor;
            ApplyCurrentColor();
        }
        yield return null;
    }

    private IEnumerator ScaleRoutine (float fromScale, float toScale, float duration)
    {
        _scaleTimer = 0f;
        while (_scaleTimer < duration && _scalingEnabled)
        {
            _scaleTimer += Time.unscaledDeltaTime;
            Tf.localScale = Vector3.one * Mathf.Lerp(fromScale * _size, toScale * _size, _scaleTimer / duration);
            _currentLetterColor = Color.Lerp(_negativeColor, _positiveColor, _scaleTimer / duration);
            ApplyCurrentColor();
            yield return null;
        }

        if (_scalingEnabled)
        {
            Tf.localScale = Vector3.one * toScale * _size;
            _currentLetterColor = _positiveColor;
            ApplyCurrentColor();
        }
        yield return null;
    }

    private IEnumerator PreFadeRoutine(float fromAlpha, float toAlpha, float duration, float delay = 0f)
    {
        if (delay > 0f)
        {
            if (Mathf.Approximately(fromAlpha, 0f))
            {
                _currentLetterColor.a = fromAlpha;
                ApplyCurrentColor();
            }
            yield return new WaitForSecondsRealtime(delay);
        }
        _positiveColor.a = fromAlpha;
        _fadeId = Utility.AddOne(_fadeId);
        StartCoroutine(FadeRoutine(toAlpha, duration, 0f, _fadeId));
        yield return null;
    }

    private IEnumerator FadeRoutine (float toAlpha, float duration, float delay, int id)
    {
        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(delay);
        }
        _fadeTimer = 0f;
        _preFadeColorHolder = _currentLetterColor;
        _fadeColorHolder = _positiveColor;
        _fadeColorHolder.a = toAlpha;
        while (_fadeTimer < duration && _fadeId == id)
        {
            _fadeTimer += Time.unscaledDeltaTime;
            _currentLetterColor = Color.Lerp(_preFadeColorHolder, _fadeColorHolder, _fadeTimer / duration);
            ApplyCurrentColor();
            yield return null;
        }

        if (_fadeId == id)
        {
            _currentLetterColor = _fadeColorHolder;
            ApplyCurrentColor();   
        }
        yield return null;
    }

    private IEnumerator ShakeRoutine (int intensity)
    {
        _xPixelOffset = 0;
        _yPixelOffset = 0;
        while (_isActive)
        {
            _xPixelOffset = Random.Range(-1, 2);
            _yPixelOffset = Random.Range(-1, 2);
            _shakeOffset = new Vector2(Utility.PixelsToUnit(_xPixelOffset), Utility.PixelsToUnit(_yPixelOffset));
            Tf.localPosition = _originalPos + _shakeOffset * (intensity / 4f);
            yield return null;
        }
        yield return null;
    }

    private IEnumerator RotateRoutine (float fromAngle, float toAngle, float duration)
    {
        _rotateTimer = 0f;
        _fromRotation = Quaternion.Euler (0f, 0f, fromAngle);
        _toRotation = Quaternion.Euler(0f, 0f, toAngle);
        while (_isActive && _rotateTimer < duration)
        {
            _rotateTimer += Time.unscaledDeltaTime;
            Tf.localRotation = Quaternion.Lerp(_fromRotation, _toRotation, _rotateTimer / duration);
            yield return null;
        }
        Tf.localRotation = Quaternion.Euler(0f, 0f, toAngle);
        yield return null;
    }

    private IEnumerator MoveRoutine (Vector2 toPosition, float duration, float delay, int id)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        _moveTimer = 0f;
        _fromPositionHolder = Tf.localPosition;
        while (_moveTimer < duration && _moveId == id)
        {
            _moveTimer += Time.unscaledDeltaTime;
            _currentLocalPosition = Vector2.Lerp(_fromPositionHolder, toPosition, _moveTimer / duration);
            Tf.localPosition = _currentLocalPosition;
            yield return null;
        }

        if (_moveId == id)
        {
            _currentLocalPosition = toPosition;
            Tf.localPosition = _currentLocalPosition;
        }
        
        yield return null;
    }
    
    private IEnumerator OverrideAlphaRoutine (float toAlpha, float duration, int id)
    {
        _alphaOverrideTimer = 0f;
        _alphaOverrideHolder = _alphaOverride;
        while (_alphaOverrideTimer < duration && _alphaOverrideId == id)
        {
            _alphaOverrideTimer += Time.unscaledDeltaTime;
            _alphaOverride = Mathf.Lerp(_alphaOverrideHolder, toAlpha, _alphaOverrideTimer / duration);
            ApplyCurrentColor();
            yield return null;
        }

        if (_alphaOverrideId == id)
        {
            _alphaOverride = toAlpha;
            ApplyCurrentColor();
        }
        
        yield return null;
    }
    
    protected override IEnumerator ReturnRoutine(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Return();
    }
}
