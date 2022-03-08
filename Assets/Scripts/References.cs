using UnityEngine;

public class References : MonoBehaviour
{
    public ActionManager ActionManager;
    public Alphabet AlphabetData;
    public Assessment AssessmentData;
    public AudioManager AudioManager;
    public EntityManager EntityManager;
    public EventManager EventManager;
    public IoManager IoManager;
    public LevelTimer LevelTimer;
    public Menu MenuObject;
    public Pool PoolData;
    public Transform PoolTransform;
    public PrefabManager PrefabManager;
    public Recorder AudioRecorder;
    public SettingsManager SettingsManager;
    public TaskManager TaskManager;
    public Terminal TerminalObject;
    public Tutorial TutorialObject;

    public bool InitializeEntities = true;
    public bool InitializeIoManager = true;
    
    
    private static References _instance;
    
    private void Awake()
    {
        _instance = this;
        if (PoolData != null)
        {
            PoolData.Init();
        }
    }

    private void Start()
    {
        if (IoManager != null && InitializeIoManager)
        {
            IoManager.Init();
            // read terminal settings
            Settings.Init();
        }
        if (EntityManager != null && InitializeEntities)
        {
            EntityManager.Init();
        }
    }

    public static ActionManager Actions => _instance.ActionManager;
    public static Alphabet Alphabet => _instance.AlphabetData;
    public static Assessment Assessment => _instance.AssessmentData;
    public static AudioManager Audio => _instance.AudioManager;
    public static EntityManager Entities => _instance.EntityManager;
    public static EventManager Events => _instance.EventManager;
    public static IoManager Io => _instance.IoManager;
    public static LevelTimer Timer => _instance.LevelTimer;
    public static Menu Menu => _instance.MenuObject;
    public static Pool Pool => _instance.PoolData;
    public static Transform PoolTf => _instance.PoolTransform;
    public static PrefabManager Prefabs => _instance.PrefabManager;
    public static Recorder Recorder => _instance.AudioRecorder;
    public static SettingsManager Settings => _instance.SettingsManager;
    public static TaskManager Tasks => _instance.TaskManager;
    public static Terminal Terminal => _instance.TerminalObject;
    public static Tutorial Tutorial => _instance.TutorialObject;
    public static References Coroutines => _instance;
}
