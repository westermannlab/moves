using System.Collections;
using UnityEngine;

public class Action : ScriptableObject
{
    public float MaxActionDuration = 1f;
    public float Cooldown = 2f;
    
    protected ControllableObject ParentObject;
    private ResourceCircle _resourceDisplayHolder;
    private ResourceCircle _currentResourceDisplay;
    protected bool IsActive;
    protected int ActionId;
    protected float ActionTimer;
    protected float BeginTime;
    private float _resourceTimer;
    protected bool ContextFlag;

    public event System.Action OnPerform;
    public event System.Action OnStop;
    public event System.Action OnCancel;
    

    public virtual void Init()
    {
        IsActive = false;
        ActionId = 0;
        ActionTimer = 0f;
        _resourceTimer = 0f;
        _currentResourceDisplay = ParentObject.ResourceDisplay;
    }
    
    public void Link(ControllableObject parent)
    {
        ParentObject = parent;
    }
    
    public virtual bool TryToPerform()
    {
        if (CanPerformAction())
        {
            Perform();
            return true;
        }

        Exhaustion();
        return false;
    }

    protected virtual void Perform()
    {
        IsActive = true;
        ActionId = Controllers.Logs.RequestActionId();
        ActionTimer = 0f;
        ContextFlag = false;
        BeginTime = Time.timeSinceLevelLoad;
        OnPerform?.Invoke();
        References.Coroutines.StartCoroutine(SpendResourcesRoutine(ActionId));
    }

    protected virtual void Exhaustion()
    {
        CreateLog(GetAction(), Log.Ending.Unable);
    }

    public virtual void Stop()
    {
        IsActive = false;
        OnStop?.Invoke();
        CreateLog(GetAction(), Log.Ending.Natural);
    }

    private float GetAvailableResourceAmount()
    {
        return MaxActionDuration - _resourceTimer;
    }

    public bool CanPerformAction()
    {
        return GetAvailableResourceAmount() > 0f;
    }

    public bool IsPerformingAction()
    {
        return IsActive;
    }

    public float GetRandomDuration(int rollCount = 1, bool useBiggestValue = true)
    {
        var duration = useBiggestValue ? 0f : MaxActionDuration;
        for (var i = 0; i < rollCount; i++)
        {
            var randomValue = Random.value * MaxActionDuration;
            if (useBiggestValue && randomValue > duration || !useBiggestValue && randomValue < duration)
            {
                duration = randomValue;
            }
        }

        return duration;
    }

    protected virtual void Cancel()
    {
        IsActive = false;
        OnCancel?.Invoke();
        CreateLog(GetAction(), Log.Ending.Exhaustion);
    }

    protected virtual Log.Action GetAction()
    {
        return Log.Action.Undefined;
    }

    protected void CreateLog(Log.Action action, Log.Ending ending)
    {
        Controllers.Logs.AddLog(BeginTime, ParentObject.CurrentPlayer, action, ActionId, ending, "None");
    }

    public void SetContextFlag()
    {
        ContextFlag = true;
    }

    private ResourceCircle GetResourceDisplay()
    {
        _resourceDisplayHolder = _currentResourceDisplay;
        _currentResourceDisplay = ParentObject.ResourceDisplay;
        //_currentResourceDisplay = ParentObject.CurrentPlayer == Controllers.Input.ControllablePlayer && References.Menu.IsOpen ? References.Menu.GlobalResourceCircle : ParentObject.ResourceDisplay;
        if (_resourceDisplayHolder != _currentResourceDisplay)
        {
            _resourceDisplayHolder.Deactivate();
        }

        return _currentResourceDisplay;
    }
    
    private IEnumerator SpendResourcesRoutine(int id)
    {

        while (_resourceTimer < MaxActionDuration && IsActive && ActionId == id)
        {
            _resourceTimer += Time.deltaTime;
            GetResourceDisplay().UpdateState(Mathf.Clamp01(1f - _resourceTimer / MaxActionDuration));
            yield return null;
        }

        if (ActionId == id && IsActive)
        {
            _resourceTimer = MaxActionDuration;
            Cancel();
        }

        if (Mathf.Approximately(_resourceTimer, MaxActionDuration) && ActionId == id)
        {
            yield return new WaitForSeconds(Cooldown);
        }

        while (_resourceTimer > 0f && ActionId == id)
        {
            _resourceTimer -= Time.deltaTime * 2f;
            GetResourceDisplay().UpdateState(Mathf.Clamp01(1f - _resourceTimer / MaxActionDuration));
            yield return null;
        }

        if (ActionId == id)
        {
            _resourceTimer = 0f;
            GetResourceDisplay().UpdateState(1f);
        }

        yield return null;
    }
}
