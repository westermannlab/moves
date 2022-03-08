using System.Collections;
using System.Globalization;
using UnityEngine;

public class Ground : BlendObject
{
    public SpriteRenderer[] GroundRenderers;
    public CircleCollider2D NoteTrigger;
    
    private Vector3 _currentEulerRotation;
    private float _currentEulerAngle;

    private const float MaxAngle = 15f;
    private float _rotateTimer;
    private float _beginRotateTime;
    private float _lastRotateInputTime;
    private int _rotateId;
    private int _shakeId;

    private bool _inputProcessed;
    private bool _hasApplyRequest;
    private bool _isRotating;

    public float CurrentEulerAngle => _currentEulerAngle;
    public float MaxEulerAngle => MaxAngle;

    public bool IsShaking => Action.IsPerformingAction();

    private void FixedUpdate()
    {
        if (_hasApplyRequest)
        {
            ApplyCurrentAngle();
        }

        if (!Mathf.Approximately(_currentEulerAngle, 0f))
        {
            References.Events.ApplyGravitationalForce(Quaternion.AngleAxis(_currentEulerAngle, Vector3.forward) * (_currentEulerAngle < 0f ? Vector3.right : Vector3.left), Mathf.Abs(_currentEulerAngle * 5f));
        }
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
                        _beginRotateTime = Time.timeSinceLevelLoad;
                        _rotateTimer = 0f;
                        References.Events.BeginRotateGround();
                        break;
                    case InputController.Mode.Hold:
                        _rotateTimer += Time.deltaTime;
                        TiltLeft();
                        break;
                    case InputController.Mode.Release:
                        // log: rotate to
                        Controllers.Logs.AddLog(_beginRotateTime, CurrentPlayer, Log.Action.RotateTo, _rotateId, Log.Ending.Natural, CurrentAngleToString());
                        break;
                }

                _lastRotateInputTime = Time.timeSinceLevelLoad;
                break;
            case InputController.Type.Right:
                switch (inputMode)
                {
                    case InputController.Mode.Press:
                        _beginRotateTime = Time.timeSinceLevelLoad;
                        _rotateTimer = 0f;
                        References.Events.BeginRotateGround();
                        break;
                    case InputController.Mode.Hold:
                        _rotateTimer += Time.deltaTime;
                        TiltRight();
                        break;
                    case InputController.Mode.Release:
                        // log: rotate to
                        Controllers.Logs.AddLog(_beginRotateTime, CurrentPlayer, Log.Action.RotateTo, _rotateId, Log.Ending.Natural, CurrentAngleToString());
                        break;
                }

