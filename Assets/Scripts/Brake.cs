using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brake : Action
{
    public SoundEffect BrakeOnSound;
    public SoundEffect BrakeOffSound;
    
    private bool _isMoving;
    private bool _isGroundShaking;

    private bool _hasJumped => ContextFlag;
    
    protected override void Perform()
    {
        base.Perform();

        _isMoving = ParentObject.IsRolling;
        _isGroundShaking = References.Actions.IsItShaking();
        
        Controllers.Audio.PlaySoundEffect(BrakeOnSound);
    }

    public override void Stop()
    {
        base.Stop();
        
        Controllers.Audio.PlaySoundEffect(BrakeOffSound);
    }

    protected override void Cancel()
    {
        base.Cancel();
        
        Controllers.Audio.PlaySoundEffect(BrakeOffSound);
    }

    protected override Log.Action GetAction()
    {
        if (_isGroundShaking)
        {
            return Log.Action.BrakeShaking;
        }
        if (_isMoving)
        {
            return Log.Action.Brake;
        }

        if (_hasJumped)
        {
            return Log.Action.BrakeJump;
        }
        if (_isGroundShaking != References.Actions.IsItShaking() || _isMoving != ParentObject.IsRolling)
        {
            return Log.Action.BrakeUndefined;
        }

        return Log.Action.BrakeNotMoving;
    }

    public void Toggle()
    {
        if (IsPerformingAction())
        {
            Stop();
        }
        else
        {
            Perform();
        }
    }
}
