using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Parser : ScriptableObject
{
    private enum Command { Go, Control, Enter, Leave, Start, Stop, Apply, Release, Toggle, Patrol, Follow, Collect, Hold, Send, Play, Menu, Timer, Time, Fps, Restart, Version, Help, None }

    private enum Argument { On, Off, Red, Orange, Yellow, Green, Blue, Purple, Grey, Cloud, Ground, Handcar, Cart, All, Trigger, Soul, Rain, Shake, Brake, Boost, Companion, Left, Right, Notes, Log }
    private enum Mode { Set, Show, Hide }

    private Mode _mode;
    private readonly List<Command> _commands = new List<Command>();
    private readonly List<Argument> _arguments = new List<Argument>();
    private float _value;

    private Player _player;
    private ControllableObject _target;
    private ControllableObject _actor;
    
    public void Process(string[] commands)
    {
        _mode = Mode.Show;
        _commands.Clear();
        _arguments.Clear();
        _value = -1f;
        foreach (var command in commands)
        {
            switch (command.ToLower())
            {
                case "set":
                    _mode = Mode.Set;
                    break;
                case "show": case "open":
                    _mode = Mode.Show;
                    break;
                case "hide": case "close":
                    _mode = Mode.Hide;
                    break;
                case "go": case "goto": case "move":
                    _commands.Add(Command.Go);
                    break;
                case "control": 
                    _commands.Add(Command.Control);
                    break;
                case "enter": case "animate":
                    _commands.Add(Command.Enter);
                    break;
                case "leave": case "disembody":
                    _commands.Add(Command.Leave);
                    break;
                case "start": case "begin":
                    _commands.Add(Command.Start);
                    break;
                case "stop": case "end":
                    _commands.Add(Command.Stop);
                    break;
                case "apply": case "pull":
                    _commands.Add(Command.Apply);
                    break;
                case "release": case "loosen":
                    _commands.Add(Command.Release);
                    break;
                case "toggle": case "flip":
                    _commands.Add(Command.Toggle);
                    break;
                case "patrol":
                    _commands.Add(Command.Patrol);
                    break;
                case "follow": case "following":
                    _commands.Add(Command.Follow);
                    break;
                case "collect": case "collecting":
                    _commands.Add(Command.Collect);
                    break;
                case "hold":
                    _commands.Add(Command.Hold);
                    break;
                case "send":
                    _commands.Add(Command.Send);
                    break;
                case "play":
                    _commands.Add(Command.Play);
                    break;
                case "menu": case "ui":
                    _commands.Add(Command.Menu);
                    break;
                case "timer": case "clock": case "hourglass":
                    _commands.Add(Command.Timer);
                    break;
                case "time": case "timescale":
                    _commands.Add(Command.Time);
                    break;
                case "fps":
                    _commands.Add(Command.Fps);
                    break;
                case "restart": case "reload":
                    _commands.Add(Command.Restart);
                    break;
                case "version":
                    _commands.Add(Command.Version);
                    break;
                case "help":
                    _commands.Add(Command.Help);
                    break;
                case "on": case "active":
                    _arguments.Add(Argument.On);
                    break;
                case "off": case "inactive":
                    _arguments.Add(Argument.Off);
                    break;
                case "red": case "rd":
                    _arguments.Add(Argument.Red);
                    break;
                case "orange": case "or":
                    _arguments.Add(Argument.Orange);
                    break;
                case "yellow": case "yl":
                    _arguments.Add(Argument.Yellow);
                    break;
                case "green": case "gr":
                    _arguments.Add(Argument.Green);
                    break;
                case "blue": case "bl":
                    _arguments.Add(Argument.Blue);
                    break;
                case "purple": case "violet": case "pr":
                    _arguments.Add(Argument.Purple);
                    break;
                case "grey": case "gray":
                    _arguments.Add(Argument.Grey);
                    break;
                case "cloud": case "wolke": case "cd":
                    _arguments.Add(Argument.Cloud);
                    break;
                case "ground": case "earth": case "boden": case "gd":
                    _arguments.Add(Argument.Ground);
                    break;
                case "handcar": case "train": case "draisine": case "hc":
                    _arguments.Add(Argument.Handcar);
                    break;
                case "cart": case "minecart": case "lore": case "ct":
                    _arguments.Add(Argument.Cart);
                    break;
                case "all": case "everything":
                    _arguments.Add(Argument.Cloud);
                    _arguments.Add(Argument.Ground);
                    _arguments.Add(Argument.Handcar);
                    _arguments.Add(Argument.Cart);
                    break;
                case "trigger":
                    _arguments.Add(Argument.Trigger);
                    break;
                case "soul": case "soulform": case "seele":
                    _arguments.Add(Argument.Soul);
                    break;
                case "rain": case "raining": case "downpour": case "burst":
                    _arguments.Add(Argument.Rain);
                    break;
                case "shake": case "shaking": case "earthquake": case "quake": case "seism":
                    _arguments.Add(Argument.Shake);
                    break;
                case "brake": case "brakes": case "break": case "lever":
                    _arguments.Add(Argument.Brake);
                    break;
                case "boost": case "turbo": case "afterburner":
                    _arguments.Add(Argument.Boost);
                    break;
                case "companion":
                    _arguments.Add(Argument.Companion);
                    break;
                case "left": case "west": case "l":
                    _arguments.Add(Argument.Left);
                    break;
                case "right": case "east": case "r":
                    _arguments.Add(Argument.Right);
                    break;
                case "notes": case "note":
                    _arguments.Add(Argument.Notes);
                    break;
                case "log":
                    _arguments.Add(Argument.Log);
                    if (!_commands.Contains(Command.Send))
                    {
                        Controllers.Logs.CreateLogfile();
                        References.Terminal.AddEntry(Controllers.Logs.GetLogfileContents());
                    }
                    break;
                default:
                    float.TryParse(command, NumberStyles.Float, CultureInfo.InvariantCulture, out _value);
                    break;
            }
        }
        
        _player = null;
        _target = null;
        _actor = null;

        foreach (var argument in _arguments)
        {
            if (argument == Argument.Red)
            {
                if (References.Entities.PlayerOne.ColorId == 0)
                {
                    _player = References.Entities.PlayerOne;
                    if (_target != null)
                    {
                        _actor = _target;
                    }
                    _target = _player.Soul;
                }
                else if (References.Entities.PlayerTwo.ColorId == 0)
                {
                    _player = References.Entities.PlayerTwo;
                    if (_target != null)
                    {
                        _actor = _target;
                    }
                    _target = _player.Soul;
                }
            }
            else if (argument == Argument.Orange)
            {
                if (References.Entities.PlayerOne.ColorId == 1)
                {
                    _player = References.Entities.PlayerOne;
                    if (_target != null)
                    {
                        _actor = _target;
                    }
                    _target = _player.Soul;
                }
                else if (References.Entities.PlayerTwo.ColorId == 1)
                {
                    _player = References.Entities.PlayerTwo;
                    if (_target != null)
                    {
                        _actor = _target;
                    }
                    _target = _player.Soul;
                }
            }
            else if (argument == Argument.Yellow)
            {
                if (References.Entities.PlayerOne.ColorId == 2)
                {
                    _player = References.Entities.PlayerOne;
                    if (_target != null)
                    {
                        _actor = _target;
                    }
                    _target = _player.Soul;
                }
                else if (References.Entities.PlayerTwo.ColorId == 2)
                {
                    _player = References.Entities.PlayerTwo;
                    if (_target != null)
                    {
                        _actor = _target;
                    }
                    _target = _player.Soul;
                }
            }
            else if (argument == Argument.Green)
            {
                if (References.Entities.PlayerOne.ColorId == 3)
                {
                    _player = References.Entities.PlayerOne;
                    if (_target != null)
                    {
                        _actor = _target;
                    }
                    _target = _player.Soul;
                }
                else if (References.Entities.PlayerTwo.ColorId == 3)
                {
                    _player = References.Entities.PlayerTwo;
                    if (_target != null)
                    {
                        _actor = _target;
                    }
                    _target = _player.Soul;
                }
            }
            else if (argument == Argument.Blue)
            {
                if (References.Entities.PlayerOne.ColorId == 4)
                {
                    _player = References.Entities.PlayerOne;
                    if (_target != null)
                    {
                        _actor = _target;
                    }
                    _target = _player.Soul;
                }
                else if (References.Entities.PlayerTwo.ColorId == 4)
                {
                    _player = References.Entities.PlayerTwo;
                    if (_target != null)
                    {
                        _actor = _target;
                    }
                    _target = _player.Soul;
                }
            }
            else if (argument == Argument.Purple)
            {
                if (References.Entities.PlayerOne.ColorId == 5)
                {
                    _player = References.Entities.PlayerOne;
                    if (_target != null)
                    {
                        _actor = _target;
                    }
                    _target = _player.Soul;
                }
                else if (References.Entities.PlayerTwo.ColorId == 5)
                {
                    _player = References.Entities.PlayerTwo;
                    if (_target != null)
                    {
                        _actor = _target;
                    }
                    _target = _player.Soul;
                }
            }
            else if (argument == Argument.Grey)
            {
                if (References.Entities.PlayerOne.ColorId == 6)
                {
                    _player = References.Entities.PlayerOne;
                    if (_target != null)
                    {
                        _actor = _target;
                    }
                    _target = _player.Soul;
                }
                else if (References.Entities.PlayerTwo.ColorId == 6)
                {
                    _player = References.Entities.PlayerTwo;
                    if (_target != null)
                    {
                        _actor = _target;
                    }
                    _target = _player.Soul;
                }
            }
            else if (argument == Argument.Cloud)
            {
                if (_target != null)
                {
                    _actor = _target;
                }
                _target = References.Entities.Cloud;
            }
            else if (argument == Argument.Ground)
            {
                if (_target != null)
                {
                    _actor = _target;
                }
                _target = References.Entities.Ground;
            }
            else if (argument == Argument.Handcar)
            {
                if (_target != null)
                {
                    _actor = _target;
                }
                _target = References.Entities.Handcar;
            }
            else if (argument == Argument.Cart)
            {
                if (_target != null)
                {
                    _actor = _target;
                }
                _target = References.Entities.Cart;
            }
        }

        if (_commands.Count == 0)
        {
            _commands.Add(Command.None);
        }

        foreach (var item in _commands)
        {
            ProcessCommand(item);
        }
    }

    private void ProcessCommand(Command command)
    {
        switch (command)
            {
                case Command.Go:
                    if (_target != null)
                    {
                        var distanceText = (_value > 0f ? _value + (Mathf.Approximately(_value, 1f) ? " unit " : " units ") : "");
                        if (_arguments.Contains(Argument.Left))
                        {
                            _target.SimulateInput(InputController.Type.Left, InputController.Mode.Hold, _value);
                            References.Terminal.AddEntry("<grey>" + _target.name + " moves " + distanceText + "to the left.</>");
                        }
                        else if (_arguments.Contains(Argument.Right))
                        {
                            _target.SimulateInput(InputController.Type.Right, InputController.Mode.Hold, _value);
                            References.Terminal.AddEntry("<grey>" + _target.name + " moves " + distanceText + "to the right.</>");
                        }
                    }
                    ProcessCommand(Command.Enter);
                    
                    break;
                case Command.Control:
                    if (_arguments.Contains(Argument.Red))
                    {
                        References.Entities.PlayerOne.TakeControl();
                        References.Terminal.AddEntry("You are now controlling <red>Red</>.");
                    }
                    else if (_arguments.Contains(Argument.Blue))
                    {
                        References.Entities.PlayerTwo.TakeControl();
                        References.Terminal.AddEntry("You are now controlling <blue>Blue</>.");
                    }
                    break;
                case Command.Enter:
                    if (_player != null)
                    {
                        if (_arguments.Contains(Argument.Cloud))
                        {
                            _player.MoveToTarget(References.Entities.Cloud);
                        }
                        else if (_arguments.Contains(Argument.Ground))
                        {
                            _player.MoveToTarget(References.Entities.Ground);
                        }
                        else if (_arguments.Contains(Argument.Handcar))
                        {
                            _player.MoveToTarget(References.Entities.Handcar);
                        }
                        else if (_arguments.Contains(Argument.Cart))
                        {
                            _player.MoveToTarget(References.Entities.Cart);
                        }
                        else if (_arguments.Contains(Argument.Soul))
                        {
                            ProcessCommand(Command.Leave);
                        }
                    }
                    break;
                case Command.Leave:
                    if (_player != null)
                    {
                        if (_player.CurrentType != ControllableObject.Type.Soul)
                        {
                            _player.ReturnToSoul(true);
                        }
                    }
                    break;
                case Command.Start:
                    if (_arguments.Contains(Argument.Rain))
                    {
                        var durationText = (_value > 0f ? "\nThe rain will last for <cyan>" + _value + (Mathf.Approximately(_value, 1f) ? " second" : " seconds") + ".</>" : "");
                        References.Entities.Cloud.SimulateInput(InputController.Type.Space, InputController.Mode.Hold, _value);
                        References.Terminal.AddEntry("<grey><cyan>Rain</> starts to fall." + durationText + "</>");
                    }
                    else if (_arguments.Contains(Argument.Shake))
                    {
                        var durationText = (_value > 0f ? "\nThe earthquake will last for <cyan>" + _value + (Mathf.Approximately(_value, 1f) ? " second" : " seconds") + ".</>" : "");
                        References.Entities.Ground.SimulateInput(InputController.Type.Space, InputController.Mode.Hold, _value);
                        References.Terminal.AddEntry("<grey>The <orange>ground</> starts to <orange>shake</>." + durationText + "</>");
                    }
                    else if (_arguments.Contains(Argument.Boost))
                    {
                        var durationText = (_value > 0f ? "\nIt will last for <cyan>" + _value + (Mathf.Approximately(_value, 1f) ? " second" : " seconds") + ".</>" : "");
                        References.Entities.Handcar.SimulateInput(InputController.Type.Space, InputController.Mode.Hold, _value);
                        References.Terminal.AddEntry("<grey>The handcar ignites its <orange>boost</>." + durationText + "</>");
                    }
                    break;
                case Command.Stop:
                    if (_arguments.Contains(Argument.Rain))
                    {
                        if (References.Entities.Cloud.IsRaining)
                        {
                            References.Terminal.AddEntry("<grey><white>Cloud</> stops and <cyan>it stops raining</>.</>");
                            References.Entities.Cloud.StopInputSimulation(4);
                        }
                        else
                        {
                            References.Terminal.AddEntry("<grey>...but it didn't rain at all.</>");
                        }
                    }
                    else if (_arguments.Contains(Argument.Shake))
                    {
                        if (References.Entities.Ground.IsShaking)
                        {
                            References.Terminal.AddEntry("<grey><brown>Ground</> <orange>stops shaking</>.</>");
                            References.Entities.Ground.StopInputSimulation(4);
                        }
                        else
                        {
                            References.Terminal.AddEntry("<grey><brown>Ground</> did not shake at all.</>");
                        }
                    }
                    else if (_arguments.Contains(Argument.Boost))
                    {
                        if (References.Entities.Handcar.Action.IsPerformingAction())
                        {
                            References.Terminal.AddEntry("<grey>Handcar <orange>deactivates boost</>.</>");
                            References.Entities.Handcar.StopInputSimulation(4);
                        }
                        else
                        {
                            References.Terminal.AddEntry("<grey>The handcar's boost wasn't even active.</>");
                        }
                    }
                    else if (_target != null)
                    {
                        if (_commands.Contains(Command.Stop))
                        {
                            _target.StopInputSimulation(-1);
                            References.Terminal.AddEntry("<grey>" + _target.name + " stops.</>");
                        }
                    }
                    break;
                case Command.Apply:
                    if (_arguments.Contains(Argument.Brake))
                    {
                        if (!References.Entities.Cart.Action.IsPerformingAction())
                        {
                            if (References.Entities.Cart.Action.TryToPerform())
                            {
                                References.Terminal.AddEntry("<grey>The cart's brake is now <red>applied</>.</>");
                            }
                            else
                            {
                                References.Terminal.AddEntry("<grey>The cart's brake is currently <purple>exhausted</>.</>");
                            }
                        }
                        else
                        {
                            References.Terminal.AddEntry("<grey>The cart's brake is already applied.</>");
                        }
                    }
                    break;
                case Command.Release:
                    if (_arguments.Contains(Argument.Brake))
                    {
                        if (References.Entities.Cart.Action.IsPerformingAction())
                        {
                            References.Entities.Cart.Action.Stop();
                            References.Terminal.AddEntry("<grey>The cart's brake is now <green>released</>.</>");
                        }
                        else
                        {
                            References.Terminal.AddEntry("<grey>The cart's brake is already released.</>");
                        }
                    }
                    break;
                case Command.Toggle:
                    if (_arguments.Contains(Argument.Brake))
                    {
                        if (References.Entities.Cart.IsBraked)
                        {
                            ProcessCommand(Command.Release);
                        }
                        else
                        {
                            ProcessCommand(Command.Apply);
                        }
                    } 
                    else if (_arguments.Contains(Argument.Rain))
                    {
                        if (References.Entities.Cloud.IsRaining)
                        {
                            ProcessCommand(Command.Stop);
                        }
                        else
                        {
                            ProcessCommand(Command.Start);
                        }
                    }
                    else if (_arguments.Contains(Argument.Shake))
                    {
                        if (References.Entities.Ground.IsShaking)
                        {
                            ProcessCommand(Command.Stop);
                        }
                        else
                        {
                            ProcessCommand(Command.Start);
                        }
                    }
                    break;
                case Command.Patrol:
                    if (_target != null)
                    {
                        if (_commands.Contains(Command.Stop))
                        {
                            _target.StopSimulatedInputList();
                            References.Terminal.AddEntry("<grey>" + _target.name + " stops patrolling.</>");
                            break;
                        }
                        _target.ResetSimulatedInputList();
                        _target.AddSimulatedInputToList(new InputInfo(InputController.Type.Left, -1f, 3f, 0f));
                        _target.AddSimulatedInputToList(new InputInfo(InputController.Type.Right, -1f, 6f, 0f));
                        _target.AddSimulatedInputToList(new InputInfo(InputController.Type.Left, -1f, 3f, 0f));
                        _target.ProcessSimulatedInputList(true);
                        References.Terminal.AddEntry("<grey>" + _target.name + " starts to patrol.</>");
                    }
                    break;
                case Command.Follow:
                    if (_target != null && _commands.Contains(Command.Stop))
                    {
                        _target.Follow(null);
                        References.Terminal.AddEntry("<grey>" + _target.name + " stops following.</>");
                        break;
                    }
                    if (_actor != null)
                    {
                        _actor.Follow(_target, _value);
                        References.Terminal.AddEntry("<grey>" + _actor.name + " follows " + _target.name + ".</>");
                    }
                    break;
                case Command.Collect:
                    if (_target != null)
                    {
                        if (_commands.Contains(Command.Stop))
                        {
                            _target.StopArtificialMovement();
                            References.Terminal.AddEntry("<grey>" + _target.name + " stops collecting notes.</>");
                            break;
                        }
                        _target.CollectNotes();
                        References.Terminal.AddEntry("<grey>" + _target.name + " starts collecting notes.</>");
                    }
                    break;
                case Command.Hold:
                    if (_player != null)
                    {
                        if (_commands.Contains(Command.Stop))
                        {
                            _player.Soul.HoldPosition(null);
                        }
                        else if (_target != null && _player.Soul != null &&
                                 _target.ObjectType != ControllableObject.Type.Soul)
                        {
                            _player.Soul.HoldPosition(_target);
                        }
                    }
                    break;
                case Command.Send:
                    if (_arguments.Contains(Argument.Log))
                    {
                        Controllers.Logs.SaveLogfile();
                    }
                    break;
                case Command.Play:
                    if (_arguments.Contains(Argument.Red) || _arguments.Contains(Argument.Blue))
                    {
                        ProcessCommand(Command.Control);
                        return;
                    }
                    break;
                case Command.Menu:
                    if (_arguments.Contains(Argument.On) || _mode == Mode.Show)
                    {
                        References.Menu.Open(_value >= 0f ? _value : 0.125f);
                    }
                    else if (_arguments.Contains(Argument.Off) || _mode == Mode.Hide)
                    {
                        References.Menu.Close(_value >= 0f ? _value : 0.125f);
                    }
                    break;
                case Command.Timer:
                    if (_arguments.Contains(Argument.On) || (_mode == Mode.Show && !_commands.Contains(Command.Start) && !_commands.Contains(Command.Stop)))
                    {
                        References.Timer.Show(_value >= 0f ? _value : 0.25f);
                    }
                    else if (_arguments.Contains(Argument.Off) || _mode == Mode.Hide)
                    {
                        References.Timer.Hide(_value >= 0f ? _value : 0.25f);
                    }
                    else if (_commands.Contains(Command.Start))
                    {
                        References.Timer.Continue();
                    }
                    else if (_commands.Contains(Command.Stop))
                    {
                        References.Timer.Stop();
                    }
                    else if (_mode == Mode.Set)
                    {
                        References.Timer.SetCountdown(_value > 0f ? _value : 60f);
                    }
                    break;
                case Command.Time:
                    if (_mode == Mode.Show)
                    {
                        References.Settings.ShowCurrentTimeScale();
                    }
                    else if (_mode == Mode.Set)
                    {
                        if (_value >= 0f && _value <= 100f)
                        {
                            References.Settings.SetTimeScale(_value);
                        }
                        else
                        {
                            References.Terminal.AddEntry("<red>Error:</> " + _value + " is not an allowed value for time scale.");
                        }
                    }
                    break;
                case Command.Fps:
                    if (_arguments.Contains(Argument.On) || _mode == Mode.Show && !_arguments.Contains(Argument.Off))
                    {
                        if (!Controllers.Ui.FpsDisplay.IsActive)
                        {
                            Controllers.Ui.FpsDisplay.SetActive(true);
                            References.Terminal.AddEntry("<orange>FPS display</> is now <green>on</>.");
                        }
                        else
                        {
                            References.Terminal.AddEntry("<orange>FPS display</> is already active.");
                        }
                        
                    }
                    else if (_arguments.Contains(Argument.Off) || _mode == Mode.Hide)
                    {
                        if (Controllers.Ui.FpsDisplay.IsActive)
                        {
                            Controllers.Ui.FpsDisplay.SetActive(false);
                            References.Terminal.AddEntry("<orange>FPS display</> is now <red>off</>.");
                        }
                        else
                        {
                            References.Terminal.AddEntry("<orange>FPS display</> is already inactive.");
                        }
                    }
                    break;
                case Command.Restart:
                    if (_value >= 1)
                    {
                        References.Io.Init();
                        References.Settings.Init();
                        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(References.Settings.DefaultScene.SceneName);
                        break;
                    }
                    UnityEngine.SceneManagement.SceneManager.LoadScene(References.Settings.MainMenuScene.SceneName);
                    break;
                case Command.Version:
                    References.Terminal.AddEntry("<blue>\"Moves\"</> version " + Application.version + ".");
                    break;
                case Command.Help:
                    References.Terminal.AddEntry("<green>Players</>\nred | blue\n<green>Objects</>\nsoul | cloud | ground | cart | handcar\n<green>Directions</>\nleft | right\n<green>Actions</>\nrain | shake | brake | boost\n<green>Commands</>\ncontrol [player]\n[player] enter [object]\n[player] leave [object]\n[player] go [object]\n[object] go [direction] [t*]\nstart [action] [t*] | stop [action]\nshow/hide [object] [t*]\ntoggle [action]\nset timescale [x]\nset timer [t] | show/hide/start/stop timer\nshow/hide fps\nlog | send log\nrestart | version | play\n<yellow>* optional value</>");
                    break;
                case Command.None:
                    if (_mode == Mode.Show)
                    {
                        if (_arguments.Contains(Argument.Cloud))
                        {
                            References.Entities.ActivateObject(ControllableObject.Type.Cloud, _value >= 0f ? _value : 1f);
                        }
                        if (_arguments.Contains(Argument.Handcar))
                        {
                            References.Entities.ActivateObject(ControllableObject.Type.Handcar, _value >= 0f ? _value : 1f);
                        }
                        if (_arguments.Contains(Argument.Cart))
                        {
                            References.Entities.ActivateObject(ControllableObject.Type.Cart, _value >= 0f ? _value : 1f);
                        }
                        if (_arguments.Contains(Argument.Ground))
                        {
                            References.Entities.ActivateObject(ControllableObject.Type.Ground, _value >= 0f ? _value : 1f);
                        }
                        if (_arguments.Contains(Argument.Notes))
                        {
                            References.Events.ChangeNoteVisibility(true, _value >= 0f ? _value : 1f);
                        }
                        if (_arguments.Contains(Argument.Companion))
                        {
                            References.Settings.EnableCompanion(true);
                        }
                    }
                    else if (_mode == Mode.Hide)
                    {
                        if (_arguments.Contains(Argument.Cloud))
                        {
                            References.Entities.DeactivateObject(ControllableObject.Type.Cloud, _value >= 0f ? _value : 1f);
                        }
                        if (_arguments.Contains(Argument.Handcar))
                        {
                            References.Entities.DeactivateObject(ControllableObject.Type.Handcar, _value >= 0f ? _value : 1f);
                        }
                        if (_arguments.Contains(Argument.Cart))
                        {
                            References.Entities.DeactivateObject(ControllableObject.Type.Cart, _value >= 0f ? _value : 1f);
                        }
                        if (_arguments.Contains(Argument.Ground))
                        {
                            References.Entities.DeactivateObject(ControllableObject.Type.Ground, _value >= 0f ? _value : 1f);
                        }
                        if (_arguments.Contains(Argument.Notes))
                        {
                            References.Events.ChangeNoteVisibility(false, _value >= 0f ? _value : 1f);
                        }
                        if (_arguments.Contains(Argument.Companion))
                        {
                            References.Settings.EnableCompanion(false);
                        }
                    }
                    else if (_mode == Mode.Set)
                    {
                        if (_arguments.Contains(Argument.Trigger) && _target != null)
                        {
                            if (_arguments.Contains(Argument.On))
                            {
                                _target.SetSoulTriggerActive(true);
                            }
                            else if (_arguments.Contains(Argument.Off))
                            {
                                _target.SetSoulTriggerActive(false);
                            }
                        }
                    }
                    break;
            }
    }
}
