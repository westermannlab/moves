using UnityEngine;

public class FadePlane : UiElement
{
    public SpriteRenderer SpriteRenderer => Sr;

    public void SetTileSize(Vector2 size)
    {
        Sr.size = size;
    }
}
