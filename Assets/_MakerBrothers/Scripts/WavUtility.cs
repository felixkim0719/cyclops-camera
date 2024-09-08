using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    // Converts an AudioClip to WAV format and returns the byte array
    public static byte[] FromAudioClip(AudioClip clip)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            // Write WAV file header
            WriteHeader(stream, clip);

            // Write audio data
            WriteData(stream, clip);

            return stream.ToArray();
        }
    }

    private static void WriteHeader(Stream stream, AudioClip clip)
    {
        int sampleCount = clip.samples * clip.channels;
        int byteRate = clip.frequency * clip.channels * 2; // 16-bit audio

        // ChunkID "RIFF"
        stream.Write(new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' }, 0, 4);
        // ChunkSize (file size - 8 bytes)
        stream.Write(BitConverter.GetBytes(36 + sampleCount * 2), 0, 4);
        // Format "WAVE"
        stream.Write(new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' }, 0, 4);

        // Subchunk1ID "fmt "
        stream.Write(new byte[] { (byte)'f', (byte)'m', (byte)'t', (byte)' ' }, 0, 4);
        // Subchunk1Size (16 for PCM)
        stream.Write(BitConverter.GetBytes(16), 0, 4);
        // AudioFormat (1 for PCM)
        stream.Write(BitConverter.GetBytes((short)1), 0, 2);
        // NumChannels
        stream.Write(BitConverter.GetBytes((short)clip.channels), 0, 2);
        // SampleRate
        stream.Write(BitConverter.GetBytes(clip.frequency), 0, 4);
        // ByteRate (SampleRate * NumChannels * BitsPerSample / 8)
        stream.Write(BitConverter.GetBytes(byteRate), 0, 4);
        // BlockAlign (NumChannels * BitsPerSample / 8)
        stream.Write(BitConverter.GetBytes((short)(clip.channels * 2)), 0, 2);
        // BitsPerSample
        stream.Write(BitConverter.GetBytes((short)16), 0, 2);

        // Subchunk2ID "data"
        stream.Write(new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' }, 0, 4);
        // Subchunk2Size (NumSamples * NumChannels * BitsPerSample / 8)
        stream.Write(BitConverter.GetBytes(sampleCount * 2), 0, 4);
    }


    private static void WriteData(Stream stream, AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        foreach (var sample in samples)
        {
            short intSample = (short)(sample * 32767);
            byte[] byteArr = BitConverter.GetBytes(intSample);
            stream.Write(byteArr, 0, 2);
        }
    }
}
