using UnityEngine;

public class AssessmentScreen : UiElement
{
    public string Title;
    public Label TitleLabel;
    public Label InstructionsLabel;
    public AssessmentButton Button;
    public Transform BackgroundTf;

    private LikertScaleItem[] _items;
    private SpriteRenderer[] _backgroundRenderers;
    private LikertScaleItem _currentItem;

    private string _name;

    private int _currentItemIndex;
    private int _currentLevelIndex;
    private int _screenIndex;

    private bool _isFinalScreen;

    private const float TransitionDuration = 0.0333f;

    protected override void Awake()
    {
        base.Awake();
        
        _items = GetComponentsInChildren<LikertScaleItem>();
        if (_items.Length > 0)
        {
            _currentItem = _items[_currentItemIndex];
        }
        _backgroundRenderers = BackgroundTf.GetComponentsInChildren<SpriteRenderer>();
        
        TitleLabel.SetHorizontalAlignment(Label.HorizontalAlignment.Center);
        InstructionsLabel.SetVerticalAlignment(Label.VerticalAlignment.Center);
        ChangeAlpha(0f, 0f);
    }

    public void LoadData(MovesData data, int index)
    {
        _screenIndex = index;
        Title += string.Concat(' ', index + 1, '/', data.scales.Length);
        for (var i = 0; i < data.scales[index].items.Length; i++)
        {
            _items[i].Item.Text = data.scales[index].items[i].GetFormattedQuestion();
            for (var j = 0; j < data.scales[index].items[i].options.Length; j++)
            {
                _items[i].Item.LevelCaptions[j] = data.scales[index].items[i].options[j];
            }
        }
        InstructionsLabel.SetText(data.scales[index].GetFormattedInstructions(), TransitionDuration);
        _name = data.scales[index].name;
    }

    public void Open(float duration, bool isFinalScreen = false)
    {
        Show(duration);
        _isFinalScreen = isFinalScreen;
        if (_items.Length == 0) return;
        _items[0].ChangeCurrentLevel(3, duration);
    }

    public void Close(float duration)
    {
        // verify that every item has a checked level
        var allChecked = true;
        foreach (var item in _items)
        {
            if (!item.HasCheckedLevel())
            {
                item.HighlightLevels();
                allChecked = false;
            }
        }

        if (!allChecked) return;

        if (_isFinalScreen)
        {
            duration = 1f;
        }
        Hide(duration);

        if (_currentItem != null)
        {
            _currentItem.ChangeCurrentLevel(null, TransitionDuration);
        }
        
        References.Assessment.AddScale(new AssessmentScale(_name, GetResults()));
        
        Controllers.Assessment.FinishAssessment(_screenIndex);
    }

    private int[] GetResults()
    {
        var results = new int[_items.Length];
        for (var i = 0; i < results.Length; i++)
        {
            results[i] = _items[i].Item.Value;
        }

        return results;
    }

    public void ProcessInput(InputController.Type inputType, InputController.Mode inputMode)
    {
        switch (inputType)
        {
            case InputController.Type.Up:
                if (_currentItemIndex == -1)
                {
                    _currentItemIndex = _items.Length;
                }
                if (_currentItemIndex <= 0) return;

                ChangeCurrentItemIndex(_currentItemIndex - 1, _currentItem != null ? _currentItem.CurrentLevelIndex : 3, false);
                break;
                
            case InputController.Type.Down:
                if (_currentItemIndex >= _items.Length) return;
                
                ChangeCurrentItemIndex(_currentItemIndex + 1, _currentItem.CurrentLevelIndex, false);
                break;
        }
        if (_currentItem == null)
        {
            Button.ProcessInput(inputType, inputMode);
            return;
        }
        _currentItem.ProcessInput(inputType, inputMode);
    }

    public void ChangeCurrentItemIndex(int itemIndex, int levelIndex, bool isUsingMouse)
    {
        if (isUsingMouse)
        {
            if (_currentItemIndex == -1)
            {
                Button.Deselect(TransitionDuration);
            }
        }
        else
        {
            if (_currentItemIndex == _items.Length)
            {
                Button.Deselect(TransitionDuration);
                _currentLevelIndex = 3;
            }
        }
        
        _currentItemIndex = itemIndex;

        if (isUsingMouse)
        {
            if (_currentItemIndex == -1)
            {
                Button.Select(TransitionDuration);
            }
        }
        else
        {
            if (_currentItemIndex == _items.Length)
            {
                if (_currentItem != null)
                {
                    _currentItem.ChangeCurrentLevel(null, TransitionDuration);
                }
                _currentItem = null;
                Button.Select(TransitionDuration);
            }
        }

        if (_currentItem != null)
        {
            _currentLevelIndex = isUsingMouse ? levelIndex : _currentItem.CurrentLevelIndex;
            _currentItem.ChangeCurrentLevel(null, TransitionDuration);
        }

        if (_currentItemIndex >= 0 && _currentItemIndex < _items.Length)
        {
            _currentItem = _items[_currentItemIndex];
            _currentItem.ChangeCurrentLevel(levelIndex, TransitionDuration);
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

    private void Show(float duration)
    {
        TitleLabel.SetText(Title, 0.25f);
        ChangeAlpha(1f, duration);
        foreach (var item in _items)
        {
            item.Show(duration);
        }
        Button.Show(duration);
    }

    private void Hide(float duration)
    {
        TitleLabel.SetText("", duration);
        InstructionsLabel.SetText("", duration);
        ChangeAlpha(0f, duration);
        foreach (var item in _items)
        {
            item.Hide(duration);
        }
        Button.Hide(duration);
        Move(Vector2.up * 10f, 0.5f, duration);
    }
}
