using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class ControllableObject : MonoBehaviour
{
    public enum Type { None, Soul, Cloud, Ground, Handcar, Cart, All }
    public Type ObjectType;
    public Action Action;
    public ResourceCircle ResourceDisplay;
    public SpriteRenderer[] FadeRenderers;
    
    public float MoveSpeed = 1f;
    public Player CurrentPlayer;
    public Vector2 CurrentPosition;
    public SoundEffect InvalidInputSound;
    private Vector2 _previousPosition;

    protected float LastMoveInputTime;
    
    private float _baseMass;
    private float _baseLinearDrag;
    private float _moveLinearDrag;
    private float _maxLinearDrag;
    private float _currentLinearDrag;
    private float _linearDragHolder;
    private float _currentSlowdown;

    [HideInInspector]
    public Transform Tf;
    protected Rigidbody2D Rb;
    protected SpriteRenderer Sr;
    private SoulTrigger _soulTrigger;

    [SerializeField]
    protected bool IsMoving;
    protected Vector2 Direction;
    private Vector2 _lastMoveDirection;

    private Color _currentColor = Color.white;
    private float _changeAlphaTimer;
    private float _currentAlpha = 1f;
    private float _alphaHolder;
    private int _changeAlphaId;
    private int[] _simulateInputIds;
    private bool[] _isSimulatingInput;
    private int _moveId;
    
    private const float SectorSize = 1f;
    private Vector2Int _currentSector;
    private Vector2Int _previousSector;
    private float _lastSectorChangeTime;

    private float _beginMoveTime;
    private float _moveBuffer;

    private float _increaseFrictionTimer;
    private int _increaseFrictionId;

    private readonly List<InputInfo> _simulatedInputList = new List<InputInfo>();
    private int _simulationIndex;
    private bool _loopSimulation;
    private int _continuouslySimulatedInputId;

    private float _activationTimer;

    protected int ArtificialMotivationId;
    private float _collectNotesTimer;
    private float _waitTimer;
    protected Vector2 NoteCount;
    protected InputController.Type CurrentCollectDirection;
    private InputController.Type _previousCollectDirection;

    private bool _isVisible = true;

    public Rigidbody2D Rigidbody2D => Rb;
    public virtual bool IsRolling => false;
    public bool IsVisible => _isVisible;
    public Vector2 LastMoveDirection => _lastMoveDirection;
    public float CurrentSlowdown => _currentSlowdown;

    protected virtual void Awake()
    {
        Tf = transform;
        CurrentPosition = Tf.position;
        _previousPosition = CurrentPosition;
        DetermineCurrentSector();
        _previousSector = _currentSector;
        
        _simulateInputIds = new int[9];
        _isSimulatingInput = new bool[9];

        _lastMoveDirection = Vector2.right;
        Sr = GetComponent<SpriteRenderer>();
        Rb = GetComponent<Rigidbody2D>();
        _soulTrigger = GetComponentInChildren<SoulTrigger>();

        _isVisible = true;
        
        if (Rb)
        {
            _baseMass = Rb.mass;
            _baseLinearDrag = Rb.drag;
            _maxLinearDrag = _baseLinearDrag + Rb.mass * 20f;
            _currentLinearDrag = _baseLinearDrag;
            ChangeMoveLinearDrag(0f);
        }

        if (Sr)
        {
            _currentColor = Sr.color;
            _currentAlpha = _currentColor.a;
        }

        if (Action != null)
        {
            Action.Link(this);
            Action.Init();
        }
    }

    public virtual void Show(float duration)
    {
        if (_soulTrigger)
        {
            _soulTrigger.SetActive(true);
        }
        
        ChangeAlpha(1f, duration);
        References.Events.ActivateControllableObject(ObjectType, true);
        _isVisible = true;
    }

    public virtual void Hide(float duration)
    {
        if (CurrentPlayer != null)
        {
            KickOut(CurrentPlayer);
        }

        if (_soulTrigger)
        {
            _soulTrigger.SetActive(false);
        }
        
        ChangeAlpha(0f, duration);
        References.Events.ActivateControllableObject(ObjectType, false);
        _isVisible = false;
    }

    public void SetSoulTriggerActive(bool isActive)
    {
        _soulTrigger.SetActive(isActive);
    }

    public virtual void ProcessInput(InputController.Type inputType, InputController.Mode inputMode, float x = 0f, float y = 0f)
    {
        switch (inputType)
        {
            case InputController.Type.Space:
                if (inputMode == InputController.Mode.Press)
                {
                    if (Action != null && !Action.IsPerformingAction())
                    {
                        Action.TryToPerform();
                    }
                }
                else if (inputMode == InputController.Mode.Release)
                {
                    if (Action != null && Action.IsPerformingAction())
                    {
                        Action.Stop();
                    }
                }
                break;
        }
    }

    public virtual void Enter(Player player)
    {
        if (CurrentPlayer != null && ObjectType != Type.Soul)
        {
            CurrentPlayer.ReturnToSoul(true);
        }

        if (ObjectType != Type.Soul)
        {
            References.Terminal.AddEntry(player.PlayerName + " animated " + name + ".");
        }
        else
        {
            References.Terminal.AddEntry(player.PlayerName + " returned to soul form.");
        }
        
        CurrentPlayer = player;
        player.Enter(this);

        if (ResourceDisplay != null)
        {
            ResourceDisplay.SetColor(player.Color);
        }
    }

    public virtual void Leave(Player player)
    {
        if (CurrentPlayer == player)
        {
            CurrentPlayer = null;
            
            if (ResourceDisplay != null)
            {
                ResourceDisplay.ResetColor();
            }
        }

        if (Action != null && Action.IsPerformingAction())
        {
            Action.Stop();
        }
    }

    public virtual void SoulPixelImpact(Pixel pixel)
    {
        pixel.OnHasReachedTarget -= SoulPixelImpact;
    }

    public virtual void PerformSoulTransfer(Player player, ControllableObject target)
    {
        Leave(player);
        target.Enter(player);
        player.CancelSoulTransfer(target.ObjectType);
    }

    public virtual Vector2 GetSoulTargetPoint(bool addRandom, float xOffset = 0f)
    {
        return CurrentPosition;
    }

    protected void Snap()
    {
        CurrentPosition = Utility.Snap(CurrentPosition);
    }

    protected virtual void OnStopMoving()
    {
        
    }

    public void UpdateSlowdown(float slowdown)
    {
        _currentSlowdown = slowdown * 100f;
        UpdateMoveSpeed();
    }

    private void UpdateMoveSpeed()
    {
        Rb.drag = _currentLinearDrag + _currentSlowdown;
    }
    
    protected virtual void MoveLeft(float intensity = 1f)
    {
        AddForce(Vector2.left, MoveSpeed * 100f * intensity * _baseMass * Time.deltaTime);
    }

    protected virtual void MoveRight(float intensity = 1f)
    {
        AddForce(Vector2.right, MoveSpeed * 100f * intensity * _baseMass * Time.deltaTime);
    }
    
    protected virtual void MoveUp(float intensity = 1f)
    {
        AddForce(Vector2.up, MoveSpeed * 100f * intensity * _baseMass * Time.deltaTime);
    }
    
    protected virtual void MoveDown(float intensity = 1f)
    {
        AddForce(Vector2.down, MoveSpeed * 100f * intensity * _baseMass * Time.deltaTime);
    }

    protected void Move(Vector2 direction, float intensity = 1f)
    {
        AddForce(direction, MoveSpeed * 100f * intensity * _baseMass * Time.deltaTime);
    }

    public void MoveInLastDirection(float intensity)
    {
        AddForce(_lastMoveDirection, MoveSpeed * 100f * intensity * _baseMass * Time.deltaTime);
    }
    
    protected void AddForce(Vector2 direction, float amount)
    {
        Rb.AddForce(direction * amount);
        _lastMoveDirection = direction.normalized;
        if (!IsMoving)
        {
           InitiateMovement();
        }

        if (ObjectType != Type.Cart)
        {
            IncreaseFriction(0.125f, 0.25f);
        }
    }

    protected void AddRelativeForce(Vector2 direction, float amount)
    {
        Rb.AddRelativeForce(direction * amount);
        _lastMoveDirection = direction.normalized;
        if (!IsMoving)
        {
            InitiateMovement();
        }
    }

    protected void InitiateMovement()
    {
        IsMoving = true;
        _moveId = Controllers.Logs.RequestActionId();
        _beginMoveTime = Time.timeSinceLevelLoad;
        StartCoroutine(WhileMovingRoutine(_moveId));
    }

    protected void ChangeAlpha(float targetAlpha, float duration)
    {
        _changeAlphaId++;
        StartCoroutine(ChangeAlphaRoutine(targetAlpha, duration, _changeAlphaId));
    }

    private void ApplyCurrentAlpha()
    {
        _currentColor.a = _currentAlpha;
        foreach (var sr in FadeRenderers)
        {
            sr.color = _currentColor;
        }
    }

    public void Follow(ControllableObject target, float proximityLimit = 0.5f)
    {
        ArtificialMotivationId = Utility.AddOne(ArtificialMotivationId);
        if (target == null)
        {
            return;
        }
        StartCoroutine(FollowRoutine(target, 0.75f, Mathf.Max(proximityLimit, 0.25f), ArtificialMotivationId));
    }

    public void CollectNotes()
    {
        if (CurrentPlayer == null)
        {
            return;
        }
        ArtificialMotivationId = Utility.AddOne(ArtificialMotivationId);
        StartCoroutine(CollectNotesRoutine(CurrentPlayer.ColorId, 1f, ArtificialMotivationId));
    }

    public virtual void StopArtificialMovement()
    {
        ArtificialMotivationId = Utility.AddOne(ArtificialMotivationId);
    }

    public void AddSimulatedInputToList(InputInfo inputInfo)
    {
        _simulatedInputList.Add(inputInfo);
    }

    public void ResetSimulatedInputList()
    {
        _simulatedInputList.Clear();
    }

    public void ProcessSimulatedInputList(bool loop)
    {
        _loopSimulation = loop;
        _continuouslySimulatedInputId = Utility.AddOne(_continuouslySimulatedInputId);
        StartCoroutine(ContinuouslySimulateInput(_continuouslySimulatedInputId));
    }

    public void StopSimulatedInputList()
    {
        _continuouslySimulatedInputId = Utility.AddOne(_continuouslySimulatedInputId);
    }

    public void SimulateInput(InputController.Type inputType, InputController.Mode inputMode, float value = -1f)
    {
        _simulateInputIds[(int)inputType] = Utility.AddOne(_simulateInputIds[(int)inputType]);
        _isSimulatingInput[(int)inputType] = true;
        switch (inputType)
        {
            case InputController.Type.Left:
                if (_isSimulatingInput[(int) InputController.Type.Right])
                {
                    StopInputSimulation((int) InputController.Type.Right);
                }
                break;
            case InputController.Type.Up:
                if (_isSimulatingInput[(int) InputController.Type.Down])
                {
                    StopInputSimulation((int) InputController.Type.Down);
                }
                break;
            case InputController.Type.Right:
                if (_isSimulatingInput[(int) InputController.Type.Left])
                {
                    StopInputSimulation((int) InputController.Type.Left);
                }
                break;
            case InputController.Type.Down:
                if (_isSimulatingInput[(int) InputController.Type.Up])
                {
                    StopInputSimulation((int) InputController.Type.Up);
                }
                break;
        }
        StartCoroutine(SimulateInputRoutine(inputType, inputMode, value, _simulateInputIds[(int)inputType]));
    }

    public void StopInputSimulation(int inputTypeIndex)
    {
        if (inputTypeIndex < 0)
        {
            for (var i = 0; i < _isSimulatingInput.Length; i++)
            {
                _isSimulatingInput[i] = false;
            }
            return;
        }
        _isSimulatingInput[inputTypeIndex] = false;
    }

    public void KickOut(Player player)
    {
        StartCoroutine(KickOutRoutine(player));
    }

    public void HoldPosition(ControllableObject targetObject)
    {
        ArtificialMotivationId = Utility.AddOne(ArtificialMotivationId);
        if (targetObject != null)
        {
            StartCoroutine(HoldPositionRoutine(targetObject, 2f, ArtificialMotivationId));
        }
    }

    private void DetermineCurrentSector()
    {
        _currentSector.x = Mathf.RoundToInt(CurrentPosition.x);
        _currentSector.y = Mathf.RoundToInt(CurrentPosition.y);
    }

    protected void CheckForSectorChange()
    {
        UpdateCurrentPosition();
        _previousSector = _currentSector;
        DetermineCurrentSector();
        if (_previousSector != _currentSector)
        {
            if (CurrentPlayer != null)
            {
                var sector = string.Concat('[', _currentSector.x, ',', _currentSector.y, ']');
                Controllers.Logs.AddLog(_beginMoveTime, CurrentPlayer, Log.Action.Move, _moveId, Log.Ending.Natural, sector);
                _beginMoveTime = Time.timeSinceLevelLoad;
            }
            _lastSectorChangeTime = Time.timeSinceLevelLoad;
        }
    }

    protected virtual void UpdateCurrentPosition(bool isRecursive = true)
    {
        _previousPosition = CurrentPosition;
        CurrentPosition = Tf.position;
    }

    private void IncreaseFriction(float delay, float duration)
    {
        _increaseFrictionId = Utility.AddOne(_increaseFrictionId);
        StartCoroutine(IncreaseFrictionRoutine(delay, duration, _increaseFrictionId));
    }

    protected void ChangeMoveLinearDrag(float additionalDrag)
    {
        _moveLinearDrag = _baseLinearDrag + additionalDrag;
    }

    public float TimeSinceLastSectorChange()
    {
        return Time.timeSinceLevelLoad - _lastSectorChangeTime;
    }

    public virtual Vector2 PredictPosition(float time)
    {
        return CurrentPosition + (CurrentPosition - _previousPosition) * (time / Time.deltaTime);
    }

    private IEnumerator ChangeAlphaRoutine(float targetAlpha, float duration, int id)
    {
        _changeAlphaTimer = 0f;
        _alphaHolder = _currentAlpha;
        
        while (_changeAlphaTimer < duration)
        {
            _changeAlphaTimer += Time.deltaTime;
            _currentAlpha = Mathf.Lerp(_alphaHolder, targetAlpha, _changeAlphaTimer / duration);
            ApplyCurrentAlpha();
            yield return null;
        }

        if (_changeAlphaId == id)
        {
            _currentAlpha = targetAlpha;
            ApplyCurrentAlpha();
        }
        
        yield return null;
    }

    private IEnumerator KickOutRoutine(Player player)
    {
        player.CanTransferSoul = true;
        while (player.CanTransferSoul)
        {
            player.TransferSoulTo(player.Soul);
            yield return null;
        }
        
        yield return null;
        
        player.CanTransferSoul = true;
    }

    private IEnumerator SimulateInputRoutine(InputController.Type inputType, InputController.Mode inputMode, float value, int id)
    {
        // References.Terminal.AddEntry("<red>Start simulation: " + inputType + " with id " + id + ".</>");
        var positionHolder = CurrentPosition;
        var timer = 0f;
        ProcessInput(inputType, InputController.Mode.Press);
        yield return null;
        if (inputMode == InputController.Mode.Hold)
        {
            while (_isSimulatingInput[(int)inputType] && _simulateInputIds[(int)inputType] == id)
            {
                timer += Time.deltaTime;
                ProcessInput(inputType, inputMode);
                if (inputType != InputController.Type.Space)
                {
                    _previousPosition = CurrentPosition;
                    CurrentPosition = Tf.position;
                    // stop simulation after reaching a certain (squared) distance
                    if (value > 0f && (positionHolder - CurrentPosition).sqrMagnitude >= value * value)
                    {
                        StopInputSimulation((int)inputType);
                    }
                }
                else
                {
                    // stop simulation after the expiry of a certain time or if the object runs out of resources
                    if (value > 0f && timer >= value || !Action.CanPerformAction())
                    {
                        StopInputSimulation((int)inputType);
                    }
                }
                
                yield return null;
            }
        }
        ProcessInput(inputType, InputController.Mode.Release);
        // References.Terminal.AddEntry("<green>Stop simulation: " + inputType + " with id " + id + ".</>");
        yield return null;
    }

    private IEnumerator ContinuouslySimulateInput(int id)
    {
        while (_loopSimulation && _continuouslySimulatedInputId == id)
        {
            _simulationIndex = 0;
            while (_simulationIndex < _simulatedInputList.Count && _continuouslySimulatedInputId == id)
            {
                var input = _simulatedInputList[_simulationIndex];
                if (input.Delay > 0f)
                {
                    yield return new WaitForSeconds(input.Delay);
                }

                if (_continuouslySimulatedInputId == id)
                {
                    SimulateInput(input.InputType, InputController.Mode.Hold, input.Distance);
                }
                
                if (input.Duration > 0f)
                {
                    yield return new WaitForSeconds(input.Duration);
                }

                _simulationIndex++;
            }

            yield return null;
        }
    }

    private IEnumerator FollowRoutine(ControllableObject target, float inertia, float proximityLimit, int id)
    {
        while (target.ObjectType != Type.Ground && ArtificialMotivationId == id)
        {
            if (CurrentPosition.x < target.CurrentPosition.x - proximityLimit)
            {
                SimulateInput(InputController.Type.Right, InputController.Mode.Hold, 0.5f);
            }
            else if (CurrentPosition.x > target.CurrentPosition.x + proximityLimit)
            {
                SimulateInput(InputController.Type.Left, InputController.Mode.Hold, 0.5f);
            }
            else
            {
                yield return new WaitForSeconds(Random.value * inertia);
            }
            yield return new WaitForSeconds(0.25f);
        }

        yield return null;
    }

    private IEnumerator CollectNotesRoutine(int colorId, float decisionInterval, int id)
    {
        UpdateNoteCount(colorId);
        _previousCollectDirection = InputController.Type.Up;
        
        // wait one frame so the stop input simulation calls at the end of the previous routine don't override the new inputs
        yield return null;
        
        while (CurrentPlayer != null && ArtificialMotivationId == id)
        {
            _collectNotesTimer += 0.25f;
            if (CurrentCollectDirection != _previousCollectDirection)
            {
                SimulateInput(CurrentCollectDirection, InputController.Mode.Hold);
                _previousCollectDirection = CurrentCollectDirection;
            }
            
            if (_collectNotesTimer >= decisionInterval)
            {
                _collectNotesTimer = 0f;
                UpdateNoteCount(colorId);
                _waitTimer = Random.value * 1.5f;
                while (_waitTimer > 0f && ArtificialMotivationId == id)
                {
                    _waitTimer -= Time.deltaTime;
                    yield return null;
                }
            }

            _waitTimer = 0.25f;
            while (_waitTimer > 0f && ArtificialMotivationId == id)
            {
                _waitTimer -= Time.deltaTime;
                yield return null;
            }
        }
        StopInputSimulation(0);
        StopInputSimulation(2);

        yield return null;
    }

    protected virtual void UpdateNoteCount(int colorId)
    {
        // ground overrides this function!
        NoteCount = Controllers.Level.GetNoteValuesOnEitherSide(CurrentPosition.x, colorId);
        if (_lastMoveDirection.x < 0f)
        {
            NoteCount.x *= 2.5f;
        }
        else if (_lastMoveDirection.x > 0f)
        {
            NoteCount.y *= 2.5f;
        }
        
        var results = Vector2Int.zero;
        for (var i = 0; i < 3; i++)
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

    private IEnumerator WhileMovingRoutine(int id)
    {
        _moveBuffer = 0f;
        while (IsMoving && _moveId == id)
        {
            //_lastMoveDirection = (CurrentPosition - _lastPosition).normalized;
            CheckForSectorChange();
            if (Rb.velocity.sqrMagnitude < 0.01f || CurrentPlayer == null)
            {
                _moveBuffer += Time.deltaTime;
                if (_moveBuffer > 0.5f)
                {
                    IsMoving = false;
                    OnStopMoving();
                }
            }
            else
            {
                _moveBuffer = 0f;
            }
            yield return null;
        }

        yield return null;
    }

    private IEnumerator HoldPositionRoutine(ControllableObject targetObject, float timeInterval, int id)
    {
        while (ArtificialMotivationId == id)
        {
            yield return new WaitForSeconds(timeInterval);
            if (ArtificialMotivationId == id && CurrentPlayer != null && targetObject.CurrentPlayer == null)
            {
                CurrentPlayer.MoveToTarget(targetObject);
            }
        }
    }

    private IEnumerator IncreaseFrictionRoutine(float delay, float duration, int id)
    {
        _currentLinearDrag = _moveLinearDrag;
        _linearDragHolder = _currentLinearDrag;
        _increaseFrictionTimer = 0f;
        
        UpdateMoveSpeed();

        yield return new WaitForSeconds(delay);

        while (_increaseFrictionTimer < duration && _increaseFrictionId == id)
        {
            _increaseFrictionTimer += Time.deltaTime;
            _currentLinearDrag = Mathf.Lerp(_linearDragHolder, _maxLinearDrag, _increaseFrictionTimer / duration);
            UpdateMoveSpeed();
            yield return null;
        }

        yield return null;
    }
}
