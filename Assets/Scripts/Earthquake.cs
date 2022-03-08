using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Earthquake : Action
{
    public SoundEffect EarthquakeSound;
    
    private const float MaxEarthquakeIntensity = 5f;
    private const float EarthquakeThreshold = 1f;
    private const int ShakeCycleFrames = 4;
    private float _currentEarthquakeIntensity;
    private float _earthquakeTimer;
    private int _shakeFrameCount;
    private Vector2 _shakePositionHolder;
    private Vector2 _currentShakeOffset;

    private Player _otherPlayer;
    private ControllableObject.Type _otherType;
    private bool _otherIsMoving;
    private bool _otherIsBraked;
    
    protected override void Perform()
    {
        base.Perform();
        
        _otherPlayer = ParentObject.CurrentPlayer != null ? ParentObject.CurrentPlayer.Counterpart : null;
        _otherType = _otherPlayer != null ? _otherPlayer.CurrentType : ControllableObject.Type.None;
        _otherIsMoving = References.Entities.Handcar.IsRolling;
        _otherIsBraked = References.Actions.IsItBraked();
        
        References.Coroutines.StartCoroutine(EarthquakeRoutine(ActionId));
    }

    private void Shake()
    {
        if (_shakeFrameCount == 0)
        {
            _shakePositionHolder = _currentShakeOffset;
            _currentShakeOffset = new Vector2(_currentEarthquakeIntensity * Random.Range(-Utility.PixelToUnit, Utility.PixelToUnit), _currentEarthquakeIntensity * Random.Range(-Utility.PixelToUnit, Utility.PixelToUnit));
        }

        _shakeFrameCount++;
        ParentObject.Tf.localPosition = Vector2.Lerp(_shakePositionHolder, _currentShakeOffset, (float)_shakeFrameCount / ShakeCycleFrames);

        // Utility.DrawPoint(Vector2.zero, Color.green, 1f);
        // Utility.DrawPoint(References.Entities.Ground.GetLocalUpVector(), Color.red, 1f);

        if (_shakeFrameCount >= ShakeCycleFrames)
        {
            _shakeFrameCount = 0;
            if (References.Entities.Cart.IsBraked)
            {
                References.Events.ApplyGroundShakeForce(References.Entities.Ground.GetDownhillVector(), 400f);
            }
            References.Events.ApplyGroundShakeForce(References.Entities.Ground.GetLocalUpVector(), 1000f);
        }
    }
    
    protected override Log.Action GetAction()
    {
        if (_otherType == ControllableObject.Type.Handcar && _otherIsMoving)
        {
            return Log.Action.ShakeMoving;
        }
        if (_otherType == ControllableObject.Type.Cart && _otherIsBraked)
        {
            return Log.Action.ShakeBraked;
        }
        if (_otherPlayer != null &&
            (_otherPlayer.CurrentType == ControllableObject.Type.Handcar && References.Entities.Handcar.IsRolling ||
             _otherPlayer.CurrentType == ControllableObject.Type.Cart &&
             References.Actions.Brake.IsPerformingAction()))
        {
            return Log.Action.ShakeUndefined;
        }

        return Log.Action.Shake;
    }

    private IEnumerator EarthquakeRoutine(int id)
    {
        References.Events.ChangeEarthquakeState(true);

        var speaker = References.Prefabs.GetSpeaker();
        speaker.StartLoop(EarthquakeSound, 0.25f);
        
        while (IsPerformingAction() && ActionId == id)
        {
            _earthquakeTimer += Time.fixedDeltaTime;
            _currentEarthquakeIntensity = Mathf.Lerp(0f, MaxEarthquakeIntensity, _earthquakeTimer / EarthquakeThreshold);
            Shake();
            yield return new WaitForFixedUpdate();
        }
        
        speaker.EndLoop(0.5f);

        if (ActionId == id)
        {
            _earthquakeTimer = Mathf.Min(_earthquakeTimer, EarthquakeThreshold);
            while (_earthquakeTimer > 0f && ActionId == id)
            {
                _earthquakeTimer -= Time.fixedDeltaTime;
                _currentEarthquakeIntensity =
                    Mathf.Lerp(0f, MaxEarthquakeIntensity, _earthquakeTimer / EarthquakeThreshold);
                Shake();
                yield return new WaitForFixedUpdate();
            }

            if (ActionId == id)
            {
                _earthquakeTimer = 0f;
                References.Events.ChangeEarthquakeState(false);
            }
        }
        
        yield return null;
    }
}
