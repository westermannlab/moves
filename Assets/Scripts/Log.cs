using System.Globalization;
using UnityEngine;

public class Log
{
    public enum Type { Undefined }

    public enum Action
    {
        Undefined, CancelAnimate, Animate,CancelDisembody, Disembody, Move, RotateTo,
        Rain, RainIndifferent, RainUndefined, RainOnNote, EmitThunder, SlowdownOther,
        Shake, ShakeBraked, ShakeMoving, ShakeUndefined, Brake, BrakeShaking, BrakeNotMoving, BrakeJump,
        BrakeUndefined, Boost, BoostRaining, BoostShaking, BoostUpSlope, BoostUndefined, TouchNote, GoToMenu
    }
    
    public enum Ending { Natural, Exhaustion, Unable, Collect }

    public Type Category;

    private readonly int _id;
    private readonly float _beginTime;
    private readonly float _endTime;
    private readonly Player _player;
    private readonly string _playerColor;
    private readonly string _playerIdentifier;
    private readonly string _currentState;
    private readonly string _otherCurrentState;
    private readonly Action _action;
    private readonly int _actionId;
    private readonly string _target;
    private readonly Vector2 _playerOnePosition;
    private readonly Vector2 _playerTwoPosition;
    private readonly Vector2 _relationship;
    private readonly Ending _ending;

    public Action ActionType => _action;
    public Vector2 PlayerPosition => _playerOnePosition;
    public Player Agent => _player;
    public float Duration => _endTime - _beginTime;

    public Log(float beginTime, Player player, Action action, int actionId, Ending ending, string target)
    {
        _id = Controllers.Logs.RequestLogId();
        _beginTime = beginTime;
        _endTime = Time.timeSinceLevelLoad;
        _player = player;
        _playerColor = _player != null ? _player.PlayerName : "N/A";
        _playerIdentifier = _player != null ? _player.Identifier : "N/A";
        _currentState = _player != null ? _player.GetCurrentState() : "N/A";
        _otherCurrentState = _player != null ? _player.Counterpart.GetCurrentState() : "N/A";
        _action = action;
        _actionId = actionId;
        _ending = ending;
        _target = target;
        _playerOnePosition = References.Entities.PlayerOne.GetCurrentPosition();
        _playerTwoPosition = References.Entities.PlayerTwo.GetCurrentPosition();
        _relationship = Vector2.zero;
    }

    public override string ToString()
    {
        return string.Concat(_id, '\t', BeginTime(), '\t', EndTime(), '\t', 
            _playerIdentifier, '\t', _playerColor, '\t', _currentState, '\t', _otherCurrentState, '\t', _action, '\t', 
            _actionId, '\t', _ending, '\t', _target, '\t', PlayerOnePosition(), '\t', PlayerTwoPosition(), '\t', Relationship());
    }

    private string BeginTime()
    {
        return _beginTime.ToString("F3", CultureInfo.InvariantCulture);
    }

    private string EndTime()
    {
        return _endTime.ToString("F3", CultureInfo.InvariantCulture);
    }

    private string PlayerOnePosition()
    {
        return string.Concat('[', _playerOnePosition.x.ToString("F2", CultureInfo.InvariantCulture), ',',
            _playerOnePosition.y.ToString("F2", CultureInfo.InvariantCulture), ']');
    }
    
    private string PlayerTwoPosition()
    {
        return string.Concat('[', _playerTwoPosition.x.ToString("F2", CultureInfo.InvariantCulture), ',',
            _playerTwoPosition.y.ToString("F2", CultureInfo.InvariantCulture), ']');
    }
    
    private string Relationship()
    {
        return string.Concat('[', _relationship.x.ToString("F2", CultureInfo.InvariantCulture), ',',
            _relationship.y.ToString("F2", CultureInfo.InvariantCulture), ']');
    }

    public float GetTargetAsFloat()
    {
        float.TryParse(_target, out var targetAsFloat);
        return targetAsFloat;
    }
    
}
