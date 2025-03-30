using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace ReplayReplacer.source.replayreplacer
{
    internal class ReplayList
    {
        private const int ENTRIES_OFFSET = 0x8;
        private const int ENTRY_SIZE = 0x390;
        private const int N_ENTRIES = 100;

        private byte[] header = new byte[ENTRIES_OFFSET];
        private ReplayHeader[] entries = new ReplayHeader[N_ENTRIES];

        public ReplayList(string ReplayListPath) 
        { 
            using (var fs = File.OpenRead(ReplayListPath))
                using (var br = new BinaryReader(fs))
            {
                header = br.ReadBytes(ENTRIES_OFFSET);
                for (int i = 0; i < 100; i++)
                {
                    var dat = br.ReadBytes(ENTRY_SIZE);
                    entries[i] = ReplayHeader.FromHeaderBytes(dat);
                }
            }
        }

        public List<Label> GetP1Names()
        {
            var names = new List<Label>();
            for (int i = 0; i < 100; i ++)
            {
                if(entries[i].IsValid)
                {
                    var l = new Label();
                    l.Text = entries[i].P1Name;
                    names.Add(l);
                }
            }
            return names;
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
