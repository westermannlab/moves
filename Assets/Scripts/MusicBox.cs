using System.Collections;
using UnityEngine;

public class MusicBox : MonoBehaviour
{
    public SoundEffect[] Songs;

    private AudioSource[] _audioSources = new AudioSource[PlayerCount];
    private AudioReverbFilter[] _reverbFilters = new AudioReverbFilter[PlayerCount];

    private const float MaxNotesPerColor = 5.5f;
    private const int PlayerCount = 2;
    private const float FadeOutDelay = 2f;
    private const float DurationPerNote = 1.125f;
    private const float MaxMusicVolume = 0.375f;
    private const float MaxRainMusicVolume = 0.125f;
    private const AudioReverbPreset RainReverbPreset = AudioReverbPreset.Drugged;
    
    private readonly float[] _noteBuffer = new float[PlayerCount];
    private readonly float[] _currentVolumes = new float[PlayerCount];
    private readonly float[] _volumeHolders = new float[PlayerCount];
    private readonly float[] _changeVolumeTimers = new float[PlayerCount];
    private readonly float[] _fadeOutTimers = new float[PlayerCount];
    private readonly float[] _decreaseAmounts = new float[PlayerCount];

    private readonly int[] _changeVolumeIds = new int[PlayerCount];
    private readonly int[] _fadeOutIds = new int[PlayerCount];
    private readonly int[] _decreaseBufferIds = new int[PlayerCount];

    // vectors contain the off value (x), the reverb value (y) and a holder value (z)
    private Vector3 _dryLevel;
    private Vector3 _room;
    private Vector3 _roomHf;
    private Vector3 _roomLf;
    private Vector3 _decayTime;
    private Vector3 _decayHfRatio;
    private Vector3 _reflectionsLevel;
    private Vector3 _reflectionsDelay;
    private Vector3 _reverbLevel;
    private Vector3 _reverbDelay;
    private Vector3 _hfReference;
    private Vector3 _lfReference;
    private Vector3 _diffusion;
    private Vector3 _density;

    private Vector3 _pitch;

    private float _changeReverbFilterTimer;
    private int _changeReverbFilterId;
    private int _currentFilterMode;

    private void Awake()
    {
        _audioSources = GetComponentsInChildren<AudioSource>();

        for (var i = 0; i < _audioSources.Length; i++)
        {
            if (i < Songs.Length)
            {
                _audioSources[i].clip = Songs[i].Clip;
                _audioSources[i].volume = 0f;
                _audioSources[i].Play();
            }

            _reverbFilters[i] = _audioSources[i].GetComponent<AudioReverbFilter>();
        }
        InitializeReverbFilters();
    }

    private void OnEnable()
    {
        References.Events.OnCollectNote += CollectNote;
        References.Events.OnRainHitsNote += RainOnNote;
    }

    private void OnDisable()
    {
        References.Events.OnCollectNote -= CollectNote;
        References.Events.OnRainHitsNote -= RainOnNote;
    }

    public void SetSong(SoundEffect song, int playerId)
    {
        if (playerId >= Songs.Length || playerId >= _audioSources.Length)
        {
            return;
        }

        Songs[playerId] = song;
        _audioSources[playerId].Stop();
        _audioSources[playerId].clip = song.Clip;
        _audioSources[playerId].volume = 0f;
        _audioSources[playerId].Play();
    }

    private void CollectNote(int playerId)
    {
        if (Controllers.Input.CurrentState != InputController.State.Default || playerId >= _noteBuffer.Length) return;
        if (Mathf.Approximately(_noteBuffer[playerId], 0f))
        {
            DecreaseBuffer(playerId);
        }

        if (_currentFilterMode != 0)
        {
            ChangeReverbFilterMode(playerId, 0, 1f);
        }
        _noteBuffer[playerId] = Mathf.Min(_noteBuffer[playerId] + 1f, MaxNotesPerColor);
        ChangeVolume(playerId, MaxMusicVolume, 0.5f);
        FadeOut(playerId, _noteBuffer[playerId] * DurationPerNote);
    }

    private void RainOnNote(int playerId)
    {
        if (playerId < 0 || playerId >= _noteBuffer.Length || _noteBuffer[playerId] > 0f) return;
        if (Mathf.Approximately(_currentVolumes[playerId], 0f))
        {
            ChangeReverbFilterMode(playerId, 1, 0f);
            ChangeVolume(playerId, MaxRainMusicVolume, 0.5f);
        }
        FadeOut(playerId, 0.5f);
    }

    private void DecreaseBuffer(int index)
    {
        _decreaseBufferIds[index] = Utility.AddOne(_decreaseBufferIds[index]);
        StartCoroutine(DecreaseBuffer(index, _decreaseBufferIds[index]));
    }

    private void ChangeVolume(int audioSourceIndex, float targetVolume, float duration)
    {
        _changeVolumeIds[audioSourceIndex] = Utility.AddOne(_changeVolumeIds[audioSourceIndex]);
        StartCoroutine(ChangeVolumeRoutine(audioSourceIndex, targetVolume, duration, _changeVolumeIds[audioSourceIndex]));
    }

