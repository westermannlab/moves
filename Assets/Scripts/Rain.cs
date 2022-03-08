using System.Collections;
using UnityEngine;

public class Rain : Action
{
    public SoundEffect MediumRainLoop;
    public SoundEffect HeavyRainLoop;
    public SoundEffect[] LightningSounds;
    
    public enum Mode { None, Light, Heavy }
    public Mode RainMode;
    
    private const float TimeBeforeThunderbolt = 4f;
    private float _halfTime;
    
    private Raindrop _raindropHolder;
    private Lightning _lightningHolder;
    private float _rainTimer;
    private float _lightningTimer;
    private float _raindropTimer;
    private float _lightningGap = 1.5f;
    private int _raindropIndex;
    private int _raindropCount;
    private const int RaindropsPerSecond = 60;

    private Player _otherPlayer;
    private bool _otherIsHandcar;
    private bool _otherIsMoving;

    private const float ProximityLimit = 3f;

    public override void Init()
    {
        base.Init();
        _halfTime = TimeBeforeThunderbolt / 2f;
        RainMode = Mode.None;
    }

    protected override void Perform()
    {
        base.Perform();

        _otherPlayer = ParentObject.CurrentPlayer != null ? ParentObject.CurrentPlayer.Counterpart : null;
        _otherIsHandcar = _otherPlayer != null && _otherPlayer.CurrentType == ControllableObject.Type.Handcar;
        _otherIsMoving = References.Entities.Handcar.IsRolling;

        RainMode = !_otherIsHandcar || _otherIsHandcar && References.Entities.Handcar.TimeSinceLastSectorChange() > 2f
            ? Mode.Light
            : Mode.Heavy;

        References.Coroutines.StartCoroutine(RainRoutine(ActionId));
    }

    public override void Stop()
    {
        base.Stop();
        RainMode = Mode.None;
    }

    protected override void Cancel()
    {
        base.Cancel();
        RainMode = Mode.None;
    }

    protected override void Exhaustion()
    {
        base.Exhaustion();
        RainMode = Mode.None;
    }

    private void CreateRaindrop(int amount = 1, int size = 0)
    {
        for (var i = 0; i < 4; i++)
        {
            _raindropIndex = (_raindropIndex + 1) % 13;
            _raindropHolder = References.Prefabs.GetRaindrop();
            //_raindropHolder.SetParent(Tf);
            _raindropHolder.SetPosition(GetRaindropSpawnPosition(_raindropIndex));
            _raindropHolder.Drop(size);
        }
    }
    
    private Vector2 GetRaindropSpawnPosition(int index)
    {
        var xPos = Vector2.right * (-1.625f + 0.25f * index + 0.25f * Random.value);
        return (Vector2) ParentObject.Tf.position + ParentObject.Rigidbody2D.velocity / 16f + xPos + Vector2.down * Random.Range(0f, 0.5f);
    }

    private Vector2 GetLightningSpawnPosition()
    {
        return (Vector2) ParentObject.Tf.position + 0.5f * Vector2.down + ParentObject.Rigidbody2D.velocity / 4f;
    }

    private void CreateHeavyRaindrop(int amount = 1)
    {
        CreateRaindrop(amount, 1);
    }

    private void CreateThunderstormRaindrop(int amount = 1)
    {
        CreateRaindrop(amount, 2);
    }

    private void CreateLightning()
    {
        _lightningHolder = References.Prefabs.GetLightning();
        _lightningHolder.SetPosition(GetLightningSpawnPosition());
        _lightningHolder.Extend();
        Controllers.Audio.PlaySoundEffect(LightningSounds[Random.Range(0, LightningSounds.Length)]);
        
        // log: emit thunder
        Controllers.Logs.AddLog(Time.time, ParentObject.CurrentPlayer, Log.Action.EmitThunder, ActionId, Log.Ending.Natural, "None");
    }
    
    protected override Log.Action GetAction()
    {
        if (_otherIsHandcar && _otherIsMoving)
        {
            return Log.Action.Rain;
        }
        if (_otherPlayer != null && _otherPlayer.CurrentType == ControllableObject.Type.Handcar && References.Entities.Handcar.IsRolling)
        {
            return Log.Action.RainUndefined;
        }

        return Log.Action.RainIndifferent;
    }

    private float GetDistanceToOther()
    {
        if (_otherPlayer == null || _otherPlayer.CurrentType == ControllableObject.Type.Ground) return 50f;
        return Mathf.Abs(_otherPlayer.GetCurrentPosition().x - References.Entities.Cloud.CurrentPosition.x);
    }
        
    private IEnumerator RainRoutine(int id)
    {
        var mediumRainSpeaker = References.Prefabs.GetSpeaker();
        mediumRainSpeaker.StartLoop(HeavyRainLoop, 3f);
        _rainTimer = 0f;
        while (IsPerformingAction())
        {
            _raindropTimer += Time.deltaTime;

            while (_raindropCount < _raindropTimer * RaindropsPerSecond)
            {
                if (_rainTimer < _halfTime || RainMode == Mode.Light || GetDistanceToOther() > ProximityLimit)
                {
                    if (Random.value > _rainTimer / _halfTime || RainMode == Mode.Light || GetDistanceToOther() > ProximityLimit)
                    {
                        CreateRaindrop();
                    }
                    else
                    {
                        CreateHeavyRaindrop();
                    }
                }
                else
                {
                    if (Random.value > (_rainTimer - _halfTime) / _halfTime)
                    {
                        CreateHeavyRaindrop();
                    }
                    else
                    {
                        CreateThunderstormRaindrop();
                    }
                }

                _raindropCount++;
                if (_raindropTimer >= 1f)
                {
                    _raindropTimer = 0f;
                    _raindropCount = 0;
                }
            }

            for (var i = 0; i < Mathf.Max(TimeBeforeThunderbolt - _rainTimer, 1f); i++)
            {
                _rainTimer += Time.deltaTime;
                yield return null;
            }

            if (_rainTimer > TimeBeforeThunderbolt && RainMode == Mode.Heavy && GetDistanceToOther() < ProximityLimit)
            {
                _lightningTimer += Time.deltaTime;
                if (_lightningTimer > _lightningGap)
                {
                    CreateLightning();
                    _lightningTimer -= _lightningGap;
                }
            }
        }
        
        mediumRainSpeaker.EndLoop(2f);

        _rainTimer = Mathf.Clamp01(_rainTimer);
        while (_rainTimer > 0f && !IsPerformingAction())
        {
            _rainTimer -= Time.deltaTime;
            if (Random.value < _rainTimer)
            {
                CreateRaindrop();
            }
            yield return null;
            _rainTimer -= Time.deltaTime;
            yield return null;
        }

        yield return null;
    }
}
