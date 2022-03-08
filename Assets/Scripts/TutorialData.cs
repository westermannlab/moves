[System.Serializable]
public class TutorialData
{
   public string version = "unknown";
   public TaskData[] tasks;
   public string[] types;
   public string[] targets;
   public string[] buttons;

   public override string ToString()
   {
      return string.Concat("Tutorial data containing ", tasks != null ? tasks.Length.ToString() : "null", " tasks.");
   }
}
