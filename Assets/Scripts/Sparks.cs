using UnityEngine;

public class Sparks : SpriteObject
{
    private readonly int _sprayHash = Animator.StringToHash("_Spray");
    
    private Animator _animator;

    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
    }

    public void Spray()
    {
        Sr.flipX = Random.value > 0.5f;
        _animator.SetTrigger(_sprayHash);
        Return(1f);
    }
}
