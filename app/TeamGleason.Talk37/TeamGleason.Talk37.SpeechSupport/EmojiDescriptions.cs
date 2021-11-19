﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace TeamGleason.Talk37.SpeechSupport
{
    /// <summary>
    /// Programmed emojis.
    /// </summary>
    public static class EmojiDescriptions
    {
        public const int Emotionless = -1;
        public const int Idle = -2;
        public const int Editing = -3;

        static readonly string Prefix = Path.Combine(Path.GetFileNameWithoutExtension(typeof(EmojiDescriptions).GetTypeInfo().Assembly.ManifestModule.Name), "Assets");
        static readonly string Theme = "beepy";
        static readonly string Suffix = ".wav";

        static readonly Dictionary<int, EmojiDescription> _emojis = new Dictionary<int, EmojiDescription>();

        static void AddEmoji(EmojiDescription description)
        {
            foreach (var utf32 in description.Utf32s)
            {
                _emojis.Add(utf32, description);
            }
        }

        static void AddEmoji(int utf32, string audioFileName, string visualString)
        {
            var path = audioFileName != null ? Path.Combine(Prefix, Theme + '_' + audioFileName + Suffix) : null;
            var description = new EmojiDescription(utf32, path, visualString);
            AddEmoji(description);
        }

        static void AddEmoji(string emoji, string audioFileName, string visualString)
        {
            if (emoji.Length != (char.IsSurrogatePair(emoji, 0) ? 2 : 1))
            {
                throw new ArgumentOutOfRangeException("emoji");
            }

            var utf32 = char.ConvertToUtf32(emoji, 0);

            AddEmoji(utf32, audioFileName, visualString);
        }
        static EmojiDescriptions()
        {
            AddEmoji("😡", "angry", "0100b302000000fd01010303I4000000101010100000001010101010100010101010101010101010101010101010101010101010101010101010101010100010101010101000000010101010000P0612001500230024002a002d00P00");
            AddEmoji("😂", "happy", "01034909000000271b08f9f9f94f38107654189d70209e7020c58c28fbad390707I4000000101010100000001010101010100010100010100010101010101010101010102020202020201010202020202020100010202020201000000010101010000P00I4000000303030300000003030303030300030300030300030303030303030303030302020202020203030202020202020300030202020203000000030303030000I4000000404040400000004040404040400040400040400040404040404040404040402020202020204040202020202020400040202020204000000040404040000I4000000506060600000005060606060500060600060600060606050606060605060602020202020206060202020202020600060202020206000000060606060000P1e02070307040705070a070b070c070d0710071107130714071607170718071a071b071c071d071f072005270528072f07310736073a073b073c073d07I4000000808080800000008080808080800080800080800080808080808080808080802020202020208080202020202020800080202020208000000080808080000");
            AddEmoji("❤", "heart", "01018804000000fd01019700004b00000606P2009010a010d010e0110011101120113011401150116011701180119011a011b011c011d011e011f012101220123012401250126012a012b012c012d0133013401P0e09020a020d020e021002170218021f02210226022a022d0233023402P1009030a030d030e03100313021402170318031f03210326032a032d0333033403P1209000a000d000e0010001102130314031602170018001f00210026002a002d0033003400P041103130014001603P0211001600");
            AddEmoji("☹", "sad", "010283070000003a290c0106fd765418b27e24eea93100aedd0909I4000000101010100000001010101010100010100010100010101010101010101010101010101010101010101020201010100010201010201000000010101010000I4000000303030300000003030303030300030300030300030303030303030303030303030303030303030303020203030300030203030203000000030303030000I4000000404040400000004040404040400040400040400040404040404040404040404040404040404040404020204040400040204040204000000040404040000I4000000505050500000005050505050500050500050500050505050505050505050505050505050505050505020205050500050205050205000000050505050000P011e06P0319061e052606P00P0519051e06260529063606P051e052606290531063605");
            AddEmoji("💩", "poop", "01017804000000f15336009abdfed2b13614P010301P010c01P011501P011c01P012301P012a01P013101P0238013901P013a01P013b01P013c01P013d01P013e01P013f01P013601P013501P013401P013301P013201P013001P013701P012e01P012d01P012c01P012b01P012901P012101P012201P012401P012501P012601P011d01P011b01P011a01P011401P011301P020b011201P010401P0622022502320335033b033c03P00P00P00P00P00P00P00P00P00P00P00P00P00P00P00");
            AddEmoji("😘", "winkykiss", "0100b004000000eea9310123fdfd01090202I4000000101010100000001010101010100010102010102010101010101010101010101010101010101010101010101010100010103030101000000010101010000P0312012b032c03");
            AddEmoji("😲", "surprise", "0100c103000000eea931011ffd0302I4000000101010100000001010101010100010102010102010101010101010101010101010000010101010100000000010100010100000101000000010101010000P00P08230124012a012b012c012d0133013401");
            AddEmoji("😱", "fear", "0100c103000000eea931011ffd0302I4000000101010100000001010101010100010102010102010101010101010101010101010000010101010100000000010100010100000101000000010101010000P00P08230124012a012b012c012d0133013401");
            AddEmoji("👌", "bird", "01008902000000ffdca90503P170c0114011c01220123012401250129012a012b012c012d012e013101320133013401350136013a013b013c013d01P010c00P00P011400P011c00");
            AddEmoji("😃", "poop", "01017804000000f15336009abdfed2b13614P010301P010c01P011501P011c01P012301P012a01P013101P0238013901P013a01P013b01P013c01P013d01P013e01P013f01P013601P013501P013401P013301P013201P013001P013701P012e01P012d01P012c01P012b01P012901P012101P012201P012401P012501P012601P011d01P011b01P011a01P011401P011301P020b011201P010401P0622022502320335033b033c03P00P00P00P00P00P00P00P00P00P00P00P00P00P00P00");
            AddEmoji("😒", "meh", "01017804000000f15336009abdfed2b13614P010301P010c01P011501P011c01P012301P012a01P013101P0238013901P013a01P013b01P013c01P013d01P013e01P013f01P013601P013501P013401P013301P013201P013001P013701P012e01P012d01P012c01P012b01P012901P012101P012201P012401P012501P012601P011d01P011b01P011a01P011401P011301P020b011201P010401P0622022502320335033b033c03P00P00P00P00P00P00P00P00P00P00P00P00P00P00P00");

            AddEmoji(Emotionless, null, "0100a603000000eea9310d62af0202I4000000101010100000001010101010100010102010102010101010101010101010101010101010101010100000000010100010101010101000000010101010000P0233003400");
            AddEmoji(Idle, null, true ? "-" : "01009b03000000eea931007dc30103I4000000101010100000001010101010100010102010102010101010101010101010101010101010101010100000000010100010101010101000000010101010000");
            AddEmoji(Editing, null, "0100f602000000e03a821414P040201030104010501P0205000901P0204001001P0203001801P0202002001P0209002801P0210003101P0218003a01P0220003b01P0228003c01P0231003d01P0236013a00P022f013b00P0227013c00P021f013d00P0217013600P020e012f00P0205012700P0204011f00P0203011700");
        }

        /// <summary>
        /// Get the emoji description for a UTF-32 character.
        /// </summary>
        /// <param name="utf32">The character to decode.</param>
        /// <returns>The Emoji description or null if no mapping.</returns>
        public static EmojiDescription Get(int utf32)
        {
            EmojiDescription description;

            _emojis.TryGetValue(utf32, out description);

            return description;
        }
    }
}
