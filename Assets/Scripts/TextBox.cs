using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextBox : PoolObject
{
    public Transform BackgroundTf;
    
    public SpriteRenderer CenterSr;
    
    public SpriteRenderer TopEdgeSr;
    public SpriteRenderer RightEdgeSr;
    public SpriteRenderer BottomEdgeSr;
    public SpriteRenderer LeftEdgeSr;
    
    public SpriteRenderer TopLeftCornerSr;
    public SpriteRenderer TopRightCornerSr;
    public SpriteRenderer BottomRightCornerSr;
    public SpriteRenderer BottomLeftCornerSr;

    public KeyboardButton LeftButton;
    public KeyboardButton CenterButton;
    public KeyboardButton RightButton;

    public float BoxWidth = 12f;
    public float BoxHeight = 3f;
    public float BoxScale = 0.5f;

    private float _originalWidth;
    private float _originalHeight;

    private Label _label;

    private Transform _topEdgeTf;
    private Transform _rightEdgeTf;
    private Transform _bottomEdgeTf;
    private Transform _leftEdgeTf;
    
    private Transform _topLeftCornerTf;
    private Transform _topRightCornerTf;
    private Transform _bottomRightCornerTf;
    private Transform _bottomLeftCornerTf;

    private Vector2 _topBottomEdgeSize;
    private Vector2 _leftRightEdgeSize;

    private string _currentText;

    private float _currentWidth;
    private float _currentHeight;
    private float _widthHolder;
    private float _heightHolder;

    private float _changeSizeTimer;
    private int _changeSizeId;

    private float _maxScale;
    private float _currentScale;
    private float _scaleHolder;
    
    private float _changeScaleTimer;
    private int _changeScaleId;

    private int _setTextId;
    private int _setButtonsId;

    private Color _currentColor;
    private Color _colorHolder;
    private float _changeColorTimer;
    private int _changeColorId;

    protected override void Awake()
    {
        base.Awake();

        _label = GetComponentInChildren<Label>();
        _label.SetHorizontalAlignment(Label.HorizontalAlignment.Center);
        _label.SetVerticalAlignment(Label.VerticalAlignment.Center);

        _topEdgeTf = TopEdgeSr.transform;
        _rightEdgeTf = RightEdgeSr.transform;
        _bottomEdgeTf = BottomEdgeSr.transform;
        _leftEdgeTf = LeftEdgeSr.transform;
        
        _topLeftCornerTf = TopLeftCornerSr.transform;
        _topRightCornerTf = TopRightCornerSr.transform;
        _bottomRightCornerTf = BottomRightCornerSr.transform;
        _bottomLeftCornerTf = BottomLeftCornerSr.transform;
        
        _topBottomEdgeSize = new Vector2(0f, 0.25f);
        _leftRightEdgeSize = new Vector2(0.25f, 0f);
    }

    public override void Init()
    {
        base.Init();
        _originalWidth = BoxWidth;
        _originalHeight = BoxHeight;
        ChangeScale(0f, 0f);
    }

    public override void Reset()
    {
        base.Reset();
        _currentColor = Color.white;
        BoxWidth = _originalWidth;
        BoxHeight = _originalHeight;
        SetScale(1f);
        ApplyCurrentColor();
    }

    public void Show(string text, List<InputController.Type> buttonTypes, float duration, float delay = 0f)
    {
        ChangeSize(BoxWidth, BoxHeight, duration, delay);
        ChangeScale(_maxScale, 0.25f, delay);
        SetText(text, duration / 2f, duration + delay);
        SetButtons(buttonTypes, duration / 2f, duration / 2f + delay);
    }

    public void Hide(float duration, float delay = 0f)
    {
        _setButtonsId = Utility.AddOne(_setButtonsId);
        ChangeSize(0f, 0f, duration, delay);
        ChangeScale(0f, 0.125f, duration + delay);
        SetText("", duration / 4f, delay);
        LeftButton.Deactivate(duration / 2f);
        CenterButton.Deactivate(duration / 2f);
        RightButton.Deactivate(duration / 2f);
        Return(duration + delay + 0.125f);
    }

    public void ChangeTextColor(Color color, float duration, float charactersPerSecond, int moduleIndex = -1, float delay = 0f)
    {
        StartCoroutine(ChangeTextColorRoutine(color, duration, charactersPerSecond, moduleIndex, delay));
    }
    
    public void SetScale(float maxScale)
    {
        _maxScale = maxScale;
        _label.TextSize = maxScale;
        _label.ScaleMaxLineWidth(maxScale);
        LeftButton.SetScale(maxScale);
        CenterButton.SetScale(maxScale);
        RightButton.SetScale(maxScale);
        CenterButton.SetLocalPosition(Vector2.down * 5f);
    }

    private void SetText(string text, float duration, float delay)
    {
        _setTextId = Utility.AddOne(_setTextId);
        StartCoroutine(SetTextRoutine(text, duration, delay, _setTextId));
    }

    private void SetButtons(List<InputController.Type> buttonTypes, float duration, float delay)
    {
        _setButtonsId = Utility.AddOne(_setButtonsId);
        StartCoroutine(SetButtonsRoutine(buttonTypes, duration, delay, _setButtonsId));
    }

    private void ApplyButtonTypes(List<InputController.Type> buttonTypes, float duration)
    {
        if (buttonTypes.Count == 0)
        {
            return;
        }
        if (buttonTypes.Count == 1)
        {
            if (buttonTypes[0] == InputController.Type.Direction) return;
            CenterButton.ChangeButtonType(buttonTypes[0]);
            CenterButton.Activate(duration);
        }
        else
        {
            LeftButton.ChangeButtonType(buttonTypes[0]);
            LeftButton.Activate(duration);
            RightButton.ChangeButtonType(buttonTypes[1]);
            RightButton.Activate(duration);
        }
    }

    private void ChangeSize(float width, float height, float duration, float delay = 0f)
    {
        _changeSizeId = Utility.AddOne(_changeSizeId);
        StartCoroutine(ChangeSizeRoutine(width, height, duration, delay, _changeSizeId));
    }

    private void ChangeScale(float scale, float duration, float delay = 0f)
    {
        _changeScaleId = Utility.AddOne(_changeScaleId);
        StartCoroutine(ChangeScaleRoutine(scale, duration, delay, _changeScaleId));
    }

    public void ChangeBackgroundColor(Color color, float duration, float delay = 0f)
    {
        _changeColorId = Utility.AddOne(_changeColorId);
        StartCoroutine(ChangeBackgroundColorRoutine(color, duration, delay, _changeColorId));
    }

    private void ApplyCurrentSize()
    {
        CenterSr.size = new Vector2(_currentWidth, _currentHeight);
        
        _topBottomEdgeSize.x = _currentWidth;
        _leftRightEdgeSize.y = _currentHeight;
        
        TopEdgeSr.size = _topBottomEdgeSize;
        BottomEdgeSr.size = _topBottomEdgeSize;
        LeftEdgeSr.size = _leftRightEdgeSize;
        RightEdgeSr.size = _leftRightEdgeSize;
        
        _topEdgeTf.localPosition = new Vector2(0f, _currentHeight / 2f);
        _rightEdgeTf.localPosition = new Vector2(_currentWidth / 2f, 0f);
        _bottomEdgeTf.localPosition = new Vector2(0f, -_currentHeight / 2);
        _leftEdgeTf.localPosition = new Vector2(-_currentWidth / 2f, 0f);
        
        _topLeftCornerTf.localPosition = new Vector2(-_currentWidth / 2f, _currentHeight / 2f);
        _topRightCornerTf.localPosition = new Vector2(_currentWidth / 2f, _currentHeight / 2f);
        _bottomRightCornerTf.localPosition = new Vector2(_currentWidth / 2f, -_currentHeight / 2f);
        _bottomLeftCornerTf.localPosition = new Vector2(-_currentWidth / 2f, -_currentHeight / 2f);
        
        LeftButton.SetLocalPosition(new Vector2(-0.5625f, (-_currentHeight * _currentScale) / 2f - Utility.PixelsToUnit(6) * _currentScale));
        CenterButton.SetLocalPosition(new Vector2(0f, (-_currentHeight * _currentScale) / 2f - Utility.PixelsToUnit(6) * _currentScale));
        RightButton.SetLocalPosition(new Vector2(0.5625f, (-_currentHeight * _currentScale) / 2f - Utility.PixelsToUnit(6) * _currentScale));
    }

    private void ApplyCurrentScale()
    {
        BackgroundTf.localScale = Vector3.one * _currentScale;
    }

    private void ApplyCurrentColor()
    {
        CenterSr.color = _currentColor;
        
        TopEdgeSr.color = _currentColor;
        RightEdgeSr.color = _currentColor;
        BottomEdgeSr.color = _currentColor;
        LeftEdgeSr.color = _currentColor;
    
        TopLeftCornerSr.color = _currentColor;
        TopRightCornerSr.color = _currentColor;
        BottomRightCornerSr.color = _currentColor;
        BottomLeftCornerSr.color = _currentColor;
    }

    private IEnumerator ChangeSizeRoutine(float width, float height, float duration, float delay, int id)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        _changeSizeTimer = 0f;
        _widthHolder = _currentWidth;
        _heightHolder = _currentHeight;
        while (_changeSizeTimer < duration && _changeSizeId == id)
        {
            _changeSizeTimer += Time.deltaTime;
            _currentWidth = Mathf.Lerp(_widthHolder, width, _changeSizeTimer / duration);
            _currentHeight = Mathf.Lerp(_heightHolder, height, _changeSizeTimer / duration);
            ApplyCurrentSize();
            yield return null;
        }

        if (_changeSizeId == id)
        {
            _currentWidth = width;
            _currentHeight = height;
            ApplyCurrentSize();
        }
        
        yield return null;
    }

    private IEnumerator ChangeScaleRoutine(float scale, float duration, float delay, int id)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        _changeScaleTimer = 0f;
        _scaleHolder = _currentScale;
        while (_changeScaleTimer < duration && _changeScaleId == id)
        {
            _changeScaleTimer += Time.deltaTime;
            _currentScale = Mathf.Lerp(_scaleHolder, scale, _changeScaleTimer / duration);
            ApplyCurrentScale();
            yield return null;
        }

        if (_changeScaleId == id)
        {
            _currentScale = scale;
            ApplyCurrentScale();
        }

        yield return null;
    }

    private IEnumerator SetTextRoutine(string text, float duration, float delay, int id)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (_setTextId == id)
        {
            _currentText = text;
            _label.SetText(text, duration);
        }
        yield return null;
    }

    private IEnumerator SetButtonsRoutine(List<InputController.Type> buttonTypes, float duration, float delay, int id)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (_setButtonsId == id)
        {
            ApplyButtonTypes(buttonTypes, duration);
        }

        yield return null;
    }

    private IEnumerator ChangeBackgroundColorRoutine(Color targetColor, float duration, float delay, int id)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        _changeColorTimer = 0f;
        _colorHolder = _currentColor;

        while (_changeColorTimer < duration && _changeColorId == id)
        {
            _changeColorTimer += Time.deltaTime;
            _currentColor = Color.Lerp(_colorHolder, targetColor, _changeColorTimer / duration);
            ApplyCurrentColor();
            yield return null;
        }

        yield return null;
    }

    private IEnumerator ChangeTextColorRoutine(Color color, float duration, float charactersPerSecond, int moduleIndex = -1, float delay = 0f)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        
        _label.ChangeColor(color, duration, charactersPerSecond, moduleIndex);
        
        yield return null;
    }
}
