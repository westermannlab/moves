using System.Collections;
using UnityEngine;

public class MainMenuInput : MonoBehaviour
{
    public enum State { Default, Blocked }

    public State InputState;
    public UiButton QuitButton;
    public Label VersionLabel;
    public Label MessageLabel;

    private int _fadeId;

    private void Start()
    {
        Controllers.Input.CurrentState = InputController.State.MainMenu;
        MessageLabel.SetHorizontalAlignment(Label.HorizontalAlignment.Center);
        MessageLabel.SetVerticalAlignment(Label.VerticalAlignment.Center);
        MessageLabel.Deactivate();
        
        VersionLabel.SetLocalPosition(Controllers.Ui.GetLowerLeftCorner() - new Vector2(Utility.PixelsToUnit(6f), Utility.PixelsToUnit(4f)));
        VersionLabel.SetHorizontalAlignment(Label.HorizontalAlignment.Left);
        VersionLabel.SetText(string.Concat('v', Application.version), 0.5f);

        QuitButton.SetLocalPosition(Controllers.Ui.GetUpperRightCorner());
        
        if (References.Io.GetData().debug == 2 || References.Io.HaveAllLevelsBeenCompleted())
        {
            QuitButton.Activate();
        }
        else
        {
            QuitButton.Deactivate();
        }

        if (References.Io.HaveAllLevelsBeenCompleted())
        {
            DisplayMessage(References.Io.GetData().msgThankYou, -1f);
        }

        if (References.Settings.CompanionEnabled)
        {
            var companion = References.Prefabs.GetCompanion();
            companion.SetPosition(new Vector2(4f, -1.3125f));
        }
    }

    public void DisplayMessage(string message, float duration)
    {
        MessageLabel.SetText(message, 0.25f);
        MessageLabel.Activate();
        if (duration > 0f)
        {
            _fadeId = Utility.AddOne(_fadeId);
            StartCoroutine(FadeMessageRoutine(duration, 0.25f, _fadeId));
        }
    }

    private IEnumerator FadeMessageRoutine(float delay, float duration, int id)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (_fadeId == id)
        {
            MessageLabel.Deactivate(duration);
        }
    }

}
