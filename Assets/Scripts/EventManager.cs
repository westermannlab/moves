using UnityEngine;

public class EventManager : ScriptableObject
{
    public event System.Action<Player> OnPlayerStateChange;
    public event System.Action<bool> OnEarthquakeStateChange;
    public event System.Action<Vector2, float> OnGroundShake;
    public event System.Action OnBeginRotateGround;
    public event System.Action<float> OnChangeGroundAngle;
    public event System.Action<Vector2, float> OnApplyGravitationalForce;
    public event System.Action<int, bool> OnChangeButtonState;
    public event System.Action OnChangeKeyBindings;
    public event System.Action<int, string> OnChangeKeyFunction;
    public event System.Action<int> OnChangeMenuState;
    public event System.Action<int> OnCollectNote;
    public event System.Action<bool, float> OnChangeNoteVisibility;
    public event System.Action OnHoverOverTimer;
    public event System.Action OnTimerExpired;
    public event System.Action<Log> OnCreateLog;
    public event System.Action OnAssessmentCompleted;
    public event System.Action OnTaskCompleted;
    public event System.Action<Player, bool> OnSelectLevelButton;
    public event System.Action<ControllableObject.Type, bool> OnActivateControllableObject;
    public event System.Action<int> OnRainHitsNote;

    public void ChangePlayerState(Player player)
    {
        OnPlayerStateChange?.Invoke(player);
    }
    
    public void ChangeEarthquakeState(bool isEarthquakeActive)
    {
        OnEarthquakeStateChange?.Invoke(isEarthquakeActive);
    }

    public void ApplyGroundShakeForce(Vector2 force, float amount)
    {
        OnGroundShake?.Invoke(force, amount);
    }

    public void BeginRotateGround()
    {
        OnBeginRotateGround?.Invoke();
    }

    public void ChangeGroundAngle(float eulerAngle)
    {
        OnChangeGroundAngle?.Invoke(eulerAngle);
    }

    public void ApplyGravitationalForce(Vector2 force, float amount)
    {
        OnApplyGravitationalForce?.Invoke(force, amount);
    }

    public void ChangeButtonState(int buttonType, bool isDown)
    {
        OnChangeButtonState?.Invoke(buttonType, isDown);
    }

    public void ChangeKeyBindings()
    {
        OnChangeKeyBindings?.Invoke();
    }

    public void ChangeKeyFunction(int buttonType, string label)
    {
        OnChangeKeyFunction?.Invoke(buttonType, label);
    }

    public void ChangeMenuState(int state)
    {
        // 0: off, 1: on
        OnChangeMenuState?.Invoke(state);
    }

    public void CollectNote(int playerId)
    {
        OnCollectNote?.Invoke(playerId);
    }

    public void ChangeNoteVisibility(bool areVisible, float duration)
    {
        OnChangeNoteVisibility?.Invoke(areVisible, duration);
    }

    public void CreateLog(Log log)
    {
        OnCreateLog?.Invoke(log);
    }

    public void HoverOverTimer()
    {
        OnHoverOverTimer?.Invoke();
    }

    public void TimerExpired()
    {
        OnTimerExpired?.Invoke();
    }

    public void AssessmentCompleted()
    {
        OnAssessmentCompleted?.Invoke();
    }

    public void TaskCompleted()
    {
        OnTaskCompleted?.Invoke();
    }

    public void SelectLevelButton(Player inhabitant, bool isSelected)
    {
        OnSelectLevelButton?.Invoke(inhabitant, isSelected);
    }

    public void ActivateControllableObject(ControllableObject.Type type, bool isActive)
    {
        OnActivateControllableObject?.Invoke(type, isActive);
    }

    public void RainHitsNote(int playerId)
    {
        OnRainHitsNote?.Invoke(playerId);
    }
}
