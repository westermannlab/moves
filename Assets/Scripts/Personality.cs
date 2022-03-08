using System.Collections;
using UnityEngine;

public class Personality : ScriptableObject
{
    public enum Role { Soul, Cloud, Handcar, Ground, Cart }
    public enum Motivation { Demand, Avoid }
    public enum Act { None, RainOnNotes, RainOnHandcar, RainOnCart, RainOnGround, HandcarBoost, HandcarWait, GroundShake, GroundWait, CartJump, CartBrake }

    public float UpdateRoleInterval = 2f;
    public float MoveInterval = 2f;
    public float ActionInterval = 2f;
    public float MinTimeBetweenDecisions = 3f;
    public float ProximityLimit = 1f;
    
    public Transition[] Transitions;

    private Player _player;
    private Act _currentAction;
    private ControllableObject.Type _otherType;
    private ControllableObject.Type _nextType;
    private ControllableObject.Type _targetType;
    private float _roleTimer;
    private float _actionTimer;
    private float _lastRoleUpdateTime;
    private float _lastActionUpdateTime;
    private int _currentUpdateRolePriority;
    private bool _isActive;

    public void LoadPersonalityFromData(Player player)
    {
        _player = player;
        var data = References.Io.GetPersonalitiesData();
        var personalityIndex = data.personalities[_player.ColorId].treatAs >= 0
            ? data.personalities[_player.ColorId].treatAs
            : _player.ColorId;
        
        if (personalityIndex < 0 || personalityIndex >= data.personalities.Length) return;

        var personality = data.personalities[personalityIndex];
        _isActive = personality.isActive;
        Transitions = personality.transitions;
        foreach (var transition in Transitions)
        {
            transition.Init();
        }
        UpdateRoleInterval = personality.updateRoleInterval;
        MoveInterval = personality.moveInterval;
        ActionInterval = personality.actionInterval;
        MinTimeBetweenDecisions = personality.minTimeBetweenDecisions;
        ProximityLimit = personality.proximityLimit;

        _player.AssessmentCaption = personality.caption;
        _player.AssessmentAdjective = personality.adjective;

        _lastRoleUpdateTime = -MinTimeBetweenDecisions;
        _lastActionUpdateTime = 0f;

        if (_isActive)
        {
            References.Coroutines.StartCoroutine(ArtificialMotivationRoutine());
            References.Coroutines.StartCoroutine(DetermineActionRoutine());
        }
    }

    public ControllableObject.Type GetNextType(int index = -1)
    {
        var role = TypeToRole(_player.CurrentType);
        if (index < 0)
        {
            var otherRole = TypeToRole(_player.Counterpart.CurrentType);
            index = (int) otherRole;
        }
        
        if (index >= Transitions.Length)
        {
            Debug.Log("Get next type called with " + _player.Counterpart.CurrentType + "!");
            return ControllableObject.Type.Soul;
        }

        _currentUpdateRolePriority = Transitions[index].priority;

        return RoleToType(Transitions[index].Evaluate(role));
    }

    private ControllableObject.Type GetMostLikelyType(int index = -1)
    {
        var role = TypeToRole(_player.CurrentType);
        if (index < 0)
        {
            var otherRole = TypeToRole(_player.Counterpart.CurrentType);
            index = (int) otherRole;
        }
        
        if (index >= Transitions.Length)
        {
            Debug.Log("Get most likely type called with " + _player.Counterpart.CurrentType + "!");
            return ControllableObject.Type.Soul;
        }

        _currentUpdateRolePriority = Transitions[index].priority;
        return RoleToType(Transitions[index].GetMostLikelyRole(role));
    }