    private void FadeOut(int audioSourceIndex, float delay)
    {
        _fadeOutIds[audioSourceIndex] = Utility.AddOne(_fadeOutIds[audioSourceIndex]);
        StartCoroutine(FadeOutRoutine(audioSourceIndex, delay, _fadeOutIds[audioSourceIndex]));
    }

    private void ApplyCurrentVolume(int index)
    {
        _audioSources[index].volume = _currentVolumes[index];
    }

    private void ChangeReverbFilterMode(int playerId, int filterMode, float duration)
    {
        if (playerId < 0 || playerId >= _reverbFilters.Length || _reverbFilters[playerId] == null) return;
        
        // filter modes:
        // 0 -> no reverb (off)
        // 1 -> reverb (rain reverb preset)
        
        _changeReverbFilterId = Utility.AddOne(_changeReverbFilterId);
        StartCoroutine(ChangeReverbFiltersRoutine(playerId, filterMode, duration, _changeReverbFilterId));
        _currentFilterMode = filterMode;
    }
    
    private void InitializeReverbFilters()
    {
        _reverbFilters[0].reverbPreset = AudioReverbPreset.Off;
        _reverbFilters[1].reverbPreset = RainReverbPreset;
        
        _dryLevel.x = _reverbFilters[0].dryLevel;
        _room.x = _reverbFilters[0].room;
        _roomHf.x = _reverbFilters[0].roomHF;
        _roomLf.x = _reverbFilters[0].roomLF;
        _decayTime.x = _reverbFilters[0].decayTime;
        _decayHfRatio.x = _reverbFilters[0].decayHFRatio;
        _reflectionsLevel.x = _reverbFilters[0].reflectionsLevel;
        _reflectionsDelay.x = _reverbFilters[0].reflectionsDelay;
        _reverbLevel.x = _reverbFilters[0].reverbLevel;
        _reverbDelay.x = _reverbFilters[0].reverbDelay;
        _hfReference.x = _reverbFilters[0].hfReference;
        _lfReference.x = _reverbFilters[0].lfReference;
        _diffusion.x = _reverbFilters[0].diffusion;
        _density.x = _reverbFilters[0].density;
        _pitch.x = 1f;

        _dryLevel.y = _reverbFilters[1].dryLevel;
        _room.y = _reverbFilters[1].room;
        _roomHf.y = _reverbFilters[1].roomHF;
        _roomLf.y = _reverbFilters[1].roomLF;
        _decayTime.y = _reverbFilters[1].decayTime;
        _decayHfRatio.y = _reverbFilters[1].decayHFRatio;
        _reflectionsLevel.y = _reverbFilters[1].reflectionsLevel;
        _reflectionsDelay.y = _reverbFilters[1].reflectionsDelay;
        _reverbLevel.y = _reverbFilters[1].reverbLevel;
        _reverbDelay.y = _reverbFilters[1].reverbDelay;
        _hfReference.y = _reverbFilters[1].hfReference;
        _lfReference.y = _reverbFilters[1].lfReference;
        _diffusion.y = _reverbFilters[1].diffusion;
        _density.y = _reverbFilters[1].density;
        _pitch.y = 0.9375f;

        for (var i = 0; i <_reverbFilters.Length; i++)
        {
            _reverbFilters[i].reverbPreset = AudioReverbPreset.User;
            ChangeReverbFilterMode(i, 0, 0f);
        }
    }

    private void WriteHolderValues(int playerId)
    {
        _dryLevel.z = _reverbFilters[playerId].dryLevel;
        _room.z = _reverbFilters[playerId].room;
        _roomHf.z = _reverbFilters[playerId].roomHF;
        _roomLf.z = _reverbFilters[playerId].roomLF;
        _decayTime.z = _reverbFilters[playerId].decayTime;
        _decayHfRatio.z = _reverbFilters[playerId].decayHFRatio;
        _reflectionsLevel.z = _reverbFilters[playerId].reflectionsLevel;
        _reflectionsDelay.z = _reverbFilters[playerId].reflectionsDelay;
        _reverbLevel.z = _reverbFilters[playerId].reverbLevel;
        _reverbDelay.z = _reverbFilters[playerId].reverbDelay;
        _hfReference.z = _reverbFilters[playerId].hfReference;
        _lfReference.z = _reverbFilters[playerId].lfReference;
        _diffusion.z = _reverbFilters[playerId].diffusion;
        _density.z = _reverbFilters[playerId].density;

        _pitch.z = _audioSources[playerId].pitch;
    }

