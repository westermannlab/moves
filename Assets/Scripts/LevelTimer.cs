using System.Collections;
using UnityEngine;

public class LevelTimer : UiElement
{
    private float _countdownTime;
    public Gradient HighlightGradient;
    public Transform Display;
    public Transform Pointer;
    public SpriteRenderer FaceSr;
    
    private Label _label;
    private SpriteRenderer[] _displayRenderers;
    private Vector3 _pointerEulerRotation;
    private float _currentTime;
    private float _lastLabelUpdateTime;
    private string _previousCurrentTimeString;
    private string _currentTimeString;

    private bool _isVisible;
    private bool _isRunning;

    protected override void Awake()
    {
        base.Awake();
        // reset Sr to prevent the frame being faded on deactivation
        Sr = null;
        _label = GetComponentInChildren<Label>();
        _displayRenderers = Display.GetComponentsInChildren<SpriteRenderer>();
        _label.SetHorizontalAlignment(Label.HorizontalAlignment.Center);
        _label.SetVerticalAlignment(Label.VerticalAlignment.Center);
        _label.FullAlpha = 0f;
        _previousCurrentTimeString = "";
        _isVisible = true;
        ReadCountdownTime();
        Deactivate(0f);
        Reset();
        StartCountdown();
    }
    
    private void OnMouseEnter()
    {
        Activate();
        _label.FullAlpha = 1f;
        _label.Activate();
        References.Events.HoverOverTimer();
    }

    private void OnMouseExit()
    {
        Deactivate();
        _label.FullAlpha = 0f;
        _label.Deactivate();
    }

    private void ReadCountdownTime()
    {
        if (References.Io.GetData() == null) _countdownTime = 180f;
        _countdownTime = References.Io.GetData().duration;
    }

    public void StartCountdown()
    {
        _isRunning = true;
        StartCoroutine(CountdownRoutine());
    }

    public void Reset()
    {
        _currentTime = _countdownTime;
    }

    public void Stop()
    {
        _isRunning = false;
    }

    public void Continue()
    {
        _isRunning = true;
    }

    public void SetCountdown(float countdownTime)
    {
        _countdownTime = countdownTime;
        Reset();
        References.Terminal.AddEntry("<grey>Timer set to " + countdownTime.ToString("F1") + ".</>");
    }

    public float GetLevelTime()
    {
        if (_currentTime > 0f)
        {
            return _countdownTime - _currentTime;
        }

        return _countdownTime;
    }

    protected override void ApplyCurrentAlpha()
    {
        base.ApplyCurrentAlpha();
        foreach (var sr in _displayRenderers)
        {
            sr.color = CurrentColor;
        }

    }

    private void UpdatePointer(float expiredPercent)
    {
        _pointerEulerRotation.z = Mathf.Clamp01(expiredPercent) * -360f;
        Pointer.localRotation = Quaternion.Euler(_pointerEulerRotation);
    }

    private void UpdateLabel()
    {
        if (!_isVisible)
        {
            //Show(0.25f);
            _label.SetText(_currentTimeString, 0.5f);
        }
        else
        {
            _label.SetText(_currentTimeString, 0f);
        }
        
        _lastLabelUpdateTime = _currentTime;
    }

    public void Show(float duration)
    {
        if (!_isVisible)
        {
            _isVisible = true;
            Move(Vector2.zero, duration);
        }
    }

    public void Hide(float duration)
    {
        if (_isVisible)
        {
            _isVisible = false;
            Move(Vector2.up * 1.625f, duration);
            _label.SetText("", duration + 0.25f);
        }
    }

    public bool IsUp()
    {
        return _currentTime <= 0f;
    }

    private IEnumerator CountdownRoutine()
    {
        while (_currentTime > 0f)
        {
            _currentTime -= Time.deltaTime;
            UpdatePointer(1f - _currentTime / _countdownTime);

            _currentTimeString = Utility.FormatTime(_currentTime);

            if (_currentTime < 10f && _isVisible)
            {
                FaceSr.color = HighlightGradient.Evaluate((_currentTime / 2f) % 1f);
            }
            
            if (!_previousCurrentTimeString.Equals(_currentTimeString) && _isVisible)
            {
                UpdateLabel();    
                _previousCurrentTimeString = _currentTimeString;
            }

            while (!_isRunning)
            {
                yield return null;
            }

            yield return null;
        }
        
        References.Events.TimerExpired();

        yield return null;
    }
}
