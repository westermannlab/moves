using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Networking;

public class LogsController : MonoBehaviour
{
    private DateTime _programStartTime;
    private string _formattedProgramStartTime;
    
    private readonly List<Log> _logs = new List<Log>();
    private string _logFileFolder;
    private string _logFilePath;
    private string _zipFilePath;
    private string _tmpFolder;
    private string _tmpFilePath;

    private string _html;
    private string _pgCode;
    private string _ses;

    private string _vp;

    private int _logId;
    private int _actionId;
    private bool _hasConnectionError;
    
    private void Awake()
    {
        _programStartTime = DateTime.Now;
        _formattedProgramStartTime = _programStartTime.ToString(CultureInfo.CurrentCulture);
        _logFileFolder = Application.persistentDataPath + "/logs";
        _tmpFolder = _logFileFolder + "/tmp";
        _zipFilePath =  Application.persistentDataPath + "/log.zip";
    }

    private void Start()
    {
        _vp = References.Io.GetData().vp;
        
        _logFilePath = Application.persistentDataPath + "/logs/log_" + _vp + '_' + Utility.Timestamp() + ".txt";
        _tmpFilePath = Application.persistentDataPath + "/logs/tmp/log_" + _vp + '_' + Utility.Timestamp() + ".txt";
    }

    public void AddLog(float beginTime, Player player, Log.Action action, int actionId, Log.Ending ending, string target)
    {
        if (References.Timer.IsUp()) return;
        var log = new Log(beginTime, player, action, actionId, ending, target);
        _logs.Add(log);
        References.Events.CreateLog(log);
    }
    
    public void SaveLogfile()
    {
        CreateLogfile();
        References.Terminal.AddEntry("<yellow>Logfile saved. <grey>(showing header)<*>\n" + GetHeader());
    }

    public void CreateLogfile()
    {
        References.Entities.PlayerOne.UpdateTimeSpentInObjects();
        
        if (!Directory.Exists(_logFileFolder))
        {
            Directory.CreateDirectory(_logFileFolder);
        }

        if (!Directory.Exists(_tmpFolder))
        {
            Directory.CreateDirectory(_tmpFolder);
        }
        
        File.WriteAllText(_logFilePath, GetLogfileContents());
        
        var writer = new StreamWriter(_logFilePath, true);

        writer.Write("\n\n" + GetTableHeader() + '\n');
        foreach (var log in _logs)
        {
            writer.WriteLine(log.ToString());
        }
        
        writer.Close();

        if (File.Exists(_logFilePath))
        {
            File.Copy(_logFilePath, _tmpFilePath, true);
        }

        if (File.Exists(_zipFilePath))
        {
            File.Delete(_zipFilePath);
        }
        
        ZipFile.CreateFromDirectory(_tmpFolder, _zipFilePath);

        foreach (var file in Directory.GetFiles(_tmpFolder))
        {
            File.Delete(file);
        }

        if (Directory.Exists(_tmpFolder) && Directory.GetFiles(_tmpFolder).Length == 0)
        {
            Directory.Delete(_tmpFolder);
        }
    }

    public string GetLogfileContents()
    {
        return GetHeader();
    }

    private string GetHeader()
    {
        var decimalPlaces = 3;
        var format = string.Concat('F', decimalPlaces);
        
        var header = "## Moves Logfile\n";
        header += "# Version " + Application.version + "\n";
        header += "# Moves version " + References.Io.GetData().version + "\n";
        header += "# Personalities version " + References.Io.GetPersonalitiesData().version + "\n";
        header += "# Tutorial version " + References.Io.GetTutorialData().version + "\n";
        header += "# VP: " + _vp + "\n";
        header += "# Room: " + Controllers.Level.GetRoomId() + "\n";
        header += "# Trial: " + Controllers.Level.GetTrial() + "\n";
        header += "# " + SystemInfo.operatingSystem + "\n";
        header += "# Program start: " + _formattedProgramStartTime + "\n";
        header += "##\n";

        header += "\n";
        header += "##\n";
        header += "# Up key count: " + Controllers.Input.GetKeyPressCount(InputController.Type.Up) + " (" + Controllers.Input.GetKeyHoldDuration(InputController.Type.Up).ToString(format) + " s)\n";
        header += "# Right key count: " + Controllers.Input.GetKeyPressCount(InputController.Type.Right) + " (" + Controllers.Input.GetKeyHoldDuration(InputController.Type.Right).ToString(format) + " s)\n";
        header += "# Down key count: " + Controllers.Input.GetKeyPressCount(InputController.Type.Down) + " (" + Controllers.Input.GetKeyHoldDuration(InputController.Type.Down).ToString(format) + " s)\n";
        header += "# Left key count: " + Controllers.Input.GetKeyPressCount(InputController.Type.Left) + " (" + Controllers.Input.GetKeyHoldDuration(InputController.Type.Left).ToString(format) + " s)\n";
        header += "# Space key count: " + Controllers.Input.GetKeyPressCount(InputController.Type.Space) + " (" + Controllers.Input.GetKeyHoldDuration(InputController.Type.Space).ToString(format) + " s)\n";
        header += "##\n";
        // level name and id

        header += "\n";
        header += "##\n";
        header += "# Soul enter count: " + References.Entities.PlayerOne.GetObjectEnterCount(ControllableObject.Type.Soul) + " (" + References.Entities.PlayerOne.GetTimeSpentInObject(ControllableObject.Type.Soul).ToString(format) + " s)\n";
        header += "# Cloud enter count: " + References.Entities.PlayerOne.GetObjectEnterCount(ControllableObject.Type.Cloud) + " (" + References.Entities.PlayerOne.GetTimeSpentInObject(ControllableObject.Type.Cloud).ToString(format) + " s)\n";
        header += "# Ground enter count: " + References.Entities.PlayerOne.GetObjectEnterCount(ControllableObject.Type.Ground) + " (" + References.Entities.PlayerOne.GetTimeSpentInObject(ControllableObject.Type.Ground).ToString(format) + " s)\n";
        header += "# Handcar enter count: " + References.Entities.PlayerOne.GetObjectEnterCount(ControllableObject.Type.Handcar) + " (" + References.Entities.PlayerOne.GetTimeSpentInObject(ControllableObject.Type.Handcar).ToString(format) + " s)\n";
        header += "# Cart enter count: " + References.Entities.PlayerOne.GetObjectEnterCount(ControllableObject.Type.Cart) + " (" + References.Entities.PlayerOne.GetTimeSpentInObject(ControllableObject.Type.Cart).ToString(format) + " s)\n";
        header += "##\n";
        
        header += "\n";
        header += "Assessment:\n";
        header += "title\tvp\troom\titems\n";
        foreach (var scale in References.Assessment.ScaleList)
        {
            header += string.Concat(scale.Title, '\t', _vp, '\t', Controllers.Level.GetRoomId());
            foreach (var result in scale.Results)
            {
                header += string.Concat('\t', result);
            }

            header += '\n';
        }

        return header;
    }

    private string GetTableHeader()
    {
        return "#\tbegin\tend\tagent\tcolor\tstate\tother\taction\tact_id\tending\ttarget\tloc_h0\tloc_c0\trelationship";
    }

    public int RequestLogId()
    {
        _logId++;
        return _logId;
    }

    public int RequestActionId()
    {
        _actionId++;
        return _actionId;
    }
}
