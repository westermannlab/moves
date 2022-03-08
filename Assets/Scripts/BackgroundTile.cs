using UnityEngine;

public class BackgroundTile : SpriteObject
{
    public Sprite[] TileSprites;
    public int Code;

    public void UpdateTile(int code)
    {
        if (code < 0 || code >= TileSprites.Length) return;
        Code = code;
        SetSprite(TileSprites[code]);
    }
}
