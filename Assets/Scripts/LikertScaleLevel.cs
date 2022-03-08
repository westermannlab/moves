using UnityEngine;

public class LikertScaleLevel : UiElement
{
    private int _isCheckedHash = Animator.StringToHash("_IsChecked");
    private int _highlightHash = Animator.StringToHash("_Highlight");
    
    public LikertScaleLevel LeftLevel;
    public LikertScaleLevel RightLevel;
    public int Index;
    public int Value;
    public Label.HorizontalAlignment Alignment = Label.HorizontalAlignment.Center;
    public SpriteRenderer[] Renderers;

    private Animator _animator;
    private LikertScaleItem _parentItem;
    private Label _label;
    private Vector3 _lastMousePosition;

    private UiCursor _cursor;
    private bool _isActive;

    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponentInChildren<Animator>();
        _parentItem = GetComponentInParent<LikertScaleItem>();
        _label = GetComponentInChildren<Label>();
        _label.SetHorizontalAlignment(Alignment);
        ChangeAlpha(0f, 0f);
    }

    private void OnMouseDown()
    {
        if (!_isActive || !_parentItem || Controllers.Input.CurrentState != InputController.State.Assessment) return;
        _parentItem.CheckLevel(this);
    }

    private void OnMouseOver()
    {
        if (!_isActive || !_parentItem || Controllers.Input.CurrentState != InputController.State.Assessment) return;

        if (Input.mousePosition != _lastMousePosition)
        {
            //_parentItem.ChangeCurrentLevel(Index, 0.125f);
            _parentItem.UpdateCurrentItemIndex(Index);
        }
        _lastMousePosition = Input.mousePosition;
        
    }

    public void UpdateLabel(string text, float duration)
    {
        _label.SetText(text, duration);
    }

    public void Select(float duration)
    {
        if (_cursor != null) _cursor.Return();
        _cursor = References.Prefabs.GetCursor();
        _cursor.SetParent(Tf);
        _cursor.SetLocalPosition(Vector2.zero);
        _cursor.UpdateShape(UiCursor.Shape.AssessmentLevel);
        _cursor.Fade(1f, duration);
    }

    public void Deselect(float duration)
    {
        if (_cursor == null) return;
        _cursor.Fade(0f, duration);
        _cursor.Return(duration + 0.125f);
        _cursor = null;
    }

    public void Check()
    {
        _animator.SetBool(_isCheckedHash, true);
    }

    public void Uncheck()
    {
        _animator.SetBool(_isCheckedHash, false);
    }

    public void Highlight()
    {
        _animator.SetTrigger(_highlightHash);
    }

    protected override void ApplyCurrentAlpha()
    {
        base.ApplyCurrentAlpha();
        foreach (var sr in Renderers)
        {
            sr.color = CurrentColor;
        }
    }

    public void Show(float duration)
    {
        ChangeAlpha(1f, duration);
        _isActive = true;
    }

    public void Hide(float duration)
    {
        ChangeAlpha(0f, duration);
        _label.SetText("", duration);
        _isActive = false;
    }
}
