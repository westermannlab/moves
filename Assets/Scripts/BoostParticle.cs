using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostParticle : SpriteObject
{
    public Sprite[] Sprites;

    private const float FadeDuration = 1f;

    public void Spawn(Vector2 position)
    {
        Tf.position = position;
        Sr.sprite = Sprites[Random.Range(0, Sprites.Length)];
        SetAlpha(1f);
        Fade(0f, FadeDuration, Random.Range(0.125f, 0.25f));
        SetScale(1f);
        Scale(8f, FadeDuration);
        Return(FadeDuration + 0.375f);
    }
}
