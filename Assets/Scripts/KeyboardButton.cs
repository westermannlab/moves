using System.Collections;
using UnityEngine;

public class KeyboardButton : UiElement
{
    private readonly int _buttonIdHash = Animator.StringToHash("_ButtonId");
    private readonly int _isDownHash = Animator.StringToHash("_IsDown");
    
    public InputController.Type ButtonType;
    // 0: top, 1: right, 2: bottom, 3: left
    public ControllableObject.Type CorrespondingType;
    public int LabelPosition;
    public float ShowDuration = 5f;
    public ControllableObject ParentObject;
    public Transform ModelTf;
    public bool ShowLabels = true;

    private Animator _animator;
    private Label _label;
    private BoxCollider2D _boxCollider2D;
    private string _labelText;
    private bool _isMouseOverButton;
    private bool _isMouseDown;
    private bool _isActive;

    private float _hoverTimer;
    private int _showId;
    private bool _isHovering;

    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponentInChildren<Animator>();
        _label = GetComponentInChildren<Label>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        if (ParentObject == null)
        {
            ParentObject = GetComponentInParent<ControllableObject>();   
        }
        _labelText = "";
    }

    private void Start()
    {
        UpdateKeyBindings();
        UpdateLabelPosition();
        //ChangeVisibility();
        Deactivate(0f);
    }

    private void OnEnable()
    {
        References.Events.OnChangeButtonState += ChangeButtonState;
        References.Events.OnChangeKeyBindings += UpdateKeyBindings;
        References.Events.OnChangeKeyFunction += UpdateLabel;
        References.Events.OnPlayerStateChange += ChangePlayerState;
    }

    private void OnDisable()
    {
        References.Events.OnChangeButtonState -= ChangeButtonState;
        References.Events.OnChangeKeyBindings -= UpdateKeyBindings;
        References.Events.OnChangeKeyFunction -= UpdateLabel;
        References.Events.OnPlayerStateChange -= ChangePlayerState;
    }

    private void OnMouseDown()
    {
        _isMouseDown = true;
        Controllers.Input.ProcessInput(ButtonType, InputController.Mode.Press);
        ChangeButtonState((int)ButtonType, true);
    }

    private void OnMouseDrag()
    {
        if (_isMouseOverButton)
        {
            Controllers.Input.ProcessInput(ButtonType, InputController.Mode.Hold);
        }
    }

    private void OnMouseUp()
    {
        _isMouseDown = false;
        Controllers.Input.ProcessInput(ButtonType, InputController.Mode.Release);
        ChangeButtonState((int)ButtonType, false);
    }

    private void OnMouseEnter()
    {
        _isMouseOverButton = true;
    }

    private void OnMouseExit()
    {
        _isMouseOverButton = false;
        if (_isMouseDown)
        {
            OnMouseUp();
        }
    }

    private Player GetParentPlayer()
    {
        if (ParentObject == null)
        {
            return null;
        }

        return ParentObject.CurrentPlayer;
    }

    private void UpdateKeyBindings()
    {
        _animator.SetInteger(_buttonIdHash, (int)ButtonType + ((int) ButtonType < 4 && Controllers.Input.KeyBindings == InputController.KeyBindingType.Arrows ? 12 : 0));
    }

    private void UpdateLabelPosition()
    {
        switch (LabelPosition)
        {
            case 0:
                _label.SetLocalPosition(0.75f * Vector2.up);
                _label.SetHorizontalAlignment(Label.HorizontalAlignment.Center);
                _label.SetVerticalAlignment(Label.VerticalAlignment.Bottom);
                break;
            case 1:
                _label.SetLocalPosition(0.625f * Vector2.right);
                _label.SetHorizontalAlignment(Label.HorizontalAlignment.Left);
                _label.SetVerticalAlignment(Label.VerticalAlignment.Center);
                break;
            case 2:
                _label.SetLocalPosition(0.875f * Vector2.down);
                _label.SetHorizontalAlignment(Label.HorizontalAlignment.Center);
                _label.SetVerticalAlignment(Label.VerticalAlignment.Top);
                break;
            case 3:
                _label.SetLocalPosition(0.625f * Vector2.left);
                _label.SetHorizontalAlignment(Label.HorizontalAlignment.Right);
                _label.SetVerticalAlignment(Label.VerticalAlignment.Center);
                break;
            default:
                _label.SetLocalPosition(Vector2.zero);
                _label.SetHorizontalAlignment(Label.HorizontalAlignment.Center);
                _label.SetVerticalAlignment(Label.VerticalAlignment.Center);
                break;
        }
    }

    private void UpdateLabel(int buttonType, string label)
    {
        if (!ShowLabels) return;
        if (buttonType == (int) ButtonType)
        {
            _labelText = label;
            if (_isActive)
            {
                _label.SetText(label, 0.25f);
            }
        }
    }

    private void ChangeButtonState(int buttonType, bool isDown)
    {
        if (buttonType == (int) ButtonType)
        {
            if (isDown && Controllers.Input.ControllablePlayer != null && Controllers.Input.ControllablePlayer.CurrentType != CorrespondingType && CorrespondingType != ControllableObject.Type.All)
            {
                return;
            }
            _animator.SetBool(_isDownHash, isDown);
            _label.Move(isDown ? Vector2.down * Utility.PixelsToUnit(3f) : Vector2.zero, isDown ? 0.0167f : 0.0333f);
        }
        else if (ParentObject != null && ParentObject.CurrentPlayer == Controllers.Input.ControllablePlayer &&
                 (InputController.Type) buttonType == InputController.Type.Show)
        {
            if (isDown)
            {
                Activate(0.25f);
            }
            else
            {
                Deactivate(0.25f);
            }
        }
    }

    public void ChangeButtonType(InputController.Type buttonType)
    {
        ButtonType = buttonType;
        UpdateKeyBindings();
    }

    private void ChangePlayerState(Player player)
    {
        if (_isActive && CorrespondingType != ControllableObject.Type.All && player.CurrentType != CorrespondingType)
        {
            Deactivate();
        }
    }
    
    private void ChangeVisibility(int menuState)
    {
        if (CorrespondingType != ControllableObject.Type.None &&
            GetParentPlayer() != Controllers.Input.ControllablePlayer)
        {
            return;
        }
        
        switch (menuState)
        {
            case 0:
                Deactivate();
                break;
            case 1:
                Activate();
                break;
        }
    }

    private void ChangeVisibility()
    {
        if (GetParentPlayer() == Controllers.Input.ControllablePlayer && Controllers.Ui.ShowHotkeys)
        {
            Activate(0.5f);
            _showId = Utility.AddOne(_showId);
            StartCoroutine(ShowRoutine(_showId));
        }
        else
        {
            Deactivate(0.5f);
        }
    }
    
    public override void Activate(float duration = 0.125f)
    {
        base.Activate(duration);
        _isActive = true;
        _label.SetText(_labelText, 0.25f);
        _boxCollider2D.enabled = true;
    }

    public override void Deactivate(float duration = 0.125f)
    {
        base.Deactivate(duration);
        _isActive = false;
        _label.SetText("", 0.25f);
        _boxCollider2D.enabled = false;
    }

    private IEnumerator ShowRoutine(int id)
    {
        if (ShowDuration > 0f)
        {
            yield return new WaitForSeconds(ShowDuration);
            if (_showId == id)
            {
                Deactivate(1f);
            }
        }
        
        yield return null;
    }

    private IEnumerator HoverRoutine()
    {
        _hoverTimer = 0f;
        var distance = 0f;
        var angle = 0f;
        
        while (_isHovering)
        {
            _hoverTimer += Time.deltaTime;
            distance = Mathf.Sin(_hoverTimer) * 0.25f;
            angle = (Mathf.Sin(_hoverTimer) * 0.5f + 0.5f) * 360f;
            ModelTf.localPosition = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.up * distance;
            yield return null;
        }

        yield return null;
    }
}
