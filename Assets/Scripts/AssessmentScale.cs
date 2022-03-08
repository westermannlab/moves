[System.Serializable]
public class AssessmentScale
{
    public string Title;
    public int[] Results;

    public AssessmentScale(string title, int[] results)
    {
        Title = title;
        Results = results;
    }
}
