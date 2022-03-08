using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelButton : UiElement
{
    private readonly int _isDownHash = Animator.StringToHash("_IsDown");
    private readonly int _isEnabledHash = Animator.StringToHash("_IsEnabled");
    private readonly int _colorIdHash = Animator.StringToHash("_ColorId");
    
    public LevelData Level;
    public LevelButton TopButton;
    public int ButtonIndex;
    public string Caption;
    public Player Inhabitant;
    public UiElement PlayerColorElement;
    public UiCheckmark Checkmark;

    private MainMenuInput _mainMenuInput;
    private Animator _animator;
    private Animator _playerColorAnimator;
    private Label _label;
    private BoxCollider2D _boxCollider2D;
    private string _labelText;
    // determines visibility:
    private bool _isActive;
    // determines functionality:
    private bool _isEnabled;
    private bool _isMouseOverButton;
    private int _playerIndex;

    public int PlayerIndex => _playerIndex;

    protected override void Awake()
    {
        base.Awake();
        _mainMenuInput = GetComponentInParent<MainMenuInput>();
        _animator = GetComponent<Animator>();
        _label = GetComponentInChildren<Label>();
        _boxCollider2D = GetComponent<BoxCollider2D>();

        _playerColorAnimator = PlayerColorElement.GetComponent<Animator>();
        
        _playerIndex = ButtonIndex > 0 ? ButtonIndex : 6;
    }

    private void Start()
    {
        _label.SetHorizontalAlignment(Label.HorizontalAlignment.Center);
        _label.SetVerticalAlignment(Label.VerticalAlignment.Center);
        
        var data = References.Io.GetData();
        var roomOrderIndex = _playerIndex - 1;
        if (_playerIndex == 6)
        {
            Inhabitant = References.Entities.Players[_playerIndex];
        }
        else if (roomOrderIndex < data.roomOrder.Length && data.roomOrder[roomOrderIndex] < References.Entities.Players.Length)
        {
            Inhabitant = References.Entities.Players[data.roomOrder[roomOrderIndex]];
        }

        _playerColorAnimator.SetInteger(_colorIdHash, Inhabitant.ColorId);

        if (data.roomCaptions != null && ButtonIndex >= 0 && ButtonIndex < data.roomCaptions.Length)
        {
            UpdateLabel(data.roomCaptions[ButtonIndex]);
        }
        else
        {
            UpdateLabel(Caption);
        }
        
        Activate();
        if (IsLevelCompleted())
        {
            Checkmark.Activate(0.125f);
        }

        _isEnabled = References.Io.IsLevelAccessible(ButtonIndex, TopButton != null ? TopButton.ButtonIndex : -1);
        
        _animator.SetBool(_isEnabledHash, _isEnabled);
        if (!_isEnabled)
        {
            ChangeButtonState(true);
        }
    }

    private void OnMouseDown()
    {
        Press();
    }

    private void OnMouseUp()
    {
        Release();
    }

    private void OnMouseOver()
    {
        if (!_isMouseOverButton)
        {
            _isMouseOverButton = true;
            Select();
        }
    }

    private void OnMouseExit()
    {
        _isMouseOverButton = false;
        Deselect();
    }

    private void Select()
    {
        if (!References.Io.HasReadData) return;
        References.Events.SelectLevelButton(Inhabitant, true);
    }

    private void Deselect()
    {
        if (!References.Io.HasReadData) return;
        References.Events.SelectLevelButton(Inhabitant, false);
    }

    private void Press()
    {
        if (!References.Io.HasReadData || _mainMenuInput != null && _mainMenuInput.InputState == MainMenuInput.State.Blocked) return;
        if (!_isEnabled && _mainMenuInput != null)
        {
            var message = IsLevelCompleted()
                ? References.Io.HaveAllLevelsBeenCompleted() ? References.Io.GetData().msgAllRoomsClear 
                : References.Io.GetData().msgRoomAlreadyCompleted
                : References.Io.GetData().msgRoomNotYetAvailable;
            _mainMenuInput.DisplayMessage(message, 2.5f);
        }
        ChangeButtonState(true);
    }

    private void Release()
    {
        if (!References.Io.HasReadData || !_isEnabled || _mainMenuInput != null && _mainMenuInput.InputState == MainMenuInput.State.Blocked)
        {
            return;
        } 
        ChangeButtonState(false);
        References.Entities.PlayerTwo = Inhabitant;
        // provide information about the level to decide whether an assessment is due
        References.Assessment.CurrentLevelData = Level;
        FadeOut();
        LoadLevel(Level);
    }

    private void FadeOut()
    {
        StartCoroutine(FadeOutRoutine(0.125f, 0.5f));
    }

    private void LoadLevel(LevelData data)
    {
        _mainMenuInput.InputState = MainMenuInput.State.Blocked;
        StartCoroutine(LoadLevelRoutine(data.SceneName, 0.5f));
    }
    
    private void UpdateLabel(string label)
    {
        _labelText = label;
        if (label.Length > 16)
        {
            _label.TextSize = 0.75f;
        }
        if (_isActive)
        {
            _label.SetText(label, 0.25f);
        }
    }
    
    private void ChangeButtonState(bool isDown)
    {
        _animator.SetBool(_isDownHash, isDown);
        _label.Move(isDown ? Vector2.down * Utility.PixelsToUnit(4f) : Vector2.zero, isDown ? 0.0167f : 0.0333f);
        PlayerColorElement.Move(isDown ? Vector2.down * Utility.PixelsToUnit(4f) : Vector2.zero, isDown ? 0.0167f : 0.0333f);
        Checkmark.Move(isDown ? Vector2.down * Utility.PixelsToUnit(4f) : Vector2.zero, isDown ? 0.0167f : 0.0333f);
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

    private bool IsLevelCompleted()
    {
        return References.Io.GetAssessmentState(ButtonIndex) > 0;
    }

    private IEnumerator LoadLevelRoutine(string sceneName, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        SceneManager.LoadSceneAsync(sceneName);
        yield return null;
    }

    private IEnumerator FadeOutRoutine(float delay, float duration)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        Controllers.Ui.FadeOut(duration);
        yield return null;
    }
}