    private void DetermineAction()
    {
        if (Transitions.Length <= 6 || _lastActionUpdateTime > Time.time - MinTimeBetweenDecisions) return;
        if (_player.CurrentType == ControllableObject.Type.Soul) return;
        switch (_player.CurrentType)
        {
            case ControllableObject.Type.Cloud:
                
                if (_player.GetCurrentControllableObject().Action.IsPerformingAction()) break;
                
                switch (_player.Counterpart.CurrentType)
                {
                    case ControllableObject.Type.Handcar:
     
                        if (References.Entities.Handcar.TimeSinceLastSectorChange() > 2f)
                        {
                            // CLOUD DEMAND -> RAIN ON NOTES
                            if (Transitions[6].Evaluate(Role.Cloud, Motivation.Demand))
                            {
                                References.Entities.Cloud.RainOnNearNote();
                                References.Terminal.AddEntry("Cloud drizzles. It wants handcar to act on its own.");
                                _currentAction = Act.RainOnNotes;
                                _lastActionUpdateTime = Time.time;
                            }
                            else if (_currentAction != Act.None)
                            {
                                // CLOUD AFTER DEMAND -> FOLLOW HANDCAR
                                References.Entities.Cloud.Follow(References.Entities.Handcar, ProximityLimit);
                                // References.Terminal.ProcessCommand("cloud follow handcar " + ProximityLimit);
                                References.Terminal.AddEntry("Cloud follows handcar.");
                                _currentAction = Act.None;
                                _lastActionUpdateTime = Time.time;
                            }
                        }
                        else if (References.Entities.Handcar.TimeSinceLastSectorChange() < 2f)
                        {
                            // CLOUD AVOID -> RAIN ON HANDCAR
                            if (Transitions[6].Evaluate(Role.Cloud, Motivation.Avoid))
                            {
                                References.Entities.Cloud.RainOnHandcar();
                                References.Terminal.AddEntry("Cloud rains heavily. It dislikes handcar's independence.");
                                _currentAction = Act.RainOnHandcar;
                                _lastActionUpdateTime = Time.time;
                            }
                            else if (_currentAction != Act.None)
                            {
                                // CLOUD AFTER AVOID -> FOLLOW HANDCAR
                                References.Entities.Cloud.Follow(References.Entities.Handcar, ProximityLimit);
                                // References.Terminal.ProcessCommand("cloud follow handcar " + ProximityLimit);
                                References.Terminal.AddEntry("Cloud follows handcar.");
                                _currentAction = Act.None;
                                _lastActionUpdateTime = Time.time;
                            }
                        }
                        
                        break;
                    
                    case ControllableObject.Type.Cart:
                        // CLOUD DEMAND -> RAIN ON CART
                        if (Transitions[6].Evaluate(Role.Cloud, Motivation.Demand))
                        {
                            References.Entities.Cloud.RainOnCart();
                            References.Terminal.AddEntry("Cloud drizzles. It would rather see " + _player.Counterpart.PlayerName + " animate handcar.");
                            _currentAction = Act.RainOnCart;
                            _lastActionUpdateTime = Time.time;
                        }
                        else if (_currentAction != Act.None)
                        {
                            // CLOUD AFTER RAIN ON CART -> FOLLOW CART
                            References.Entities.Cloud.Follow(References.Entities.Cart, ProximityLimit);
                            // References.Terminal.ProcessCommand("cloud follow handcar " + ProximityLimit);
                            References.Terminal.AddEntry("Cloud follows cart.");
                            _currentAction = Act.None;
                            _lastActionUpdateTime = Time.time;
                        }
                        break;
                    
                    case ControllableObject.Type.Ground:
                        // CLOUD DEMAND -> RAIN ON GROUND
                        if (Transitions[6].Evaluate(Role.Cloud, Motivation.Demand))
                        {
                            References.Entities.Cloud.RainOnGround();
                            References.Terminal.AddEntry("Cloud drizzles. It would rather see " + _player.Counterpart.PlayerName + " animate handcar.");
                            _currentAction = Act.RainOnGround;
                            _lastActionUpdateTime = Time.time;
                        }
                        else
                        {
                            // CLOUD AFTER RAIN ON GROUND -> DO NOTHING
                            _currentAction = Act.None;
                        }
                        break;
                }
                break;
            case ControllableObject.Type.Handcar:
                
                if (_player.GetCurrentControllableObject().Action.IsPerformingAction()) break;

                switch (_player.Counterpart.CurrentType)
                {
                    case ControllableObject.Type.Cloud:
                        
                        if (References.Entities.Handcar.CurrentSlowdown > 0f || References.Actions.Rain.RainMode == Rain.Mode.Heavy)
                        {
                            // HANDCAR DEMAND -> USE BOOST
                            if (Transitions[6].Evaluate(Role.Handcar, Motivation.Demand))
                            {
                                References.Entities.Handcar.Boost();
                                References.Terminal.AddEntry("Handcar accelerates. It rejects being slowed down.");
                                _currentAction = Act.HandcarBoost;
                                _lastActionUpdateTime = Time.time;
                            }
                        }
                        else if (_currentAction != Act.HandcarWait && !Controllers.Level.AreThereSparklingNotes())
                        {
                            // HANDCAR AVOID -> STAND STILL
                            if (Transitions[6].Evaluate(Role.Handcar, Motivation.Avoid))
                            {
                                References.Entities.Handcar.StopArtificialMovement();
                                References.Terminal.AddEntry("Handcar stops. It's overwhelmed by its independence.");
                                _currentAction = Act.HandcarWait;
                                _lastActionUpdateTime = Time.time;
                            }
                        }
                        else if (_currentAction == Act.HandcarWait && Controllers.Level.AreThereSparklingNotes())
                        {
                            References.Entities.Handcar.CollectNotes();
                            References.Terminal.AddEntry("Handcar starts collecting notes.");
                            _currentAction = Act.None;
                            _lastActionUpdateTime = Time.time;
                        
                        }
                        break;
                    
                    case ControllableObject.Type.Cart:
                        
                        // NO BOOST
                        
                        if (_currentAction != Act.HandcarWait && !Controllers.Level.AreThereSparklingNotes())
                        {
                            // HANDCAR AVOID -> STAND STILL
                            if (Transitions[6].Evaluate(Role.Handcar, Motivation.Avoid))
                            {
                                References.Entities.Handcar.StopArtificialMovement();
                                References.Terminal.AddEntry("Handcar stops. It's overwhelmed by its independence.");
                                _currentAction = Act.HandcarWait;
                                _lastActionUpdateTime = Time.time;
                            }
                        }
                        else if (_currentAction == Act.HandcarWait && (Controllers.Level.AreThereSparklingNotes() || !Transitions[6].Evaluate(Role.Handcar, Motivation.Avoid)))
                        {
                            References.Entities.Handcar.CollectNotes();
                            References.Terminal.AddEntry("Handcar starts collecting notes with cart in tow.");
                            _currentAction = Act.None;
                            _lastActionUpdateTime = Time.time;
                        }
                        
                        break;
                    
                    case ControllableObject.Type.Ground:

                        if (References.Entities.Handcar.IsGoingUpSlope())
                        {
                            // HANDCAR DEMAND -> BOOST UP SLOPE
                            if (Transitions[6].Evaluate(Role.Handcar, Motivation.Demand))
                            {
                                References.Entities.Handcar.Boost();
                                References.Terminal.AddEntry("Handcar accelerates. It refuses to be taken downhill.");
                                _currentAction = Act.HandcarBoost;
                                _lastActionUpdateTime = Time.time;
                                break;
                            }
                        }
                        
                        if (_currentAction != Act.HandcarWait && !Controllers.Level.AreThereSparklingNotes())
                        {
                            // HANDCAR AVOID -> STAND STILL
                            if (Transitions[6].Evaluate(Role.Handcar, Motivation.Avoid))
                            {
                                References.Entities.Handcar.StopArtificialMovement();
                                References.Terminal.AddEntry("Handcar stops. It's overwhelmed by its independence.");
                                _currentAction = Act.HandcarWait;
                                _lastActionUpdateTime = Time.time;
                            }
                        }
                        else if (_currentAction == Act.HandcarWait && (Controllers.Level.AreThereSparklingNotes() || !Transitions[6].Evaluate(Role.Handcar, Motivation.Avoid)))
                        {
                            References.Entities.Handcar.CollectNotes();
                            References.Terminal.AddEntry("Handcar starts collecting notes despite all adversaries.");
                            _currentAction = Act.None;
                            _lastActionUpdateTime = Time.time;
                        }
                        break;
                }
                
                break;
            case ControllableObject.Type.Ground:
                
                if (_player.GetCurrentControllableObject().Action.IsPerformingAction()) break;

                switch (_player.Counterpart.CurrentType)
                {
                    case ControllableObject.Type.Cart:

                        if (References.Entities.Cart.IsBraked && !References.Entities.Ground.IsEven())
                        {
                            // GROUND DEMAND -> SHAKE
                            if (Transitions[6].Evaluate(Role.Ground, Motivation.Demand))
                            {
                                References.Entities.Ground.Shake();
                                References.Terminal.AddEntry("Ground shakes. It doesn't appreciate cart's reluctance.");
                                _currentAction = Act.GroundShake;
                                _lastActionUpdateTime = Time.time;
                            }
                        }
                        else if (_currentAction != Act.GroundWait && !References.Entities.Cart.HasJumpedRecently(6f))
                        {
                            // GROUND AVOID -> STAND STILL
                            if (Transitions[6].Evaluate(Role.Ground, Motivation.Avoid))
                            {
                                References.Entities.Ground.StopArtificialMovement();
                                References.Terminal.AddEntry("Ground stops. It refuses to take responsibility for cart.");
                                _currentAction = Act.GroundWait;
                                _lastActionUpdateTime = Time.time;
                            }
                        }
                        else if (_currentAction == Act.GroundWait && References.Entities.Cart.HasJumpedRecently(6f))
                        {
                            // GROUND AFTER AVOID -> CONTINUE ROTATION
                            References.Entities.Ground.CollectNotes();
                            References.Terminal.AddEntry("Ground starts rotating.");
                            _currentAction = Act.None;
                            _lastActionUpdateTime = Time.time;
                    
                        }
                        
                        break;
                    
                    case ControllableObject.Type.Cloud:
                        // GROUND DEMAND -> TIME TO DISEMBODY, CLOUD
                        if (Transitions[6].Evaluate(Role.Ground, Motivation.Demand))
                        {
                            References.Entities.Ground.Shake();
                            References.Terminal.AddEntry("Ground shakes. Why in the world would " + _player.Counterpart.PlayerName + " animate cloud?");
                            _currentAction = Act.GroundShake;
                            _lastActionUpdateTime = Time.time;
                        }
                        break;
                    
                    case ControllableObject.Type.Handcar:
                        if (!References.Entities.Ground.IsEven() && References.Entities.Handcar.IsGoingUpSlope())
                        {
                            // GROUND DEMAND -> SHAKE HANDCAR
                            if (Transitions[6].Evaluate(Role.Ground, Motivation.Demand))
                            {
                                References.Entities.Ground.Shake();
                                References.Terminal.AddEntry("Ground shakes. Handcar must not go uphill!");
                                _currentAction = Act.GroundShake;
                                _lastActionUpdateTime = Time.time;
                            }
                        }
                        else if (_currentAction != Act.GroundWait && !References.Entities.Cart.HasJumpedRecently(6f))
                        {
                            // GROUND AVOID -> STAND STILL
                            if (Transitions[6].Evaluate(Role.Ground, Motivation.Avoid))
                            {
                                References.Entities.Ground.StopArtificialMovement();
                                References.Terminal.AddEntry("Ground stops. It refuses to take any responsibility.");
                                _currentAction = Act.GroundWait;
                                _lastActionUpdateTime = Time.time;
                            }
                        }
                        else if (_currentAction == Act.GroundWait && !Transitions[6].Evaluate(Role.Ground, Motivation.Avoid))
                        {
                            // GROUND AFTER AVOID -> CONTINUE ROTATION
                            References.Entities.Ground.CollectNotes();
                            References.Terminal.AddEntry("Ground starts rotating.");
                            _currentAction = Act.None;
                            _lastActionUpdateTime = Time.time;
                    
                        }
                        break;
                }

                break;
            case ControllableObject.Type.Cart:
                
                if (_player.GetCurrentControllableObject().Action.IsPerformingAction()) break;

                switch (_player.Counterpart.CurrentType)
                {
                    case ControllableObject.Type.Ground:

                        if (References.Entities.Ground.IsEven())
                        {
                            // CART DEMAND -> JUMP
                            if (Transitions[6].Evaluate(Role.Cart, Motivation.Demand))
                            {
                                References.Entities.Cart.Brake(false);
                                References.Terminal.AddEntry("Cart jumps. It would like to be guided by ground.");
                                _currentAction = Act.CartJump;
                                _lastActionUpdateTime = Time.time;
                            }
                        }
                        else
                        {
                            // CART AVOID -> BRAKE
                            if (Transitions[6].Evaluate(Role.Cart, Motivation.Avoid))
                            {
                                References.Entities.Cart.Brake(true);
                                References.Terminal.AddEntry("Cart brakes. It doesn't like being controlled by ground.");
                                _currentAction = Act.CartBrake;
                                _lastActionUpdateTime = Time.time;
                            }
                        }
                        
                        break;
                    
                    case ControllableObject.Type.Cloud:

                        // CART DEMAND -> PLEASE DON'T BE CLOUD NOW
                        if (Transitions[6].Evaluate(Role.Cart, Motivation.Demand))
                        {
                            References.Entities.Cart.Brake(false);
                            References.Terminal.AddEntry("Cart jumps. It would appreciate guidance by " + _player.Counterpart.PlayerName + ".");
                            _currentAction = Act.CartJump;
                            _lastActionUpdateTime = Time.time;
                        }
                        
                        break;
                    
                    case ControllableObject.Type.Handcar:

                        // CART DEMAND -> PLEASE DON'T BE CLOUD NOW
                        if (Transitions[6].Evaluate(Role.Cart, Motivation.Avoid))
                        {
                            References.Entities.Cart.Brake(true);
                            References.Terminal.AddEntry("Cart brakes. It doesn't like being pushed around!");
                            _currentAction = Act.CartBrake;
                            _lastActionUpdateTime = Time.time;
                        }
                        
                        break;
                }

                break;
        }
    }

