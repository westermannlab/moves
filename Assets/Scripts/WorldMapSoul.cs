using System.Collections;
using UnityEngine;

public class WorldMapSoul : WorldMapObject
{
    private Pixel _pixel;
    private Vector2 _randomPosition;
    private float _moveSpeed = 0.25f;
    private const float MoveSpeedRandomness = 0.03125f;
    private const float EffectRadius = 0.09375f;
    private float _effectScale = 1f;
    private float _pixelScale = 1f;

    private int _sortingOrderOffset;

    protected override void Awake()
    {
        base.Awake();
        MoveTimer = Random.value * (1f / _moveSpeed);
        _moveSpeed += Random.Range(-MoveSpeedRandomness, MoveSpeedRandomness);
    }

    

    protected override void Update()
    {
        base.Update();

        if (Mathf.Approximately(MaxAlpha, 0f))
        {
            return;
        }
        
        _pixel = References.Prefabs.GetSoulPixel();
        _randomPosition = Random.insideUnitCircle * EffectRadius * _effectScale;

        Offset.x = Mathf.Sin(MoveTimer * _moveSpeed * Mathf.PI) * 0.375f;
        Offset.y = Mathf.Sin(MoveTimer * _moveSpeed * Mathf.PI + Mathf.PI / 2f) * 0.125f;

        _pixel.SetPosition(CenterPosition + Offset + _randomPosition);
        if (Inhabitant != null)
        {
            _pixel.SetColor(Inhabitant.SoulGradient.Evaluate(_randomPosition.sqrMagnitude * 4f));
        }
        _pixel.SetAlpha(0f);
        // _pixel.SetLayer(SoulPixelLayer);
        _pixel.SetSpriteSortingOrder(63 + _sortingOrderOffset);
        _pixel.SetScale(Mathf.Lerp(1f, 0.25f, _randomPosition.sqrMagnitude * (EffectRadius / 2f)) * _pixelScale);
        _pixel.Fade(MaxAlpha, 0.25f);
        _pixel.Fade(0f, 1f, 0.25f);
        _pixel.Return(1.375f);

        _sortingOrderOffset = Utility.AddOne(_sortingOrderOffset);
    }

    protected override void ApplyMaxAlpha()
    {
        base.ApplyMaxAlpha();
        _effectScale = Mathf.Approximately(MaxAlpha, 1f) ? 1.125f : 1f;
        _pixelScale = Mathf.Approximately(MaxAlpha, 1f) ? 1.25f : 1f;
    }
}
