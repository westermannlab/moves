using UnityEngine;

public class SoulPixel : Pixel
{
    public Sprite[] Sprites;
    
    public override void Reset()
    {
        base.Reset();
        if (Sprites.Length > 0)
        {
            Sr.sprite = Sprites[Random.Range(0, Sprites.Length)];
        }
    }
}