                _lastRotateInputTime = Time.timeSinceLevelLoad;
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
        var randomOval = addRandom ? Random.insideUnitCircle : Vector2.zero;
        // randomOval.x *= 3f;
        return new Vector2(xOffset, -2f) + randomOval;
    }

    public override void Enter(Player player)
    {
        base.Enter(player);
        // set begin rotate time for situations when the left or right key are already held down while animating
        _beginRotateTime = Time.timeSinceLevelLoad;
    }

    public override void Leave(Player player)
    {
        base.Leave(player);
        ResetAngle();
    }

    private void ApplyCurrentAngle()
    {
        _currentEulerRotation.z = _currentEulerAngle;
        Tf.rotation = Quaternion.Euler(_currentEulerRotation);
        _hasApplyRequest = false;
    }

    private string CurrentAngleToString()
    {
        return _currentEulerAngle.ToString("F3", CultureInfo.InvariantCulture);
    }

    private void TiltLeft()
    {
        _inputProcessed = true;
        if (!_isRotating)
        {
            _isRotating = true;
            StartCoroutine(WhileRotatingRoutine());
        }
        _currentEulerAngle += MaxAngle * 0.5f * Time.deltaTime;
        if (_currentEulerAngle > MaxAngle)
        {
            _currentEulerAngle = MaxAngle;
        }
        //_hasApplyRequest = true;
        References.Events.ChangeGroundAngle(_currentEulerAngle);
    }

    private void TiltRight()
    {
        _inputProcessed = true;
        if (!_isRotating)
        {
            _isRotating = true;
            StartCoroutine(WhileRotatingRoutine());
        }
        _currentEulerAngle -= MaxAngle * 0.5f * Time.deltaTime;
        if (_currentEulerAngle < -MaxAngle)
        {
            _currentEulerAngle = -MaxAngle;
        }
        //_hasApplyRequest = true;
        References.Events.ChangeGroundAngle(_currentEulerAngle);
    }

    private void ResetAngle()
    {
        StartCoroutine(ResetAngleRoutine());
    }

    public override Vector2 PredictPosition(float time)
    {
        return Vector2.zero;
    }

    protected override void ApplyCurrentBlendValue(float blendValue)
    {
        foreach (var sr in GroundRenderers)
        {
            sr.GetPropertyBlock(Block);
            Block.SetFloat(BlendHash, blendValue);
            sr.SetPropertyBlock(Block);
        }
    }

    protected override void SetBlendOffsets(int offsetX, int offsetY)
    {
        foreach (var sr in GroundRenderers)
        {
            sr.GetPropertyBlock(Block);
            Block.SetInt(PreviousOffsetXHash, CurrentBlendOffsets.x);
            Block.SetInt(PreviousOffsetYHash, CurrentBlendOffsets.y);
            sr.SetPropertyBlock(Block);
        }
        
        CurrentBlendOffsets.x = offsetX;
        CurrentBlendOffsets.y = offsetY;
        
        foreach (var sr in GroundRenderers)
        {
            sr.GetPropertyBlock(Block);
            Block.SetInt(OffsetXHash, offsetX);
            Block.SetInt(OffsetYHash, offsetY);
            sr.SetPropertyBlock(Block);
        }
    }

    public Vector2 GetDownhillVector()
    {
        if (Mathf.Approximately(_currentEulerAngle, 0f))
        {
            return Vector2.zero;
        }
        return Quaternion.AngleAxis(_currentEulerAngle, Vector3.forward) * (_currentEulerAngle > 0f ? Vector3.left : Vector3.right);
    }

    public Vector2 GetLocalUpVector()
    {
        var localLeft = Quaternion.AngleAxis(_currentEulerAngle, Vector3.forward) * Vector3.left;
        return Vector3.Cross(localLeft, Vector3.forward).normalized;
    }
    
    protected override void UpdateNoteCount(int colorId)
    {
        // compares the cart's position (instead of this object's own position) with the notes' positions
        NoteCount = Controllers.Level.GetNoteValuesOnEitherSide(References.Entities.Cart.CurrentPosition.x, colorId);
        if (_currentEulerAngle > 0f)
        {
            NoteCount.x *= 1.5f;
        }
        else if (_currentEulerAngle < 0f)
        {
            NoteCount.y *= 1.5f;
        }

        var results = Vector2Int.zero;
        for (var i = 0; i < 5; i++)
        {
            var randomNumber = Random.value * (NoteCount.x + NoteCount.y);
            if (randomNumber < NoteCount.x)
            {
                results.x++;
            }
            else
            {
                results.y++;
            }
        }
       
        CurrentCollectDirection = results.x > results.y ? InputController.Type.Left : InputController.Type.Right;
    }

    public bool IsEven()
    {
        return Mathf.Abs(References.Entities.Ground.CurrentEulerAngle) < 2f;
    }

    public override void StopArtificialMovement()
    {
        base.StopArtificialMovement();
        ResetAngle();
    }

    public void Shake()
    {
        _shakeId = Utility.AddOne(_shakeId);
        StartCoroutine(ShakeRoutine(_shakeId));
    }

    private IEnumerator ResetAngleRoutine()
    {
        _inputProcessed = false;
        var player = CurrentPlayer;
        var fromAngle = _currentEulerAngle;
        var timer = 0f;
        var duration = Mathf.Abs(fromAngle) / 15f;
        while (timer < duration && (!_inputProcessed || CurrentPlayer == null || CurrentPlayer == player))
        {
            timer += Time.deltaTime;
            _currentEulerAngle = Mathf.Lerp(fromAngle, 0f, timer / duration);
            References.Events.ChangeGroundAngle(_currentEulerAngle);
            yield return null;
        }

        yield return null;
    }

    private IEnumerator WhileRotatingRoutine()
    {
        _rotateId = Controllers.Logs.RequestActionId();
        while (Time.timeSinceLevelLoad < _lastRotateInputTime + 0.25f)
        {
            yield return null;
        }

        _isRotating = false;
        yield return null;
    }

    private IEnumerator ShakeRoutine(int id)
    {
        var shakeTimer = 0f;
        var shakeDuration = Action.GetRandomDuration(4);
        Action.TryToPerform();
    
        while (shakeTimer < shakeDuration && _shakeId == id)
        {
            shakeTimer += Time.deltaTime;
            yield return null;
        }
        
        Action.Stop();
        
        yield return null;
    }
}
