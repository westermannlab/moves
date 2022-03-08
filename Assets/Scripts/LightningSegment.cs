using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningSegment : SpriteObject
{
    public Sprite[] SegmentSprites;
    public event System.Action OnHitGround;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnHitGround?.Invoke();
    }
    
    public void ShuffleSprite()
    {
        Sr.sprite = SegmentSprites[Random.Range(0, SegmentSprites.Length)];
    }

    public float GetWidth()
    {
        return Sr.sprite.rect.width * Utility.PixelToUnit;
    }

    public float GetHeight()
    {
        return Sr.sprite.rect.height * Utility.PixelToUnit;
    }
}
