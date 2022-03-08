using System.Collections;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    private readonly int _colorHash = Animator.StringToHash("_Color");
    private readonly int _cycleOffsetHash = Animator.StringToHash("_CycleOffset");

    public Transform ModelTf;
    
    private Transform _tf;
    private SpriteRenderer _sr;
    private Animator _animator;
    private PickUpTrigger _pickUpTrigger;
    private VehicleTrigger _vehicleTrigger;
    private RainTrigger[] _rainTriggers;
    
    private Vector3 _currentEulerRotation;
    private Vector2 _currentLocalPosition;
    private Vector2 _currentPosition;
    private float _currentEulerAngle;
    private float _currentYPosition;
    private float _currentYOffset;
    private int _colorId;
    
    // fading
    private Color _currentColor;
    private float _changeAlphaTimer;
    private float _currentAlpha;
    private float _alphaHolder;
    private int _changeAlphaId;
    private bool _isVisible;

    // scaling
    private float _scaleTimer;
    private float _currentScale;
    private float _scaleHolder;
    private int _scaleId;
    
    private const float MaxAngle = 5f;
    private const float SwaySpeed = 0.75f;
    private const float MaxHoverDistance = 0.125f;
    private const float HoverSpeed = 0.5f;
    private float _maxAlpha;
    private bool _isSwaying;
    private bool _isHovering;
    private bool _hasBeenPickedUp;
    private bool _isVehicleNear;

    private const int OwnValue = 50;
    private const int OtherValue = 10;
    private int _sparkleModifier = 1;
    private float _value;
    private bool _isSparkling;
    private float _sparkleStartTime;
    private Player _playerInCloud;

    public Vector2 Position => _currentPosition;
    public bool IsSparkling => _sparkleModifier > 1;
    public bool HasBeenPickedUp => _hasBeenPickedUp;
    public int ColorId => _colorId;

    private void Awake()
    {
        _tf = transform;
        _sr = GetComponentInChildren<SpriteRenderer>();
        _animator = GetComponentInChildren<Animator>();
        _pickUpTrigger = GetComponentInChildren<PickUpTrigger>();
        _vehicleTrigger = GetComponentInChildren<VehicleTrigger>();
        _rainTriggers = GetComponentsInChildren<RainTrigger>();
        _currentLocalPosition = ModelTf.localPosition;
        _currentPosition = ModelTf.position;
        _currentYPosition = _currentLocalPosition.y;
        _currentColor = _sr.color;
        _isVisible = true;
        SetColor(-1);
        SetCycleOffset(Random.value);
        StartCoroutine(SwayRoutine());
        StartCoroutine(HoverRoutine());
        Scale(0f, 0f);
        Scale(1f, 2f);
    }

    private void OnEnable()
    {
        _pickUpTrigger.OnCollect += OnCollect;
        _vehicleTrigger.OnAgentEnter += OnVehicleEnter;
        _vehicleTrigger.OnAgentExit += OnVehicleExit;
        References.Events.OnChangeNoteVisibility += ChangeVisibility;
    }

    private void OnDisable()
    {
        _pickUpTrigger.OnCollect -= OnCollect;
        _vehicleTrigger.OnAgentEnter -= OnVehicleEnter;
        _vehicleTrigger.OnAgentExit -= OnVehicleExit;
        References.Events.OnChangeNoteVisibility -= ChangeVisibility;
    }

    private void OnCollect(Player player)
    {
        if (_hasBeenPickedUp || player == null || _colorId == -1) return;
        var pixelExplosion = References.Prefabs.GetPixelExplosion();
        pixelExplosion.SetPosition(ModelTf.position);
        pixelExplosion.SetColor(_colorId);
        pixelExplosion.Explode();
        _hasBeenPickedUp = true;
        Scale(0f, 0f);
        foreach (var rainTrigger in _rainTriggers)
        {
            rainTrigger.SetActive(false);
        }
        StartCoroutine(RefreshRoutine(2f));
        References.Events.CollectNote(player.ColorId == _colorId ? player.PlayerId : player.Counterpart.PlayerId);
        Controllers.Logs.AddLog(Time.timeSinceLevelLoad, player, Log.Action.TouchNote, Controllers.Logs.RequestActionId(), Log.Ending.Natural, ColorToString());
    }

    private void OnVehicleEnter()
    {
        _isVehicleNear = true;
    }

    private void OnVehicleExit()
    {
        _isVehicleNear = false;
    }

    public float GetValue(int colorId)
    {
        _value = (_colorId == colorId ? OwnValue : OtherValue) * _sparkleModifier;
        if (_hasBeenPickedUp)
        {
            _value /= 4f;
        }

        return _value;
    }

    public void FlagAsSparkling(bool isSparkling)
    {
        if (_isSparkling == isSparkling) return;
        _isSparkling = isSparkling;
        _sparkleModifier = isSparkling ? 100 : 1;
        if (isSparkling)
        {
            _sparkleStartTime = References.Timer.GetLevelTime();
            _playerInCloud = References.Entities.Cloud.CurrentPlayer;
        }
        else
        {
            Controllers.Logs.AddLog(_sparkleStartTime, _playerInCloud, Log.Action.RainOnNote, Controllers.Logs.RequestActionId(), _hasBeenPickedUp ? Log.Ending.Collect : Log.Ending.Natural, ColorToString());
        }
    }

    public void ChangeVisibility(bool isVisible, float duration)
    {
        _isVisible = isVisible;
        _pickUpTrigger.SetActive(_isVisible);
        _vehicleTrigger.SetActive(_isVisible);
        foreach (var rainTrigger in _rainTriggers)
        {
            rainTrigger.SetActive(isVisible);
        }
        ChangeAlpha(_isVisible ? _maxAlpha : 0f, duration);
    }

    public void Scale(float toScale, float duration)
    {
        _scaleId++;
        StartCoroutine(ScaleRoutine(toScale, duration, _scaleId));
    }

    private void ChangeAlpha(float targetAlpha, float duration)
    {
        _changeAlphaId = Utility.AddOne(_changeAlphaId);
        StartCoroutine(ChangeAlphaRoutine(targetAlpha, duration, _changeAlphaId));
    }

    private void ApplyCurrentRotation()
    {
        _currentEulerRotation.z = _currentEulerAngle;
        ModelTf.localRotation = Quaternion.Euler(_currentEulerRotation);
    }

    private void ApplyCurrentLocalPosition()
    {
        _currentLocalPosition.y = _currentYPosition + _currentYOffset;
        ModelTf.localPosition = _currentLocalPosition;
    }

    private void ApplyCurrentScale()
    {
        ModelTf.localScale = _currentScale * Vector3.one;
    }
    
    private void ApplyCurrentAlpha()
    {
        _currentColor.a = _currentAlpha;
        _sr.color = _currentColor;
    }

    public void SetColor(int colorId)
    {
        if (_colorId == colorId) return;
        
        if (_colorId == -1)
        {
            _maxAlpha = 1f;
        } 
        else if (colorId == -1)
        {
            _maxAlpha = 0.5f;
        }

        if (_isVisible)
        {
            ChangeAlpha(_maxAlpha, 0.25f);
        }
        
        _colorId = colorId;
        _animator.SetInteger(_colorHash, _colorId);
        
    }

    private string ColorToString()
    {
        switch (_colorId)
        {
            case 0:
                return "Red Note";
            case 1:
                return "Orange Note";
            case 2:
                return "Yellow Note";
            case 3:
                return "Green Note";
            case 4:
                return "Blue Note";
            case 5:
                return "Purple Note";
            default:
                return "N/A";
        }
    }

    private void SetCycleOffset(float offset)
    {
        _animator.SetFloat(_cycleOffsetHash, offset);
    }

    private IEnumerator ChangeAlphaRoutine(float targetAlpha, float duration, int id)
    {
        _changeAlphaTimer = 0f;
        _alphaHolder = _currentAlpha;
        while (_changeAlphaTimer < duration && _changeAlphaId == id)
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
    
    private IEnumerator SwayRoutine()
    {
        var timer = _currentLocalPosition.x / 2f;
        _isSwaying = true;

        while (_isSwaying)
        {
            timer += Time.deltaTime;
            _currentEulerAngle = Mathf.Sin(timer * Mathf.PI * SwaySpeed) * MaxAngle;
            ApplyCurrentRotation();
            yield return null;
        }
        yield return null;
    }

    private IEnumerator HoverRoutine()
    {
        var timer = _currentLocalPosition.x / 2f;
        _isHovering = true;

        while (_isHovering)
        {
            timer += Time.deltaTime;
            _currentYOffset = Mathf.Sin(timer * Mathf.PI * HoverSpeed) * MaxHoverDistance;
            ApplyCurrentLocalPosition();
            yield return null;
        }
        
        yield return null;
    }

    private IEnumerator ScaleRoutine(float toScale, float duration, int id)
    {
        _scaleTimer = 0f;
        _scaleHolder = _currentScale;
        while (_scaleTimer < duration && _scaleId == id)
        {
            _scaleTimer += Time.deltaTime;
            _currentScale = Mathf.Lerp(_scaleHolder, toScale, _scaleTimer / duration);
            ApplyCurrentScale();
            yield return null;
        }

        if (_scaleId == id)
        {
            _currentScale = toScale;
            ApplyCurrentScale();
        }
        
        yield return null;
    }

    private IEnumerator RefreshRoutine(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        while (_isVehicleNear)
        {
            yield return new WaitForSeconds(0.25f);
        }
        Scale(1f, 2f);
        yield return new WaitForSeconds(1f);
        foreach (var rainTrigger in _rainTriggers)
        {
            rainTrigger.SetActive(true);
        }
        _hasBeenPickedUp = false;
        yield return null;
    }
}
