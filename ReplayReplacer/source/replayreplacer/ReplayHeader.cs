using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ReplayReplacer.source.replayreplacer
{
    internal class ReplayHeader
    {
        private const int HEADER_OFFSET = 0x08;
        // if we're being passed just a header byte array, we'll need to
        //      subtract the header offset from all of these addresses.
        private const int HEADER_SIZE = 0x390;

        private const int UNKNOWN_OFFSET_0x08   = 0x08;
        private const int VALID_FLAG_OFFSET     = 0x10;
        private const int UNKNOWN_OFFSET_0x14   = 0x14;
        private const int DATE1_OFFSET          = 0x20;
        private const int DATE1_CHAR_OFFSET     = 0x38;
        private const int UNKNOWN_OFFSET_0x50   = 0x50;
        private const int DATE2_OFFSET          = 0x60;
        private const int DATE2_CHAR_OFFSET     = 0x78;
        private const int UNKNOWN_OFFSET_0x90   = 0x90;
        private const int WINNER_OFFSET         = 0x98; // was labelled "winner_maybe"
        private const int P1_STEAMID_OFFSET     = 0x9C;
        private const int P1_NAME_OFFSET        = 0xA4;
        private const int UNKNOWN_OFFSET_0xc8   = 0xc8;
        private const int P2_STEAMID_OFFSET     = 0x166;
        private const int P2_NAME_OFFSET        = 0x16E;
        private const int UNKNOWN_OFFSET_0x192  = 0x192;
        private const int P1_CHAR_OFFSET        = 0x230;
        private const int P2_CHAR_OFFSET        = 0x234;
        private const int RECORDER_STEAMID_OFFSET = 0x238;
        private const int RECORDER_NAME_OFFSET  = 0x240;
        private const int UNKNOWN_OFFSET_0x264  = 0x264;
        private const int P1_LEVEL_OFFSET       = 0x314;
        private const int P2_LEVEL_OFFSET       = 0x318;

        private byte[]  _unknown0x08        = new byte[0x08];
        private uint    _valid;
        private byte[]  _unknown0x14        = new byte[0x0c];
        private uint[]  _date1Ints          = new uint[6];
        private char[]  _date1Char          = new char[0x18];
        private byte[]  _unknown0x50        = new byte[0x10];
        private uint[]  _date2Ints          = new uint[6];
        private char[]  _date2Char          = new char[0x18];
        private byte[]  _unknown0x90        = new byte[0x8];
        private uint    _winnerFlag;
        private ulong   _p1SteamID;
        private byte[]  _p1NameBytes        = new byte[0x12 * 2]; // expected 12 characters of utf-16
        private byte[]  _unknown0xc8        = new byte[0x9e];
        private ulong   _p2SteamID;
        private byte[]  _p2NameBytes        = new byte[0x12 * 2];
        private byte[]  _unknown0x192       = new byte[0x9e];
        private uint    _p1Char;
        private uint    _p2Char;
        private ulong   _recorderSteamID;
        private byte[]  _recorderNameBytes  = new byte[0x12 * 2];
        private byte[]  _unknown0x264       = new byte[0x314 - 0x264]; // math this out
        private uint    _p1Level; // was labelled "minus one"?
        private uint    _p2Level;

        public string P1Name { get; private set; } = "";
        public string P2Name { get; private set; } = "";
        public DateTime Date1 { get; private set; } = DateTime.Now;
        public DateTime Date2 { get; private set; } = DateTime.Now;

        public bool IsValid => _valid != 0;
        
        // ... and I'm not dealing with the rest yet.

        public byte[] _headerBinary = new byte[HEADER_SIZE];


        public static ReplayHeader FromFile(string filePath)
        {
            byte[] bytes;
            using (var fs = File.OpenRead(filePath))
                using (var br = new BinaryReader(fs))
            {
                br.BaseStream.Seek(HEADER_OFFSET, SeekOrigin.Begin);
                bytes = br.ReadBytes(HEADER_SIZE);
            }

            return FromHeaderBytes(bytes);

        }

        public static ReplayHeader FromHeaderBytes(byte[] byteArray)
        {
            var header = new ReplayHeader();
            using (var ms = new MemoryStream(byteArray))
                using (var br = new BinaryReader(ms))
            {
                // yeet the entire header into storage first.
                header._headerBinary = br.ReadBytes(HEADER_SIZE);
                br.BaseStream.Seek(0, SeekOrigin.Begin);

                header._unknown0x08        = br.ReadBytes(0x08);
                header._valid              = br.ReadUInt32();
                header._unknown0x14        = br.ReadBytes(0x0c);
                for (int i = 0; i < 6; i ++)
                {
                    header._date1Ints[i]   = br.ReadUInt32();
                }
                header._date1Char          = br.ReadChars(0x18);
                header._unknown0x50        = br.ReadBytes(0x10);
                for (int i = 0; i < 6; i ++)
                {
                    header._date2Ints[i]   = br.ReadUInt32();
                }
                header._date2Char          = br.ReadChars(0x18);
                header._unknown0x90        = br.ReadBytes(0x8);
                header._winnerFlag         = br.ReadUInt32();
                header._p1SteamID          = br.ReadUInt64();
                header._p1NameBytes        = br.ReadBytes(0x12 * 2);
                header._unknown0xc8        = br.ReadBytes(0x9e);
                header._p2SteamID          = br.ReadUInt64();
                header._p2NameBytes        = br.ReadBytes(0x12 * 2);
                header._unknown0x192       = br.ReadBytes(0x9e);
                header._p1Char             = br.ReadUInt32();
                header._p2Char             = br.ReadUInt32();
                header._recorderSteamID    = br.ReadUInt64();
                header._recorderNameBytes  = br.ReadBytes(0x12 * 2);
                header._unknown0x264       = br.ReadBytes(0x314 - 0x264);
                header._p1Level            = br.ReadUInt32();
                header._p2Level            = br.ReadUInt32();
                
            }
            header.PopulatePrettyData();
            return header;
        }

        private void PopulatePrettyData()
        {
            P1Name = ParseNameBinary(_p1NameBytes);
            P2Name = ParseNameBinary(_p2NameBytes);
            Date1 = ParseDate(_date1Ints);
            Date2 = ParseDate(_date2Ints);
        }

        private static string ParseNameBinary(byte[] nameBinary)
        {
            // names are stored in unicode
            // appears to be some "lazy writing" -- if the new name is shorter than what was there previously,
            //      it doesn't bother nulling out the old data.
            // therefore, we have to split on the first terminator we see, not just strip.
            var name = Encoding.Unicode.GetString(nameBinary).Split('\0')[0];
            return name;
        }

        private static DateTime ParseDate(uint[] dateInts)
        {
            var strDate = string.Join(':', dateInts.Select(n => n.ToString()));
            DateTime dateOut = DateTime.Now;
            if (!DateTime.TryParseExact(strDate, "yyyy:M:d:H:m:s", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateOut))
            {
                // ... Handle failed parsing
                throw new FileFormatException("Cannot translate " + strDate + " into a date.");
            }
            return dateOut;
        }
    }
}
