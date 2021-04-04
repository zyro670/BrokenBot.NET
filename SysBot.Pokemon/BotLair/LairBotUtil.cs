using PKHeX.Core;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace SysBot.Pokemon
{
    public sealed class LairBotUtil
    {
        public static CancellationTokenSource EmbedSource = new();
        public static List<LairSpecies> NoteRequest = new();
        public static bool EmbedsInitialized;
        public static (PK8?, bool) EmbedMon;
        public static readonly MoveInfoRoot? Moves = LoadMoves();
        // Copied over from PKHeX due to accessibility
        internal static readonly ushort[] Pouch_Regular_SWSH =
{
            045, 046, 047, 048, 049, 050, 051, 052, 053, 076, 077, 079, 080, 081, 082, 083, 084, 085, 107, 108, 109,
            110, 112, 116, 117, 118, 119, 135, 136, 213, 214, 215, 217, 218, 219, 220, 221, 222, 223, 224, 225, 228,
            229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 240, 241, 242, 243, 244, 245, 246, 247, 248, 249,
            250, 251, 252, 253, 254, 255, 257, 258, 259, 265, 266, 267, 268, 269, 270, 271, 272, 273, 274, 275, 276,
            277, 278, 279, 280, 281, 282, 283, 284, 285, 286, 287, 288, 289, 290, 291, 292, 293, 294, 295, 296, 297,
            298, 299, 300, 301, 302, 303, 304, 305, 306, 307, 308, 309, 310, 311, 312, 313, 314, 315, 316, 317, 318,
            319, 320, 321, 322, 323, 324, 325, 326, 485, 486, 487, 488, 489, 490, 491, 537, 538, 539, 540, 541, 542,
            543, 544, 545, 546, 547, 564, 565, 566, 567, 568, 569, 570, 639, 640, 644, 645, 646, 647, 648, 649, 650,
            846, 849, 879, 880, 881, 882, 883, 884, 904, 905, 906, 907, 908, 909, 910, 911, 912, 913, 914, 915, 916,
            917, 918, 919, 920, 1103, 1104, 1109, 1110, 1111, 1112, 1113, 1114, 1115, 1116, 1117, 1118, 1119, 1120,
            1121, 1122, 1123, 1124, 1125, 1126, 1127, 1128, 1129, 1231, 1232, 1233, 1234, 1235, 1236, 1237, 1238, 1239,
            1240, 1241, 1242, 1243, 1244, 1245, 1246, 1247, 1248, 1249, 1250, 1251, 1252, 1253, 1254,

            1279,
            1280, 1281, 1282, 1283, 1284, 1285, 1286, 1287, 1288, 1289, 1290, 1291, 1292, 1293, 1294, 1295, 1296, 1297,
            1298, 1299, 1300, 1301, 1302, 1303, 1304, 1305, 1306, 1307, 1308, 1309, 1310, 1311, 1312, 1313, 1314, 1315,
            1316, 1317, 1318, 1319, 1320, 1321, 1322, 1323, 1324, 1325, 1326, 1327, 1328, 1329, 1330, 1331, 1332, 1333,
            1334, 1335, 1336, 1337, 1338, 1339, 1340, 1341, 1342, 1343, 1344, 1345, 1346, 1347, 1348, 1349, 1350, 1351,
            1352, 1353, 1354, 1355, 1356, 1357, 1358, 1359, 1360, 1361, 1362, 1363, 1364, 1365, 1366, 1367, 1368, 1369,
            1370, 1371, 1372, 1373, 1374, 1375, 1376, 1377, 1378, 1379, 1380, 1381, 1382, 1383, 1384, 1385, 1386, 1387,
            1388, 1389, 1390, 1391, 1392, 1393, 1394, 1395, 1396, 1397, 1398, 1399, 1400, 1401, 1402, 1403, 1404, 1405,
            1406, 1407, 1408, 1409, 1410, 1411, 1412, 1413, 1414, 1415, 1416, 1417, 1418, 1419, 1420, 1421, 1422, 1423,
            1424, 1425, 1426, 1427, 1428, 1429, 1430, 1431, 1432, 1433, 1434, 1435, 1436, 1437, 1438, 1439, 1440, 1441,
            1442, 1443, 1444, 1445, 1446, 1447, 1448, 1449, 1450, 1451, 1452, 1453, 1454, 1455, 1456, 1457, 1458, 1459,
            1460, 1461, 1462, 1463, 1464, 1465, 1466, 1467, 1468, 1469, 1470, 1471, 1472, 1473, 1474, 1475, 1476, 1477,
            1478, 1479, 1480, 1481, 1482, 1483, 1484, 1485, 1486, 1487, 1488, 1489, 1490, 1491, 1492, 1493, 1494, 1495,
            1496, 1497, 1498, 1499, 1500, 1501, 1502, 1503, 1504, 1505, 1506, 1507, 1508, 1509, 1510, 1511, 1512, 1513,
            1514, 1515, 1516, 1517, 1518, 1519, 1520, 1521, 1522, 1523, 1524, 1525, 1526, 1527, 1528, 1529, 1530, 1531,
            1532, 1533, 1534, 1535, 1536, 1537, 1538, 1539, 1540, 1541, 1542, 1543, 1544, 1545, 1546, 1547, 1548, 1549,
            1550, 1551, 1552, 1553, 1554, 1555, 1556, 1557, 1558, 1559, 1560, 1561, 1562, 1563, 1564, 1565, 1566, 1567,
            1568, 1569, 1570, 1571, 1572, 1573, 1574, 1575, 1576, 1577, 1578, 1581, 1582, 1588,

            // DLC 2
            1592, 1604, 1606
        };

        public class MoveInfo
        {
            public int MoveID { get; set; }
            public string Name { get; set; } = string.Empty;
            public MoveType Type { get; set; }
            public MoveCategory Category { get; set; }
            public int Power { get; set; }
            public int Priority { get; set; }
            public int EffectSequence { get; set; }
            public int Recoil { get; set; }
            public int PowerGmax { get; set; }
            public bool Charge { get; set; }
            public bool Recharge { get; set; }
            public bool Gravity { get; set; }
            public MoveTarget Target { get; set; }
        }

        public class MoveInfoRoot
        {
            public HashSet<MoveInfo> Moves { get; set; } = new();
        }

        public static int CalculateSpeed(PK8 pk) => ((pk.IV_SPE + (2 * pk.PersonalInfo.SPE) + (pk.EV_SPE / 4)) * pk.CurrentLevel / 100) + 5; // Taken from PKHeX

        public static int PriorityIndex(PK8 pk)
        {
            int selectIndex = -1;
            for (int i = 0; i < pk.Moves.Length; i++)
            {
                if (Enum.IsDefined(typeof(PriorityMoves), pk.Moves[i]))
                {
                    selectIndex = i;
                    break;
                }
            }
            return selectIndex;
        }

        public static bool TypeImmunity(PK8 pk, PK8 lairPk, int move)
        {
            bool[] immune = new bool[2];
            bool drySkin = Moves?.Moves.FirstOrDefault(x => x.MoveID == pk.Moves[move] && x.Type == MoveType.Water) != default && (lairPk.Ability == (int)Ability.DrySkin || lairPk.Ability == (int)Ability.WaterAbsorb || lairPk.Ability == (int)Ability.StormDrain);
            bool voltAbsorb = Moves?.Moves.FirstOrDefault(x => x.MoveID == pk.Moves[move] && x.Type == MoveType.Electric) != default && lairPk.Ability == (int)Ability.VoltAbsorb;
            if (drySkin || voltAbsorb)
                return true;

            int[] types = new int[] { lairPk.PersonalInfo.Type1, lairPk.PersonalInfo.Type2 };
            for (int i = 0; i < 2; i++)
            {
                immune[i] = (types[i]) switch
                {
                    0 => Moves?.Moves.FirstOrDefault(x => x.MoveID == pk.Moves[move] && x.Type == MoveType.Ghost) != default,
                    2 => Moves?.Moves.FirstOrDefault(x => x.MoveID == pk.Moves[move] && x.Type == MoveType.Ground) != default || lairPk.Ability == (int)Ability.Levitate,
                    4 => Moves?.Moves.FirstOrDefault(x => x.MoveID == pk.Moves[move] && x.Type == MoveType.Electric) != default,
                    7 => Moves?.Moves.FirstOrDefault(x => x.MoveID == pk.Moves[move] && (x.Type == MoveType.Normal || x.Type == MoveType.Fighting)) != default,
                    8 => Moves?.Moves.FirstOrDefault(x => x.MoveID == pk.Moves[move] && x.Type == MoveType.Poison) != default && pk.Ability != (int)Ability.Corrosion,
                    16 => Moves?.Moves.FirstOrDefault(x => x.MoveID == pk.Moves[move] && x.Type == MoveType.Psychic) != default,
                    17 => Moves?.Moves.FirstOrDefault(x => x.MoveID == pk.Moves[move] && x.Type == MoveType.Dragon) != default,
                    _ => false,
                };
            }
            return immune[0] || immune[1];
        }

        public static MoveInfoRoot? LoadMoves()
        {
            using Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SysBot.Pokemon.BotLair.MoveInfo.json");
            using TextReader reader = new StreamReader(stream);
            var root = TradeExtensions.GetRoot<MoveInfoRoot>("", reader);
            reader.Close();
            return root;
        }
    }
}
