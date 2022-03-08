using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Cloud : BlendObject
{
    public Transform ModelTf;
    
    private const float MaxYOffset = 0.125f;
    private const float FloatSpeed = 0.25f;
    private float _initialYPosition;

    private float _moveSpeed = 5f;
    
    private Vector2 _currentOffset;
    private float _currentYOffset;
    private float _baseY;
    private float _offsetTimer;

    private float _scaleTimer;
    private float _currentScale = 1f;
    private float _scaleHolder;
    private int _scaleId;
    private int _hoverId;

    public bool IsRaining => Action.IsPerformingAction();

    protected override void Awake()
    {
        base.Awake();
        Sr = ModelTf.GetComponent<SpriteRenderer>();
        _initialYPosition = Tf.position.y;
        _baseY = Tf.position.y;
        //Hover();
    }

    public override void ProcessInput(InputController.Type inputType, InputController.Mode inputMode, float x = 0f, float y = 0f)
    {
        base.ProcessInput(inputType, inputMode);
        switch (inputType)
        {
            case InputController.Type.Left:
                if (inputMode == InputController.Mode.Hold)
                {
                    MoveLeft();
                }
                break;
            case InputController.Type.Right:
                if (inputMode == InputController.Mode.Hold)
                {
                    MoveRight();
                }
                break;
            case InputController.Type.Up:
                MoveUp();
                break;
            case InputController.Type.Down:
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
        randomOval.x *= 1.0f;
        randomOval.y *= 0.5f;
        return CurrentPosition + randomOval;
    }

    protected override void OnStopMoving()
    {
        base.OnStopMoving();
        //Hover();
    }

    protected override void MoveLeft(float intensity = 1f)
    {
        // no base call
        AddForce(Vector2.left, _moveSpeed * 25f * Time.deltaTime);
    }

    protected override void MoveRight(float intensity = 1f)
    {
        // no base call
        AddForce(Vector2.right, _moveSpeed * 25f * Time.deltaTime);
    }

    protected override void MoveUp(float intensity = 1f)
    {
        // no base call
        return;
        AddForce(Vector2.up, _moveSpeed * 25f * Time.deltaTime);
    }

    protected override void MoveDown(float intensity = 1f)
    {
        // no base call
    }

    private void ApplyOffset()
    {
        CurrentPosition.y = _baseY + _currentYOffset;
        Tf.position = CurrentPosition;
    }

    public override void SoulPixelImpact(Pixel pixel)
    {
        base.SoulPixelImpact(pixel);
        _currentScale += 0.03125f;
        ApplyCurrentScale();
        Scale(1f, 0.125f);
    }

    private void Scale(float targetScale, float duration)
    {
        _scaleId++;
        StartCoroutine(ScaleRoutine(targetScale, duration, _scaleId));
    }

    private void ApplyCurrentScale()
    {
        ModelTf.localScale = _currentScale * Vector3.one;
    }

    private void Hover()
    {
        _hoverId = Utility.AddOne(_hoverId);
        StartCoroutine(HoverRoutine(_hoverId));
    }
    
    public void RainOnNearNote()
    {
        ArtificialMotivationId = Utility.AddOne(ArtificialMotivationId);
        StartCoroutine(RainOnNearNoteRoutine(ArtificialMotivationId));
    }

    public void RainOnHandcar()
    {
        ArtificialMotivationId = Utility.AddOne(ArtificialMotivationId);
        StartCoroutine(RainOnObjectRoutine(References.Entities.Handcar, true, ArtificialMotivationId));
    }

    public void RainOnCart()
    {
        ArtificialMotivationId = Utility.AddOne(ArtificialMotivationId);
        StartCoroutine(RainOnObjectRoutine(References.Entities.Cart, false, ArtificialMotivationId));
    }
    
    public void RainOnGround()
    {
        ArtificialMotivationId = Utility.AddOne(ArtificialMotivationId);
        StartCoroutine(RainOnObjectRoutine(References.Entities.Ground, false, ArtificialMotivationId));
    }

    private IEnumerator ScaleRoutine(float targetScale, float duration, int id)
    {
        _scaleTimer = 0f;
        _scaleHolder = _currentScale;
        while (_scaleTimer < duration && _scaleId == id)
        {
            _scaleTimer += Time.deltaTime;
            _currentScale = Mathf.Lerp(_scaleHolder, targetScale, _scaleTimer / duration);
            ApplyCurrentScale();
            yield return null;
        }

        if (_scaleId == id)
        {
            _currentScale = targetScale;
            ApplyCurrentScale();
        }
        yield return null;
    }

    private IEnumerator HoverRoutine(int id)
    {
        _offsetTimer %= 2f / MoveSpeed;
        while (!IsMoving && _hoverId == id)
        {
            _offsetTimer += Time.deltaTime;
            _currentYOffset = Mathf.Sin(_offsetTimer * FloatSpeed * Mathf.PI) * MaxYOffset;
            ApplyOffset();
            yield return null;
        }

        yield return null;
    }
    
    private IEnumerator RainOnNearNoteRoutine(int id)
    {
        // find note
        var note = Controllers.Level.GetNearNote(References.Entities.Handcar.CurrentPosition);
        var xDistance = Mathf.Abs(CurrentPosition.x - note.Position.x);
        
        // move over note
        while (xDistance > 0.25f && ArtificialMotivationId == id)
        {
            xDistance = Mathf.Abs(CurrentPosition.x - note.Position.x);

            if (CurrentPosition.x < note.Position.x)
            {
                MoveRight();
            }
            else
            {
                MoveLeft();
            }
            
            yield return null;
        }

        if (ArtificialMotivationId == id)
        {
            var rainTimer = 0f;
            var rainDuration = Action.GetRandomDuration(4);
            Action.TryToPerform();
        
            while (rainTimer < rainDuration && ArtificialMotivationId == id)
            {
                rainTimer += Time.deltaTime;
                yield return null;
            }
            
            Action.Stop();
        }

        yield return null;
    }
    
    private IEnumerator RainOnObjectRoutine(ControllableObject controllableObject, bool rainLonger, int id)
    {
        var predictedPosition = controllableObject.PredictPosition(1f);
        var xDistance = Mathf.Abs(CurrentPosition.x - predictedPosition.x);
        
        // move over object
        while (xDistance > 0.25f && ArtificialMotivationId == id)
        {
            predictedPosition = controllableObject.PredictPosition(1f);
            xDistance = Mathf.Abs(CurrentPosition.x - predictedPosition.x);

            if (CurrentPosition.x < predictedPosition.x)
            {
                MoveRight();
            }
            else
            {
                MoveLeft();
            }
            
            yield return null;
        }

        if (ArtificialMotivationId == id)
        {
            var rainTimer = 0f;
            var rainDuration = Action.GetRandomDuration(4, rainLonger);
            Action.TryToPerform();
        
            while (rainTimer < rainDuration && ArtificialMotivationId == id)
            {
                rainTimer += Time.deltaTime;
                
                predictedPosition = controllableObject.PredictPosition(1f);

                if (CurrentPosition.x < predictedPosition.x)
                {
                    MoveRight();
                }
                else
                {
                    MoveLeft();
                }
                
                yield return null;
            }
            
            Action.Stop();
        }

        yield return null;
    }
}
