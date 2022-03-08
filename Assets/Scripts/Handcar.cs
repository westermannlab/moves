using System.Collections;
using UnityEngine;

public class Handcar : Vehicle
{
    public Transform Handle;
    public Transform LargeGear;
    public Transform SmallGear;
    public SpriteRenderer CouplingSr;

    private const int LargeGearPixelDiameter = 15;
    private const int SmallGearPixelDiameter = 9;

    private float _largeGearCircumference;
    private float _smallGearCircumference;

    private float _moveHandleTimer;

    private Vector3 _currentLargeGearEulerRotation;
    private Vector3 _currentSmallGearEulerRotation;
    private Vector3 _currentHandleEulerRotation;
    private float _currentLargeGearAngle;
    private float _currentSmallGearAngle;
    private float _currentHandleAngle;

    private int _turnGearsId;
    private int _moveHandleId;
    private int _currentDirection;

    private Color _currentCouplingColor = Color.white;
    private float _changeCouplingAlphaTimer;
    private float _currentCouplingAlpha = 1f;
    private float _couplingAlphaHolder;
    private int _changeCouplingAlphaId;
    private int _boostId;

    protected override void Awake()
    {
        base.Awake();
        _largeGearCircumference = LargeGearPixelDiameter * Utility.PixelToUnit * Mathf.PI;
        _smallGearCircumference = SmallGearPixelDiameter * Utility.PixelToUnit * Mathf.PI;
    }
    
    public override void ProcessInput(InputController.Type inputType, InputController.Mode inputMode, float x = 0f, float y = 0f)
    {
        base.ProcessInput(inputType, inputMode);
        switch (inputType)
        {
            case InputController.Type.Left:
                switch (inputMode)
                {
                    case InputController.Mode.Press:
                        _currentDirection = -1;
                        MoveHandle();
                        TurnGears();
                        break;
                    case InputController.Mode.Hold:
                        MoveLeft();
                        break;
                    case InputController.Mode.Release:
                        if (_currentDirection == -1)
                        {
                            _currentDirection = 0;
                        }
                        break;
                }
                break;
            case InputController.Type.Right:
                switch (inputMode)
                {
                    case InputController.Mode.Press:
                        _currentDirection = 1;
                        MoveHandle();
                        TurnGears();
                        break;
                    case InputController.Mode.Hold:
                        MoveRight();
                        break;
                    case InputController.Mode.Release:
                        if (_currentDirection == 1)
                        {
                            _currentDirection = 0;
                        }
                        break;
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
        return References.Entities.Cart;
    }

    public override void Leave(Player player)
    {
        base.Leave(player);
        _currentDirection = 0;
    }

    private void TurnGears()
    {
        _turnGearsId++;
        StartCoroutine(TurnGearsRoutine(_turnGearsId));
    }

    private void MoveHandle()
    {
        _moveHandleId++;
        StartCoroutine(MoveHandleRoutine(_moveHandleId));
    }

    public void ChangeCouplingAlpha(float targetAlpha, float duration)
    {
        _changeCouplingAlphaId = Utility.AddOne(_changeCouplingAlphaId);
        StartCoroutine(ChangeCouplingAlphaRoutine(targetAlpha, duration, _changeCouplingAlphaId));
    }
    
    private void UpdateGearAngles()
    {
        _currentLargeGearEulerRotation.z = _currentLargeGearAngle;
        _currentSmallGearEulerRotation.z = _currentSmallGearAngle;
        LargeGear.localRotation = Quaternion.Euler(_currentLargeGearEulerRotation);
        SmallGear.localRotation = Quaternion.Euler(_currentSmallGearEulerRotation);
    }

    private void UpdateHandleAngle()
    {
        _currentHandleEulerRotation.z = _currentHandleAngle;
        Handle.localRotation = Quaternion.Euler(_currentHandleEulerRotation);
    }
    
    public void Boost()
    {
        // using a specific boost id instead of artificial motivation id here to continue move input
        _boostId = Utility.AddOne(_boostId);
        StartCoroutine(BoostRoutine(_boostId));
    }
    
    private IEnumerator TurnGearsRoutine(int id)
    {
        while ((_currentDirection != 0 || Mathf.Abs(Rb.velocity.x) > 0f) && _turnGearsId == id)
        {
            _currentLargeGearAngle -= (Rb.velocity.x / MoveSpeed) / _largeGearCircumference * 10f;
            _currentSmallGearAngle += (Rb.velocity.x / MoveSpeed) / _smallGearCircumference * 10f;
            UpdateGearAngles();
            yield return null;
        }

        yield return null;
    }

    private IEnumerator MoveHandleRoutine(int id)
    {
        while (_currentDirection != 0 && _moveHandleId == id)
        {
            _moveHandleTimer += Time.deltaTime * Mathf.PI * 2f;
            _currentHandleAngle = Mathf.Sin(_moveHandleTimer) * 20f;
            UpdateHandleAngle();
            yield return null;
        }

        yield return null;
    }

    private IEnumerator ChangeCouplingAlphaRoutine(float targetAlpha, float duration, int id)
    {
        _changeCouplingAlphaTimer = 0f;
        _couplingAlphaHolder = _currentCouplingAlpha;

        while (_changeCouplingAlphaTimer < duration && _changeCouplingAlphaId == id)
        {
            _changeCouplingAlphaTimer += Time.deltaTime;
            _currentCouplingAlpha = Mathf.Lerp(_couplingAlphaHolder, targetAlpha, _changeCouplingAlphaTimer / duration);
            _currentCouplingColor.a = _currentCouplingAlpha;
            if (CouplingSr)
            {
                CouplingSr.color = _currentCouplingColor;
            }
            yield return null;
        }

        if (_changeCouplingAlphaId == id)
        {
            _currentCouplingAlpha = targetAlpha;
            _currentCouplingColor.a = _currentCouplingAlpha;
            if (CouplingSr)
            {
                CouplingSr.color = _currentCouplingColor;
            }
        }
        
        yield return null;
    }
    
    private IEnumerator BoostRoutine(int id)
    {
        var boostTimer = 0f;
        var boostDuration = Action.GetRandomDuration(4);
        Action.TryToPerform();
    
        while (boostTimer < boostDuration && _boostId == id)
        {
            boostTimer += Time.deltaTime;
            yield return null;
        }
        
        Action.Stop();
        
        yield return null;
    }
}
