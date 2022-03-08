using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BufferStop : MonoBehaviour
{
    public enum Location { Left, Right }

    public Location LevelLocation;

    private BoxCollider2D _boxCollider;

    private Vector2 _colliderOffset;
    private float _shiftColliderTimer;
    private float _currentXOffset;
    private float _baseXPosition;
    private float _xOffsetHolder;
    private int _shiftColliderId;

    private void Awake()
    {
        _boxCollider = GetComponentInChildren<BoxCollider2D>();
        _colliderOffset = _boxCollider.offset;
        _baseXPosition = _boxCollider.offset.x;
    }

    private void OnEnable()
    {
        References.Events.OnActivateControllableObject += AdjustCollider;
    }

    private void OnDisable()
    {
        References.Events.OnActivateControllableObject -= AdjustCollider;
    }

    private void AdjustCollider(ControllableObject.Type type, bool isActive)
    {
        switch (type)
        {
            case ControllableObject.Type.Handcar:
                if (LevelLocation == Location.Right)
                {
                    ShiftCollider(isActive ? 0f : 1.25f, 1f);
                }
                break;
            case ControllableObject.Type.Cart:
                if (LevelLocation == Location.Left)
                {
                    ShiftCollider(isActive ? 0f : -1.25f, 1f);
                }
                break;
        }
    }

    private void ShiftCollider(float xAmount, float duration)
    {
        _shiftColliderId = Utility.AddOne(_shiftColliderId);
        StartCoroutine(ShiftColliderRoutine(xAmount, duration, _shiftColliderId));
    }

    private void ApplyCurrentPosition()
    {
        _colliderOffset.x = _baseXPosition + _currentXOffset;
        _boxCollider.offset = _colliderOffset;
    }

    private IEnumerator ShiftColliderRoutine(float xOffset, float duration, int id)
    {
        _shiftColliderTimer = 0f;
        _xOffsetHolder = _currentXOffset;

        while (_shiftColliderTimer < duration && _shiftColliderId == id)
        {
            _shiftColliderTimer += Time.deltaTime;
            _currentXOffset = Mathf.Lerp(_xOffsetHolder, xOffset, _shiftColliderTimer / duration);
            ApplyCurrentPosition();
            yield return null;
        }

        if (_shiftColliderId == id)
        {
            _currentXOffset = xOffset;
            ApplyCurrentPosition();
        }
        
        yield return null;
    }
}
