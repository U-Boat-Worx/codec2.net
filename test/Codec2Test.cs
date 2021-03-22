using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace test
{
    public class Codec2Test
    {
        [Theory]
        // TODO fails on other modes
        [InlineData(Codec2.Mode.b450)]
        [InlineData(Codec2.Mode.b450PWB)]
        // [InlineData(Codec2.Mode.b700)]
        // [InlineData(Codec2.Mode.b700B)]
        [InlineData(Codec2.Mode.b700C)]
        // [InlineData(Codec2.Mode.b1200)]
        [InlineData(Codec2.Mode.b1300)]
        // [InlineData(Codec2.Mode.b1400)]
        // [InlineData(Codec2.Mode.b1600)]
        // [InlineData(Codec2.Mode.b2400)]
        // [InlineData(Codec2.Mode.b3200)]
        void testEncodingOverload(Codec2.Mode mode)
        {
            // Assemble
            
            Codec2 c2 = new Codec2(mode);
            string filePath = "../../../../audioSamples/speech_orig_16k.raw";
            byte[] fileContent = File.ReadAllBytes(filePath);
            
            // test
            
            //encode everything at once
            byte[] encodeAllByte = c2.encodeAll(fileContent);

            //encode the first frame
            byte[] encodeFrameBytes = c2.encodeFrame(fileContent.Take(c2.samplesPerFrame * 2).ToArray());

            List<byte> encodeFrameList = new List<byte>();
            
            byte[] buf = new byte[c2.samplesPerFrame * sizeof(short)];
            byte[] bits = new byte[c2.bytesPerFrame];
            
            FileStream readfile = File.OpenRead(filePath);

            // read all content of the file.
            while (readfile.Read(buf, 0, c2.samplesPerFrame * 2) == c2.samplesPerFrame * 2)
            {
                c2.encodeFrame(ref bits, buf);
                encodeFrameList.AddRange(bits);
            }

            // assert
            
            // the first frame of all the bytes and the single frame should be the same.
            Assert.Equal(encodeAllByte.Take(c2.bytesPerFrame).ToArray(), encodeFrameBytes);
            
            Assert.Equal(encodeFrameList.Take(c2.bytesPerFrame).ToArray(), encodeFrameBytes);
            
            // all the encodedframes added together and the encodedAll should have the same result.
            Assert.Equal(encodeAllByte, encodeFrameList.ToArray());
        }
    }
}