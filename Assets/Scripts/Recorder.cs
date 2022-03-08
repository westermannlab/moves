using System.Collections;
using UnityEngine;

public class Recorder : ScriptableObject
{
    /*
    private AudioClip _clip;
    private const int MaxRecordingDuration = 60;
    private const int MaxFrequency = 44100;
    private int _minFrequency;
    private int _maxFrequency;

    public void StartRecording()
    {
#if !UNITY_WEBGL
        if (!References.Settings.RecordingsEnabled)
        {
            References.Terminal.AddEntry("<red>Recording mode is disabled.</>");
            return;
        }
        if (FindMicrophone())
        {
            _clip = Microphone.Start(null, true, MaxRecordingDuration, _maxFrequency);
            References.Terminal.AddEntry("<yellow>Recording audio...</>");
            References.Coroutines.StartCoroutine(VisualizeWavesRoutine());
        }
        else
        {
            References.Terminal.AddEntry("<red>No microphone.</>");
        }
#endif
    }

    public void StopRecording()
    {
#if !UNITY_WEBGL
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
            References.Terminal.AddEntry("<yellow>Recording stopped.</>");
            Controllers.Audio.SaveClip(_clip, true);
        }
#endif
    }

    public AudioClip GetClip()
    {
        return _clip;
    }

    private bool FindMicrophone()
    {
#if !UNITY_WEBGL
        if (Microphone.devices.Length > 0)
        {
            Microphone.GetDeviceCaps(null, out _minFrequency, out _maxFrequency);
            if (_minFrequency == 0 && _maxFrequency == 0 || _maxFrequency > MaxFrequency)
            {
                _maxFrequency = MaxFrequency;
            }

            return true;
        }
#endif
        return false;
    }
    
    public float GetLevelMax(int sampleSize = 32)
    {
        // source: https://forum.unity.com/threads/check-current-microphone-input-volume.133501/
        var levelMax = 0f;
#if !UNITY_WEBGL
        var waveData = new float[sampleSize];
        var micPosition = Microphone.GetPosition(null) - (sampleSize + 1);
        if (micPosition < 0) return 0f;
        
        _clip.GetData(waveData, micPosition);

        for (var i = 0; i < sampleSize; i++) {
            var wavePeak = waveData[i] * waveData[i];
            if (levelMax < wavePeak) {
                levelMax = wavePeak;
            }
        }
#endif
        return levelMax;
    }

    private IEnumerator VisualizeWavesRoutine()
    {
#if !UNITY_WEBGL
        var display = References.Prefabs.GetWaveFormDisplay();
        display.Show();
        while (Microphone.IsRecording(null))
        {
            // get current sample
            
            yield return null;
        }
        display.Hide();
#endif
        yield return null;
    }
    */
}
