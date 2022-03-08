using UnityEngine;

public class WorldMapCloud : WorldMapObject
{
    private float _moveSpeed = 0.25f;
    private const float MoveSpeedRandomness = 0.03125f;
    private const float MoveDistance = 0.125f;

    private SpriteRenderer _sr;
    private Color _currentColor = Color.white;

    protected override void Awake()
    {
        base.Awake();
        Tf = transform;
        _sr = GetComponent<SpriteRenderer>();
        MoveTimer = Random.value * (1f / _moveSpeed);
        _moveSpeed += Random.Range(-MoveSpeedRandomness, MoveSpeedRandomness);
    }
    
    protected override void Update()
    {
        base.Update();
        Offset = Mathf.Sin(MoveTimer * Mathf.PI * _moveSpeed) * MoveDistance * Vector2.right;
        Tf.localPosition = CenterPosition + Offset;
    }

    protected override void ApplyMaxAlpha()
    {
        base.ApplyMaxAlpha();
        _currentColor.a = MaxAlpha;
        _sr.color = _currentColor;
    }
}
