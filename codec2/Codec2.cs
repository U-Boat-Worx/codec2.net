using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

public class Codec2
{
    public enum Mode
    {
        b3200 = 0,
        b2400 = 1,
        b1600 = 2,
        b1400 = 3,
        b1300 = 4,
        b1200 = 5,
        b700 = 6,
        b700B = 7,
        b700C = 8,
        b450 = 10,
        b450PWB = 11,
    }


    [DllImport("libcodec2")]
    private static extern IntPtr codec2_create(int mode);

    [DllImport("libcodec2")]
    private static extern void codec2_destroy(IntPtr codec2_state);

    [DllImport("libcodec2")]
    private static extern void codec2_encode(IntPtr codec2_state, [In, Out] byte[] bits, short[] speech_in);

    [DllImport("libcodec2")]
    private static extern void codec2_decode(IntPtr codec2_state, [In, Out] short[] speech_out, byte[] bits);

    [DllImport("libcodec2")]
    private static extern void codec2_decode_ber(IntPtr codec2_state, [In, Out] short[] speech_out, byte[] bits,
        float ber_est);

    [DllImport("libcodec2")]
    private static extern int codec2_samples_per_frame(IntPtr codec2_state);

    [DllImport("libcodec2")]
    private static extern int codec2_bits_per_frame(IntPtr codec2_state);

    // borrowed from sine.h
    [DllImport("libcodec2")]
    private static extern int codec2_rand();

    // TODO add full support for all codec2_* calls. https://github.com/drowe67/codec2/blob/master/src/codec2.h

    private IntPtr state;

    /// <summary>
    /// Get the number samples per frame used for this mode. A sample is int16_t.
    /// </summary>
    public int samplesPerFrame => codec2_samples_per_frame(state);

    /// <summary>
    /// Get the amount of bits for and encoded audio frame for this mode.
    /// </summary>
    public int bitsPerFrame => codec2_bits_per_frame(state);

    /// <summary>
    /// Get the amount of bytes for and encoded audio frame for this mode.
    /// </summary>
    public int bytesPerFrame => (bitsPerFrame + 7) / 8; // +7 magic from c2enc.c

    /// <summary>
    /// Codec2 to encode and decode audio fragments.
    /// </summary>
    /// <param name="bitrate">Mode to use</param>
    public Codec2(Mode bitrate)
    {
        this.state = codec2_create((int) bitrate);
    }

    ~Codec2()
    {
        codec2_destroy(state);
    }

    // copied from https://github.com/lostromb/concentus/blob/master/CSharp/ConcentusDemo/AudioMath.cs

    /// <summary>
    /// Converts interleaved byte samples (such as what you get from a capture device)
    /// into linear short samples (that are much easier to work with)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    internal static short[] BytesToShorts(byte[] input)
    {
        return BytesToShorts(input, 0, input.Length);
    }

    /// <summary>
    /// Converts interleaved byte samples (such as what you get from a capture device)
    /// into linear short samples (that are much easier to work with)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    internal static short[] BytesToShorts(byte[] input, int offset, int length)
    {
        short[] processedValues = new short[length / 2];
        for (int c = 0; c < processedValues.Length; c++)
        {
            processedValues[c] = (short) (((int) input[(c * 2) + offset]) << 0);
            processedValues[c] += (short) (((int) input[(c * 2) + 1 + offset]) << 8);
        }

        return processedValues;
    }

    /// <summary>
    /// Converts linear short samples into interleaved byte samples, for writing to a file, waveout device, etc.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    internal static byte[] ShortsToBytes(short[] input)
    {
        return ShortsToBytes(input, 0, input.Length);
    }

    /// <summary>
    /// Converts linear short samples into interleaved byte samples, for writing to a file, waveout device, etc.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    internal static byte[] ShortsToBytes(short[] input, int offset, int length)
    {
        byte[] processedValues = new byte[length * 2];
        for (int c = 0; c < length; c++)
        {
            processedValues[c * 2] = (byte) (input[c + offset] & 0xFF);
            processedValues[c * 2 + 1] = (byte) ((input[c + offset] >> 8) & 0xFF);
        }

        return processedValues;
    }

    /// <summary>
    /// Encode a single frame of audio and store result in byte array.
    /// </summary>
    /// <param name="encodedOutput">buffer to store the result in</param>
    /// <param name="rawAudioFrameInput">input audio frame</param>
    public void encodeFrame(ref byte[] encodedOutput, short[] rawAudioFrameInput)
    {
        codec2_encode(state, encodedOutput, rawAudioFrameInput);
    }

    /// <summary>
    /// Encode a single frame of audio and store result in byte array.
    /// </summary>
    /// <param name="encodedOutput">buffer to store the result in</param>
    /// <param name="rawAudioFrameInput">input audio frame</param>
    public void encodeFrame(ref byte[] encodedOutput, byte[] rawAudioFrameInput)
    {
        encodeFrame(ref encodedOutput, BytesToShorts(rawAudioFrameInput));
    }