    private void ApplyFilterValues(int playerId, int filterMode, float progress)
    {
        _reverbFilters[playerId].dryLevel = Mathf.Lerp(_dryLevel.z, filterMode == 0 ? _dryLevel.x : _dryLevel.y, progress);
        _reverbFilters[playerId].room = Mathf.Lerp(_room.z, filterMode == 0 ? _room.x : _room.y, progress);
        _reverbFilters[playerId].roomHF = Mathf.Lerp(_roomHf.z, filterMode == 0 ? _roomHf.x : _roomHf.y, progress);
        _reverbFilters[playerId].roomLF = Mathf.Lerp(_roomLf.z, filterMode == 0 ? _roomLf.x : _roomLf.y, progress);
        _reverbFilters[playerId].decayTime = Mathf.Lerp(_decayTime.z, filterMode == 0 ? _decayTime.x : _decayTime.y, progress);
        _reverbFilters[playerId].decayHFRatio = Mathf.Lerp(_decayHfRatio.z, filterMode == 0 ? _decayHfRatio.x : _decayHfRatio.y, progress);
        _reverbFilters[playerId].reflectionsLevel = Mathf.Lerp(_reflectionsLevel.z, filterMode == 0 ? _reflectionsLevel.x : _reflectionsLevel.y, progress);
        _reverbFilters[playerId].reflectionsDelay = Mathf.Lerp(_reflectionsDelay.z, filterMode == 0 ? _reflectionsDelay.x : _reflectionsDelay.y, progress);
        _reverbFilters[playerId].reverbLevel = Mathf.Lerp(_reverbLevel.z, filterMode == 0 ? _reverbLevel.x : _reverbLevel.y, progress);
        _reverbFilters[playerId].reverbDelay = Mathf.Lerp(_reverbDelay.z, filterMode == 0 ? _reverbDelay.x : _reverbDelay.y, progress);
        _reverbFilters[playerId].hfReference = Mathf.Lerp(_hfReference.z, filterMode == 0 ? _hfReference.x : _hfReference.y, progress);
        _reverbFilters[playerId].lfReference = Mathf.Lerp(_lfReference.z, filterMode == 0 ? _lfReference.x : _lfReference.y, progress);
        _reverbFilters[playerId].diffusion = Mathf.Lerp(_diffusion.z, filterMode == 0 ? _diffusion.x : _diffusion.y, progress);
        _reverbFilters[playerId].density = Mathf.Lerp(_density.z, filterMode == 0 ? _density.x : _density.y, progress);

        var pitch = Mathf.Lerp(_pitch.z, filterMode == 0 ? _pitch.x : _pitch.y, progress);

        _audioSources[playerId].pitch = pitch;
    }

    private IEnumerator ChangeVolumeRoutine(int audioSourceIndex, float targetVolume, float duration, int id)
    {
        _changeVolumeTimers[audioSourceIndex] = 0f;
        _volumeHolders[audioSourceIndex] = _currentVolumes[audioSourceIndex];
        while (_changeVolumeTimers[audioSourceIndex] < duration && _changeVolumeIds[audioSourceIndex] == id)
        {
            _changeVolumeTimers[audioSourceIndex] += Time.deltaTime;
            _currentVolumes[audioSourceIndex] = Mathf.Lerp(_volumeHolders[audioSourceIndex], targetVolume, _changeVolumeTimers[audioSourceIndex] / duration);
            ApplyCurrentVolume(audioSourceIndex);
            yield return null;
        }

        if (_changeVolumeIds[audioSourceIndex] == id)
        {
            _currentVolumes[audioSourceIndex] = targetVolume;
            ApplyCurrentVolume(audioSourceIndex);
        }

        yield return null;
    }

    private IEnumerator FadeOutRoutine(int audioSourceIndex, float delay, int id)
    {
        _fadeOutTimers[audioSourceIndex] = 0f;
        while (_fadeOutTimers[audioSourceIndex] < delay && _fadeOutIds[audioSourceIndex] == id && Controllers.Input.IsGameRunning())
        {
            _fadeOutTimers[audioSourceIndex] += Time.deltaTime;
            yield return null;
        }

        if (_fadeOutIds[audioSourceIndex] == id)
        {
            ChangeVolume(audioSourceIndex, 0f, 0.75f);
        }

        yield return null;
    }

    private IEnumerator DecreaseBuffer(int playerId, int id)
    {
        yield return null;
        
        while (_noteBuffer[playerId] > 0f && _decreaseBufferIds[playerId] == id)
        {
            _decreaseAmounts[playerId] = Time.deltaTime / DurationPerNote;
            _noteBuffer[playerId] = Mathf.Max(_noteBuffer[playerId] - _decreaseAmounts[playerId], 0f);
            References.Menu.UpdateNoteBuffer(playerId, _noteBuffer[playerId]);
            yield return null;
        }
    }

    private IEnumerator ChangeReverbFiltersRoutine(int playerId, int filterMode, float duration, int id)
    {
        _changeReverbFilterTimer = 0f;
        WriteHolderValues(playerId);
        while (_changeReverbFilterTimer < duration && _changeReverbFilterId == id)
        {
            _changeReverbFilterTimer += Time.deltaTime;
            ApplyFilterValues(playerId, filterMode, _changeReverbFilterTimer / duration);
            yield return null;
        }

        if (_changeReverbFilterId == id)
        {
            ApplyFilterValues(playerId, filterMode, 1f);
        }
        
        yield return null;
    }
}
