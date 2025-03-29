using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using Force.Crc32;

namespace ReplayReplacer.source.replayreplacer
{
    internal class ReplayList
    {
        String s1;
        String s2;
        int n1;
        int n2=0;
        int n3=0;

        int reread_year;
        byte[] hashed;
        String name;

        public void FromFile(string ReplayListPath)
        {

        }

        public void DummyTestRewriting(string ReplayListPath)
        {
            // overwrite something random -- here i'm replacing the year.
            using (FileStream fs = File.OpenWrite(ReplayListPath))
            using (BinaryWriter reader = new BinaryWriter(fs))
            {
                // 0x20 didn't change the replay theater display, maybe date2 at 0x60?
                reader.BaseStream.Seek(0x60, SeekOrigin.Begin);
                reader.Write(((UInt32)2022));
            }


            // Read in all the bytes so we can generate a new checksum
            byte[] b = File.ReadAllBytes(ReplayListPath);

            // zero out old checksum
            for (int i = 0; i < 2; i++)
            {
                b[i] = 0;
            }

            // generate checksum
            ushort chsm = CreateChecksum(b);

            // write new bytes
            using (FileStream fs = File.OpenWrite(ReplayListPath))
            using (BinaryWriter reader = new BinaryWriter(fs))
            {
                reader.Write(chsm);
            }
        }

        public override string ToString()
        {
            return (s1 + " " + n1 + " " + n2 + " " + n3);
        }

        public static ushort CreateChecksum(byte[] data)
        {
            uint x = 0x0;
            for (uint i = 0; i < data.Length; i += 0x2)
            {
                var y = (uint)((data[i + 1] << 8) | data[i]);
                x += y;
                y = x;
                x &= 0xFFFF;
                y >>>= 0x10;
                x += y;
            }
            return (ushort)~x;
        }
    }
}
