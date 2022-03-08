using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public TaskManager TaskManager;
    public Color FinishedBackgroundColor;
    public Color FinishedTextColor = Color.green;
    public Color FinishedSubTaskTextColor = Color.green;

    public SoundEffect[] SuccessSounds;
    
    private float _textBoxDelay = 1f;

    private Task _currentTask;
    private Task _previousTask;
    private TextBox _currentTextBox;
    private Transform _tf;

    public float TextBoxDelay => _textBoxDelay;

    private void Awake()
    {
        _tf = transform;
    }
    
    private void Start()
    {
        TaskManager.Init();
        References.Entities.PlayerTwo.Hide();
        CompleteCurrentTask();
    }

    private void OnEnable()
    {
        References.Events.OnCreateLog += CompareLog;
        References.Events.OnChangeButtonState += ChangeButtonState;
        References.Events.OnHoverOverTimer += HoverOverTimer;
        References.Events.OnTimerExpired += TutorialCompleted;
    }

    private void OnDisable()
    {
        References.Events.OnCreateLog -= CompareLog;
        References.Events.OnChangeButtonState -= ChangeButtonState;
        References.Events.OnHoverOverTimer -= HoverOverTimer;
        References.Events.OnTimerExpired -= TutorialCompleted;
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.P) && References.Settings.TerminalEnabled)
        {
            CompleteCurrentTask();
        }
    }

    private void CompareLog(Log log)
    {
        if (_currentTask != null)
        {
            _currentTask.CompareLog(log);
            CheckTaskProgress();
        }
    }

    private void ChangeButtonState(int buttonType, bool isDown)
    {
        if (_currentTask != null)
        {
            _currentTask.ChangeButtonState(buttonType, isDown);
            CheckTaskProgress();
        }
    }

    private void HoverOverTimer()
    {
        if (_currentTask != null && _currentTask.TaskType == Task.Type.HoverOverClock)
        {
            _currentTask.CompletionProgress = 1f;
            CheckTaskProgress();
        }
    }

    public void CheckTaskProgress()
    {
        if (_currentTask != null && _currentTask.CompletionProgress >= 1f && !_currentTask.HasBeenClosed)
        {
            CompleteCurrentTask();
        }
    }

    public void CompleteCurrentTask()
    {
        if (_currentTextBox != null)
        {
            if (_currentTask != null && _currentTask.TaskType > 0)
            {
                _currentTextBox.ChangeBackgroundColor(FinishedBackgroundColor, 0.25f);
                _currentTextBox.ChangeTextColor(FinishedTextColor, 0.25f, 1280f);
            }
            _currentTextBox.Hide(0.25f, 2f);
            _currentTextBox = null;
            if (_currentTask != null)
            {
                if (_currentTask.TaskType != Task.Type.Message && SuccessSounds.Length > 0)
                {
                    Controllers.Audio.PlaySoundEffect(SuccessSounds[0]);
                }
                _currentTask.Close();
            }
            ShowNextTask(_textBoxDelay);
            return;
        }
        ShowNextTask(_textBoxDelay);
        _textBoxDelay = 2.25f;
    }

    public void CompleteCurrentSubTask(int index, int soundIndex = 1)
    {
        if (_currentTextBox != null)
        {
            _currentTextBox.ChangeTextColor(FinishedSubTaskTextColor, 0.25f, 1280f, index);
        }

        if (soundIndex >= 0 && soundIndex < SuccessSounds.Length)
        {
            Controllers.Audio.PlaySoundEffect(SuccessSounds[soundIndex]);
        }
    }

    public void ShowNextTask(float delay = 0f)
    {
        _currentTask = TaskManager.GetNextTask();
        if (_currentTask != null)
        {
            _currentTextBox = References.Prefabs.GetTextBox();
            _currentTextBox.SetParent(Controllers.Ui.UiTf);
            _currentTextBox.SetLocalPosition(Controllers.Ui.GetTextBoxPosition(_currentTextBox.BoxWidth * _currentTextBox.BoxScale, _currentTextBox.BoxHeight * _currentTextBox.BoxScale));
            _currentTextBox.Show(_currentTask.GetText(), _currentTask.GetButtonTypeList(), 0.25f, delay);
        }
    }

    private void TutorialCompleted()
    {
        Controllers.Ui.FadeOut(3f);
        Controllers.Level.QuitToMainMenu();
    }
}
