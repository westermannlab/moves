using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : PoolObject
{
    private LightningSegment _currentSegment;
    private Lightning _currentJunction;
    private Sparks _sparks;
    
    private Vector2 _basePosition;
    private Vector2 _currentLocalPosition;
    private float _cloudXPosition;

    private const int MaxSteps = 50;
    private int _maxSteps;
    private int _stepCount;
    private bool _hasReachedGround;
    private bool _goesLeft;
    

    public override void Reset()
    {
        base.Reset();
        _hasReachedGround = false;
        _currentLocalPosition = Vector2.zero;
    }

    public void Extend(int steps = -1, bool goesLeft = false)
    {
        _goesLeft = goesLeft;
        _maxSteps = steps > 0 ? steps : MaxSteps;
        _cloudXPosition = References.Entities.Cloud.CurrentPosition.x;
        _basePosition = Tf.position;
        StartCoroutine(ExtendRoutine());
    }

    private void CreateJunction()
    {
        _currentJunction = References.Prefabs.GetLightning();
        _currentJunction.SetParent(Tf);
        _currentJunction.SetLocalPosition(_currentLocalPosition);
        _currentJunction.Extend(_maxSteps - _stepCount, !_goesLeft);
    }

    private void HitGround()
    {
        _hasReachedGround = true;
        _sparks = References.Prefabs.GetSparks();
        _sparks.SetPosition(Tf.position + (Vector3)_currentLocalPosition);
        _sparks.Spray();
    }

    private IEnumerator ExtendRoutine()
    {
        _stepCount = 0;
        while (_stepCount < _maxSteps && !_hasReachedGround)
        {
            // layer mask could potentially cause trouble later in case layer order changes
            if (Physics2D.OverlapCircle(_basePosition + _currentLocalPosition, Utility.PixelToUnit * 2f, 1 << 3 | 1 << 8) !=
                null)
            {
                HitGround();
                yield break;
            }
            
            _currentSegment = References.Prefabs.GetLightningSegment();
            _currentSegment.SetParent(Tf);
            _currentSegment.SetLocalPosition(_currentLocalPosition);
            _currentSegment.ShuffleSprite();
            _currentSegment.FlipX(_goesLeft);
            _currentSegment.Fade(0.125f, 0.25f);
            _currentSegment.Return(0.5f);

            if (!_goesLeft)
            {
                _currentLocalPosition.x += _currentSegment.GetWidth() - Utility.PixelToUnit;
            }
            else
            {
                _currentLocalPosition.x -= _currentSegment.GetWidth() - Utility.PixelToUnit;
            }
            _currentLocalPosition.y -= _currentSegment.GetHeight() - Utility.PixelToUnit;

            if (Random.value < 0.125f)
            {
                _goesLeft = !_goesLeft;
            }

            if (_cloudXPosition - 1.5f > _basePosition.x + _currentLocalPosition.x)
            {
                _goesLeft = false;
            }
            else if (_cloudXPosition + 1.5f < _basePosition.x + _currentLocalPosition.x)
            {
                _goesLeft = true;
            }

            if (Random.value < Mathf.InverseLerp(0f, _maxSteps * 6f, _stepCount))
            {
                CreateJunction();
            }

            _stepCount++;

            if (_stepCount % 8 == 0)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        yield return null;
        Return(1f);
    }
}