    public void LeaveCurrentRole()
    {
        _nextType = ControllableObject.Type.Soul;
        _currentUpdateRolePriority = 1;
        EnterNextType();
    }

    private Role TypeToRole(ControllableObject.Type type)
    {
        switch (type)
        {
            case ControllableObject.Type.Soul:
                return Role.Soul;
            case ControllableObject.Type.Cloud:
                return Role.Cloud;
            case ControllableObject.Type.Handcar:
                return Role.Handcar;
            case ControllableObject.Type.Ground:
                return Role.Ground;
            case ControllableObject.Type.Cart:
                return Role.Cart;
            default:
                Debug.Log("Type to role called with " + type + "!");
                return Role.Soul;
        }
    }
    
    private ControllableObject.Type RoleToType(Role role)
    {
        switch (role)
        {
            case Role.Soul:
                return ControllableObject.Type.Soul;
            case Role.Cloud:
                return ControllableObject.Type.Cloud;
            case Role.Handcar:
                return ControllableObject.Type.Handcar;
            case Role.Ground:
                return ControllableObject.Type.Ground;
            case Role.Cart:
                return ControllableObject.Type.Cart;
            default:
                Debug.Log("Role to type called with " + role + "!");
                return ControllableObject.Type.Soul;
        }
    }
    
