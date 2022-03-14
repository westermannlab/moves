using UnityEngine;

public class SettingsManager : ScriptableObject
{
    public LevelData MainMenuScene;
    public LevelData DefaultScene;
    public bool TerminalEnabled;
    public bool CompanionEnabled;
    private float _currentTimeScale = 1f;

    public void Init()
    {
        var data = References.Io.GetData();
        TerminalEnabled = data.debug == 2;
        SetFullScreenMode(data.debug != 2);
    }

    public void SetTimeScale(float timeScale)
    {
        _currentTimeScale = timeScale;
        Time.timeScale = _currentTimeScale;
        References.Terminal.AddEntry("Changed time scale to " + _currentTimeScale + ".");
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ContinueGame()
    {
        Time.timeScale = _currentTimeScale;
    }

    public void ShowCurrentTimeScale()
    {
        References.Terminal.AddEntry("Current time scale is " + _currentTimeScale + ".");
    }

    private void SetFullScreenMode(bool fullScreen)
    {
        Screen.fullScreenMode = fullScreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;
    }

    public void EnableCompanion(bool enabled)
    {
        CompanionEnabled = enabled;
    }
}
