using System;
using System.Diagnostics;
using PKHeX.Core;

namespace SysBot.Pokemon
{
    public sealed class TradePartnerBS
    {
        public uint IDHash { get; }

        public int TID7 { get; }
        public int SID7 { get; }

        public string TID { get; }
        public string SID { get; }
        public string TrainerName { get; }

        public byte Game { get; }
        public byte Language { get; }
        public byte Gender { get; }
        public TradePartnerBS(byte[] TIDSID, byte[] idbytes, byte[] trainerNameObject)
        {
            Debug.Assert(TIDSID.Length == 4);
            IDHash = BitConverter.ToUInt32(TIDSID, 0);
            TID7 = (int)Math.Abs(IDHash % 1_000_000);
            SID7 = (int)Math.Abs(IDHash / 1_000_000);
            TID = $"{TID7:000000}";
            SID = $"{SID7:0000}";

            Game = idbytes[0];
            Gender = idbytes[1];
            Language = idbytes[3];

            TrainerName = ReadStringFromRAMObject(trainerNameObject);
        }

        public const int MaxByteLengthStringObject = 0x14 + 0x1A;

        public static string ReadStringFromRAMObject(byte[] obj)
        {
            // 0x10 typeinfo/monitor, 0x4 len, char[len]
            const int ofs_len = 0x10;
            const int ofs_chars = 0x14;
            Debug.Assert(obj.Length >= ofs_chars);

            // Detect string length, but be cautious about its correctness (protect against bad data)
            int maxCharCount = (obj.Length - ofs_chars) / 2;
            int length = BitConverter.ToInt32(obj, ofs_len);
            if (length < 0 || length > maxCharCount)
                length = maxCharCount;

            return StringConverter8.GetString(obj.AsSpan(ofs_chars, length * 2));
        }
    }
}
