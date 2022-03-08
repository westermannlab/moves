using UnityEngine;

public class UiNote : UiElement
{
    public Sprite[] NoteSprites;

    private float _currentFillState;

    public void SetFillState(float fillState)
    {
        _currentFillState = fillState;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        Sr.sprite = NoteSprites[Mathf.RoundToInt(_currentFillState * (NoteSprites.Length - 1))];
    }
}
