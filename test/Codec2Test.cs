using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace test
{
    public class Codec2Test
    {
        [Theory]
        [InlineData(Codec2.Mode.b450)]
        [InlineData(Codec2.Mode.b450PWB)]
        [InlineData(Codec2.Mode.b700)]
        [InlineData(Codec2.Mode.b700B)]
        [InlineData(Codec2.Mode.b700C)]
        [InlineData(Codec2.Mode.b1200)]
        [InlineData(Codec2.Mode.b1300)]
        [InlineData(Codec2.Mode.b1400)]
        [InlineData(Codec2.Mode.b1600)]
        [InlineData(Codec2.Mode.b2400)]
        [InlineData(Codec2.Mode.b3200)]
        void testEncodingOverload(Codec2.Mode mode)
        {
            // Assemble
            
            Codec2 c2_1 = new Codec2(mode);
            Codec2 c2_2 = new Codec2(mode);
            Codec2 c2_3 = new Codec2(mode);
            string filePath = "../../../../audioSamples/speech_orig_16k.raw";
            byte[] fileContent = File.ReadAllBytes(filePath);
            
            // test
            
            //encode everything at once
            byte[] encodeAllByte = c2_1.encodeAll(fileContent);

            //encode the first frame
            byte[] encodeFrameBytes = c2_2.encodeFrame(fileContent.Take(c2_2.samplesPerFrame * 2).ToArray());

            List<byte> encodeFrameList = new List<byte>();
            
            byte[] buf = new byte[c2_3.samplesPerFrame * sizeof(short)];
            byte[] bits = new byte[c2_3.bytesPerFrame];
            
            FileStream readfile = File.OpenRead(filePath);

            // read all content of the file.
            while (readfile.Read(buf, 0, c2_3.samplesPerFrame * 2) == c2_3.samplesPerFrame * 2)
            {
                c2_3.encodeFrame(ref bits, buf);
                encodeFrameList.AddRange(bits);
            }

            // assert
            
            // the first frame of all the bytes and the single frame should be the same.
            // TODO b450 and b450PWB sometimes fail
            Assert.Equal(encodeAllByte.Take(c2_1.bytesPerFrame).ToArray(), encodeFrameBytes);
            
            Assert.Equal(encodeFrameList.Take(c2_3.bytesPerFrame).ToArray(), encodeFrameBytes);
            
            // all the encodedframes added together and the encodedAll should have the same result.
            Assert.Equal(encodeAllByte, encodeFrameList.ToArray());
        }
        
        [Theory]
        [InlineData(Codec2.Mode.b450)]
        [InlineData(Codec2.Mode.b450PWB)]
        [InlineData(Codec2.Mode.b700)]
        [InlineData(Codec2.Mode.b700B)]
        [InlineData(Codec2.Mode.b700C)]
        [InlineData(Codec2.Mode.b1200)]
        [InlineData(Codec2.Mode.b1300)]
        [InlineData(Codec2.Mode.b1400)]
        [InlineData(Codec2.Mode.b1600)]
        [InlineData(Codec2.Mode.b2400)]
        [InlineData(Codec2.Mode.b3200)]
        void testDecodingOverload(Codec2.Mode mode)
        {
            // Assemble
            
            Codec2 c2_1 = new Codec2(mode);
            Codec2 c2_2 = new Codec2(mode);
            Codec2 c2_3 = new Codec2(mode);
            string filePath = "../../../../audioSamples/speech_orig_16k." + mode.ToString() + ".enc";
            byte[] fileContent = File.ReadAllBytes(filePath);
            
            // test
            
            //encode everything at once
            byte[] decodeAllBytes = c2_1.decodeAll(fileContent);
            byte[] decodeAllBytes2 = c2_2.decodeAll(fileContent);
            Assert.Equal(decodeAllBytes, decodeAllBytes2);

            //encode the first frame
            byte[] decodeFrameBytes = c2_2.decodeFrame(fileContent.Take(c2_2.bytesPerFrame).ToArray());

            List<byte> decodeFrameList = new List<byte>();
            
            byte[] buf = new byte[c2_3.samplesPerFrame * sizeof(short)];
            byte[] bits = new byte[c2_3.bytesPerFrame];
            
            FileStream readfile = File.OpenRead(filePath);

            // read all content of the file.
            while (readfile.Read(buf, 0, c2_3.bytesPerFrame) == c2_3.bytesPerFrame)
            {
                c2_3.decodeFrame(ref buf, bits);
                decodeFrameList.AddRange(buf);
            }

            // assert
            // TODO all tests fail, decoding seems have a random function somewhere
            
            // the first frame of all the bytes and the single frame should be the same.
            Assert.Equal(decodeAllBytes.Take(c2_1.samplesPerFrame * 2).ToArray(), decodeFrameBytes);
            
            Assert.Equal(decodeFrameList.Take(c2_3.samplesPerFrame * 2).ToArray(), decodeFrameBytes);
            
            // all the encodedframes added together and the encodedAll should have the same result.
            Assert.Equal(decodeAllBytes, decodeFrameList.ToArray());
        }

        [Theory]
        // TODO 700B and 450PWB fails
        [InlineData(Codec2.Mode.b450)]
        [InlineData(Codec2.Mode.b450PWB)]
        [InlineData(Codec2.Mode.b700)]
        [InlineData(Codec2.Mode.b700B)] 
        [InlineData(Codec2.Mode.b700C)]
        [InlineData(Codec2.Mode.b1200)]
        [InlineData(Codec2.Mode.b1300)]
        [InlineData(Codec2.Mode.b1400)]
        [InlineData(Codec2.Mode.b1600)]
        [InlineData(Codec2.Mode.b2400)]
        [InlineData(Codec2.Mode.b3200)]
        void testEncodingIntegrity(Codec2.Mode mode)
        {
            string filePathEnc = "../../../../audioSamples/speech_orig_16k." + mode.ToString() + ".enc";
            string filePathRaw = "../../../../audioSamples/speech_orig_16k.raw";
            byte[] fileContentEnc = File.ReadAllBytes(filePathEnc);
            byte[] fileContentRaw = File.ReadAllBytes(filePathRaw);

            Codec2 c2 = new Codec2(mode);

            byte[] c2Encoded = c2.encodeAll(fileContentRaw);

            Assert.Equal(fileContentEnc, c2Encoded);
        }
        
        [Theory]
        [InlineData(Codec2.Mode.b450)]
        [InlineData(Codec2.Mode.b450PWB)]
        [InlineData(Codec2.Mode.b700)]
        [InlineData(Codec2.Mode.b700B)]
        [InlineData(Codec2.Mode.b700C)]
        [InlineData(Codec2.Mode.b1200)]
        [InlineData(Codec2.Mode.b1300)]
        [InlineData(Codec2.Mode.b1400)]
        [InlineData(Codec2.Mode.b1600)]
        [InlineData(Codec2.Mode.b2400)]
        [InlineData(Codec2.Mode.b3200)]
        void testDecodingIntegrity(Codec2.Mode mode)
        {
            string filePathDec = "../../../../audioSamples/speech_orig_16k." + mode.ToString() + ".dec";
            string filePathEnc = "../../../../audioSamples/speech_orig_16k." + mode.ToString() + ".enc";
            byte[] fileContentDec = File.ReadAllBytes(filePathDec);
            byte[] fileContentEnc = File.ReadAllBytes(filePathEnc);

            Codec2 c2 = new Codec2(mode);

            byte[] c2Decoded = c2.decodeAll(fileContentEnc);

            // TODO decoded data is diffrent everytime.
            Assert.Equal(fileContentDec, c2Decoded);
        }
    }
}