    /// <summary>
    /// Encode a single frame of audio and store result in byte array.
    /// </summary>
    /// <param name="rawAudioFrameInput">input audio frame</param>
    /// <returns>encoded audio in bytearray</returns>
    public byte[] encodeFrame(short[] rawAudioFrameInput)
    {
        byte[] buffer = new byte[bytesPerFrame];
        encodeFrame(ref buffer, rawAudioFrameInput);
        return buffer;
    }

    /// <summary>
    /// Encode a single frame of audio and store result in byte array.
    /// </summary>
    /// <param name="rawAudioFrameInput">input audio frame</param>
    /// <returns>encoded audio in bytearray</returns>
    public byte[] encodeFrame(byte[] rawAudioFrameInput)
    {
        return encodeFrame(BytesToShorts(rawAudioFrameInput));
    }

    /// <summary>
    /// Encode all data in rawAudioInput, incomplete frame will be dropped
    /// </summary>
    /// <param name="rawAudioInput">audio input containing one or more frames</param>
    /// <returns>encoded audio in bytearray</returns>
    public byte[] encodeAll(short[] rawAudioInput)
    {
        List<byte> outputbuffer = new List<byte>();
        int iterations = (int) Math.Floor((double) rawAudioInput.Length / (double) samplesPerFrame);
        for (int i = 0; i < iterations; i++)
        {
            outputbuffer.AddRange(encodeFrame(rawAudioInput.Skip(i * samplesPerFrame).Take(samplesPerFrame).ToArray()));
        }

        return outputbuffer.ToArray();
    }

    /// <summary>
    /// Encode all data in rawAudioInput, incomplete frame will be dropped
    /// </summary>
    /// <param name="rawAudioInput">audio input containing one or more frames</param>
    /// <returns>encoded audio in bytearray</returns>
    public byte[] encodeAll(byte[] rawAudioInput)
    {
        return encodeAll(BytesToShorts(rawAudioInput));
    }

    // decode

    /// <summary>
    /// Decode a single frame of audio and store result in short array.
    /// </summary>
    /// <param name="rawAudioFrameOutput">output audio frame</param>
    /// <param name="encodedInput">encoded data to decode</param>
    public void decodeFrame(ref short[] rawAudioFrameOutput, byte[] encodedInput)
    {
        codec2_decode(state, rawAudioFrameOutput, encodedInput);
    }

    /// <summary>
    /// Decode a single frame of audio and store result in byte array.
    /// </summary>
    /// <param name="rawAudioFrameOutput">output audio frame</param>
    /// <param name="encodedInput">encoded data to decode</param>
    public void decodeFrame(ref byte[] rawAudioFrameOutput, byte[] encodedInput)
    {
        short[] outbuf_s = new short[samplesPerFrame];
        decodeFrame(ref outbuf_s, encodedInput);
        rawAudioFrameOutput = ShortsToBytes(outbuf_s);
    }

    /// <summary>
    /// Decode a single frame of audio.
    /// </summary>
    /// <param name="encodedInput">input encoded audio frame</param>
    /// <returns>raw audio bytearray</returns>
    public byte[] decodeFrame(byte[] encodedInput)
    {
        byte[] buffer = new byte[samplesPerFrame * sizeof(short)];
        decodeFrame(ref buffer, encodedInput);
        return buffer;
    }

    /// <summary>
    /// Decode a single frame of audio.
    /// </summary>
    /// <param name="encodedInput">input encoded audio frame</param>
    /// <returns>raw audio shortarray</returns>
    public short[] decodeFrameShort(byte[] encodedInput)
    {
        return BytesToShorts(decodeFrame(encodedInput));
    }

    /// <summary>
    /// decode all data in encodedInput, incomplete frame will be dropped
    /// </summary>
    /// <param name="encodedInput">encoded audio input containing one or more frames</param>
    /// <returns>raw decoded audio in bytearray</returns>
    public byte[] decodeAll(byte[] encodedInput)
    {
        List<byte> outputbuffer = new List<byte>();
        int iterations = (int) Math.Floor((double) encodedInput.Length / (double) bytesPerFrame);
        for (int i = 0; i < iterations; i++)
        {
            outputbuffer.AddRange(decodeFrame(encodedInput.Skip(i * bytesPerFrame).Take(bytesPerFrame).ToArray()));
        }

        return outputbuffer.ToArray();
    }

    /// <summary>
    /// decode all data in encodedInput, incomplete frame will be dropped
    /// </summary>
    /// <param name="encodedInput">encoded audio input containing one or more frames</param>
    /// <returns>raw decoded audio in shortarray</returns>
    public short[] decodeAllShort(byte[] encodedInput)
    {
        return BytesToShorts(decodeAll(encodedInput));
    }
}