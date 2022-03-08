using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    public LevelData MainMenuLevel;
    
    private PickUp[] _notes;
    private Player _p1;
    private Player _p2;
    private readonly List<PickUp> _notesList = new List<PickUp>();
    private float _currentGroundAngle;
    private float _handcarXPosition;
    private float _deltaXPosition;
    private float _groundRotationTimer;
    private int _whileIsInGroundId;
    private int _currentGroundDirection;
    private int _lastGroundDirection;

    private void Start()
    {
        _notes = FindObjectsOfType<PickUp>();
        _p1 = References.Entities.PlayerOne;
        _p2 = References.Entities.PlayerTwo;
    }

    private void OnEnable()
    {
        References.Events.OnPlayerStateChange += ReevaluatePlayerStates;
        References.Events.OnChangeGroundAngle += ApplyWorldRotation;
        References.Events.OnAssessmentCompleted += QuitToMainMenu;
    }

    private void OnDisable()
    {
        References.Events.OnPlayerStateChange -= ReevaluatePlayerStates;
        References.Events.OnChangeGroundAngle -= ApplyWorldRotation;
        References.Events.OnAssessmentCompleted -= QuitToMainMenu;
    }

    private void ReevaluatePlayerStates(Player player)
    {
        if (_p1.CurrentType == ControllableObject.Type.Handcar && 
            _p2.CurrentType != ControllableObject.Type.Ground && _p2.CurrentType != ControllableObject.Type.Cart)
        {
            ChangeNoteColor(_p1.ColorId);
        }
        else if (_p2.CurrentType == ControllableObject.Type.Handcar &&
                 _p1.CurrentType != ControllableObject.Type.Ground && _p1.CurrentType != ControllableObject.Type.Cart)
        {
            ChangeNoteColor(_p2.ColorId);
        }
        else if (_p1.CurrentType == ControllableObject.Type.Cart && _p2.CurrentType == ControllableObject.Type.Ground)
        {
            ChangeNoteColor(_p2.ColorId);
        }
        else if (_p2.CurrentType == ControllableObject.Type.Cart && _p1.CurrentType == ControllableObject.Type.Ground)
        {
            ChangeNoteColor(_p1.ColorId);
        }
        else if (_p1.CurrentType == ControllableObject.Type.Handcar &&
                 _p2.CurrentType == ControllableObject.Type.Ground)
        {
            _whileIsInGroundId = Utility.AddOne(_whileIsInGroundId);
            StartCoroutine(WhileIsGroundRoutine(_p2, _whileIsInGroundId));
        }
        else if (_p2.CurrentType == ControllableObject.Type.Handcar &&
                 _p1.CurrentType == ControllableObject.Type.Ground)
        {
            _whileIsInGroundId = Utility.AddOne(_whileIsInGroundId);
            StartCoroutine(WhileIsGroundRoutine(_p1, _whileIsInGroundId));
        }
        else if (_p1.CurrentType == ControllableObject.Type.Cart && _p2.CurrentType == ControllableObject.Type.Handcar)
        {
            ChangeNoteColors(_p1.ColorId, _p2.ColorId);
        }
        else if (_p2.CurrentType == ControllableObject.Type.Cart && _p1.CurrentType == ControllableObject.Type.Handcar)
        {
            ChangeNoteColors(_p2.ColorId, _p1.ColorId);
        }
        else
        {
            ChangeNoteColor(-1);
        }
    }

    public void ChangeNoteColor(int colorId)
    {
        foreach (var note in _notes)
        {
            note.SetColor(colorId);
        }
    }

    public void ChangeNoteColors(int colorIdA, int colorIdB)
    {
        foreach (var note in _notes)
        {
            if (Mathf.RoundToInt(Utility.Mod(note.Position.x, 2f)) == (note.Position.x >= 0f ? 1: 0))
            {
                note.SetColor(colorIdA);
            }
            else
            {
                note.SetColor(colorIdB);
            }
        }
    }

    public Vector2 GetNoteValuesOnEitherSide(float xPosition, int colorId)
    {
        var values = Vector2.zero;
        foreach (var note in _notes)
        {
            if (note.Position.x < xPosition)
            {
                values.x += note.GetValue(colorId);
            }
            else if (note.Position.x > xPosition)
            {
                values.y += note.GetValue(colorId);
            }
        }

        return values;
    }

    public PickUp GetNearNote(Vector2 position)
    {
        _notesList.Clear();

        _notesList.Add(_notes[0]);
        for (var i = 1; i < _notes.Length; i++)
        {
            var hasBeenAdded = false;
            for (var j = 0; j < _notesList.Count; j++)
            {
                if (!_notes[i].HasBeenPickedUp && (_notes[i].Position - position).sqrMagnitude < (_notesList[j].Position - position).sqrMagnitude)
                {
                    _notesList.Insert(j, _notes[i]);
                    hasBeenAdded = true;
                    break;
                }
            }

            if (!hasBeenAdded)
            {
                _notesList.Add(_notes[i]);
            }
        }
        return _notesList[Random.Range(0, 3)];
    }

    public bool AreThereSparklingNotes()
    {
        foreach (var note in _notes)
        {
            if (note.IsSparkling) return true;
        }

        return false;
    }

    private void ApplyWorldRotation(float groundAngle)
    {
        Controllers.Camera.GroundCamera.SetCameraAngle(-groundAngle);
    }

    public int GetRoomId()
    {
        var colorId = References.Entities.PlayerTwo.ColorId;
        // return 0 for tutorial level (color id 6):
        return colorId < 6 ? colorId : 0;
    }

    public int GetTrial()
    {
        return References.Io.GetAssessmentState(GetRoomId());
    }

    public void QuitToMainMenu()
    {
        var roomId = GetRoomId();
        References.Io.UpdateAssessmentState(roomId, References.Io.GetAssessmentState(roomId) + 1);
        Controllers.Input.CurrentState = InputController.State.Ended;
        // deactivate for tutorial?
        Controllers.Logs.SendLogfile();
        StartCoroutine(LoadMainMenuRoutine(3f));
    }

    private IEnumerator WhileIsGroundRoutine(Player player, int id)
    {
        _groundRotationTimer = 0f;
        
        while (player.CurrentType == ControllableObject.Type.Ground && _whileIsInGroundId == id)
        {
            _currentGroundAngle = References.Entities.Ground.CurrentEulerAngle;
            _handcarXPosition = References.Entities.Handcar.CurrentPosition.x;
            _groundRotationTimer += 0.125f;
            
            if (_currentGroundAngle > 0f)
            {
                _currentGroundDirection = 1;
            }
            else if (_currentGroundAngle < 0f)
            {
                _currentGroundDirection = -1;
            }
            else
            {
                _currentGroundDirection = 0;
            }
            
            foreach (var note in _notes)
            {
                _deltaXPosition = note.Position.x - _handcarXPosition;

                if (_currentGroundAngle > 0f)
                {
                    // tilted to the left
                    if (_deltaXPosition < 0f)
                    {
                        note.SetColor(player.ColorId);
                    }
                    else
                    {
                        note.SetColor(player.Counterpart.ColorId);
                    }
                }
                else if (_currentGroundAngle < 0f)
                {
                    // tilted to the right
                    if (_deltaXPosition > 0f)
                    {
                        note.SetColor(player.ColorId);
                    }
                    else
                    {
                        note.SetColor(player.Counterpart.ColorId);
                    }
                }
                else
                {
                    note.SetColor(player.Counterpart.ColorId);
                }
            }

            if (_lastGroundDirection != _currentGroundDirection)
            {
                _groundRotationTimer = 0f;
                _lastGroundDirection = _currentGroundDirection;
            }
            
            yield return new WaitForSeconds(0.125f);
        }

        yield return null;
    }

    private IEnumerator LoadMainMenuRoutine(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        SceneManager.LoadScene(MainMenuLevel.SceneName);
        yield return null;
    }
}
