using UnityEngine;

public class TaskManager : ScriptableObject
{
    public Task[] Tasks;
    public Task TaskTemplate;
    public bool UseTemplate;

    private TaskData[] _taskData;

    private Task _nextTask;
    private int _taskIndex;
    
    public void Init()
    {
        _taskIndex = 0;
        _taskData = References.Io.GetTutorialData().tasks;
    }

    public Task GetNextTask()
    {
        if (UseTemplate && _taskIndex >= _taskData.Length)
        {
            return null;
        }
        if (!UseTemplate && _taskIndex >= Tasks.Length)
        {
            return null;
        }

        if (UseTemplate)
        {
            TaskTemplate.LoadData(_taskData[_taskIndex]);
            _nextTask = TaskTemplate;
        }
        else
        {
            _nextTask = (Task)Tasks[_taskIndex];
        }
        
        if (_nextTask != null)
        {
            _nextTask.Open();
        }
        
        _taskIndex++;
        return _nextTask;
    }
}
