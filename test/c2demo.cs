using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using System.Text;

namespace test
{
    public class c2demoTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public c2demoTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
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

        [Fact]
        public void c2demo()
        {
            IntPtr CODEC2;
            string filePath = "../../../../audioSamples/speech_orig_16k.raw";
            byte[] buf;
            byte[] bits;
            int nsam, nbit, i, r;
            for (i = 0; i < 10; i++)
            {
                r = codec2.codec2_rand();
                _testOutputHelper.WriteLine($"[{i}] r = {r}");
            }

            /* Note only one set of Codec 2 states is required for an encoder and decoder pair. */
            CODEC2 = codec2.codec2_create(codec2.CODEC2_MODE_3200);
            nsam = codec2.codec2_samples_per_frame(CODEC2);
            buf = new byte[nsam * sizeof(short)];
            nbit = codec2.codec2_bits_per_frame(CODEC2);
            bits = new byte[nbit];
            StringBuilder str_bits = new StringBuilder(nbit * sizeof(char));

            FileStream readfile = File.OpenRead(filePath);
            FileStream w = File.OpenWrite("out");

            while (readfile.Read(buf, 0, nsam * 2) == nsam * 2)
            {
                short[] buf_s = BytesToShorts(buf);
                codec2.codec2_encode(CODEC2, bits, buf_s);
                codec2.codec2_decode(CODEC2, buf_s, bits);
                w.Write(ShortsToBytes(buf_s), 0, nsam * 2);
            }
            // TODO make assert
            w.Close();
            readfile.Close();

            codec2.codec2_destroy(CODEC2);
        }
    }
}