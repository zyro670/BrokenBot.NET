using PKHeX.Core;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SysBot.Pokemon
{
    public abstract class LairBotUtil
    {
        public static CancellationTokenSource EmbedSource = new();
        public static bool DiscordQueueOverride;
        public static bool EmbedsInitialized;
        public static (PK8?, bool) EmbedMon;
        public int TerrainDur = -1;

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

        public double[] TypeDamageMultiplier(int[] types, int moveType)
        {
            double[] effectiveness = { -1, -1 };
            for (int i = 0; i < types.Length; i++)
            {
                effectiveness[i] = moveType switch
                {
                    0 => new double[] { 1.0, 1.0, 1.0, 1.0, 1.0, 0.5, 1.0, 0.0, 0.5, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 }[types[i]],
                    1 => new double[] { 2.0, 1.0, 0.5, 0.5, 1.0, 2.0, 0.5, 0.0, 2.0, 1.0, 1.0, 1.0, 1.0, 0.5, 2.0, 1.0, 2.0, 0.5 }[types[i]],
                    2 => new double[] { 1.0, 2.0, 1.0, 1.0, 1.0, 0.5, 2.0, 1.0, 0.5, 1.0, 1.0, 2.0, 0.5, 1.0, 1.0, 1.0, 1.0, 1.0 }[types[i]],
                    3 => new double[] { 1.0, 1.0, 1.0, 0.5, 0.5, 0.5, 1.0, 0.5, 0.0, 1.0, 1.0, 2.0, 1.0, 1.0, 1.0, 1.0, 1.0, 2.0 }[types[i]],
                    4 => new double[] { 1.0, 1.0, 0.0, 2.0, 1.0, 2.0, 0.5, 1.0, 2.0, 2.0, 1.0, 0.5, 2.0, 1.0, 1.0, 1.0, 1.0, 1.0 }[types[i]],
                    5 => new double[] { 1.0, 0.5, 2.0, 1.0, 0.5, 1.0, 2.0, 1.0, 0.5, 2.0, 1.0, 1.0, 1.0, 1.0, 2.0, 1.0, 1.0, 1.0 }[types[i]],
                    6 => new double[] { 1.0, 0.5, 0.5, 0.5, 1.0, 1.0, 1.0, 0.5, 0.5, 0.5, 1.0, 2.0, 1.0, 2.0, 1.0, 1.0, 2.0, 0.5 }[types[i]],
                    7 => new double[] { 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 1.0, 1.0, 0.5, 1.0 }[types[i]],
                    8 => new double[] { 1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 1.0, 1.0, 0.5, 0.5, 0.5, 1.0, 0.5, 1.0, 2.0, 1.0, 1.0, 2.0 }[types[i]],
                    9 => new double[] { 1.0, 1.0, 1.0, 1.0, 1.0, 0.5, 2.0, 1.0, 2.0, 0.5, 0.5, 2.0, 1.0, 1.0, 2.0, 0.5, 1.0, 1.0 }[types[i]],
                    10 => new double[] { 1.0, 1.0, 1.0, 1.0, 2.0, 2.0, 1.0, 1.0, 1.0, 2.0, 0.5, 0.5, 1.0, 1.0, 1.0, 0.5, 1.0, 1.0 }[types[i]],
                    11 => new double[] { 1.0, 1.0, 0.5, 0.5, 2.0, 2.0, 0.5, 1.0, 0.5, 0.5, 2.0, 0.5, 1.0, 1.0, 1.0, 0.5, 1.0, 1.0 }[types[i]],
                    12 => new double[] { 1.0, 1.0, 2.0, 1.0, 0.0, 1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 0.5, 0.5, 1.0, 1.0, 0.5, 1.0, 1.0 }[types[i]],
                    13 => new double[] { 1.0, 2.0, 1.0, 2.0, 1.0, 1.0, 1.0, 1.0, 0.5, 1.0, 1.0, 1.0, 1.0, 0.5, 1.0, 1.0, 0.0, 1.0 }[types[i]],
                    14 => new double[] { 1.0, 1.0, 2.0, 1.0, 2.0, 1.0, 1.0, 1.0, 0.5, 0.5, 0.5, 2.0, 1.0, 1.0, 0.5, 2.0, 1.0, 1.0 }[types[i]],
                    15 => new double[] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0.5, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 1.0, 0.0 }[types[i]],
                    16 => new double[] { 1.0, 0.5, 1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 1.0, 1.0, 0.5, 0.5 }[types[i]],
                    17 => new double[] { 1.0, 2.0, 1.0, 0.5, 1.0, 1.0, 1.0, 1.0, 0.5, 0.5, 1.0, 1.0, 1.0, 1.0, 1.0, 2.0, 2.0, 1.0 }[types[i]],
                    _ => 1.0,
                };
            }
            return effectiveness;
        }

        public class PokeMoveInfo
        {
            public class MoveInfoRoot
            {
                public HashSet<PokeMoveInfo> Moves { get; private set; } = new();
            }

            public int MoveID { get; set; }
            public string Name { get; set; } = string.Empty;
            public MoveType Type { get; set; }
            public MoveCategory Category { get; set; }
            public int Power { get; set; }
            public int Accuracy { get; set; }
            public int Priority { get; set; }
            public int EffectSequence { get; set; }
            public int Recoil { get; set; }
            public int PowerGmax { get; set; }
            public bool Contact { get; set; }
            public bool Charge { get; set; }
            public bool Recharge { get; set; }
            public bool Sound { get; set; }
            public bool Gravity { get; set; }
            public bool Defrost { get; set; }
            public MoveTarget Target { get; set; }
        }

        public int CalculateEffectiveStat(int statIV, int statEV, int statBase, int level) => ((statIV + (2 * statBase) + (statEV / 4)) * level / 100) + 5; // Taken from PKHeX

        public int PriorityIndex(PK8 pk)
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

        private bool AbilityImmunity(int ourAbility, int encounterAbility, int[] encounterTypes, MoveType ourMoveType, int ourMoveID, PK8[]? party = default)
        {
            if (ourAbility == (int)Ability.Turboblaze || ourAbility == (int)Ability.Teravolt || ourAbility == (int)Ability.MoldBreaker)
                return false;

            bool partyStop = false;
            if (party != default)
            {
                switch (ourMoveType)
                {
                    case MoveType.Water: partyStop = party.Any(x => x.Ability == (int)Ability.StormDrain); break;
                    case MoveType.Electric: partyStop = party.Any(x => x.Ability == (int)Ability.LightningRod); break;
                };
            }

            if (partyStop)
                return true;

            if (ourMoveType == MoveType.Ground && ourMoveID != (int)Move.ThousandArrows && (encounterTypes[0] == 2 || encounterTypes[1] == 2))
                return true;

            return encounterAbility switch
            {
                (int)Ability.DrySkin or (int)Ability.WaterAbsorb or (int)Ability.StormDrain when ourMoveType == MoveType.Water => true,
                (int)Ability.VoltAbsorb or (int)Ability.LightningRod or (int)Ability.MotorDrive when ourMoveType == MoveType.Electric => true,
                (int)Ability.Levitate when ourMoveType == MoveType.Ground => true,
                (int)Ability.FlashFire when ourMoveType == MoveType.Fire => true,
                (int)Ability.SapSipper when ourMoveType == MoveType.Grass => true,
                _ => false,
            };
        }

        public double[] WeightedDamage(PK8[] party, PK8 pk, PK8 lairPk, PokeMoveInfo.MoveInfoRoot root, bool dmax)
        {
            if (TerrainDur >= 0)
                --TerrainDur;

            int[] movePP = new int[] { pk.Move1_PP, pk.Move2_PP, pk.Move3_PP, pk.Move4_PP };
            double[] dmgCalc = new double[4];
            int[] types = { lairPk.PersonalInfo.Type1, lairPk.PersonalInfo.Type2 };
            var encAbility = lairPk.Ability;
            var ourAbility = pk.Ability;

            for (int i = 0; i < pk.Moves.Length; i++)
            {
                double typeMultiplier = -1.0;
                var move = root.Moves.FirstOrDefault(x => x.MoveID == pk.Moves[i]);
                var power = Convert.ToDouble(move.Power);
                bool immune = AbilityImmunity(pk.Ability, lairPk.Ability, types, move.Type, move.MoveID, party);

                var typeMulti = move.Category == MoveCategory.Status ? new double[] { 1.0, 1.0 } : TypeDamageMultiplier(types, (int)move.Type);
                if (typeMulti[0] == 0.0 || typeMulti[1] == 0.0)
                    typeMultiplier = 0.0;
                else if (typeMulti[0] == 0.5 && typeMulti[1] == 0.5 && types[0] != types[1])
                    typeMultiplier = 0.25;
                else if (typeMulti[0] == 0.5 || typeMulti[1] == 0.5)
                    typeMultiplier = 0.5;
                else if (typeMulti[0] == 1.0 && typeMulti[1] == 1.0)
                    typeMultiplier = 1.0;
                else if (typeMulti[0] == 2.0 && typeMulti[1] == 2.0 && types[0] != types[1])
                    typeMultiplier = 4.0;
                else if (typeMulti[0] == 2.0 || typeMulti[1] == 2.0)
                    typeMultiplier = 2.0;

                if (immune || (move.MoveID == (int)Move.WillOWisp && types.Contains(9)) || (move.MoveID == (int)Move.DreamEater && lairPk.Status_Condition != (int)StatusCondition.Asleep))
                    typeMultiplier = -1.0;

                double target = move.Target switch
                {
                    MoveTarget.All or MoveTarget.AllAdjacentOpponents => 0.75,
                    _ => 1.0,
                };

                double stab = ourAbility == (int)Ability.Adaptability && (pk.PersonalInfo.Type1 == (int)move.Type || pk.PersonalInfo.Type2 == (int)move.Type) ? 2.0 : pk.PersonalInfo.Type1 == (int)move.Type || pk.PersonalInfo.Type2 == (int)move.Type ? 1.5 : 1.0;
                double multiplier = movePP[i] == 0 || (move.MoveID == (int)Move.SteelRoller && TerrainDur == -1) ? -100.0 : 1.0;
                multiplier *= encAbility switch // Target ability influence
                {
                    (int)Ability.Fluffy => move.Type == MoveType.Fire && !move.Contact ? 2.0 : 0.5,
                    (int)Ability.DrySkin => move.Type == MoveType.Fire ? 1.25 : move.Type == MoveType.Water ? -1.25 : 1.0,
                    (int)Ability.ThickFat => move.Type == MoveType.Fire || move.Type == MoveType.Ice ? 0.5 : 1.0,
                    (int)Ability.Heatproof => move.Type == MoveType.Fire ? 0.5 : 1.0,
                    (int)Ability.PrismArmor => typeMultiplier >= 2.0 ? 0.75 : 1.0,
                    (int)Ability.PunkRock => move.Sound ? 0.5 : 1.0,
                    _ => 1.0,
                };

                multiplier *= ourAbility switch // Our ability influence
                {
                    (int)Ability.TintedLens => typeMultiplier < 1.0 ? 2.0 : 1.0,
                    (int)Ability.IronFist => move.Name.Contains("Punch") || move.Name.Contains("Hammer") || move.MoveID == (int)Move.MeteorMash || move.MoveID == (int)Move.SkyUppercut ? 1.2 : 1.0,
                    (int)Ability.StrongJaw => move.Name.Contains("Fang") || move.MoveID == (int)Move.Bite || move.MoveID == (int)Move.Crunch || move.MoveID == (int)Move.JawLock ? 1.5 : 1.0,
                    (int)Ability.Adaptability => (int)move.Type == pk.PersonalInfo.Type1 || (int)move.Type == pk.PersonalInfo.Type2 ? 1.75 : 1.0,
                    (int)Ability.PunkRock => move.Sound ? 1.3 : 1.0,
                    (int)Ability.Normalize or (int)Ability.Refrigerate or (int)Ability.Aerilate or (int)Ability.Galvanize or (int)Ability.Pixilate => move.Type == MoveType.Normal ? 1.2 : 1.0,
                    _ => 1.0,
                };

                multiplier *= pk.HeldItem switch // Held item influence
                {
                    268 => typeMultiplier >= 2 ? 1.2 : 1.0, // Expert Belt
                    270 => 1.3, // Life Orb
                    _ => 1.0,
                };

                multiplier *= move.Type switch
                {
                    MoveType.Fairy => (ourAbility == (int)Ability.FairyAura || encAbility == (int)Ability.FairyAura) && (ourAbility == (int)Ability.AuraBreak || encAbility == (int)Ability.AuraBreak) ? 0.75 : ourAbility == (int)Ability.FairyAura || encAbility == (int)Ability.FairyAura ? 1.33 : 1.0,
                    MoveType.Dark => (ourAbility == (int)Ability.FairyAura || encAbility == (int)Ability.FairyAura) && (ourAbility == (int)Ability.AuraBreak || encAbility == (int)Ability.AuraBreak) ? 0.75 : ourAbility == (int)Ability.DarkAura || encAbility == (int)Ability.DarkAura ? 1.33 : 1.0,
                    _ => 1.0,
                };

                multiplier *= target * 0.925 * typeMultiplier * stab * (dmax ? 1.0 : move.Accuracy / 100.0);
                bool physical = move.Category == MoveCategory.Physical;
                bool bodyPress = move.MoveID == (int)Move.BodyPress;
                bool foulPlay = move.MoveID == (int)Move.FoulPlay;
                bool psy = move.MoveID == (int)Move.Psyshock || move.MoveID == (int)Move.Psystrike;
                double effectiveAttack = physical switch
                {
                    true => CalculateEffectiveStat(bodyPress ? pk.IV_DEF : foulPlay ? lairPk.IV_ATK : pk.IV_ATK, bodyPress ? pk.EV_DEF : foulPlay ? lairPk.EV_ATK : pk.EV_ATK, bodyPress ? pk.PersonalInfo.DEF : foulPlay ? lairPk.PersonalInfo.ATK : pk.PersonalInfo.ATK, pk.CurrentLevel),
                    false => CalculateEffectiveStat(pk.IV_SPA, pk.EV_SPA, pk.PersonalInfo.SPA, pk.CurrentLevel),
                };

                double effectiveDefense = physical switch
                {
                    true => CalculateEffectiveStat(lairPk.IV_DEF, lairPk.EV_DEF, lairPk.PersonalInfo.DEF, lairPk.CurrentLevel),
                    false => CalculateEffectiveStat(psy ? lairPk.IV_DEF : lairPk.IV_SPD, psy ? lairPk.EV_DEF : lairPk.EV_SPD, psy ? lairPk.PersonalInfo.DEF : lairPk.PersonalInfo.SPD, lairPk.CurrentLevel),
                };

                power *= move.MoveID switch
                {
                    (int)Move.Acrobatics => pk.HeldItem == 0 && !dmax ? 2.0 : 1.0,
                    (int)Move.Hex => lairPk.Status_Condition != (int)StatusCondition.NoCondition && !dmax ? 2.0 : 1.0,
                    (int)Move.Venoshock => lairPk.Status_Condition == (int)StatusCondition.Poisoned && !dmax ? 2.0 : 1.0,
                    (int)Move.DreamEater => lairPk.Status_Condition == (int)StatusCondition.Asleep && !dmax ? 1.5 : 1.0,
                    _ => dmax ? 2.0 : 1.0,
                };

                double status = pk.Status_Condition switch // Add extra weight based on niche circumstances
                {
                    (int)StatusCondition.Burned => move.Category == MoveCategory.Physical && ourAbility != (int)Ability.Guts ? 0.5 : 1.0,
                    (int)StatusCondition.Frozen => move.Defrost ? 10.0 : 1.0,
                    (int)StatusCondition.Asleep => move.MoveID == (int)Move.Snore || move.MoveID == (int)Move.SleepTalk ? 10.0 : 1.0,
                    _ => 1.0,
                };

                double usefulStatus = 
                    (!dmax && ((move.MoveID == (int)Move.Toxic && lairPk.Status_Condition != (int)StatusCondition.Poisoned) || move.MoveID == (int)Move.Counter || move.MoveID == (int)Move.LifeDew ||
                    move.MoveID == (int)Move.WideGuard || (move.MoveID == (int)Move.Yawn && lairPk.Status_Condition != (int)StatusCondition.Asleep)))
                    || (move.MoveID == (int)Move.Protect && dmax) ? 1.2 : 1.0;

                power *= status * (!dmax && (move.Charge || move.Recharge) ? 0.5 : 1.0);
                double terrain = 1.0;
                if (dmax || TerrainDur > 0)
                {
                    terrain = move.Type switch
                    {
                        MoveType.Fire or MoveType.Water => 1.5,
                        MoveType.Grass or MoveType.Electric or MoveType.Psychic => 1.3,
                        _ => 1.0,
                    };

                    if (terrain > 1.0 && TerrainDur < 0)
                        TerrainDur = 5;
                }

                power *= terrain;
                dmgCalc[i] = ((((2 * pk.CurrentLevel / 5) + 2) * power * (effectiveAttack / effectiveDefense) / 50) + 2) * multiplier * usefulStatus;
            }
            return dmgCalc;
        }

        public PokeMoveInfo.MoveInfoRoot LoadMoves()
        {
            using Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SysBot.Pokemon.SWSH.BotLair.MoveInfo.json");
            using TextReader reader = new StreamReader(stream);
            JsonSerializer serializer = new();
            var root = (PokeMoveInfo.MoveInfoRoot?)serializer.Deserialize(reader, typeof(PokeMoveInfo.MoveInfoRoot));
            reader.Close();
            return root ?? new();
        }
    }
}
