using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soul : ControllableObject
{
    public LayerMask SoulPixelLayerMask;
    
    private ControllableObject _animatedObject;
    private Pixel _pixel;
    private Collider2D _collider2D;
    private Vector2 _randomPosition;

    private bool _hasPlayer;
    private int _emitPixelsId;
    private int _pixelCount;
    private const int PixelsPerSecond = 180;
    private int _followPlayerId;
    private int _autoMoveId;
    private int _sortingOrderOffset;
    private float _transferTimer;
    private float _currentSpeed;
    private float _emitPixelsTimer;

    private readonly List<SoulTrigger> _soulTriggerList = new List<SoulTrigger>();

    private SoulTrigger _closestTrigger;
    private float _closestTriggerSqrDistance;

    public ControllableObject AnimatedObject => _animatedObject;
    public SoulTrigger SoulTrigger => _soulTriggerList.Count > 0 ? _soulTriggerList[0] : null;
    public int SoulPixelLayer => Mathf.RoundToInt(Mathf.Log(SoulPixelLayerMask, 2));

    protected override void Awake()
    {
        base.Awake();
        _collider2D = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        if (_soulTriggerList.Count > 0)
        {
            _closestTriggerSqrDistance = float.PositiveInfinity;
            foreach (var soulTrigger in _soulTriggerList)
            {
                if ((CurrentPosition - soulTrigger.ControllableObject.GetSoulTargetPoint(false, CurrentPosition.x)).sqrMagnitude < _closestTriggerSqrDistance)
                {
                    _closestTriggerSqrDistance = (CurrentPosition - soulTrigger.ControllableObject.GetSoulTargetPoint(false, CurrentPosition.x)).sqrMagnitude;
                    _closestTrigger = soulTrigger;
                }
            }

            if (Controllers.Input.ControllablePlayer == CurrentPlayer)
            {
                Rb.AddForce(2f * (_closestTrigger.ControllableObject.GetSoulTargetPoint(false, CurrentPosition.x) - CurrentPosition));
            }
            
            if (IsVisible)
            {
                EmitPixelTowardsObject(_closestTrigger.ControllableObject, 1f);
                EmitPixelTowardsObject(_closestTrigger.ControllableObject, 0.5f + Random.value / 2f);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var soulTrigger = collision.GetComponent<SoulTrigger>();
        if (soulTrigger != null && !_soulTriggerList.Contains(soulTrigger))
        {
            _soulTriggerList.Add(soulTrigger);
            if (_hasPlayer && CurrentPlayer == Controllers.Input.ControllablePlayer)
            {
                UpdateKeyFunction();
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        var soulTrigger = collision.GetComponent<SoulTrigger>();
        if (soulTrigger != null && _soulTriggerList.Contains(soulTrigger))
        {
            _soulTriggerList.Remove(soulTrigger);
            if (_hasPlayer && CurrentPlayer == Controllers.Input.ControllablePlayer)
            {
                UpdateKeyFunction();
            }
        }
    }
    
    private void EmitPixelTowardsObject(ControllableObject controllableObject, float duration)
    {
        var pixel = References.Prefabs.GetSoulPixel();
        var targetPoint = controllableObject.GetSoulTargetPoint(true, CurrentPosition.x);
        var centerPoint = CurrentPosition + (targetPoint - CurrentPosition) / 2f + Random.insideUnitCircle / 2f;
        pixel.SetAlpha(0f);
        pixel.SetScale(0.5f);
        if (_hasPlayer) pixel.SetColor(CurrentPlayer.SoulGradient.Evaluate(Random.value));
        pixel.Fade(1f, duration / 2f);
        pixel.Fade(0f, duration / 2f, duration / 2f);
        pixel.SetPosition(CurrentPosition);
        pixel.CreateBezier(CurrentPosition, centerPoint, targetPoint);
        pixel.MoveOnBezier(duration);
        pixel.Return(duration + 0.125f);
    }

    private void UpdateKeyFunction()
    {
        if (_soulTriggerList.Count == 0)
        {
            References.Events.ChangeKeyFunction(4, "");
        }
        else
        {
            switch (_soulTriggerList[0].ControllableObject.ObjectType)
            {
                case Type.Cloud:
                    References.Events.ChangeKeyFunction(4, References.Io.GetData().msgEnactCloud);
                    break;
                case Type.Ground:
                    References.Events.ChangeKeyFunction(4, References.Io.GetData().msgEnactGround);
                    break;
                case Type.Cart:
                    References.Events.ChangeKeyFunction(4, References.Io.GetData().msgEnactCart);
                    break;
                case Type.Handcar:
                    References.Events.ChangeKeyFunction(4, References.Io.GetData().msgEnactHandcar);
                    break;
            }
        }
    }
    
    public override void Enter(Player player)
    {
        base.Enter(player);
        _hasPlayer = true;
        if (IsVisible)
        {
            EmitPixels();
        }
    }

    public override void Leave(Player player)
    {
        base.Leave(player);
        _hasPlayer = false;
    }

    private void EmitPixels()
    {
        _emitPixelsId = Utility.AddOne(_emitPixelsId);
        StartCoroutine(EmitPixelsRoutine(_emitPixelsId));
    }

    private void StopEmittingPixels()
    {
        _emitPixelsId = Utility.AddOne(_emitPixelsId);
    }

    public override void ProcessInput(InputController.Type inputType, InputController.Mode inputMode, float x = 0f, float y = 0f)
    {
        base.ProcessInput(inputType, inputMode);
        switch (inputType)
        {
            case InputController.Type.Left:
                ProcessInput(InputController.Type.Direction, inputMode, -1f, 0f);
                return;
            case InputController.Type.Right:
                ProcessInput(InputController.Type.Direction, inputMode, 1f, 0f);
                return;
            case InputController.Type.Up:
                ProcessInput(InputController.Type.Direction, inputMode, 0f, 1f);
                return;
            case InputController.Type.Down:
                ProcessInput(InputController.Type.Direction, inputMode, 0f, -1f);
                return;
            case InputController.Type.Direction:
                Direction.x = x;
                Direction.y = y;
                if (Direction.sqrMagnitude > 1f)
                {
                    Direction = Direction.normalized;
                }
                Move(Direction);
                break;
        }
    }

    public override void PerformSoulTransfer(Player player, ControllableObject target)
    {
        base.PerformSoulTransfer(player, target);
        if (target.ObjectType != Type.Soul)
        {
            Follow(player, target);
        }
    }

    private void Follow(Player player, ControllableObject target)
    {
        _animatedObject = target;
        _followPlayerId = Utility.AddOne(_followPlayerId);
        StartCoroutine(FollowRoutine(player, target.Tf, _followPlayerId));
    }

    public void MoveToTarget(Player player, ControllableObject target, bool kickOutPlayer)
    {
        _autoMoveId = Utility.AddOne(_autoMoveId);
        StartCoroutine(MoveRoutine(player, target, kickOutPlayer, _autoMoveId));
    }

    public override Vector2 GetSoulTargetPoint(bool addRandom, float xOffset = 0f)
    {
        return CurrentPosition + (addRandom ? Random.insideUnitCircle * 0.375f : Vector2.zero);
    }

    public override void Show(float duration)
    {
        base.Show(duration);
        EmitPixels();
    }

    public override void Hide(float duration)
    {
        base.Hide(duration);
        StopEmittingPixels();
    }

    public void MoveToPosition(Vector2 position)
    {
        _autoMoveId = Utility.AddOne(_autoMoveId);
        StartCoroutine(MoveToPositionRoutine(position, _autoMoveId));
    }

    private IEnumerator EmitPixelsRoutine(int id)
    {
        _emitPixelsTimer = 0f;
        _pixelCount = 0;
            
        while (_hasPlayer && _emitPixelsId == id)
        {
            CurrentPosition = Tf.position;

            _emitPixelsTimer += Time.deltaTime;
            
            while (_pixelCount < Mathf.Min(_emitPixelsTimer * PixelsPerSecond, 600f))
            {
                _pixel = References.Prefabs.GetSoulPixel();
                _currentSpeed = Rb.velocity.sqrMagnitude / (MoveSpeed * MoveSpeed);
                _randomPosition = Random.insideUnitCircle * Mathf.Lerp(0.5f, 0.125f, _currentSpeed);
                _randomPosition.x = Mathf.Round(_randomPosition.x * 32f) / 32f;
                _randomPosition.y = Mathf.Round(_randomPosition.y * 32f) / 32f;
                _pixel.SetPosition(CurrentPosition + _randomPosition);
                if (CurrentPlayer != null)
                {
                    _pixel.SetColor(CurrentPlayer.SoulGradient.Evaluate(_randomPosition.sqrMagnitude * 4f));
                }
                _pixel.SetAlpha(0f);
               // _pixel.SetLayer(SoulPixelLayer);
                _pixel.SetSpriteSortingOrder(63 + _sortingOrderOffset);
                _pixel.SetScale(Mathf.Lerp(3f, 1.25f, _randomPosition.sqrMagnitude * 4f));
                _pixel.Fade(1f, 0.25f);
                _pixel.Fade(0f, 1f, 0.25f);
                _pixel.Return(1.375f);

                _pixelCount++;
                _sortingOrderOffset = Utility.AddOne(_sortingOrderOffset);
            }

            if (_emitPixelsTimer > 1f)
            {
                _emitPixelsTimer = 0f;
                _pixelCount = 0;
            }
            
            yield return null;
        }

        yield return null;
    }

    private IEnumerator MoveRoutine(Player player, ControllableObject target, bool kickOutPlayer, int id)
    {
        while (!_hasPlayer && _autoMoveId == id)
        {
            player.ReturnToSoul();
            yield return null;
        }
        _transferTimer = 0f;
        
        if (target.ObjectType == Type.Soul) yield break;

        var sqrDistance = (CurrentPosition - target.CurrentPosition).sqrMagnitude;
        while (sqrDistance > 1f && _autoMoveId == id)
        {
            sqrDistance = (CurrentPosition - target.GetSoulTargetPoint(true, CurrentPosition.x)).sqrMagnitude;
            var xDifference = Mathf.Abs(CurrentPosition.x - target.GetSoulTargetPoint(true, CurrentPosition.x).x);
            var yDifference = Mathf.Abs(CurrentPosition.y - target.GetSoulTargetPoint(true, CurrentPosition.y).y);
            if (CurrentPosition.x < target.GetSoulTargetPoint(true, CurrentPosition.x).x)
            {
                MoveRight(Mathf.InverseLerp(0f, 10f, xDifference));
            }
            else
            {
                MoveLeft(Mathf.InverseLerp(0f, 10f, xDifference));
            }

            if (CurrentPosition.y < target.GetSoulTargetPoint(true, CurrentPosition.y).y)
            {
                MoveUp(Mathf.InverseLerp(0f, 10f, yDifference));
            }
            else
            {
                MoveDown(Mathf.InverseLerp(0f, 10f, yDifference));
            }
            yield return null;
        }

        if (target.CurrentPlayer == null || kickOutPlayer)
        {
            player.CanTransferSoul = true;
        
            while (_hasPlayer && _autoMoveId == id)
            {
                _transferTimer += Time.deltaTime;
                player.TransferSoulTo(target);
                yield return null;
            }
            ProcessInput(InputController.Type.Space, InputController.Mode.Release);
        }

        yield return null;
    }

    private IEnumerator FollowRoutine(Player player, Transform target, int id)
    {
        _collider2D.enabled = false;
        while (!_hasPlayer && _followPlayerId == id)
        {
            if (player != null && player.CurrentType == Type.Ground)
            {

                CurrentPosition.x = player.Counterpart.GetCurrentPosition().x;
                References.Entities.Ground.CurrentPosition.x = CurrentPosition.x;
            }
            else
            {
                CurrentPosition = target.position;
            }
            
            Tf.position = CurrentPosition;
            yield return null;
        }

        _collider2D.enabled = true;
        yield return null;
    }

    private IEnumerator MoveToPositionRoutine(Vector2 position, int id)
    {
        var initialDirection = Direction;
        var sqrDistance = (CurrentPosition - position).sqrMagnitude;
        while (initialDirection == Direction && sqrDistance > 0.03125f && _autoMoveId == id)
        {
            sqrDistance = (CurrentPosition - position).sqrMagnitude;
            var xDifference = Mathf.Abs(CurrentPosition.x - position.x);
            var yDifference = Mathf.Abs(CurrentPosition.y - position.y);
            if (CurrentPosition.x < position.x)
            {
                MoveRight(Mathf.InverseLerp(0f, 10f, xDifference) * 2f);
            }
            else
            {
                MoveLeft(Mathf.InverseLerp(0f, 10f, xDifference) * 2f);
            }

            if (CurrentPosition.y < position.y)
            {
                MoveUp(Mathf.InverseLerp(0f, 10f, yDifference));
            }
            else
            {
                MoveDown(Mathf.InverseLerp(0f, 10f, yDifference));
            }
            yield return null;
        }

        yield return null;
    }
}
