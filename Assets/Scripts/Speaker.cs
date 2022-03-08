using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speaker : PoolObject
{
    private AudioSource _audioSource;

    private float _currentVolume;
    private float _volumeHolder;
    private float _fadeTimer;
    private int _fadeId;

    /*
    public AudioSource AudioSource => _audioSource;
    public AudioClip Clip => _audioSource != null ? _audioSource.clip : null;
    */

    protected override void Awake()
    {
        base.Awake();
        _audioSource = GetComponent<AudioSource>();
    }


    public override void Reset()
    {
        base.Reset();
        _audioSource.loop = false;
    }

    public void Play(SoundEffect soundEffect)
    {
        Play(soundEffect.Clip, soundEffect.Volume);
    }
    
    public void Play(AudioClip clip, float volume)
    {
        _audioSource.clip = clip;
        _audioSource.volume = volume;
        _audioSource.Play();
        Return(clip != null ? clip.length + 0.125f : 0f);
    }

    public void StartLoop(SoundEffect soundEffect, float fadeInDuration)
    {
        _audioSource.clip = soundEffect.Clip;
        _audioSource.loop = true;
        SetVolume(0f);
        _audioSource.Play();
        Fade(soundEffect.Volume, fadeInDuration);
    }

    public void EndLoop(float fadeOutDuration)
    {
        Fade(0f, fadeOutDuration);
        Return(fadeOutDuration + 0.125f);
    }

    private void SetVolume(float volume)
    {
        _currentVolume = volume;
    }

    private void Fade(float targetVolume, float duration)
    {
        _fadeId++;
        StartCoroutine(FadeRoutine(targetVolume, duration, _fadeId));
    }

    private void ApplyCurrentVolume()
    {
        _audioSource.volume = _currentVolume;
    }

    private IEnumerator FadeRoutine(float targetVolume, float duration, int id)
    {
        _fadeTimer = 0f;
        _volumeHolder = _currentVolume;
        while (_fadeTimer < duration && _fadeId == id)
        {
            _fadeTimer += Time.deltaTime;
            _currentVolume = Mathf.Lerp(_volumeHolder, targetVolume, _fadeTimer / duration);
            ApplyCurrentVolume();
            yield return null;
        }

        if (_fadeId == id)
        {
            _currentVolume = targetVolume;
            ApplyCurrentVolume();
        }

        yield return null;
    }
}
