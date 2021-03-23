using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using System.Text;

namespace test
{
    public class c2demoTest
    {
        
        [Fact]
        public void c2demo()
        {
            Codec2 c2 = new Codec2(Codec2.Mode.b3200);
            string filePath = "../../../../audioSamples/speech_orig_16k.raw";
            byte[] buf;
            byte[] bits;
            
            buf = new byte[c2.samplesPerFrame * sizeof(short)];
            bits = new byte[c2.bytesPerFrame];

            FileStream readfile = File.OpenRead(filePath);
            FileStream writeFile = File.OpenWrite("out3200");

            while (readfile.Read(buf, 0, c2.samplesPerFrame * 2) == c2.samplesPerFrame * 2)
            {
                c2.encodeFrame(ref bits, buf);
                c2.decodeFrame(ref buf, bits);
                writeFile.Write(buf, 0, c2.samplesPerFrame * 2);
            }

            // TODO make assert
            writeFile.Close();
            readfile.Close();
        }
    }
}