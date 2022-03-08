using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class IoManager : ScriptableObject
{
    public bool HasReadData;
    public bool HasConnectionError;
    public bool HasBeenInitialized;

    private string _screenshotFolder;
    
    private MovesData _data;
    private PersonalitiesData _personalitiesData;
    private TutorialData _tutorialData;
    private string _filePath;
    private string _personalitiesPath;
    private string _tutorialPath;
    
    public void Init()
    {
#if !UNITY_WEBGL
        
        _screenshotFolder = Application.persistentDataPath + "/images/";
        _filePath = string.Concat(Application.streamingAssetsPath, "/", "moves.ini");
        _personalitiesPath = string.Concat(Application.streamingAssetsPath, "/", "personalities.ini");
        _tutorialPath = string.Concat(Application.streamingAssetsPath, "/", "tutorial.ini");

        ReadData();
#endif
    }

    public void TryToAccessFiles()
    {
        HasBeenInitialized = false;
        References.Coroutines.StartCoroutine(TryToAccessFilesRoutine());
    }
    
    private void ReadData()
    {
        HasReadData = true;
        var errorMessage = "";
        try
        {
            var jsonData = File.ReadAllText(_filePath);
            _data = JsonUtility.FromJson<MovesData>(jsonData);
            _data.Init();

            if (HasConnectionError)
            {
                var textBox = References.Prefabs.GetTextBox();
                textBox.BoxWidth = 13f;
                textBox.BoxHeight = 1.6875f;
                textBox.SetParent(Controllers.Ui.UiTf);
                textBox.SetLocalPosition(new Vector2(4f, -3.5f));
                textBox.SetScale(1f);
                textBox.Show(References.Io.GetData().msgSendLogError, new List<InputController.Type> {InputController.Type.Direction}, 0.25f);
                textBox.ChangeTextColor(new Color(0.75f, 0.125f, 0.125f), 0.125f, 512f, 0, 0.25f);
            }
        }
        catch (FileNotFoundException e)
        {
            errorMessage = string.Concat("{", References.Io.GetData().msgError, ":} ",
                String.Format(References.Io.GetData().msgFileNotFound, "\"{moves.ini}\""), "{}");
        }
        catch (ArgumentException e)
        {
            errorMessage = string.Concat("{", References.Io.GetData().msgError, ":} ",
                String.Format(References.Io.GetData().msgFileCorrupted, "\"{moves.ini}\""), 
                "{(" + 
                e.Message.Replace('}', ']') +
                ")}");
        }
        catch (Exception e)
        {
            errorMessage = string.Concat(References.Io.GetData().msgUnknownError, "{}{}{(", e.Message, ")}");
            _data = new MovesData();
        }
        
        try
        {
            var jsonData = File.ReadAllText(_personalitiesPath);
            _personalitiesData = JsonUtility.FromJson<PersonalitiesData>(jsonData);
        }
        catch (FileNotFoundException e)
        {
            errorMessage = string.Concat("{", References.Io.GetData().msgError, ":} ",
                String.Format(References.Io.GetData().msgFileNotFound, "\"{personalities.ini}\""), "{}");
        }
        catch (ArgumentException e)
        {
            errorMessage = string.Concat("{", References.Io.GetData().msgError, ":} ",
                String.Format(References.Io.GetData().msgFileCorrupted, "\"{personalities.ini}\""), 
                "{(" + 
                e.Message.Replace('}', ']') +
                ")}");
        }
        catch (Exception e)
        {
            errorMessage = string.Concat(References.Io.GetData().msgUnknownError, "{}{}{(", e.Message, ")}");
                _personalitiesData = new PersonalitiesData();
        }
        
        try
        {
            var jsonData = File.ReadAllText(_tutorialPath);
            _tutorialData = JsonUtility.FromJson<TutorialData>(jsonData);
        }
        catch (FileNotFoundException e)
        {
            errorMessage = string.Concat("{", References.Io.GetData().msgError, ":} ",
                String.Format(References.Io.GetData().msgFileNotFound, "\"{tutorial.ini}\""), "{}");
        }
        catch (ArgumentException e)
        {
            errorMessage = string.Concat("{", References.Io.GetData().msgError, ":} ",
                String.Format(References.Io.GetData().msgFileCorrupted, "\"{tutorial.ini}\""), 
                "{(" + 
                e.Message.Replace('}', ']') +
                ")}");
        }
        catch (Exception e)
        {
            errorMessage = errorMessage = string.Concat(References.Io.GetData().msgUnknownError, "{}{}{(", e.Message, ")}");
                _tutorialData = new TutorialData();
        }

        if (_data != null && (string.IsNullOrEmpty(_data.vp) || _data.vp.Equals("-1")))
        {
            errorMessage = _data.msgNoVpCode + "{}{}{}";
        }

        if (errorMessage.Length > 0)
        {
            var textBox = References.Prefabs.GetTextBox();
            textBox.SetParent(Controllers.Ui.UiTf);
            textBox.SetLocalPosition(Vector2.zero);
            textBox.SetScale(1f);
            textBox.Show(errorMessage, new List<InputController.Type> {InputController.Type.Escape}, 0.25f);
            textBox.ChangeTextColor(new Color(0.75f, 0.125f, 0.125f), 0.125f, 512f, 0, 0.25f);
            textBox.ChangeTextColor(new Color(0.125f, 0.5f, 0.5f), 0.125f, 512f, 1, 0.25f);
            textBox.ChangeTextColor(new Color(0.5f, 0.5f, 0.5f), 0.125f, 512f, 2, 0.25f);
            HasReadData = false;
        }

        HasBeenInitialized = true;
    }

    public void UpdateAssessmentState(int index, int state)
    {
        if (_data == null) return;
        _data.roomVisits[index] = state;
        
#if !UNITY_WEBGL
        File.WriteAllText(_filePath,JsonUtility.ToJson(_data, true));
#else
        References.Coroutines.StartCoroutine(TryToUpdateDataRoutine());
#endif

    }

    public int GetAssessmentState(int index)
    {
        if (_data == null) return 0;
        return _data.roomVisits[index];
    }

    public bool IsLevelAccessible(int levelIndex, int levelIndexOfTopButton)
    {
        // allow access in debug mode:
        if (_data.debug == 2) return true;
        
        // reject invalid values:
        if (levelIndex < 0 || levelIndex >= _data.roomVisits.Length) return false;
        
        // enable tutorial if it has not been completed:
        if (_data.roomVisits[levelIndex] == 0 && levelIndexOfTopButton < 0) return true;

        // enable each level that has not been completed but its predecessor has:
        if (_data.roomVisits[levelIndex] == 0 && _data.roomVisits[levelIndexOfTopButton] > 0) return true;
        
        // deny access to everything else:
        return false;
    }

    public bool HaveAllLevelsBeenCompleted()
    {
        for (var i = 0; i < _data.roomVisits.Length; i++)
        {
            if (_data.roomVisits[i] == 0)
            {
                return false;
            }
        }

        return true;
    }

    public MovesData GetData()
    {
        return _data;
    }

    public PersonalitiesData GetPersonalitiesData()
    {
        return _personalitiesData;
    }

    public TutorialData GetTutorialData()
    {
        return _tutorialData;
    }

    public void SaveScreenshot()
    {
        if (!Directory.Exists(_screenshotFolder))
        {
            Directory.CreateDirectory(_screenshotFolder);
        }
        
        ScreenCapture.CaptureScreenshot(string.Concat(_screenshotFolder, "screen_capture_", Utility.Timestamp(), ".png"));
    }

    private IEnumerator TryToAccessFilesRoutine()
    {
        string streamingAssetsPath = string.Concat(Application.absoluteURL, "/StreamingAssets/");

        using (var movesRequest = UnityWebRequest.Get(string.Concat(streamingAssetsPath, "moves.ini")))
        {
            yield return movesRequest.SendWebRequest();

#pragma warning disable 618
            if (!movesRequest.isNetworkError && !movesRequest.isHttpError)
#pragma warning restore 618
            {
                _data = JsonUtility.FromJson<MovesData>(movesRequest.downloadHandler.text);
                _data.Init();
            }
            else
            {
                _data = new MovesData();
            }
        }
        
        using (var personalitiesRequest = UnityWebRequest.Get(string.Concat(streamingAssetsPath, "personalities.ini")))
        {
            yield return personalitiesRequest.SendWebRequest();

#pragma warning disable 618
            if (!personalitiesRequest.isNetworkError && !personalitiesRequest.isHttpError)
#pragma warning restore 618
            {
                _personalitiesData = JsonUtility.FromJson<PersonalitiesData>(personalitiesRequest.downloadHandler.text);
            }
            else
            {
                _personalitiesData = new PersonalitiesData();
            }
        }
        
        using (var tutorialRequest = UnityWebRequest.Get(string.Concat(streamingAssetsPath, "tutorial.ini")))
        {
            yield return tutorialRequest.SendWebRequest();

#pragma warning disable 618
            if (!tutorialRequest.isNetworkError && !tutorialRequest.isHttpError)
#pragma warning restore 618
            {
                _tutorialData = JsonUtility.FromJson<TutorialData>(tutorialRequest.downloadHandler.text);
            }
            else
            {
                _tutorialData = new TutorialData();
            }
        }

        HasReadData = true;
        HasBeenInitialized = true;
        yield return null;
    }

    private IEnumerator TryToUpdateDataRoutine()
    {
        // this would be the place to upload progress, but we're probably better off without that...
        yield return null;
    }
}
