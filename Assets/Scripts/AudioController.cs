using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AudioController : MonoBehaviour
{ 
    public MusicBox MusicBox;
    
    private struct ClipData
    {
        public int Samples;
        public int Channels;
        public float[] SamplesData;
    }

    public void PlaySoundEffect(SoundEffect soundEffect)
    {
        var speaker = References.Prefabs.GetSpeaker();
        speaker.Play(soundEffect);
    }
}
