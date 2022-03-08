using System.Collections;
using UnityEngine;

public class Raindrop : PoolObject
{
    private readonly int _hasCollidedHash = Animator.StringToHash("_HasCollided");
    private readonly int _sizeHash = Animator.StringToHash("_Size");
    private readonly int _isActiveHash = Animator.StringToHash("_IsActive");

    private Animator _animator;

    private const float FallSpeed = 4f;
    private bool _hasCollided;
    
    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_hasCollided && (Tf.position.y < 0f || Random.value > 0.3f))
        {
            Splash();
        }
    }
    
    public override void Reset()
    {
        base.Reset();
        _hasCollided = false;
        _animator.SetBool(_hasCollidedHash, false);
        _animator.SetBool(_isActiveHash, true);
    }
    
    public void Drop(int size = 0)
    {
        _animator.SetInteger(_sizeHash, size);
        StartCoroutine(FallRoutine());
    }

    private void Splash()
    {
        _hasCollided = true;
        _animator.SetBool(_hasCollidedHash, true);
    }

    private IEnumerator FallRoutine()
    {
        while (!_hasCollided)
        {
            Tf.position += Vector3.down * Time.fixedDeltaTime * FallSpeed;
            yield return new WaitForFixedUpdate();
        }

        for (var i = 0; i < 50; i++)
        {
            Tf.position += Vector3.down * Time.fixedDeltaTime * 0.5f;
            yield return new WaitForFixedUpdate();
        }
        
        _animator.SetBool(_isActiveHash, false);
        
        yield return null;
        Return(0.25f);
    }
}
