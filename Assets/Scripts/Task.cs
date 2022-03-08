using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Task : ScriptableObject
{
    public string Identifier;
    [TextArea(2, 3)]
    public string Description;

    public Type TaskType;
    public ControllableObject.Type TaskTarget;

    [TextArea(2, 3)]
    public string FullText;
    public string Preface;
    public string TextModule;
    public string Conclusion;
    public InputController.Type FirstButtonType;
    public InputController.Type SecondButtonType;
    public enum Type { Message, Move, Rotate, Animate, Disembody, PerformAction, CollectNote, PressTab, HoverOverClock }

    public float Amount;

    public string ActivationCommand;
    public string CompletionCommand;
    
    public float CompletionProgress;
    
    private readonly List<InputController.Type> _buttonTypeList = new List<InputController.Type>();

    private float _firstSubProgress;
    private float _secondSubProgress;

    private float _previousFirstSubProgress;
    private float _previousSecondSubProgress;

    private Vector2 _playerPosition;
    private float _groundAngle;

    private bool _hasBeenClosed;

    public bool HasBeenClosed => _hasBeenClosed;

    public void LoadData(TaskData data)
    {
        TaskType = (Type) data.type;
        TaskTarget = (ControllableObject.Type) data.target;
        FullText = data.text;
        Amount = data.amount;
        FirstButtonType = (InputController.Type)data.firstButton;
        SecondButtonType = (InputController.Type)data.secondButton;
        ActivationCommand = data.activationCommands;
        CompletionCommand = data.completionCommands;
    }

    public string GetText()
    {
        return FullText;
    }

    public void CompareLog(Log log)
    {
        if (log.Agent != References.Entities.PlayerOne && TaskType != Type.CollectNote)
        {
            // ignore logs produced by other player except those generated by collecting notes
            return;
        }
        
        switch (TaskType)
        {
            case Type.Move:

                if (TaskTarget != ControllableObject.Type.All && TaskTarget != References.Entities.PlayerOne.CurrentType)
                {
                    break;
                }

                if (_buttonTypeList.Count == 1)
                {
                    CompletionProgress = Mathf.Clamp01(CompletionProgress + GetMoveProgressForButton(_buttonTypeList[0], log.PlayerPosition - _playerPosition));
                }
                else if (_buttonTypeList.Count == 2)
                {
                    _firstSubProgress = Mathf.Clamp01(_firstSubProgress + GetMoveProgressForButton(_buttonTypeList[0], log.PlayerPosition - _playerPosition));
                    _secondSubProgress = Mathf.Clamp01(_secondSubProgress + GetMoveProgressForButton(_buttonTypeList[1], log.PlayerPosition - _playerPosition));
                    CompletionProgress = (_firstSubProgress + _secondSubProgress) / 2f;

                    if (CompletionProgress < 1f)
                    {
                        if (_firstSubProgress >= 1f && _previousFirstSubProgress < 1f)
                        {
                            References.Tutorial.CompleteCurrentSubTask(0);
                        }
                        if (_secondSubProgress >= 1f && _previousSecondSubProgress < 1f)
                        {
                            References.Tutorial.CompleteCurrentSubTask(1);    
                        }
                    }

                    _previousFirstSubProgress = _firstSubProgress;
                    _previousSecondSubProgress = _secondSubProgress;
                }
                
                _playerPosition = log.PlayerPosition;
                
                break;
            case Type.Rotate:

                if (log.ActionType != Log.Action.RotateTo)
                {
                    break;
                }
                
                if (_buttonTypeList.Count == 1)
                {
                    CompletionProgress = Mathf.Clamp01(CompletionProgress + GetRotateProgressForButton(_buttonTypeList[0], log.GetTargetAsFloat() - _groundAngle));
                }
                else if (_buttonTypeList.Count == 2)
                {
                    _firstSubProgress = Mathf.Clamp01(_firstSubProgress + GetRotateProgressForButton(_buttonTypeList[0], log.GetTargetAsFloat() - _groundAngle));
                    _secondSubProgress = Mathf.Clamp01(_secondSubProgress + GetRotateProgressForButton(_buttonTypeList[1], log.GetTargetAsFloat() - _groundAngle));
                    CompletionProgress = (_firstSubProgress + _secondSubProgress) / 2f;

                    if (CompletionProgress < 1f)
                    {
                        if (_firstSubProgress >= 1f && _previousFirstSubProgress < 1f)
                        {
                            References.Tutorial.CompleteCurrentSubTask(0);
                        }
                        if (_secondSubProgress >= 1f && _previousSecondSubProgress < 1f)
                        {
                            References.Tutorial.CompleteCurrentSubTask(1);    
                        }
                    }

                    _previousFirstSubProgress = _firstSubProgress;
                    _previousSecondSubProgress = _secondSubProgress;
                }

                _groundAngle = log.GetTargetAsFloat();
                
                break;
            case Type.Animate:
                if (log.ActionType == Log.Action.Animate)
                {
                    if (References.Entities.PlayerOne.CurrentType == TaskTarget)
                    {
                        SetAsCompleted();
                    }
                }
                break;
            case Type.Disembody:
                if (log.ActionType == Log.Action.Disembody)
                {
                    if (References.Entities.PlayerOne.CurrentType != TaskTarget)
                    {
                        SetAsCompleted();
                    }
                }
                break;
            case Type.PerformAction:
                switch (TaskTarget)
                {
                    case ControllableObject.Type.Cloud:
                        if (log.ActionType == Log.Action.Rain || log.ActionType == Log.Action.RainIndifferent || 
                            log.ActionType == Log.Action.RainUndefined)
                        {
                            CompletionProgress = Mathf.Clamp01(CompletionProgress + Mathf.Abs(log.Duration / Mathf.Max(Amount, 0.01f)));
                        }
                        break;
                    case ControllableObject.Type.Ground:
                        if (log.ActionType == Log.Action.Shake || log.ActionType == Log.Action.ShakeBraked || 
                            log.ActionType == Log.Action.ShakeMoving || log.ActionType == Log.Action.ShakeUndefined)
                        {
                            CompletionProgress = Mathf.Clamp01(CompletionProgress + Mathf.Abs(log.Duration / Mathf.Max(Amount, 0.01f)));
                        }
                        break;
                    case ControllableObject.Type.Handcar:
                        if (log.ActionType == Log.Action.Boost || log.ActionType == Log.Action.BoostRaining || 
                            log.ActionType == Log.Action.BoostShaking || log.ActionType == Log.Action.BoostUndefined ||
                            log.ActionType == Log.Action.BoostUpSlope)
                        {
                            CompletionProgress = Mathf.Clamp01(CompletionProgress + Mathf.Abs(log.Duration / Mathf.Max(Amount, 0.01f)));
                        }
                        break;
                    case ControllableObject.Type.Cart:
                        if (log.ActionType == Log.Action.Brake || log.ActionType == Log.Action.BrakeShaking || 
                            log.ActionType == Log.Action.BrakeJump || log.ActionType == Log.Action.BrakeUndefined || log.ActionType == Log.Action.BrakeNotMoving)
                        {
                            CompletionProgress = Mathf.Clamp01(CompletionProgress + Mathf.Abs(log.Duration / Mathf.Max(Amount, 0.01f)));
                        }
                        break;
                }
                break;
            case Type.CollectNote:
                if (log.ActionType != Log.Action.TouchNote) break;
                switch (TaskTarget)
                {
                    case ControllableObject.Type.Handcar:
                        if (References.Entities.PlayerOne.CurrentType == ControllableObject.Type.Handcar)
                        {
                            References.Tutorial.CompleteCurrentSubTask(Mathf.RoundToInt(CompletionProgress * Amount), 2);
                            CompletionProgress += 1f / Mathf.Max(Amount, 1f);
                        }
                        break;
                    case ControllableObject.Type.Cart:
                        if (References.Entities.PlayerOne.CurrentType == ControllableObject.Type.Cart)
                        {
                            References.Tutorial.CompleteCurrentSubTask(Mathf.FloorToInt(CompletionProgress * Amount), 2);
                            CompletionProgress += 1f / Mathf.Max(Amount, 1f);
                        }
                        break;
                    case ControllableObject.Type.Ground:
                        if (References.Entities.PlayerOne.CurrentType == ControllableObject.Type.Ground)
                        {
                            References.Tutorial.CompleteCurrentSubTask(Mathf.FloorToInt(CompletionProgress * Amount), 2);
                            CompletionProgress += 1f / Mathf.Max(Amount, 1f);
                        }
                        break;
                }

                break;
        }
    }

    public void ChangeButtonState(int buttonType, bool isDown)
    {
        switch ((InputController.Type)buttonType)
        {
            case InputController.Type.Show:
                if (TaskType == Type.PressTab && isDown)
                {
                    SetAsCompleted();
                }
                break;
        }
    }

    protected void SetAsCompleted()
    {
        CompletionProgress = 1f;
    }

    private float GetMoveProgressForButton(InputController.Type buttonType, Vector2 delta)
    {
        switch (buttonType)
        {
            case InputController.Type.Left:
                if (delta.x < 0f)
                {
                    return Mathf.Abs(delta.x) / Mathf.Max(Amount, 0.01f);
                }
                break;
            case InputController.Type.Up:
                if (delta.y > 0f)
                {
                    return delta.y / Mathf.Max(Amount, 0.01f);
                }
                break;
            case InputController.Type.Right:
                if (delta.x > 0f)
                {
                    return delta.x / Mathf.Max(Amount, 0.01f);
                }
                break;
            case InputController.Type.Down:
                if (delta.y < 0f)
                {
                    return Mathf.Abs(delta.y) / Mathf.Max(Amount, 0.01f);
                }
                break;
        }

        return 0f;
    }
    
    private float GetRotateProgressForButton(InputController.Type buttonType, float angleDelta)
    {
        switch (buttonType)
        {
            case InputController.Type.Left:
                if (angleDelta > 0f)
                {
                    return Mathf.Abs(angleDelta) / Mathf.Max(Amount, 0.01f);
                }
                break;

            case InputController.Type.Right:
                if (angleDelta < 0f)
                {
                    return Mathf.Abs(angleDelta) / Mathf.Max(Amount, 0.01f);
                }
                break;
        }

        return 0f;
    }

    private void CompleteAfterTime(float delay)
    {
        References.Coroutines.StartCoroutine(CompleteAfterTimeRoutine(delay));
    }
    
    public List<InputController.Type> GetButtonTypeList()
    {
        return _buttonTypeList;
    }

    public void Open()
    {
        CompletionProgress = 0f;
        _firstSubProgress = 0f;
        _secondSubProgress = 0f;
        _previousFirstSubProgress = 0f;
        _previousSecondSubProgress = 0f;

        _groundAngle = References.Entities.Ground.CurrentEulerAngle;

        _buttonTypeList.Clear();
        if (FirstButtonType != InputController.Type.Enter && FirstButtonType != InputController.Type.Record &&
            FirstButtonType != InputController.Type.Direction)
        {
            _buttonTypeList.Add(FirstButtonType);
        }
        if (SecondButtonType != InputController.Type.Enter && SecondButtonType != InputController.Type.Record &&
            SecondButtonType != InputController.Type.Direction)
        {
            _buttonTypeList.Add(SecondButtonType);
        }
        
        if (ActivationCommand.Length > 0)
        {
            foreach (var command in ActivationCommand.Split('\n'))
            {
                References.Terminal.ProcessCommand(command);
            }
        }

        if (TaskType == Type.Message)
        {
            CompleteAfterTime(Amount + References.Tutorial.TextBoxDelay);
        }

        _playerPosition = References.Entities.PlayerOne.GetCurrentPosition();
        _hasBeenClosed = false;
    }

    public void Close()
    {
        if (!_hasBeenClosed && CompletionCommand.Length > 0)
        {
            _hasBeenClosed = true;
            foreach (var command in CompletionCommand.Split('\n'))
            {
                References.Terminal.ProcessCommand(command);
            }
        }

        _hasBeenClosed = true;
        CheckIfAlreadyCompleted();
    }

    private void CheckIfAlreadyCompleted()
    {
        switch (TaskType)
        {
            case Type.Animate:
                if (References.Entities.PlayerOne.CurrentType == TaskTarget)
                {
                    SetAsCompleted();
                }

                break;
            case Type.Disembody:
                if (References.Entities.PlayerOne.CurrentType != TaskTarget)
                {
                    SetAsCompleted();
                }

                break;
        }
    }

    private IEnumerator CompleteAfterTimeRoutine(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
    
        SetAsCompleted();
        References.Tutorial.CheckTaskProgress();
        
        yield return null;
    }
}
