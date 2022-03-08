using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Terminal : TerminalEntry
{
    public Parser Parser;
    public int MaxEntries = 10;
    public Transform EntryAnchor;
    
    private readonly List<TerminalEntry> _entries = new List<TerminalEntry>();
    private readonly List<string> _messageHistory = new List<string>();
    private int _messageIndex;
    
    private bool[] _lineBreaks;
    private bool _isInWriteMode;
    private bool _isCursorActive;

    private Vector2 _currentEntryAnchorPosition;
    private string _lastInputString;
    private float _inputHoldTimer;
    private float _cursorTimer;
    private float _moveEntryAnchorTimer;
    private float _entryAnchorHolder;
    private int _changeSizeId;
    private int _moveEntryAnchorId;

    private bool _wasMenuOpen;
    
    public Vector2Int MostRecentSpaceIndex => SpaceIndexList.Count > 0 ? SpaceIndexList[SpaceIndexList.Count - 1] : Vector2Int.zero;

    protected override void Awake()
    {
        base.Awake();
        _lineBreaks = new bool[MaxLines];
        InputField.localScale = Scale * Vector3.one;
        _currentEntryAnchorPosition = EntryAnchor.localPosition;
        _messageHistory.Add("");
        
        // Show version info
        ChangeSize(Width, Height, 0f);
        Parser.Process(new[]{"version"});
        HideMessage(0f);
        foreach (var entry in _entries)
        {
            entry.Hide(3f);
        }
    }

    public void EnterWriteMode()
    {
        /*
        _wasMenuOpen = References.Menu.IsOpen;
        if (_wasMenuOpen)
        {
            References.Menu.Close();
        }
        */
        
        SetSize(0f, Height);
        ChangeSize(Width, Height, 0.125f);
        Fade(0.75f, 0f);

        while (_entries.Count > MaxEntries)
        {
            var oldestEntry = _entries[_entries.Count - 1];
            _entries.Remove(oldestEntry);
            oldestEntry.Hide();
            oldestEntry.Return(0.5f);
        }
        
        foreach (var entry in _entries)
        {
            entry.Show();
        }
        if (!_isInWriteMode)
        {
            _isInWriteMode = true;
            StartCoroutine(WaitForInputRoutine());
        }

        References.Settings.PauseGame();
    }

    private void LeaveWriteMode()
    {
        /*
        if (_wasMenuOpen)
        {
            References.Menu.Open();
        }
        */
        
        if (Message.Length > 0)
        {
            AddEntry(Message);
            _messageHistory.Insert(1, Message);
            _messageIndex = 0;

            HideMessage(0.5f);

            ProcessCommand(Message);
            Message = "";
        }
        
        foreach (var entry in _entries)
        {
            entry.Hide(3f);
        }
        
        Fade(0f, 0.25f);
        if (_isInWriteMode)
        {
            _isInWriteMode = false;
            Controllers.Input.CurrentState = InputController.State.Default;
        }

        References.Settings.ContinueGame();
    }

    public void ProcessCommand(string command)
    {
        Parser.Process(Formatter.GetWords(command));
    }

    public void AddEntry(string message)
    {
        if (!References.Settings.TerminalEnabled) return;
        var lineBreakCount = message.Count(x => x == '\n');
        var direction = Vector2.up * (Height + Utility.PixelsToUnit(LinePixelHeight * lineBreakCount + 18)) * Scale;
        MoveEntries(direction, 0.125f);
        
        var newEntry = References.Prefabs.GetTerminalEntry();
        newEntry.SetParent(EntryAnchor);
        newEntry.SetScale(Scale);
        newEntry.SetLocalPosition(new Vector2(0f, 0f));
        newEntry.InitializeSize(CurrentWidth, Height + Utility.PixelsToUnit(LinePixelHeight * lineBreakCount)); // to do: determine by message
        newEntry.SetMessage(message);
        newEntry.Show();
        newEntry.SetLocalRotation(Vector3.zero);
        _entries.Insert(0, newEntry);
        if (!_isInWriteMode)
        {
            newEntry.Hide(3f);
        }
    }

    private void MoveEntries(Vector2 direction, float duration)
    {
        foreach (var entry in _entries)
        {
            entry.Move(direction, duration);
        }
    }
    
    private void ChangeSize(float toWidth, float toHeight, float duration)
    {
        _changeSizeId = Utility.AddOne(_changeSizeId);
        StartCoroutine(ChangeSizeRoutine(toWidth, toHeight, duration, _changeSizeId));
        
        MoveEntryAnchor((Utility.PixelsToUnit(20) + toHeight) * Scale, duration);
    }

    protected override void NewLine(float changeSizeDuration = 0.125f, bool writeNewLine = false)
    {
        base.NewLine(changeSizeDuration, writeNewLine);
        
        // scale does not matter here since the background gets scaled down on its own
        var newHeight = Height + Utility.PixelsToUnit(LinePixelHeight) * Mathf.Clamp(LineIndex, MinLines, MaxLines);
        ChangeSize(CurrentWidth, newHeight, changeSizeDuration);
    }

    private string GetInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LeaveWriteMode();
            return "";
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            _lastInputString = "backspace";
            return "backspace";
        }

        if (Input.GetKey(KeyCode.Backspace) && _lastInputString == "backspace")
        {
            _inputHoldTimer += Time.unscaledDeltaTime;
            if (_inputHoldTimer > 0.5f)
            {
                _inputHoldTimer -= 0.05f;
                return "backspace";
            }

            return "";
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            LeaveWriteMode();
            return "";
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            return "arrow-up";
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            return "arrow-down";
        }
        if (Input.anyKeyDown)
        {
            _lastInputString = Input.inputString;
            return Input.inputString;
        }
        
        _inputHoldTimer = 0f;
        return "";
    }
    
    private void HandleInput(string currentInput)
    {
        switch (currentInput)
        {
            case "":
                break;
            case "\n":
                _lineBreaks[LineIndex] = true;
                NewLine(0f, true);
                break;
            case "backspace":
                if (Message.Length > 0 && Message[Message.Length - 1] == '\n' || (LineIndex < MessageLetters.Count && MessageLetters[LineIndex].Count == 0 && LineIndex > 0))
                {
                    ShiftLines(Vector2.down, 0.125f);
                    
                    LineIndex--;
                    _lineBreaks[LineIndex] = false;
                    
                    // scale does not matter here since the background gets scaled down on its own
                    var newHeight = Height + Utility.PixelsToUnit(LinePixelHeight) * Mathf.Clamp(LineIndex, MinLines - 1, MaxLines);
                    ChangeSize(CurrentWidth, newHeight, 0.375f);
                    
                    CurrentCursorPosition.x = Utility.PixelsToUnit(GetLineWidthInPixels(LineIndex)) * Scale;
                    //_currentCursorPosition.y += Utility.PixelsToUnit(12);
                    if (CursorLetter != null)
                    {
                        CursorLetter.SetLocalPosition(CurrentCursorPosition);
                    }
                    Message = Message.Substring(0, Message.Length - 1);
                /*    if (_message.Length > 0 && _message[_message.Length - 1] != '\n' && (_spaceIndexList.Count == 0 || _spaceIndexList[_spaceIndexList.Count - 1].y < _lineIndex))
                    {
                        HandleInput("backspace");
                    }
                    */
                }
                else if (Message.Length > 0 && Message[Message.Length - 1] != '\n')
                {
                    if (LineIndex >= MessageLetters.Count || MessageLetters[LineIndex].Count == 0)
                    {
                        break;
                    }
                    var lastLetter = MessageLetters[LineIndex][MessageLetters[LineIndex].Count - 1];
                    RemoveLetterFromList(lastLetter, LineIndex);
                    CurrentCursorPosition.x -= Utility.PixelsToUnit((lastLetter.GetCharacterWidth() + 1f * Scale));
                    if (CursorLetter != null)
                    {
                        CursorLetter.SetLocalPosition(CurrentCursorPosition);
                    }
                    lastLetter.ChangeColor(DeleteColor, 0.0625f);
                    lastLetter.Fade(0f, 0.09375f);
                    lastLetter.Return(0.125f);
                    if (Message[Message.Length - 1] == ' ' && SpaceIndexList.Count > 0)
                    {
                        SpaceIndexList.RemoveAt(SpaceIndexList.Count - 1);
                    }
                    Message = Message.Substring(0, Message.Length - 1);
                    if (LineIndex > 0 && !_lineBreaks[LineIndex - 1] &&
                        Utility.PixelsToUnit(GetLineWidthInPixels(LineIndex - 1) + GetLineWidthInPixels(LineIndex)) <
                        CurrentWidth * Scale)
                    {
                        ReturnCurrentWordToPreviousLine();
                    }
                }
                break;
            case "arrow-up":
                if (_messageIndex < _messageHistory.Count - 1)
                {
                    _messageIndex++;
                    HideMessage(0.125f);
                    Message = _messageHistory[_messageIndex];
                    WriteMessage();
                    if (CursorLetter != null)
                    {
                        CurrentCursorPosition.x = Utility.PixelsToUnit(GetLineWidthInPixels(LineIndex));
                        CursorLetter.SetLocalPosition(CurrentCursorPosition);
                    }
                }
                break;
            case "arrow-down":
                if (_messageIndex > 0)
                {
                    _messageIndex--;
                    HideMessage(0.125f);
                    Message = _messageHistory[_messageIndex];
                    WriteMessage();
                    if (CursorLetter != null)
                    {
                        CurrentCursorPosition.x = Utility.PixelsToUnit(GetLineWidthInPixels(LineIndex));
                        CursorLetter.SetLocalPosition(CurrentCursorPosition);
                    }
                }
                break;
            default:
                // split input string into single characters
                foreach (var c in currentInput)
                {
                    var letter = References.Prefabs.GetLetter();
                    if (letter != null)
                    {
                        AddLetterToList(letter, LineIndex);
                        if (c == ' ' && LineIndex < MessageLetters.Count)
                        {
                            SpaceIndexList.Add(new Vector2Int(MessageLetters[LineIndex].Count - 1, LineIndex));
                        }
                        letter.SetParent(LetterHolder);
                        letter.SetCharacter(c, Font);
                        letter.SetLocalPosition(CurrentCursorPosition);
                        letter.SetColor(DefaultColor);
                        letter.SetSize(Scale);
                        letter.Scale(1.2f, 1f, 0.2f);
                        letter.SetLayer(Controllers.Camera.UiLayer);
                        letter.SetSpriteSortingOrder(8);
                        CurrentCursorPosition.x += Utility.PixelsToUnit(letter.GetCharacterWidth() + 1f * Scale);
                        if (CursorLetter != null)
                        {
                            CursorLetter.SetLocalPosition(CurrentCursorPosition);
                        }
                    }
                    Message += c;
                    if (Utility.PixelsToUnit(GetLineWidthInPixels(LineIndex)) >= CurrentWidth * Scale)
                    {
                        if (LineIndex < MaxLines - 1 && !currentInput.Equals(" "))
                        {
                            RelocateCurrentWord();
                        }
                        else
                        {
                            HandleInput("backspace");
                            if (currentInput.Equals(" "))
                            {
                                HandleInput("\n");
                            }
                        }
                    }
                }
                break;
        }
    }
    
    private void RelocateCurrentWord()
    {
        if (LineIndex >= MessageLetters.Count || MostRecentSpaceIndex.x == 0 || MostRecentSpaceIndex.y != LineIndex)
        {
            Message = Message.Insert(Mathf.Max(Message.Length - 1, 0), "\n");

            LineIndex++;
            ShiftLines(Vector2.up, 0.125f);
            LineIndex--;
        
            CurrentCursorPosition.x = 0f;
            //_currentCursorPosition.y -= Utility.PixelsToUnit(12);

            var letter = MessageLetters[LineIndex][MessageLetters[LineIndex].Count - 1];
            letter.MoveTo(CurrentCursorPosition, 0.125f);
            CurrentCursorPosition.x += Utility.PixelsToUnit(letter.GetCharacterWidth() + 1f * Scale);
            AddLetterToList(letter, LineIndex + 1);
            RemoveLetterFromList(letter, LineIndex);
            
            if (CursorLetter != null)
            {
                CursorLetter.SetLocalPosition(CurrentCursorPosition);
            }

            LineIndex++;
            var newTextAreaHeight = 0.375f * Mathf.Clamp(LineIndex + 2, MinLines, MaxLines);
            if (LineIndex + 1 == MaxLines)
            {
                newTextAreaHeight += 0.125f;
            }
            ChangeSize(CurrentWidth, newTextAreaHeight, 0.125f);
            return;
        }

        var startOfLine = Message.LastIndexOf('\n');
        Message = Message.Insert(Mathf.Min(startOfLine + MostRecentSpaceIndex.x + 2, Message.Length - 1), "\n");
        
        LineIndex++;
        ShiftLines(Vector2.up, 0.125f);
        LineIndex--;

        CurrentCursorPosition.x = 0f;
        //_currentCursorPosition.y -= Utility.PixelsToUnit(12);

        var currentLine = new List<Letter>(MessageLetters[LineIndex]);
        
        for (var i = MostRecentSpaceIndex.x + 1; i < currentLine.Count; i++)
        {
            var letter = currentLine[i];
            letter.MoveTo(CurrentCursorPosition, 0.125f);
            CurrentCursorPosition.x += Utility.PixelsToUnit(letter.GetCharacterWidth() + 1f * Scale);
            AddLetterToList(letter, LineIndex + 1);
            RemoveLetterFromList(letter, LineIndex);
        }
        
        currentLine.Clear();
        
        if (CursorLetter != null)
        {
            CursorLetter.SetLocalPosition(CurrentCursorPosition);
        }

        LineIndex++;
        // scale does not matter here since the background gets scaled down on its own
        var newHeight = Height + Utility.PixelsToUnit(LinePixelHeight) * Mathf.Clamp(LineIndex, MinLines, MaxLines);
        
        ChangeSize(CurrentWidth, newHeight, 0.125f);
    }

    private void ReturnCurrentWordToPreviousLine()
    {
        var lastLineBreakIndex = Message.LastIndexOf('\n');
        if (lastLineBreakIndex > 0)
        {
            Message = Message.Remove(lastLineBreakIndex, 1);
        }
        var currentLine = new List<Letter>(MessageLetters[LineIndex]);
        
        ShiftLines(Vector2.down, 0.125f);
        
        LineIndex--;

        _lineBreaks[LineIndex] = false;
        CurrentCursorPosition.x = Utility.PixelsToUnit(GetLineWidthInPixels(LineIndex));
        //_currentCursorPosition.y += Utility.PixelsToUnit(12);
        foreach (var letter in currentLine)
        {
            letter.MoveTo(CurrentCursorPosition, 0.125f);
            CurrentCursorPosition.x += Utility.PixelsToUnit((letter.GetCharacterWidth() + 1f * Scale));
            AddLetterToList(letter, LineIndex);
            RemoveLetterFromList(letter, LineIndex + 1);
        }
        currentLine.Clear();
        
        if (CursorLetter != null)
        {
            CursorLetter.SetLocalPosition(CurrentCursorPosition);
        }
        
        // scale does not matter here since the background gets scaled down on its own
        var newHeight = Height + Utility.PixelsToUnit(LinePixelHeight) * Mathf.Clamp(LineIndex, MinLines - 1, MaxLines);
        ChangeSize(CurrentWidth, newHeight, 0.125f);
    }

    private void RemoveLetterFromList(Letter letter, int lineIndex)
    {
        if (lineIndex >= MessageLetters.Count)
        {
            return;
        }

        if (MessageLetters[lineIndex].Contains(letter))
        {
            MessageLetters[lineIndex].Remove(letter);
        }
    }

    private float GetLineWidthInPixels(int lineIndex)
    {
        if (lineIndex >= MessageLetters.Count)
        {
            return 0;
        }

        var w = Mathf.Max(MessageLetters[lineIndex].Count - 1f, 0f) * Scale;
        foreach (var letter in MessageLetters[lineIndex])
        {
            if (letter.IsNewLine())
            {
                w -= Scale;
                continue;
            }
            w += letter.GetCharacterWidth();
        }

        return w;
    }

    private void MoveEntryAnchor(float yTarget, float duration)
    {
        _moveEntryAnchorId = Utility.AddOne(_moveEntryAnchorId);
        StartCoroutine(MoveEntryAnchorRoutine(yTarget, duration, _moveEntryAnchorId));
    }

    private IEnumerator WaitForInputRoutine()
    {
        // wait one frame to ignore input that selected the text area
        yield return null;
        
        CursorLetter = References.Prefabs.GetLetter();
        if (CursorLetter != null)
        {
            CurrentCursorPosition = Vector2.zero;
            CursorLetter.SetCharacter('°', Font);
            CursorLetter.SetParent(LetterHolder);
            CursorLetter.SetLocalPosition(CurrentCursorPosition);
            CursorLetter.SetColor(DefaultColor);
            CursorLetter.Scale(1.2f * Scale, Scale, 0.2f);
            CursorLetter.SetLayer(Controllers.Camera.UiLayer);
            CursorLetter.SetSpriteSortingOrder(8);
        }

        _cursorTimer = 0f;
        _isCursorActive = true;

        Controllers.Input.CurrentState = InputController.State.Terminal;
        while (_isInWriteMode)
        {
            HandleInput(GetInput());

            _cursorTimer += Time.unscaledDeltaTime;
            if (_cursorTimer > 0.5f)
            {
                _isCursorActive = !_isCursorActive;
                if (CursorLetter != null)
                {
                    CursorLetter.SetCharacter(_isCursorActive ? '°' : ' ', Font);
                }
                _cursorTimer -= 0.5f;
            }
            yield return null;
        }

        if (CursorLetter != null)
        {
            CursorLetter.Fade(0f, 0.2f);
            CursorLetter.Return(0.25f);
            CursorLetter = null;
        }
        
        yield return null;
    }
    
    private IEnumerator ChangeSizeRoutine(float toWidth, float toHeight, float duration, int id)
    {
        var timer = 0f;
        var fromWidth = CurrentWidth;
        var fromHeight = CurrentHeight;
        while (timer < duration && _changeSizeId == id)
        {
            timer += Time.unscaledDeltaTime;
            SetSize(Mathf.Lerp(fromWidth, toWidth, timer / duration), Mathf.Lerp(fromHeight, toHeight, timer / duration));
            yield return null;
        }

        if (_changeSizeId == id)
        {
            SetSize(toWidth, toHeight);
        }

        yield return null;
    }

    private IEnumerator MoveEntryAnchorRoutine(float yTarget, float duration, int id)
    {
        _moveEntryAnchorTimer = 0f;
        _entryAnchorHolder = _currentEntryAnchorPosition.y;
        while (_moveEntryAnchorTimer < duration && _moveEntryAnchorId == id)
        {
            _moveEntryAnchorTimer += Time.unscaledDeltaTime;
            _currentEntryAnchorPosition.y = Mathf.Lerp(_entryAnchorHolder, yTarget, _moveEntryAnchorTimer / duration);
            EntryAnchor.localPosition = _currentEntryAnchorPosition;
            yield return null;
        }
        if (_moveEntryAnchorId == id)
        {
            _currentEntryAnchorPosition.y = yTarget;
            EntryAnchor.localPosition = _currentEntryAnchorPosition;
        }

        yield return null;
    }
}
