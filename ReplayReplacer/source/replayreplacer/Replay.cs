using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplayReplacer.source.replayreplacer
{
    internal class Replay
    {
        public byte[] HeaderBinary { get; private set; }

        private DateTime _date1;
        private DateTime _date2;
        private int _winner;
        private String _p1Name;
        private String _p2Name;
        private int _p1Char;
        private int _p2Char;
        private String _recorderName;
        private int _p1Level;
        private int _p2Level;

        private int n1;


        public void FromFile(String ReplayPath) 
        {
            using (FileStream fs = File.OpenRead(ReplayPath))
                using (BinaryReader reader = new BinaryReader(fs)) 
            {
                reader.BaseStream.Seek(0x8, SeekOrigin.Begin);
                HeaderBinary = reader.ReadBytes(912);

                reader.BaseStream.Seek(0x20, SeekOrigin.Begin);
                int[] dateInts = new int[6];
                dateInts[0] = (int) reader.ReadUInt32();    // year
                dateInts[1] = (int) reader.ReadUInt32();    // month
                dateInts[2] = (int) reader.ReadUInt32();    // day
                dateInts[3] = (int) reader.ReadUInt32();    // hours
                dateInts[4] = (int) reader.ReadUInt32();    // minutes
                dateInts[5] = (int) reader.ReadUInt32();    // seconds

                String strDate = string.Join(':', dateInts.Select(n => n.ToString()));
                if (!DateTime.TryParseExact(strDate, "yyyy:MM:dd:HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out _date1))
                {
                    // ... Handle failed parsing
                    throw new FileFormatException("Cannot translate " + strDate + " into a date.");
                }

                // note: 0x12 * 2 is the maximum length of a name, including ellipses.
                //  in the case that a user's name is not long enough to require ellipses,
                //  it should terminate early on the first `\0`.
                reader.BaseStream.Seek(0xA4, SeekOrigin.Begin);
                var chars = reader.ReadBytes(0x12 * 2);
                _p1Name = Encoding.Unicode.GetString(chars).Split('\0')[0];

                reader.BaseStream.Seek(0x16E, SeekOrigin.Begin);
                chars = reader.ReadBytes(0x12 * 2);
                _p2Name = Encoding.Unicode.GetString(chars).Split('\0')[0];


                reader.BaseStream.Seek(0x60, SeekOrigin.Begin);
                n1 = (int)reader.ReadUInt16();
                
            }

        }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_p1Name);
            sb.Append(" vs ");
            sb.Append(_p2Name);
            sb.Append(" @ ");
            sb.Append(_date1.ToString());
            sb.Append(" ");
            sb.Append(n1.ToString());
            return sb.ToString();
        }
    }
}
