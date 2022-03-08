using UnityEngine;

public class Controllers : MonoBehaviour
{
    public AssessmentController AssessmentController;
    public AudioController AudioController;
    public CameraController CameraController;
    public InputController InputController;
    public LevelController LevelController;
    public LogsController LogsController;
    public UiController UiController;
    private static Controllers _instance;

    private void Awake()
    {
        _instance = this;
    }

    public static AssessmentController Assessment => _instance.AssessmentController;
    public static AudioController Audio => _instance.AudioController;
    public static CameraController Camera => _instance.CameraController;
    public static InputController Input => _instance.InputController;
    public static LevelController Level => _instance.LevelController;
    public static LogsController Logs => _instance.LogsController;
    public static UiController Ui => _instance.UiController;
}
