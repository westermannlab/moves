using UnityEngine;

public class AssessmentButton : UiElement
{
    private int _isDownHash = Animator.StringToHash("_IsDown");

    public string Caption;

    private AssessmentScreen _parentScreen;
    private Animator _animator;
    private UiCursor _cursor;
    private Label _label;

    private Vector3 _lastMousePosition;
    private bool _isActive;

    protected override void Awake()
    {
        base.Awake();
        _parentScreen = GetComponentInParent<AssessmentScreen>();
        _animator = GetComponentInChildren<Animator>();
        _label = GetComponentInChildren<Label>();
        _label.SetHorizontalAlignment(Label.HorizontalAlignment.Center);
    }
    
    private void OnMouseDown()
    {
        if (!_isActive || Controllers.Input.CurrentState != InputController.State.Assessment) return;
        ChangeButtonState(true);
    }

    private void OnMouseUp()
    {
        if (!_isActive || Controllers.Input.CurrentState != InputController.State.Assessment) return;
        ChangeButtonState(false);
        Press();
    }

    private void OnMouseOver()
    {
        if (!_isActive || !_parentScreen || Controllers.Input.CurrentState != InputController.State.Assessment) return;

        if (Input.mousePosition != _lastMousePosition)
        {
            _parentScreen.ChangeCurrentItemIndex(-1, 3, true);
        }
        _lastMousePosition = Input.mousePosition;
    }

    public void ProcessInput(InputController.Type inputType, InputController.Mode inputMode)
    {
        if (inputType == InputController.Type.Space)
        {
            if (inputMode == InputController.Mode.Press)
            {
                ChangeButtonState(true);
                Press();
            }
            else if (inputMode == InputController.Mode.Release)
            {
                ChangeButtonState(false);
            }
        }
    }

    private void Press()
    {
        _parentScreen.Close(0.25f);
    }
    
    public void Show(float duration)
    {
        ChangeAlpha(1f, duration);
        UpdateLabel(Caption, duration);
        _isActive = true;
    }

    public void Hide(float duration)
    {
        Deselect(duration);
        ChangeAlpha(0f, duration);
        UpdateLabel("", duration);
        _isActive = false;
    }

    private void UpdateLabel(string text, float duration)
    {
        _label.SetText(text, duration);
        _label.Activate();
    }
    
    private void ChangeButtonState(bool isDown)
    {
        _animator.SetBool(_isDownHash, isDown);
        _label.Move(isDown ? Vector2.down * Utility.PixelsToUnit(2f) : Vector2.zero, isDown ? 0.0167f : 0.0333f);
    }
    
    public void Select(float duration)
    {
        if (_cursor != null) _cursor.Return();
        _cursor = References.Prefabs.GetCursor();
        _cursor.SetParent(Tf);
        _cursor.SetLocalPosition(Vector2.zero);
        _cursor.UpdateShape(UiCursor.Shape.CloseButton);
        _cursor.Fade(1f, duration);
    }

    public void Deselect(float duration)
    {
        if (_cursor == null) return;
        _cursor.Fade(0f, duration);
        _cursor.Return(duration + 0.125f);
        _cursor = null;
    }
}
