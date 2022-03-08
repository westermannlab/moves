using UnityEngine;

public class LikertScaleItem : UiElement
{
    public AssessmentItem Item;
    public LikertScaleItem TopScale;
    public LikertScaleItem BottomScale;
    public Label Label;
    public int Index;
    public Transform BackgroundTf;

    private AssessmentScreen _parentScreen;
    private LikertScaleLevel[] _levels;
    private LikertScaleLevel _currentLevel;
    private LikertScaleLevel _checkedLevel;

    private SpriteRenderer[] _backgroundRenderers;
    private int _currentLevelIndex;

    public int CurrentLevelIndex => _currentLevelIndex;

    protected override void Awake()
    {
        base.Awake();
        _parentScreen = GetComponentInParent<AssessmentScreen>();
        _backgroundRenderers = BackgroundTf.GetComponentsInChildren<SpriteRenderer>();
        ChangeAlpha(0f, 0f);
    }
    
    private void Start()
    {
        PrepareLabels(0.25f);
    }

    private void PrepareLabels(float duration)
    {
        if (Label != null)
        {
            Label.SetHorizontalAlignment(Label.HorizontalAlignment.Center);
            Label.SetVerticalAlignment(Label.VerticalAlignment.Center);
            Label.SetText(Item.Text, duration);
        }
        _levels = GetComponentsInChildren<LikertScaleLevel>();
        
        for (var i = 0; i < _levels.Length; i++)
        {
            if (Item.LevelCaptions.Length <= i) break;
            _levels[i].UpdateLabel(Item.LevelCaptions[i], duration);
        }
    }
    
    public void ProcessInput(InputController.Type inputType, InputController.Mode inputMode)
    {
        if (inputMode != InputController.Mode.Press) return;
        switch (inputType)
        {
            case InputController.Type.Up:
                if (TopScale == null) break;
                // select top scale
                break;
            case InputController.Type.Right:
                if (_currentLevel == null || _currentLevel.RightLevel == null) break;
                _currentLevelIndex++;
                ChangeCurrentLevel(_currentLevel.RightLevel, 0.125f);
                break;
            case InputController.Type.Down:
                if (BottomScale == null) break;
                // select bottom scale
                break;
            case InputController.Type.Left:
                if (_currentLevel == null || _currentLevel.LeftLevel == null) break;
                _currentLevelIndex--;
                ChangeCurrentLevel(_currentLevel.LeftLevel, 0.125f);
                break;
            case InputController.Type.Space:
                if (_currentLevel == null) break;
                CheckLevel(_currentLevel);
                break;
        }
    }

    public void UpdateCurrentItemIndex(int levelIndex)
    {
        _currentLevelIndex = levelIndex;
        _parentScreen.ChangeCurrentItemIndex(Index, levelIndex, true);
    }
    
    public void ChangeCurrentLevel(int levelIndex, float duration)
    {
        if (levelIndex >= _levels.Length) return;
        _currentLevelIndex = levelIndex;
        ChangeCurrentLevel(_levels[levelIndex], duration);
    }

    public void ChangeCurrentLevel(LikertScaleLevel level, float duration)
    {
        if (_currentLevel == level) return;
        if (_currentLevel != null)
        {
            _currentLevel.Deselect(duration);
        }

        _currentLevel = level;
        
        if (_currentLevel != null)
        {
            _currentLevel.Select(duration);
        }
    }

    public void CheckLevel(LikertScaleLevel level)
    {
        if (level == _checkedLevel) return;
        
        if (_checkedLevel != null)
        {
            _checkedLevel.Uncheck();
        }
        
        _checkedLevel = level;
        _checkedLevel.Check();
        
        Item.Value = _checkedLevel.Value;
    }

    public bool HasCheckedLevel()
    {
        return _checkedLevel != null;
    }

    public void HighlightLevels()
    {
        foreach (var level in _levels)
        {
            level.Highlight();
        }
    }

    protected override void ApplyCurrentAlpha()
    {
        base.ApplyCurrentAlpha();
        foreach (var sr in _backgroundRenderers)
        {
            sr.color = CurrentColor;
        }
    }

    public void Show(float duration)
    {
        PrepareLabels(duration);
        ChangeAlpha(1f, duration);
        foreach (var level in _levels)
        {
            level.Show(duration);
        }
    }

    public void Hide(float duration)
    {
        Label.SetText("", duration);
        ChangeAlpha(0f, duration);
        foreach (var level in _levels)
        {
            level.Hide(duration);
        }
    }
}
