using System.Collections;
using UnityEngine;

public class Vehicle : BlendObject
{
    public SpriteRenderer[] BlendRenderers;
    public Collider2D[] NoteTriggers;

    private Vector2 _lastPosition;
    private bool _isBeingMonitored;
    private bool _isRolling;
    
    public override bool IsRolling => _isRolling || IsMoving;

    protected virtual void OnEnable()
    {
        References.Events.OnBeginRotateGround += MonitorMovement;
        References.Events.OnApplyGravitationalForce += AddForce;
        References.Events.OnGroundShake += AddForce;
    }

    protected virtual void OnDisable()
    {
        References.Events.OnBeginRotateGround -= MonitorMovement;
        References.Events.OnApplyGravitationalForce -= AddForce;
        References.Events.OnGroundShake -= AddForce;
    }

    public override void Show(float duration)
    {
        base.Show(duration);
        foreach (var trigger in NoteTriggers)
        {
            if (trigger)
            {
                trigger.enabled = true;
            }
        }
    }

    public override void Hide(float duration)
    {
        base.Hide(duration);
        foreach (var trigger in NoteTriggers)
        {
            if (trigger)
            {
                trigger.enabled = false;
            }
        }
    }
    
    public override void Enter(Player player)
    {
        base.Enter(player);
        if (References.Entities.AreBothVehiclesAnimated())
        {
            References.Entities.Handcar.ChangeMoveLinearDrag(10f);
        }
        else
        {
            References.Entities.Handcar.ChangeMoveLinearDrag(0f);
        }
    }

    public override void Leave(Player player)
    {
        base.Leave(player);
        References.Entities.Handcar.ChangeMoveLinearDrag(0f);
    }

    protected override void ApplyCurrentBlendValue(float blendValue)
    {
        foreach (var sr in BlendRenderers)
        {
            sr.GetPropertyBlock(Block);
            Block.SetFloat(BlendHash, blendValue);
            sr.SetPropertyBlock(Block);
        }
    }
    
    protected override void SetBlendOffsets(int offsetX, int offsetY)
    {
        foreach (var sr in BlendRenderers)
        {
            sr.GetPropertyBlock(Block);
            Block.SetInt(PreviousOffsetXHash, CurrentBlendOffsets.x);
            Block.SetInt(PreviousOffsetYHash, CurrentBlendOffsets.y);
            sr.SetPropertyBlock(Block);
        }
        
        CurrentBlendOffsets.x = offsetX;
        CurrentBlendOffsets.y = offsetY;
        
        foreach (var sr in BlendRenderers)
        {
            sr.GetPropertyBlock(Block);
            Block.SetInt(OffsetXHash, offsetX);
            Block.SetInt(OffsetYHash, offsetY);
            sr.SetPropertyBlock(Block);
        }
    }

    protected override void UpdateCurrentPosition(bool isRecursive = true)
    {
        base.UpdateCurrentPosition();
        if (isRecursive)
        {
            GetOtherVehicle().UpdateCurrentPosition(false);
        }
    }

    public virtual Vehicle GetOtherVehicle()
    {
        return this;
    }

    public bool IsGoingUpSlope()
    {
        var downHillDirection = References.Entities.Ground.GetDownhillVector();
        return LastMoveDirection.x < 0f && downHillDirection.x > 0f ||
               LastMoveDirection.x > 0f && downHillDirection.x < 0f;
    }

    protected void MonitorMovement()
    {
        /*
        if (!_isBeingMonitored)
        {
            _isBeingMonitored = true;
            StartCoroutine(MonitorMovementRoutine());
        }
        */
        if (!IsMoving)
        {
            InitiateMovement();
        }
    }
    
    private IEnumerator MonitorMovementRoutine()
    {
        _lastPosition = Rb.position;
        while (_isBeingMonitored)
        {
            CheckForSectorChange();
            _isRolling = Rb.position != _lastPosition;
            _lastPosition = Rb.position;
            yield return null;
        }
        yield return null;
    }
}
