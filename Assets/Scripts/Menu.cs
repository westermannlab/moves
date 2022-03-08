using System.Collections;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public ResourceCircle GlobalResourceCircle;

    private UiNote[] _uiNotes;
    
    private Transform _tf;
    private Vector3 _currentPosition;
    private Vector3 _positionHolder;
    private float _moveTimer;
    private int _moveId;
    private bool _isOpen;
    
    private Vector3 _activePosition = new Vector3(0f, -0.25f, 0f);
    private Vector3 _inactivePosition = new Vector3(0f, -2f, 0f);

    public bool IsOpen => _isOpen;

    private void Awake()
    {
        _tf = transform;
        _currentPosition = _tf.localPosition;
        _uiNotes = GetComponentsInChildren<UiNote>();
    }

    private void Start()
    {
        UpdateUiNoteColors();
        Open(0f);
    }
    
    public void Open(float duration)
    {
        _isOpen = true;
        Move(_activePosition, duration);
        References.Events.ChangeMenuState(1);
        foreach (var note in _uiNotes)
        {
            note.Activate(duration);
        }
    }

    public void Close(float duration)
    {
        _isOpen = false;
        Move(_inactivePosition, duration);
        References.Events.ChangeMenuState(0);
        foreach (var note in _uiNotes)
        {
            note.Deactivate(duration);
        }
    }

    public void UpdateNoteBuffer(int playerId, float buffer)
    {
        for (var i = Mathf.Max(playerId * 5 + Mathf.FloorToInt(buffer) - 1, playerId * 5); i < playerId * 5 + Mathf.Min(Mathf.CeilToInt(buffer), 5); i++)
        {
            if (i % 5 < Mathf.FloorToInt(buffer))
            {
                _uiNotes[i].SetFillState(1f);
            }
            else
            {
                _uiNotes[i].SetFillState(buffer % 1f);
            }
        }
    }

    private void UpdateUiNoteColors()
    {
        for (var i = 0; i < _uiNotes.Length; i++)
        {
            _uiNotes[i].SetColor(i < _uiNotes.Length / 2 ? References.Entities.PlayerOne.SoftColor : References.Entities.PlayerTwo.SoftColor);
        }
    }

    private void Move(Vector3 targetPosition, float duration)
    {
        _moveId = Utility.AddOne(_moveId);
        StartCoroutine(MoveRoutine(targetPosition, duration, _moveId));
    }

    public void SetLocalPosition(Vector3 activePosition)
    {
        _activePosition = activePosition;
        _inactivePosition = activePosition + 0.4375f * Vector3.down;
        _currentPosition = activePosition;
    }

    private void ApplyCurrentPosition()
    {
        _tf.localPosition = _currentPosition;
    }

    private IEnumerator MoveRoutine(Vector3 targetPosition, float duration, int id)
    {
        _moveTimer = 0f;
        _positionHolder = _currentPosition;
        while (_moveTimer < duration && _moveId == id)
        {
            _moveTimer += Time.unscaledDeltaTime;
            _currentPosition = Vector3.Lerp(_positionHolder, targetPosition, _moveTimer / duration);
            ApplyCurrentPosition();
            yield return null;
        }

        if (_moveId == id)
        {
            _currentPosition = targetPosition;
            ApplyCurrentPosition();
        }
        
        yield return null;
    }
}
