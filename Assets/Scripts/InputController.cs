using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public enum Type { Left, Up, Right, Down, Space, Enter, Show, Record, Direction, Escape }
    public enum Mode { Press, Hold, Release }
    public enum State { Default, Terminal, Assessment, MainMenu, Ended }
    public enum KeyBindingType { WASD, Arrows }

    public State CurrentState;
    public Player ControllablePlayer;
    public KeyBindingType KeyBindings;
    
    private readonly Dictionary<Type, int> _keyPressCount = new Dictionary<Type, int>();
    private readonly Dictionary<Type, float> _keyHoldDuration = new Dictionary<Type, float>();

    private float _currentHorizontalInput;
    private float _previousHorizontalInput;
    private float _currentVerticalInput;
    private float _previousVerticalInput;

    private int _recordFrameCount;

    private void Awake()
    {
        InitDictionaries();
    }

    private void Start()
    {
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case State.Default:
                
                if (References.Settings.TerminalEnabled && Input.GetKeyDown(KeyCode.Return))
                {
                    References.Terminal.EnterWriteMode();
                    CurrentState = State.Terminal;
                    _keyPressCount[Type.Enter]++;
                }
                else if (Input.GetKey(KeyCode.Return))
                {
                    _keyHoldDuration[Type.Enter] += Time.unscaledDeltaTime;
                }
                
                if (ControllablePlayer == null) return;

                _currentHorizontalInput = Input.GetAxisRaw("Horizontal");
                _currentVerticalInput = Input.GetAxisRaw("Vertical");

                if (_currentHorizontalInput < 0f)
                {
                    if (_previousHorizontalInput > 0f)
                    {
                        ReleaseButton(Type.Right);
                        PressButton(Type.Left);
                    }
                    else if (_previousHorizontalInput < 0f)
                    {
                        HoldButton(Type.Left);
                    }
                    else
                    {
                        PressButton(Type.Left);
                    }
                }
                else if (_currentHorizontalInput > 0f)
                {
                    if (_previousHorizontalInput < 0f)
                    {
                        ReleaseButton(Type.Left);
                        PressButton(Type.Right);
                    }
                    else if (_previousHorizontalInput > 0f)
                    {
                        HoldButton(Type.Right);
                    }
                    else
                    {
                        PressButton(Type.Right);
                    }
                }
                else
                {
                    if (_previousHorizontalInput < 0f)
                    {
                        ReleaseButton(Type.Left);
                    }
                    else if (_previousHorizontalInput > 0f)
                    {
                        ReleaseButton(Type.Right);
                    }
                }

                if (!Mathf.Approximately(_currentHorizontalInput, 0f) ||
                    !Mathf.Approximately(_currentVerticalInput, 0f))
                {
                    ControllablePlayer.ProcessInput(Type.Direction, Mode.Hold, _currentHorizontalInput,
                        _currentVerticalInput);
                }

                _previousHorizontalInput = _currentHorizontalInput;

                if (_currentVerticalInput < 0f)
                {
                    if (_previousVerticalInput > 0f)
                    {
                        ReleaseButton(Type.Up);
                        PressButton(Type.Down);
                    }
                    else if (_previousVerticalInput < 0f)
                    {
                        HoldButton(Type.Down);
                    }
                    else
                    {
                        PressButton(Type.Down);
                    }
                }
                else if (_currentVerticalInput > 0f)
                {
                    if (_previousVerticalInput < 0f)
                    {
                        ReleaseButton(Type.Down);
                        PressButton(Type.Up);
                    }
                    else if (_previousVerticalInput > 0f)
                    {
                        HoldButton(Type.Up);
                    }
                    else
                    {
                        PressButton(Type.Up);
                    }
                }
                else
                {
                    if (_previousVerticalInput < 0f)
                    {
                        ReleaseButton(Type.Down);
                    }
                    else if (_previousVerticalInput > 0f)
                    {
                        ReleaseButton(Type.Up);
                    }
                }

                _previousVerticalInput = _currentVerticalInput;


                if (Input.GetButtonDown("Action"))
                {
                    PressButton(Type.Space);
                }

                if (Input.GetButtonUp("Action"))
                {
                    ReleaseButton(Type.Space);
                }

                if (Input.GetButton("Action"))
                {
                    HoldButton(Type.Space);
                }

                if (Input.GetButtonDown("Show"))
                {
                    PressButton(Type.Show);
                }

                if (Input.GetButtonUp("Show"))
                {
                    ReleaseButton(Type.Show);
                }
                /*
#if !UNITY_WEBGL
                if (Input.GetButton("Record"))
                {
                    _recordFrameCount++;
                    if (_recordFrameCount == 6)
                    {
                        References.Recorder.StartRecording();
                    }
                }

                if (Input.GetButtonUp("Record"))
                {
                    _recordFrameCount = 0;
                    References.Recorder.StopRecording();
                }
#endif
                */
                if (Input.GetButtonDown("Screenshot") || Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Alpha2))
                {
                    References.Io.SaveScreenshot();
                    References.Terminal.AddEntry("<yellow>Screen captured.</>");
                }

                if (References.Settings.TerminalEnabled)
                {
                    if (Input.GetKeyDown(KeyCode.F3))
                    {
                        Controllers.Level.ChangeNoteColor(References.Entities.PlayerOne.ColorId);
                    }
                
                    if (Input.GetKeyDown(KeyCode.F4))
                    {
                        Controllers.Level.ChangeNoteColor(References.Entities.PlayerTwo.ColorId);
                    }
                
                    if (Input.GetKeyDown(KeyCode.F6))
                    {
                        Controllers.Level.ChangeNoteColor(-1);
                    }
                
                    if (Input.GetKeyDown(KeyCode.F7))
                    {
                        Controllers.Level.ChangeNoteColor(0);
                    }
                
                    if (Input.GetKeyDown(KeyCode.F8))
                    {
                        Controllers.Level.ChangeNoteColor(1);
                    }
                
                    if (Input.GetKeyDown(KeyCode.F9))
                    {
                        Controllers.Level.ChangeNoteColor(2);
                    }
                
                    if (Input.GetKeyDown(KeyCode.F10))
                    {
                        Controllers.Level.ChangeNoteColor(3);
                    }
                
                    if (Input.GetKeyDown(KeyCode.F11))
                    {
                        Controllers.Level.ChangeNoteColor(4);
                    }
                
                    if (Input.GetKeyDown(KeyCode.F12))
                    {
                        Controllers.Level.ChangeNoteColor(5);
                    }
                }

                break;
            case State.Terminal:
                
                break;
            case State.MainMenu:

                if (Input.GetButtonDown("Escape"))
                {
                    PressButton(Type.Escape);
                }

                if (Input.GetButtonUp("Escape"))
                {
                    ReleaseButton(Type.Escape);
                }
                
                break;
            case State.Assessment:
                
                _currentHorizontalInput = Input.GetAxisRaw("Horizontal");
                _currentVerticalInput = Input.GetAxisRaw("Vertical");

                if (_currentHorizontalInput < 0f)
                {
                    if (_previousHorizontalInput >= 0f)
                    {
                        Controllers.Assessment.ProcessInput(Type.Left, Mode.Press);
                    }
                }
                else if (_currentHorizontalInput > 0f)
                {
                    if (_previousHorizontalInput <= 0f)
                    {
                        Controllers.Assessment.ProcessInput(Type.Right, Mode.Press);
                    }
                }

                _previousHorizontalInput = _currentHorizontalInput;

                if (_currentVerticalInput < 0f)
                {
                    if (_previousVerticalInput >= 0f)
                    {
                        Controllers.Assessment.ProcessInput(Type.Down, Mode.Press);
                    }
                }
                else if (_currentVerticalInput > 0f)
                {
                    if (_previousVerticalInput <= 0f)
                    {
                        Controllers.Assessment.ProcessInput(Type.Up, Mode.Press);
                    }
                }

                _previousVerticalInput = _currentVerticalInput;

                if (Input.GetButtonDown("Action") || Input.GetKeyDown(KeyCode.Return))
                {
                    Controllers.Assessment.ProcessInput(Type.Space, Mode.Press);
                }
                
                if (Input.GetButtonUp("Action") || Input.GetKeyUp(KeyCode.Return))
                {
                    Controllers.Assessment.ProcessInput(Type.Space, Mode.Release);
                }
                
                break;
        }
    }

    private void PressButton(Type buttonType)
    {
        ProcessInput(buttonType, Mode.Press);
        References.Events.ChangeButtonState((int)buttonType, true);
    }

    private void HoldButton(Type buttonType)
    {
        ProcessInput(buttonType, Mode.Hold);
    }

    private void ReleaseButton(Type buttonType)
    {
        ProcessInput(buttonType, Mode.Release);
        References.Events.ChangeButtonState((int)buttonType, false);
    }

    public void ProcessInput(Type buttonType, Mode inputMode)
    {
        if (ControllablePlayer != null)
        {
            ControllablePlayer.ProcessInput(buttonType, inputMode);
            if (inputMode == Mode.Press)
            {
                _keyPressCount[buttonType]++;
            }
            else if (inputMode == Mode.Hold)
            {
                _keyHoldDuration[buttonType] += Time.unscaledDeltaTime;
            }
        }
        else
        {
            if (buttonType == Type.Escape && inputMode == Mode.Release)
            {
                Application.Quit();
            }
        }
    }

    private void InitDictionaries()
    {
        for (var i = 0; i < 10; i++)
        {
            _keyPressCount.Add((Type)i, 0);
            _keyHoldDuration.Add((Type)i, 0f);
        }
    }

    public int GetKeyPressCount(Type keyType)
    {
        if (!_keyPressCount.ContainsKey(keyType))
        {
            return -1;
        }

        return _keyPressCount[keyType];
    }

    public float GetKeyHoldDuration(Type keyType, int decimalPlaces = 3)
    {
        if (!_keyHoldDuration.ContainsKey(keyType))
        {
            return -1f;
        }

        var value = _keyHoldDuration[keyType];
        var rounded = Mathf.Round(value * Mathf.Pow(10f, decimalPlaces)) / Mathf.Pow(10f, decimalPlaces);

        return rounded;
    }

    public bool IsGameRunning()
    {
        return CurrentState == State.Default || CurrentState == State.Terminal;
    }
}
