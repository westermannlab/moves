using UnityEngine;

public class Logo : MonoBehaviour
{
    public SpriteObject[] LetterObjects;

    public Gradient Gradient;

    private void Start()
    {
        for (var i = 0; i < LetterObjects.Length; i++)
        {
            LetterObjects[i].SetScale(1f);
        }
    }

    private void OnEnable()
    {
        References.Events.OnSelectLevelButton += SelectLevelButton;
    }
    
    private void OnDisable()
    {
        References.Events.OnSelectLevelButton -= SelectLevelButton;
    }

    private void SelectLevelButton(Player player, bool isSelected)
    {
        SetColors(isSelected ? player.Color : Color.white);
    }

    private void SetColors(Color mainColor)
    {
        Color.RGBToHSV(mainColor, out var h, out var s, out var v);
        for (var i = 0; i < LetterObjects.Length; i++)
        {
            LetterObjects[i].Scale(1.125f, 0.125f, i * 0.033f);
            LetterObjects[i].Scale(1f, 0.125f, 0.125f + i * 0.033f);
            LetterObjects[i].ChangeColor(Mathf.Approximately(s, 0f) ? Color.white : Gradient.Evaluate(Utility.Mod(h - 0.0625f + i * 0.03125f, 1f)), 0.25f);
        }
        
    }

}
