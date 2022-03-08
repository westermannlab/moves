using System.Globalization;

[System.Serializable]
public class LikertItemData
{
    public string question;
    public string[] options;
    
    public string GetFormattedQuestion()
    {
        return question
            .Replace("[CAPTION]", References.Entities.PlayerTwo.GetColorString().ToUpper(CultureInfo.InvariantCulture))
            .Replace("[ADJECTIVE]", References.Entities.PlayerTwo.GetColorString(true));
    }
}
