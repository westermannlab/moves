using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boost : Action
{
    public SoundEffect BoostSound;
    
    private BoostParticle _boostParticle;
    private const float ParticleSpawnInterval = 0.015f;
    private float _lastSpawnTime;

    private bool _isRaining;
    private bool _isShaking;
    private bool _isMovingUpSlope;

    protected override void Perform()
    {
        base.Perform();
        References.Coroutines.StartCoroutine(BoostRoutine(ActionId));

        _isRaining = References.Actions.IsItRaining();
        _isShaking = References.Actions.IsItShaking();
        _isMovingUpSlope = References.Entities.IsHandcarMovingUpSlope();
    }

    public override void Stop()
    {
        base.Stop();
        _lastSpawnTime = 0f;
    }

    protected override void Cancel()
    {
        base.Cancel();
        _lastSpawnTime = 0f;
    }
    
    protected override Log.Action GetAction()
    {
        if (_isRaining)
        {
            return Log.Action.BoostRaining;
        }
        if (_isShaking)
        {
            return Log.Action.BoostShaking;
        }
        if (_isMovingUpSlope)
        {
            return Log.Action.BoostUpSlope;
        }
        if (_isRaining != References.Actions.IsItRaining() || _isShaking != References.Actions.IsItShaking() ||
            _isMovingUpSlope != References.Entities.IsHandcarMovingUpSlope())
        {
            return Log.Action.BoostUndefined;
        }

        return Log.Action.Boost;
    }

    private IEnumerator BoostRoutine(int id)
    {
        var speaker = References.Prefabs.GetSpeaker();
        speaker.StartLoop(BoostSound, 0.25f);
        
        while (IsActive && ActionId == id)
        {
            ActionTimer += Time.deltaTime;
            ParentObject.MoveInLastDirection(2f);
            if (ActionTimer > _lastSpawnTime + ParticleSpawnInterval)
            {
                _lastSpawnTime = ActionTimer;
                
                _boostParticle = References.Prefabs.GetBoostParticle();
                if (ParentObject.CurrentPlayer != null)
                {
                    _boostParticle.SetColor(ParentObject.CurrentPlayer.SoftColor);
                }
                _boostParticle.Spawn(ParentObject.CurrentPosition + new Vector2(Random.Range(-0.0625f, 0.0625f), Random.Range(-0.25f, -0.15625f)));
            }
            yield return null;
        }
        
        speaker.EndLoop(0.5f);
        
        yield return null;
    }
}
