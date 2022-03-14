using System.Globalization;

[System.Serializable]
public class LikertScaleData
{
    public string name;
    public string instructions;
    public LikertItemData[] items;

    public string GetFormattedInstructions()
    {
        return instructions
            .Replace("[CAPTION]", References.Entities.PlayerTwo.GetColorString().ToUpper(CultureInfo.InvariantCulture))
            .Replace("[COLOR]", References.Entities.PlayerTwo.GetColorString(true));
    }
}
