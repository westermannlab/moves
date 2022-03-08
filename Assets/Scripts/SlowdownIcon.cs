using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowdownIcon : SpriteObject
{
    private const float FadeDuration = 0.25f;
    private const float FadeDelay = 0.5f;
    private readonly Vector2 _moveMinMax = new Vector2(0.5f, 0.75f);
    
    public void Show(Vector2 position)
    {
        SetPosition(position);
        Fade(0f, FadeDuration, FadeDelay);
        MoveDelta(Vector2.down * Random.Range(_moveMinMax.x, _moveMinMax.y), FadeDuration + FadeDelay);
        Return(FadeDuration + FadeDelay + 0.125f);
    }
}
