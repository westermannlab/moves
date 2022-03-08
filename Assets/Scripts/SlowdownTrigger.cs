using System.Collections;
using UnityEngine;

public class SlowdownTrigger : MonoBehaviour
{
    private ControllableObject _parentObject;
    private SlowdownIcon _icon;
    private BoxCollider2D _boxCollider2D;
    
    private const float MaxSlowdown = 1f;
    private float _currentSlowdown;
    private const int MaxHits = 25;
    private const float SubtractedHitsPerSecond = 15f;
    private float _currentHits;
    private const float MaxTolerance = 0.25f;
    private float _toleranceTimer;
    private const float MinIconInterval = 0.125f;
    private const float MaxIconInterval = 0.5f;
    private float _currentIconInterval;
    private float _iconIntervalTimer;

    private Vector2 _randomPosition;
    private Vector2 _iconXPosition;
    private Vector2 _iconYPosition;
    private int _iconIndex;
    private const float MaxIconIndex = 4f;
    private float _width;
    
    private int _slowdownId;
    private bool _isSlowdownActive;

    private void Awake()
    {
        _parentObject = GetComponentInParent<ControllableObject>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _width = Mathf.Abs(_boxCollider2D.bounds.extents.x * 2f) / MaxIconIndex;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (References.Actions.Rain.RainMode != Rain.Mode.Heavy) return;
        _currentHits = Mathf.Min(_currentHits + 1f, MaxHits);
        _toleranceTimer = 0f;
        if (!_isSlowdownActive && _parentObject != null)
        {
            BeginSlowdown();
        }
    }

    private void BeginSlowdown()
    {
        _isSlowdownActive = true;
        _slowdownId = Controllers.Logs.RequestActionId();
        StartCoroutine(SlowdownRoutine(Time.timeSinceLevelLoad, References.Entities.Cloud.CurrentPlayer, _parentObject.CurrentPlayer, _slowdownId));
    }

    private void EndSlowdown(float beginTime, Player actingPlayer, Player affectedPlayer, int id)
    {
        _isSlowdownActive = false;
        Controllers.Logs.AddLog(beginTime, actingPlayer, Log.Action.SlowdownOther, id, Log.Ending.Natural, affectedPlayer != null ? affectedPlayer.Identifier : "N/A");
    }

    private void ApplyCurrentSlowdown()
    {
        _parentObject.UpdateSlowdown(_currentSlowdown);
    }

    private void CreateSlowdownIcon()
    {
        _icon = References.Prefabs.GetSlowdownIcon();
        _randomPosition.x = GetXPositionForIcon();
        _randomPosition.y = GetYPositionForIcon();
        _icon.Show(_randomPosition);
        _iconIndex = (_iconIndex + 1) % (int)MaxIconIndex;
    }

    private float GetXPositionForIcon()
    {
        _iconXPosition.x = _boxCollider2D.bounds.min.x;
        _iconXPosition.y = _boxCollider2D.bounds.max.x;
        return Random.Range(_iconXPosition.x + _iconIndex * _width + 0.25f * _width, _iconXPosition.x + (_iconIndex + 1f) * _width - 0.25f * _width);
    }

    private float GetYPositionForIcon()
    {
        _iconYPosition.x = _boxCollider2D.bounds.max.y + 0.125f;
        _iconYPosition.y = _iconYPosition.x + 0.125f;
        return Random.Range(_iconYPosition.x, _iconYPosition.y);
    }

    private IEnumerator SlowdownRoutine(float beginTime, Player actingPlayer, Player affectedPlayer, int id)
    {
        while ((_currentHits > 0f || _toleranceTimer < MaxTolerance) && _slowdownId == id)
        {
            _currentHits -= SubtractedHitsPerSecond * Time.deltaTime;
            _currentSlowdown = MaxSlowdown * Utility.EaseInOut(Mathf.Clamp01(_currentHits / MaxHits));
            ApplyCurrentSlowdown();
            _toleranceTimer += Time.deltaTime;
            _iconIntervalTimer += Time.deltaTime;
            _currentIconInterval = Mathf.Lerp(MaxIconInterval, MinIconInterval, Mathf.Clamp01(_currentHits / MaxHits));
            if (_iconIntervalTimer > _currentIconInterval)
            {
                CreateSlowdownIcon();
                _iconIntervalTimer -= _currentIconInterval;
            }
            yield return null;
        }

        EndSlowdown(beginTime, actingPlayer, affectedPlayer, id);

        yield return null;
    }
}
