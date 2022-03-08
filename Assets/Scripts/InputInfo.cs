public class InputInfo
{
    public InputController.Type InputType;
    public float Distance;
    public float Duration;
    public float Delay;

    public InputInfo(InputController.Type type, float distance, float duration, float delay)
    {
        InputType = type;
        Distance = distance;
        Duration = duration;
        Delay = delay;
    }
}
