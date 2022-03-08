using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Cart : Vehicle
{
    public SpriteRenderer HandleRenderer;
    public Rigidbody2D RightWheelRb;
    public Transform ModelTf;

    private Transform _handleTf;

    private Vector3 _currentHandleEulerRotation;
    private float _currentHandleBlendValue;
    private const float MaxHandleAngle = 20f;
    private float _currentHandleAngle = -MaxHandleAngle;
    private float _angleHolder;
    private float _rotateHandleTimer;
    private float _applyBrakesTime;
    private float _lastJumpTime;
    private int _rotateHandleId;
    private bool _isGroundShaking;
    private bool _jumpAfterBrake;

    private int _shakeId;
    private int _shakeFrameCount;
    private const int MaxShakeFrames = 2;
    private float _shakeTimer;
    private Vector2 _shakeOffset;
    private Vector2 _offsetHolder;

    public bool IsBraked => Action.IsPerformingAction();

    protected override void Awake()
    {
        base.Awake();
        Sr = GetComponentInChildren<SpriteRenderer>();
        _handleTf = HandleRenderer.transform;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        References.Events.OnEarthquakeStateChange += HandleEarthquake;
        Action.OnPerform += UpdateHandleAngle;
        Action.OnStop += UpdateHandleAngle;
        Action.OnCancel += UpdateHandleAngle;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        References.Events.OnEarthquakeStateChange -= HandleEarthquake;
        Action.OnPerform -= UpdateHandleAngle;
        Action.OnStop -= UpdateHandleAngle;
        Action.OnCancel -= UpdateHandleAngle;
    }
    
    public override void ProcessInput(InputController.Type inputType, InputController.Mode inputMode, float x = 0f, float y = 0f)
    {
        base.ProcessInput(inputType, inputMode);
        switch (inputType)
        {
            case InputController.Type.Left:
                if (inputMode == InputController.Mode.Press)
                {
                    Controllers.Audio.PlaySoundEffect(InvalidInputSound);
                }
                break;
            case InputController.Type.Right:
                if (inputMode == InputController.Mode.Press)
                {
                    Controllers.Audio.PlaySoundEffect(InvalidInputSound);
                }
                break;
            case InputController.Type.Up:
                if (inputMode == InputController.Mode.Hold)
                {
                    CurrentPlayer.ReturnToSoul();
                }
                else if (inputMode == InputController.Mode.Release)
                {
                    CurrentPlayer.CancelSoulTransfer(Type.Soul);
                }
                break;
        }
    }

    public override Vector2 GetSoulTargetPoint(bool addRandom, float xOffset = 0f)
    {
        return (Vector2)Tf.position + (addRandom ? Random.insideUnitCircle * 0.375f : Vector2.zero);
    }
    
    public override Vehicle GetOtherVehicle()
    {
        return References.Entities.Handcar;
    }

    private void UpdateHandleAngle()
    {
        _rotateHandleId++;
        StartCoroutine(RotateHandleRoutine(0.2f, Action.IsPerformingAction() ? 20f : -20f, _rotateHandleId));
        if (Action.IsPerformingAction())
        {
            Shake();
        }
    }

    private void ApplyHandleAngle()
    {
        _currentHandleEulerRotation.z = _currentHandleAngle;
        _handleTf.localRotation = Quaternion.Euler(_currentHandleEulerRotation);
    }

    private void ApplyHandleBlendValue()
    {
        HandleRenderer.GetPropertyBlock(Block);
        Block.SetFloat(BlendHash, _currentHandleBlendValue);
        HandleRenderer.SetPropertyBlock(Block);
    }

    private void HandleEarthquake(bool isEarthquakeActive)
    {
        _isGroundShaking = isEarthquakeActive;
        Rb.drag = IsBraked && !isEarthquakeActive ? GetBrakeDrag() : isEarthquakeActive ? 10f : 0f;
        Rb.constraints = IsBraked && GetOtherVehicle().CurrentPlayer == null && !isEarthquakeActive ? RigidbodyConstraints2D.FreezePositionX : RigidbodyConstraints2D.None;
    }

    private float GetBrakeDrag()
    {
        if (CurrentPlayer != null && CurrentPlayer.Counterpart != null &&
            CurrentPlayer.Counterpart.CurrentType == Type.Handcar)
        {
            return 100f;
        }
        return 10000f;
    }

    private void Shake()
    {
        _shakeId = Utility.AddOne(_shakeId);
        StartCoroutine(ShakeRoutine(0f, 0.03125f, _shakeId));
    }

    private void ApplyShakeOffset(Vector2 offset)
    {
        ModelTf.localPosition = offset;
    }

    public bool HasJumpedRecently(float maxTime)
    {
        return Time.time - _lastJumpTime < maxTime;
    }
    
    public void Brake(bool longerBrake)
    {
        ArtificialMotivationId = Utility.AddOne(ArtificialMotivationId);
        StartCoroutine(BrakeRoutine(longerBrake, ArtificialMotivationId));
    }

    private IEnumerator RotateHandleRoutine(float duration, float targetAngle, int id)
    {
        _rotateHandleTimer = 0f;
        _angleHolder = _currentHandleAngle;
        while (_rotateHandleTimer < duration && _rotateHandleId == id)
        {
            _rotateHandleTimer += Time.deltaTime;
            _currentHandleAngle = Mathf.Lerp(_angleHolder, targetAngle, _rotateHandleTimer / duration);
            ApplyHandleAngle();
            _currentHandleBlendValue = Mathf.InverseLerp(-MaxHandleAngle, MaxHandleAngle, _currentHandleAngle);
            ApplyHandleBlendValue();
            yield return null;
        }

        if (_rotateHandleId == id)
        {
            _currentHandleAngle = targetAngle;
            ApplyHandleAngle();
            _currentHandleBlendValue = Mathf.InverseLerp(-MaxHandleAngle, MaxHandleAngle, targetAngle);
            ApplyHandleBlendValue();
            HandleEarthquake(_isGroundShaking);
            RightWheelRb.freezeRotation = IsBraked && GetOtherVehicle().CurrentPlayer == null;
        }

        yield return null;
    }

    private IEnumerator ShakeRoutine(float minIntensity, float maxIntensity, int id)
    {
        _shakeTimer = 0f;
        _shakeFrameCount = 0;
        _offsetHolder = Vector2.zero;
        
        if (References.Entities.Ground.IsEven() && TimeSinceLastSectorChange() > 2f)
        {
            // jump if ground is even
            _jumpAfterBrake = true;
            Action.SetContextFlag();
        }
        else
        {
            _jumpAfterBrake = false;
        }
        
        while (IsBraked && _shakeId == id)
        {
            if (Mathf.Abs(References.Entities.Ground.CurrentEulerAngle) < 2f)
            {
                _shakeTimer += Time.deltaTime;
            
                _shakeFrameCount++;
                ApplyShakeOffset(Vector2.Lerp(_offsetHolder, _shakeOffset, (float)_shakeFrameCount / MaxShakeFrames));
            
                if (_shakeFrameCount == MaxShakeFrames)
                {
                    _offsetHolder = _shakeOffset;
                    _shakeOffset = Random.insideUnitCircle * Mathf.Lerp(minIntensity, maxIntensity, _shakeTimer / Action.MaxActionDuration);
                    _shakeFrameCount = 0;
                }   
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.21875f);

        if (_shakeId == id && _jumpAfterBrake)
        {
            // jump
            AddForce(new Vector2(Random.value * 0f, 1f), 2500f * Mathf.Lerp(1f, 2f, _shakeTimer / Action.MaxActionDuration));
            _lastJumpTime = Time.time;
        }
        
        ApplyShakeOffset(Vector2.zero);
        
        yield return null;
    }
    
    private IEnumerator BrakeRoutine(bool longerBrake, int id)
    {
        var brakeTimer = 0f;
        var brakeDuration = Action.GetRandomDuration(4, longerBrake);
        Action.TryToPerform();
    
        while (brakeTimer < brakeDuration && ArtificialMotivationId == id)
        {
            brakeTimer += Time.deltaTime;
            yield return null;
        }
        
        Action.Stop();
        
        yield return null;
    }
}
