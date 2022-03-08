using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Label : UiElement
{
    public enum HorizontalAlignment { Left, Center, Right }
    public enum VerticalAlignment { Top, Center, Bottom }

    public Color TextColor = Color.white;
    public float TextSize = 1f;
    public float MaxLineWidth = 5f;
    public float ParentScale = 1f;
    public float FullAlpha = 1f;
    public int SpriteSortingOrder = 64;
    private const int LinePixelHeight = 14;

    private HorizontalAlignment _horizontalAlignment;
    private VerticalAlignment _verticalAlignment;

    private readonly List<Letter> _letters = new List<Letter>();
    private readonly List<List<Letter>> _moduleLetters = new List<List<Letter>>();
    private Color _fadedColor;
    private Letter _letter;
    private string _currentText;
    private float _x;
    private float _y;
    private int _letterIndex;
    private int _lastNewLine;
    private float _shiftToLeft;
    private float _shiftToTop;
    private float _baseMaxLineWidth;
    
    private float _changeTextColorTimer;
    private int _changeTextColorIndex;
    
    private bool _isWritingToModule;

    protected override void Awake()
    {
        base.Awake();
        _baseMaxLineWidth = MaxLineWidth;
        _fadedColor = TextColor;
        _fadedColor.a = 0f;
        _currentText = "";
    }

    public override void Activate(float duration = 0.125f)
    {
        foreach (var letter in _letters)
        {
            letter.Fade(FullAlpha, duration);
        }
    }

    public override void Deactivate(float duration = 0.125f)
    {
        foreach (var letter in _letters)
        {
            letter.Fade(0f, duration);
        }
    }

    public void SetHorizontalAlignment(HorizontalAlignment horizontalAlignment)
    {
        _horizontalAlignment = horizontalAlignment;
        SetTextSize(TextSize);
    }

    public void SetVerticalAlignment(VerticalAlignment verticalAlignment)
    {
        _verticalAlignment = verticalAlignment;
        SetTextSize(TextSize);
    }

    public void SetTextSize(float textSize)
    {
        TextSize = textSize;
        _shiftToLeft = 0.5f * (int) _horizontalAlignment;
        _shiftToTop = 0.5f * (int) _verticalAlignment;
    }

    public void ScaleMaxLineWidth(float scale)
    {
        MaxLineWidth = _baseMaxLineWidth * scale;
    }

    public void SetText(string text, float duration)
    {
        if (text.Equals(_currentText))
        {
            return;
        }
        
        foreach (var l in _letters)
        {
            l.Fade(0f, duration);
            l.Return(duration + 0.125f);
        }
        
        _letters.Clear();
        _moduleLetters.Clear();
        _letter = null;
        _currentText = text;
        _x = 0f;
        _y = 0f;
        _letterIndex = 0;
        _lastNewLine = 0;
        foreach (var character in text)
        {
            if (_x >= MaxLineWidth && character.Equals(' ') || character.Equals('\n'))
            {
                for (var i = _lastNewLine; i < _letters.Count; i++)
                {
                    _letters[i].Move(_shiftToLeft * _x * Vector2.left * ParentScale);
                }
                _lastNewLine = _letterIndex;
                _x = 0f;
                if (_letter != null)
                {
                    _y += Utility.PixelsToUnit(LinePixelHeight) * TextSize;
                }
                continue;
            }

            if (character.Equals('{'))
            {
                if (!_isWritingToModule)
                {
                    _moduleLetters.Add(new List<Letter>());
                    _isWritingToModule = true;
                }
                continue;
            }
            if (character.Equals('}'))
            {
                if (_isWritingToModule)
                {
                    _isWritingToModule = false;
                }
                continue;
            }

            _letter = References.Prefabs.GetLetter();
            _letters.Add(_letter);
            if (_isWritingToModule)
            {
                _moduleLetters[_moduleLetters.Count - 1].Add(_letter);
            }
            _letterIndex++;
            if (_letter == null) continue;
            _letter.SetParent(Tf);
            _letter.SetLocalPosition(new Vector2(_x, -_y));
            //_letter.SetLocalRotation(Vector3.zero);
            _letter.SetCharacter(character, Alphabet.Font.Dos);
            _letter.SetColor(_fadedColor, Letter.NegativeColorMode.Positive);
            _letter.Fade(FullAlpha, duration);
            _letter.SetSize(TextSize);
            _letter.SetSpriteSortingOrder(SpriteSortingOrder);
            _x += Utility.PixelsToUnit(_letter.GetCharacterWidth() + TextSize);
        }

        // subtract last spacing
        _x -= Utility.PixelToUnit * TextSize;
        
        for (var i = _lastNewLine; i < _letters.Count; i++)
        {
            _letters[i].Move(_shiftToLeft * _x * Vector2.left * ParentScale);
        }

        foreach (var letter in _letters)
        { 
            letter.Move(_shiftToTop * _y * Vector2.up * ParentScale);
        }
    }

    public void ChangeColor(Color color, float duration, float charactersPerSecond, int moduleIndex)
    {
        if (moduleIndex >= 0 && moduleIndex < _moduleLetters.Count)
        {
            StartCoroutine(ChangeTextColorRoutine(_moduleLetters[moduleIndex], color, duration, charactersPerSecond));
            return;
        }
        StartCoroutine(ChangeTextColorRoutine(_letters, color, duration, charactersPerSecond));
    }
    
    private IEnumerator ChangeTextColorRoutine(List<Letter> letters, Color color, float duration, float charactersPerSecond)
    {
        _changeTextColorTimer = 0f;
        _changeTextColorIndex = 0;
        while (_changeTextColorIndex < letters.Count)
        {
            _changeTextColorTimer += Time.deltaTime;
            while (_changeTextColorIndex < _changeTextColorTimer * charactersPerSecond && _changeTextColorIndex < letters.Count)
            {
                letters[_changeTextColorIndex].SetColor(color, Letter.NegativeColorMode.Positive);
                letters[_changeTextColorIndex].Scale(1.5f, 1f, duration);
                _changeTextColorIndex++;
            }
            yield return null;
        }

        yield return null;
    }
}