    private IEnumerator ArtificialMotivationRoutine()
    {
        // 0 to 3 seconds delay
        yield return new WaitForSeconds(Random.value * 3f);
        
        _targetType = _player.CurrentType;

        _nextType = GetMostLikelyType();
        EnterNextType();
        
        while (Controllers.Input.IsGameRunning())
        {
            _roleTimer = 0f;
            _otherType = _player.Counterpart.CurrentType;
            while (_roleTimer < UpdateRoleInterval)
            {
                _roleTimer += Time.deltaTime;
                if (_player.Counterpart.CurrentType == ControllableObject.Type.Soul &&
                    _otherType != ControllableObject.Type.Soul)
                {
                    // if other player enters soul state
                    _nextType = GetNextType(5);
                    EnterNextType();
                    _otherType = _player.Counterpart.CurrentType;
                }
                yield return null;
            }

            if (_player.CurrentType == _targetType || _player.Counterpart.CurrentType == _targetType && _otherType != _targetType)
            {
                _nextType = GetNextType();
                EnterNextType();
            }
            
            yield return null;
        }
        DisableMovement();

        yield return null;
    }
    
    private IEnumerator DetermineActionRoutine()
    {
        while (Controllers.Input.IsGameRunning())
        {
            _actionTimer = 0f;
            
            while (_actionTimer < ActionInterval)
            {
                _actionTimer += Time.deltaTime;
                yield return null;
            }

            DetermineAction();
            yield return null;
        }

        yield return null;
    }

