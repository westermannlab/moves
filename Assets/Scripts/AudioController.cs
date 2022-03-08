using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AudioController : MonoBehaviour
{ 
    public MusicBox MusicBox;
    
    private const int HeaderSize = 44;

    private string _audioFileFolder;
    private string _audioFilePath;
    
    private struct ClipData
    {
        public int Samples;
        public int Channels;
        public float[] SamplesData;
    }

    private void Awake()
    {
        _audioFileFolder = Application.persistentDataPath + "/audio/";
    }

    public void PlaySoundEffect(SoundEffect soundEffect)
    {
        var speaker = References.Prefabs.GetSpeaker();
        speaker.Play(soundEffect);
    }

    public void SaveClip(AudioClip clip, bool trimSilence)
    {
        if (trimSilence)
        {
            clip = TrimSilence(clip, 0.005f);
        }

        if (clip == null)
        {
            return;
        }

        var data = References.Io.GetData();
        var vp = data != null ? data.vp : "unknown";
        _audioFilePath = _audioFileFolder + "vp_" + vp + "_room_" + Controllers.Level.GetRoomId() + "_" + Utility.Timestamp() + ".wav";
        
        if (!Directory.Exists(_audioFileFolder))
        {
            Directory.CreateDirectory(_audioFileFolder);
        }
        
        var clipData = new ClipData ();
        clipData.Samples = clip.samples;
        clipData.Channels = clip.channels;
        var dataFloat = new float[clip.samples*clip.channels];
        clip.GetData (dataFloat, 0);
        clipData.SamplesData = dataFloat;
        
        using (var fileStream = CreateEmpty(_audioFilePath))
        {
            var memoryStream = new MemoryStream();
            ConvertAndWrite(memoryStream, clipData);
            memoryStream.WriteTo(fileStream);
            WriteHeader(fileStream, clip);
        }
    }

    private FileStream CreateEmpty(string filepath) {
        var fileStream = new FileStream(filepath, FileMode.Create);
        byte emptyByte = new byte();

        for (var i = 0; i < HeaderSize; i++)
        {
            fileStream.WriteByte(emptyByte);
        }

        return fileStream;
    }

    private void ConvertAndWrite(MemoryStream memStream, ClipData clipData)
    {
        var samples = new float[clipData.Samples * clipData.Channels];
        samples = clipData.SamplesData;
        var intData = new Int16[samples.Length];
        var bytesData = new Byte[samples.Length * 2];

        const float rescaleFactor = 32767; //to convert float to Int16

        for (var i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
        }
        Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);
        memStream.Write(bytesData, 0, bytesData.Length);
    }

    private void WriteHeader(FileStream fileStream, AudioClip clip)
    {
        var hz = clip.frequency;
        var channels = clip.channels;
        var samples = clip.samples;

        fileStream.Seek (0, SeekOrigin.Begin);
        var riff = System.Text.Encoding.UTF8.GetBytes ("RIFF");
        fileStream.Write (riff, 0, 4);
        var chunkSize = BitConverter.GetBytes (fileStream.Length - 8);
        fileStream.Write (chunkSize, 0, 4);
        var wave = System.Text.Encoding.UTF8.GetBytes ("WAVE");
        fileStream.Write (wave, 0, 4);
        var fmt = System.Text.Encoding.UTF8.GetBytes ("fmt "); 
        fileStream.Write (fmt, 0, 4);
        var subChunk1 = BitConverter.GetBytes (16);
        fileStream.Write (subChunk1, 0, 4);
        UInt16 one = 1;
        var audioFormat = BitConverter.GetBytes (one);
        fileStream.Write (audioFormat, 0, 2);
        var numChannels = BitConverter.GetBytes (channels);
        fileStream.Write (numChannels, 0, 2);
        var sampleRate = BitConverter.GetBytes (hz);
        fileStream.Write (sampleRate, 0, 4);
        var byteRate = BitConverter.GetBytes (hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
        fileStream.Write (byteRate, 0, 4);
        var blockAlign = (ushort)(channels * 2);
        fileStream.Write (BitConverter.GetBytes (blockAlign), 0, 2);
        UInt16 bps = 16;
        var bitsPerSample = BitConverter.GetBytes (bps);
        fileStream.Write (bitsPerSample, 0, 2);
        var datastring = System.Text.Encoding.UTF8.GetBytes ("data");
        fileStream.Write (datastring, 0, 4);
        var subChunk2 = BitConverter.GetBytes (samples * channels * 2);
        fileStream.Write (subChunk2, 0, 4);
    }
    
    private AudioClip TrimSilence (AudioClip clip, float min)
    {
        var samples = new float[clip.samples];
        clip.GetData (samples, 0);
        return TrimSilence (new List<float> (samples), min, clip.channels, clip.frequency);
    }
    
    private AudioClip TrimSilence (List<float> samples, float min, int channels, int hz)
    {
        int i;
        for (i = 0; i < samples.Count; i++) {
            if (Mathf.Abs(samples [i]) > min) {
                break;
            }
        }
        
        samples.RemoveRange (0, i);

        for (i = Mathf.Max(samples.Count - 1, 0); i > 0; i--) {
            if (Mathf.Abs(samples [i]) > min) {
                break;
            }
        }
        
        samples.RemoveRange (i, Mathf.Max(samples.Count - i, 0));

        if (samples.Count == 0)
        {
            return null;
        }
        
        var clip = AudioClip.Create ("trimmedClip", samples.Count, channels, hz, false);
        clip.SetData (samples.ToArray (), 0);
        return clip;
    }
}
