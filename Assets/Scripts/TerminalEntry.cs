using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerminalEntry : PoolObject
{
    public StringFormatter Formatter;
    public Alphabet.Font Font;

    public Color DefaultColor = Color.white;
    public Color DeleteColor = Color.red;
    private Color _currentBackgroundColor = Color.white;
    public float Scale = 1f;
    
    public SpriteRenderer[] TextAreaSrs;
    public Transform InputField;
    public Transform CursorAnchor;
    public Transform LetterHolder;
    
    public float Width = 4f;
    public float Height = 1f;
    [Range(1, 30)]
    public int MinLines = 1;
    [Range(1, 30)]
    public int MaxLines = 3;

    private Vector2 _currentLocalPosition;
    private Vector2 _targetLocalPosition;
    protected Vector2 CurrentCursorPosition;
    protected const int LinePixelHeight = 14;
    protected string Message = "";
    
    protected Letter CursorLetter;
    protected readonly List<List<Letter>> MessageLetters = new List<List<Letter>>();
    protected readonly List<Vector2Int> SpaceIndexList = new List<Vector2Int>();

    protected float CurrentWidth;
    protected float CurrentHeight;
    protected int LineIndex;

    private Vector2 _positionHolder;
    private float _fadeTimer;
    private float _moveTimer;
    private float _currentAlpha;
    private float _alphaHolder;
    private int _fadeId;
    private int _moveId;
    private int _hideId;
    private bool _isActive;

    public float GetCurrentHeight => CurrentHeight;
    
    protected override void Awake()
    {
        base.Awake();
        Fade(0f, 0f);
    }

    public override void Reset()
    {
        base.Reset();
        _currentLocalPosition = Vector2.zero;
        _targetLocalPosition = Vector2.zero;
        Message = "";
    }
    
    public override void SetLocalPosition(Vector2 localPosition)
    {
        base.SetLocalPosition(localPosition);
        CurrentCursorPosition = CursorAnchor.position;
    }

    public void SetMessage(string message)
    {
        Message = message;
    }

    public void SetScale(float scale)
    {
        Scale = scale;
        InputField.localScale = Scale * Vector3.one;
        CursorAnchor.localPosition *= Scale;
    }

    public void InitializeSize(float width, float height)
    {
        Width = width;
        Height = height;
        SetSize(Width, Height);
    }

    public void Show()
    {
        if (!_isActive)
        {
            WriteMessage();
        }
        _hideId = Utility.AddOne(_hideId);
        Fade(0.5f, 0.125f);
        _isActive = true;
    }

    public void Hide(float delay = 0f)
    {
        _hideId = Utility.AddOne(_hideId);
        StartCoroutine(HideRoutine(delay, 0.25f, _hideId));
    }

    protected void WriteMessage()
    {
        CurrentCursorPosition = Vector2.zero;
        
        foreach (var letter in Formatter.GetLetters(Formatter.Format(Message), Font))
        {
            if (letter != null)
            {
                AddLetterToList(letter, LineIndex);
                if (letter.IsSpace() && LineIndex < MessageLetters.Count)
                {
                    SpaceIndexList.Add(new Vector2Int(MessageLetters[LineIndex].Count - 1, LineIndex));
                }

                if (letter.IsNewLine())
                {
                    NewLine(0f, false);
                    letter.SetCharacter(' ');
                }
                else
                {
                    letter.SetParent(LetterHolder);
                    letter.SetLocalPosition(CurrentCursorPosition);
                    letter.SetSize(Scale);
                    letter.SetLayer(Controllers.Camera.UiLayer);
                    letter.SetSpriteSortingOrder(1);
                    CurrentCursorPosition.x += Utility.PixelsToUnit((letter.GetCharacterWidth() + 1f * Scale));
                }
            }
        }
    }

    protected void HideMessage(float fadeDuration)
    {
        LineIndex = 0;
        foreach (var list in MessageLetters)
        {
            foreach (var letter in list)
            {
                letter.Fade(0f, fadeDuration);
                letter.Return(fadeDuration + 0.125f);
            }
            list.Clear();
        }
        MessageLetters.Clear();
        SpaceIndexList.Clear();
        
        CurrentCursorPosition = CursorAnchor.position;
    }

    public void SetSize(float w, float h)
    {
        CurrentWidth = Mathf.Round(w * 32f) / 32f;
        CurrentHeight = Mathf.Round(h * 32f) / 32f;

        var horizontalEdgeSize = new Vector2(CurrentWidth, Utility.PixelsToUnit(8));
        var centerAreaSize = new Vector2(CurrentWidth + Utility.PixelsToUnit(16), CurrentHeight);

        // top left corner
        TextAreaSrs[0].transform.localPosition = new Vector2(0f, centerAreaSize.y);
        // top edge
        TextAreaSrs[1].size = horizontalEdgeSize;
        TextAreaSrs[1].transform.localPosition = new Vector2(0f, centerAreaSize.y);
        // top right edge
        TextAreaSrs[2].transform.localPosition = new Vector2(horizontalEdgeSize.x, centerAreaSize.y);
        // center area
        TextAreaSrs[3].size = centerAreaSize;
        TextAreaSrs[3].transform.localPosition = new Vector2(-Utility.PixelsToUnit(8), 0f);
        // bottom left corner
        
        // bottom edge
        TextAreaSrs[5].size = horizontalEdgeSize;
        // bottom right corner
        TextAreaSrs[6].transform.localPosition = new Vector2(horizontalEdgeSize.x, 0f);

        // anchor point
        CursorAnchor.localPosition = Vector2.up * 0f;

    }
    
    protected virtual void NewLine(float changeSizeDuration = 0.125f, bool writeNewLine = false)
    {
        if (LineIndex + 1 >= MaxLines)
        {
            return;
        }
        CurrentCursorPosition.x = 0f;

        if (CursorLetter != null)
        {
            CursorLetter.SetLocalPosition(CurrentCursorPosition);
        }

        if (writeNewLine)
        {
            Message += '\n';
        }
        
        LineIndex++;
        
        ShiftLines(Vector2.up, changeSizeDuration);
    }

    protected void ShiftLines(Vector2 direction, float duration)
    {
        for (var i = 0; i < LineIndex; i++)
        {
            foreach (var letter in MessageLetters[i])
            {
                letter.MoveTo(letter.GetLocalPosition() + direction * Utility.PixelsToUnit(LinePixelHeight) * Scale, duration);
            }
        }
        //Debug.Log(Message + ", " + Scale);
    }

    protected void AddLetterToList(Letter letter, int lineIndex)
    {
        if (lineIndex >= MessageLetters.Count)
        {
            MessageLetters.Add(new List<Letter>());
            AddLetterToList(letter, lineIndex);
            return;
        }
        MessageLetters[lineIndex].Add(letter);
    }

    protected void Fade(float targetAlpha, float duration)
    {
        _fadeId = Utility.AddOne(_fadeId);
        StartCoroutine(FadeRoutine(targetAlpha, duration, _fadeId));
    }

    private void ApplyAlpha()
    {
        _currentBackgroundColor.a = _currentAlpha;
        foreach (var sr in TextAreaSrs)
        {
            sr.color = _currentBackgroundColor;
        }
    }

    private void ApplyCurrentLocalPosition()
    {
        Tf.localPosition = _currentLocalPosition;
    }

    public void Move(Vector2 direction, float duration)
    {
        _moveId = Utility.AddOne(_moveId);
        _targetLocalPosition += direction;
        StartCoroutine(MoveRoutine(_targetLocalPosition, duration, _moveId));
        /*foreach (var list in MessageLetters)
        {
            foreach (var letter in list)
            {
                letter.MoveTo(letter.GetLocalPosition() + direction, duration);
            }
        }*/
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration, int id)
    {
        _fadeTimer = 0f;
        _alphaHolder = _currentAlpha;
        while (_fadeTimer < duration && _fadeId == id)
        {
            _fadeTimer += Time.unscaledDeltaTime;
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

    private IEnumerator MoveRoutine(Vector2 targetPosition, float duration, int id)
    {
        _moveTimer = 0f;
        _positionHolder = _currentLocalPosition;
        while (_moveTimer < duration && _moveId == id)
        {
            _moveTimer += Time.unscaledDeltaTime;
            _currentLocalPosition = Vector2.Lerp(_positionHolder, targetPosition, _moveTimer / duration);
            ApplyCurrentLocalPosition();
            yield return null;
        }

        if (_moveId == id)
        {
            _currentLocalPosition = targetPosition;
            ApplyCurrentLocalPosition();
        }

        yield return null;
    }

    private IEnumerator HideRoutine(float delay, float fadeDuration, int id)
    {
        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(delay);
        }

        if (_hideId == id)
        {
            _isActive = false;
            HideMessage(fadeDuration);
            Fade(0f, fadeDuration);
        }

        yield return null;
    }
}