    private void EnterNextType()
    {
        if (_currentUpdateRolePriority <= 0 && _lastRoleUpdateTime > Time.time - MinTimeBetweenDecisions || !Controllers.Input.IsGameRunning()) return;
        var nextObject = References.Entities.GetObjectByType(_nextType);
        if (nextObject == null || _currentUpdateRolePriority <= 0 && nextObject.CurrentPlayer != null) return;
        if (_nextType != _player.CurrentType)
        {
            References.Terminal.ProcessCommand(string.Concat(_player.PlayerName, " enter ", _nextType.ToString()));
            _player.MoveToTarget(References.Entities.GetObjectByType(_nextType), _currentUpdateRolePriority > 0);
            _targetType = _nextType;
            _lastRoleUpdateTime = Time.time;
        }
    }

    public void BeforeLeavingRole()
    {
        if (_isActive)
        {
            DisableMovement();
        }
    }

    public void WhenRolesHaveChanged()
    {
        if (_isActive)
        {
            EnableMovement();
        }
    }

    private void EnableMovement()
    {
        if (_player == null) return;
        switch (_player.CurrentType)
        {
            case ControllableObject.Type.Cloud:
                if (_player.Counterpart.CurrentType == ControllableObject.Type.Handcar)
                {
                    References.Entities.Cloud.Follow(References.Entities.Handcar, ProximityLimit);
                }
                break;
            case ControllableObject.Type.Handcar:
                References.Entities.Handcar.CollectNotes();
                break;
            case ControllableObject.Type.Ground:
                if (_player.Counterpart.CurrentType == ControllableObject.Type.Handcar || _player.Counterpart.CurrentType == ControllableObject.Type.Cart)
                {
                    References.Entities.Ground.CollectNotes();
                }
                break;
        }
    }

    private void DisableMovement()
    {
        if (_player == null) return;
        switch (_player.CurrentType)
        {
            case ControllableObject.Type.Cloud:
                References.Entities.Cloud.StopArtificialMovement();
                break;
            case ControllableObject.Type.Handcar:
                References.Entities.Handcar.StopArtificialMovement();
                break;
            case ControllableObject.Type.Ground:
                References.Entities.Ground.StopArtificialMovement();
                break;
        }
    }
}
