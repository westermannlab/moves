[System.Serializable]
public class PersonalityData
{
    public string playerName;
    public string caption;
    public string adjective;
    public bool isActive;
    public int treatAs = -1;
    public float updateRoleInterval = 2f;
    public float moveInterval = 2f;
    public float actionInterval = 2f;
    public float minTimeBetweenDecisions = 3f;
    public float proximityLimit = 1f;
    public Transition[] transitions;
}
