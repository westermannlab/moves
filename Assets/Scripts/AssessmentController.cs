using UnityEngine;

public class AssessmentController : MonoBehaviour
{
    private AssessmentScreen _assessmentScreen;
    private MovesData _movesData;

    private int _currentScreenIndex;

    private void Start()
    {
        References.Assessment.ResetData();
        _movesData = References.Io.GetData();
    }
    
    private void OnEnable()
    {
        if (References.Assessment.CurrentLevelData != null && References.Assessment.CurrentLevelData.LevelType == LevelData.Type.Default)
        {
            References.Events.OnTimerExpired += CreateAssessment;
        }
    }

    private void OnDisable()
    {
        if (References.Assessment.CurrentLevelData != null && References.Assessment.CurrentLevelData.LevelType == LevelData.Type.Default)
        {
            References.Events.OnTimerExpired -= CreateAssessment;
        }
    }
    
    private void CreateAssessment()
    {
        if (References.Assessment.CurrentLevelData.LevelType == LevelData.Type.Tutorial) return;
        
        Controllers.Input.CurrentState = InputController.State.Assessment;
        Controllers.Ui.FadeOut(3f);
        
        _assessmentScreen = References.Prefabs.GetAssessmentScreen(_movesData.scales[_currentScreenIndex].items.Length - 1);
        _assessmentScreen.LoadData(_movesData, _currentScreenIndex);
        _assessmentScreen.Open(_currentScreenIndex == 0 ? 1f : 0.25f, _currentScreenIndex == _movesData.scales.Length - 1);
    }

    public void FinishAssessment(int index)
    {
        if (index + 1 < _movesData.scales.Length)
        {
            // create next assessment
            _currentScreenIndex++;
            CreateAssessment();
        }
        else
        {
            Controllers.Input.CurrentState = InputController.State.Ended;
            References.Events.AssessmentCompleted();
        }
    }

    public void ProcessInput(InputController.Type inputType, InputController.Mode inputMode)
    {
        if (!_assessmentScreen) return;
        _assessmentScreen.ProcessInput(inputType, inputMode);
    }
}
