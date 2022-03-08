using UnityEngine;

public class Sparkle : SpriteObject
{
    private readonly int _typeHash = Animator.StringToHash("_Type");
    private readonly int _initHash = Animator.StringToHash("_Init");
    
    private Animator _animator;

    public override void Init()
    {
        base.Init();
        _animator = GetComponent<Animator>();
    }

    public override void Reset()
    {
        base.Reset();
        // render sometimes in after, sometimes before notes
        Sr.sortingOrder = Random.value > 0.5f ? 41 : 39;
    }

    public void SetColor(Color color)
    {
        Sr.color = color;
    }

    public void Play(Vector2 position)
    {
        Tf.position = position;
        _animator.SetInteger(_typeHash, Random.Range(0, 2));
        _animator.SetTrigger(_initHash);
        Return(0.75f);
    }
    
}
