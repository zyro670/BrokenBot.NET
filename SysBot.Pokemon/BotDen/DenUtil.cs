using PKHeX.Core;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using FlatbuffersResource;
using Google.FlatBuffers;

namespace SysBot.Pokemon
{
    // Thanks to Zaksabeast for the FlatBuffers tutorial and den hashes, Lusamine for the wonderful IV spreads by flawless IVs, and Kurt for pkNX's easily serializable text dumps.
    public class DenUtil
    {
        private readonly string SwordTable = "SysBot.Pokemon.BotDen.FlatbuffersResource.swordEnc.bin";
        private readonly string ShieldTable = "SysBot.Pokemon.BotDen.FlatbuffersResource.shieldEnc.bin";
        private static readonly string SwordDistributionTable = "SysBot.Pokemon.BotDen.FlatbuffersResource.NestDistributionEncSW.json";
        private static readonly string ShieldDistributionTable = "SysBot.Pokemon.BotDen.FlatbuffersResource.NestDistributionEncSH.json";

        public class RaidData
        {
            private SAV8SWSH? trainerInfo;
            private DenSettings? settings;
            private RaidSpawnDetail? den;
            private EncounterNest8 raidEnc;
            private NestHoleDistributionEncounter? distEnc;
            private EncounterNest8Table raidEncounterTable;
            private NestHoleDistributionEncounterTable? raidDistributionEncounterTable;

            public SAV8SWSH TrainerInfo { get => trainerInfo ?? new(); set => trainerInfo = value; }
            public RaidSpawnDetail Den { get => den ?? new(new byte[] { }, 0); set => den = value; }
            public EncounterNest8 RaidEncounter { get => raidEnc; set => raidEnc = value; }
            public EncounterNest8Table RaidEncounterTable { get => raidEncounterTable; set => raidEncounterTable = value; }
            public NestHoleDistributionEncounter RaidDistributionEncounter { get => distEnc ?? new(); set => distEnc = value; }
            public NestHoleDistributionEncounterTable? RaidDistributionEncounterTable { get => raidDistributionEncounterTable ?? new(); set => raidDistributionEncounterTable = value; }
            public GenderType Gender { get => Settings.DenFilters.Gender; }
            public GenderRatio Ratio { get => Settings.DenFilters.GenderRatio; }
            public AbilityType Ability { get => Settings.DenFilters.Ability; }
            public Nature Nature { get => Settings.DenFilters.Nature; }
            public ShinyType Shiny { get => Settings.DenFilters.ShinyType; }
            public Characteristics Characteristic { get => Settings.DenFilters.Characteristic; }
            public uint[] IVs { get => Settings.IVParse(); }
            public uint GuaranteedIVs { get => Settings.DenFilters.GuaranteedIVs; }
            public long SearchRange { get => Settings.SearchRange; }
            public DenSettings Settings { get => settings ?? new(); set => settings = value; }
        };

        public class NestHoleDistributionEncounterTable
        {
            public HashSet<NestHoleDistributionEncounter> Entries { get; set; } = new();
        }

        public class NestHoleDistributionEncounter
        {
            public uint EntryIndex { get; set; }
            public uint Species { get; set; }
            public uint AltForm { get; set; }
            public uint Level { get; set; }
            public uint DynamaxLevel { get; set; }
            public uint Ability { get; set; }
            public bool IsGigantamax { get; set; }
            public uint[] Probabilities { get; set; } = new uint[] { };
            public uint Gender { get; set; }
            public uint FlawlessIVs { get; set; }
            public uint ShinyLock { get; set; }
            public uint Nature { get; set; }
            public uint MinRank { get; set; }
            public uint MaxRank { get; set; }
        }

        public static uint GetDenOffset(PokeTradeHub<PK8> hub)
        {
            uint denID = GetDenID(hub.Config.Den);
            uint shiftedOffset = PokeDataOffsets.DenOffset;
            if (denID >= 190)
                return shiftedOffset += 0x300 + (denID * 0x18);
            else if (denID >= 100)
                return shiftedOffset += 0x108 + (denID * 0x18);
            else return shiftedOffset + (denID * 0x18);
        }

        public static uint GetDenID(DenSettings settings)
        {
            uint denID = settings.DenType switch
            {
                DenType.Vanilla => settings.DenID <= 100 && settings.DenID > 0 ? _ = settings.DenID - 1 : 99,
                DenType.IoA => settings.DenID <= 90 && settings.DenID > 0 ? _ = settings.DenID - 1 + 100 : 189,
                DenType.CT => settings.DenID <= 86 && settings.DenID > 0 ? _ = settings.DenID - 1 + 190 : 276,
                _ => 1,
            };
            return denID;
        }

        public static RaidData GetRaid(RaidData raidInfo, byte[] denData)
        {
            raidInfo.Den = new RaidSpawnDetail(denData, 0)
            {
                RandRoll = (byte)raidInfo.Settings.Randroll,
                Stars = (byte)raidInfo.Settings.Star
            };

            switch (raidInfo.Settings.DenBeamType)
            {
                case BeamType.Event: raidInfo.Den.IsRare = false; raidInfo.Den.IsWishingPiece = true; raidInfo.Den.Flags = 3; raidInfo.Den.IsEvent = true; raidInfo.Den.DenType = RaidType.CommonWish; break;
                case BeamType.CommonWish: raidInfo.Den.IsRare = false; raidInfo.Den.IsWishingPiece = true; raidInfo.Den.Flags = 2; raidInfo.Den.IsEvent = false; raidInfo.Den.DenType = RaidType.CommonWish; break;
                case BeamType.RareWish: raidInfo.Den.IsRare = true; raidInfo.Den.IsWishingPiece = true; raidInfo.Den.Flags = 2; raidInfo.Den.IsEvent = false; raidInfo.Den.DenType = RaidType.RareWish; break;
            };

            if (raidInfo.Den.IsEvent)
            {
                raidInfo.RaidDistributionEncounter = GetSpawnEvent(raidInfo, out NestHoleDistributionEncounterTable? table);
                raidInfo.RaidDistributionEncounterTable = table;
            }
            else
            {
                raidInfo.RaidEncounter = GetSpawn(raidInfo, out EncounterNest8Table tableEnc);
                raidInfo.RaidEncounterTable = tableEnc;
            }

            return raidInfo;
        }

        public static string IVSpreadByStar(string ivSpread, RaidData raidInfo, ulong seed)
        {
            var splitIV = ivSpread.Split('\n');
            List<string> speciesList = new List<string>();
            if (raidInfo.RaidDistributionEncounterTable == null)
                return string.Empty;

            var distEntries = raidInfo.RaidDistributionEncounterTable.Entries.ToList();
#pragma warning disable CS8629
            for (int i = 0; i < (raidInfo.Den.IsEvent ? distEntries.Count : raidInfo.RaidEncounterTable.EntriesLength); i++)
            {
                List<uint> probList = new();
                for (int a = 0; a < 5; a++)
                {
                    var prob = raidInfo.Den.IsEvent ? distEntries[i].Probabilities[a] : raidInfo.RaidEncounterTable.Entries(i).Value.Probabilities(a);
                    probList.Add(prob);
                }

                var firstIndex = probList.FindIndex(0, 5, x => x != 0) + 1;
                var lastIndex = probList.FindLastIndex(4, 4, x => x != 0) + 1;
                var star = $"{((firstIndex == lastIndex) || (firstIndex > lastIndex) ? firstIndex + "★" : firstIndex + "-" + lastIndex + "★")}";
                bool baby = raidInfo.Settings.BabyDen && firstIndex <= 3;
                if (raidInfo.Settings.BabyDen)
                {
                    if (firstIndex > 3)
                        continue;
                }
                else if (firstIndex < 3)
                    continue;

                var rng = new Xoroshiro128Plus(seed);
                var gmax = raidInfo.Den.IsEvent ? distEntries[i].IsGigantamax : raidInfo.RaidEncounterTable.Entries(i).Value.IsGigantamax;
                var speciesID = (int)(raidInfo.Den.IsEvent ? distEntries[i].Species : raidInfo.RaidEncounterTable.Entries(i).Value.Species);
                var form = (int)(raidInfo.Den.IsEvent ? distEntries[i].AltForm : raidInfo.RaidEncounterTable.Entries(i).Value.AltForm);
                var speciesName = SpeciesName.GetSpeciesNameGeneration(speciesID, 2, 8);
                var pkm = AutoLegalityWrapper.GetTrainerInfo(8).GetLegal(AutoLegalityWrapper.GetTemplate(new ShowdownSet($"{speciesName}{TradeExtensions.FormOutput(speciesID, form, out _)}")), out _);
                var personal = pkm.PersonalInfo;
                var IVs = raidInfo.Den.IsEvent ? distEntries[i].FlawlessIVs : (uint)raidInfo.RaidEncounterTable.Entries(i).Value.FlawlessIVs;

                uint EC = (uint)rng.NextInt(0xFFFFFFFF);
                uint SIDTID = (uint)rng.NextInt(0xFFFFFFFF);
                uint PID = (uint)rng.NextInt(0xFFFFFFFF);
                uint shinytype = SeedSearchUtil.GetShinyType(PID, SIDTID);

                rng = SeedSearchUtil.GetIVs(rng, raidInfo.IVs, IVs, out uint[,] allIVs, out _);
                var characteristic = SeedSearchUtil.GetCharacteristic(EC, allIVs, IVs - 1, out _);

                var ability = raidInfo.Den.IsEvent ? distEntries[i].Ability : (uint)raidInfo.RaidEncounterTable.Entries(i).Value.Ability;
                rng = SeedSearchUtil.GetAbility(rng, ability, out uint abilityT);

                var ratio = personal.OnlyFemale ? 254 : personal.OnlyMale ? 0 : personal.Genderless ? 255 : personal.Gender;
                var gender = raidInfo.Den.IsEvent ? distEntries[i].Gender : (uint)raidInfo.RaidEncounterTable.Entries(i).Value.Gender;              
                rng = SeedSearchUtil.GetGender(rng, (GenderRatio)ratio, gender, out uint genderT);

                SeedSearchUtil.GetNature(rng, (uint)speciesID, (uint)form, out uint natureT);

                pkm.SetAbilityIndex((int)abilityT);
                speciesList.Add(star + " - " + speciesName + (gmax ? "-Gmax" : "") + " - " + splitIV[IVs - 1] + "\n" +
                (GenderType)genderT + " - " + (Nature)natureT + " - " + (abilityT != 2 ? abilityT + 1 : "H") + ": " + (Ability)pkm.Ability + " - " + (ShinyType)shinytype + " - " + TradeExtensions.Characteristics[characteristic]);
            }
#pragma warning restore CS8629
            speciesList.Sort();
            return string.Join("\n", speciesList);
        }

        public static EncounterNest8 GetSpawn(RaidData raidInfo, out EncounterNest8Table tables)
        {
            tables = new();
            var randroll = (int)raidInfo.Den.RandRoll;
            var data = new ByteBuffer(new DenUtil().ReadResourceBinary(raidInfo.TrainerInfo));
            var nestTable = EncounterNest8Archive.GetRootAsEncounterNest8Archive(data);
            for (int i = 0; i < nestTable.TablesLength; i++)
            {
                var table = nestTable.Tables(i);
                if (!table.HasValue)
                    return new EncounterNest8();

                var denhash = denHashes[GetDenID(raidInfo.Settings), raidInfo.Den.IsRare ? 1 : 0];
                if (table.Value.TableID == denhash && table.Value.GameVersion == (raidInfo.TrainerInfo.Version == GameVersion.SW ? 1 : 2))
                {
                    tables = table.Value;
                    var entryLength = table.Value.EntriesLength;
                    for (int p = 0; p < entryLength; p++)
                    {
                        var entry = table.Value.Entries(p);
                        if (!entry.HasValue)
                            return new EncounterNest8();

                        var prob = (int)entry.Value.Probabilities(raidInfo.Den.Stars);
                        randroll -= prob;
                        if (randroll < 0)
                            return (EncounterNest8)entry;
                    }
                }
            }
            return new EncounterNest8();
        }

        public static NestHoleDistributionEncounter GetSpawnEvent(RaidData raidInfo, out NestHoleDistributionEncounterTable? nestEventTable)
        {
            var randroll = (int)raidInfo.Den.RandRoll;
            nestEventTable = ReadResourceJson(raidInfo.TrainerInfo);
            if (nestEventTable == null)
                return new NestHoleDistributionEncounter();

            var entries = nestEventTable.Entries.ToList();
            for (int p = 0; p < entries.Count; p++)
            {
                var prob = (int)entries[p].Probabilities[raidInfo.Den.Stars];
                randroll -= prob;
                if (randroll < 0)
                    return entries[p];
            }
            return new NestHoleDistributionEncounter();
        }

        private byte[]? ReadResourceBinary(SAV8SWSH trainerInfo)
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(trainerInfo.Version == GameVersion.SW ? SwordTable : ShieldTable);
            if (stream == null)
                return null;

            byte[] array = new byte[stream.Length];
            stream.Read(array, 0, array.Length);
            return array;
        }

        private static NestHoleDistributionEncounterTable? ReadResourceJson(SAV8SWSH trainerInfo)
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(trainerInfo.Version == GameVersion.SW ? SwordDistributionTable : ShieldDistributionTable);
            using TextReader reader = new StreamReader(stream);
            var table = TradeExtensions.GetRoot<NestHoleDistributionEncounterTable>("", reader);
            reader.Close();
            return table;
        }

        public static ulong GetTargetSeed(ulong seed, int skips)
        {
            var rng = new Xoroshiro128Plus(seed);
            for (int i = 0; i < skips; i++)
            {
                seed = rng.Next();
                rng = new Xoroshiro128Plus(seed);
            }
            return seed;
        }

        public static int GetSkipsToTargetSeed(ulong curSeed, ulong targetSeed, int initialSkips)
        {
            var rng = new Xoroshiro128Plus(curSeed);
            int skips = 0;
            while (skips <= initialSkips)
            {
                if (curSeed == targetSeed)
                    return skips;

                curSeed = rng.Next();
                rng = new Xoroshiro128Plus(curSeed);
                skips++;
            }
            return -1;
        }

        public static void GenerateJson() // Literally only used for lazy distribution nest updates
        {
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "nestDist*");
            foreach (var file in files)
            {
                var jsonName = file.Contains("nestDist_sw") ? "NestDistributionEncSW.json" : "NestDistributionEncSH.json";
                File.Create(jsonName).Close();
                var text = File.ReadAllText(file).Split('\n');
                var tables = new NestHoleDistributionEncounterTable();
                for (int i = 0; i < text.Length; i++)
                {
                    if (i == 0)
                        continue;

                    var entry = text[i].Split('	');
                    var probSplit = entry[15].Split('|');
                    var prob = new uint[5];
                    for (int p = 0; p < probSplit.Length; p++)
                        prob[p] = uint.Parse(probSplit[p]);

                    tables.Entries.Add(new NestHoleDistributionEncounter()
                    {
                        EntryIndex = uint.Parse(entry[0]),
                        Species = uint.Parse(entry[1]),
                        AltForm = uint.Parse(entry[2]),
                        Level = uint.Parse(entry[3]),
                        DynamaxLevel = uint.Parse(entry[4]),
                        Ability = uint.Parse(entry[11]),
                        IsGigantamax = bool.Parse(entry[12]),
                        Probabilities = prob,
                        Gender = uint.Parse(entry[16]),
                        FlawlessIVs = uint.Parse(entry[17]),
                        ShinyLock = uint.Parse(entry[18]),
                        Nature = uint.Parse(entry[21]),
                        MinRank = uint.Parse(entry[37]),
                        MaxRank = uint.Parse(entry[38])
                    });
                }
                TradeExtensions.SerializeInfo(tables, $"{Directory.GetCurrentDirectory()}//{jsonName}");
                File.Delete(file);
            }
        }

        public static ulong eventHash = 1721953670860364124u;
        public static ulong[,] denHashes = {
        { 1675062357515959378u, 13439833545771248589u },
        { 1676893044376552243u, 13440787921864346512u },
        { 1676899641446321509u, 4973137107049022145u },
        { 1676044221399762576u, 13438834089701394015u },
        { 1676051917981160053u, 13438837388236278648u },
        { 1676897442423065087u, 13440790120887602934u },
        { 1676908437539347197u, 13440789021375974723u },
        { 1676046420423018998u, 13438839587259535070u },
        { 1676899641446321509u, 4973137107049022145u },
        { 1677896898492919661u, 13439825849189851112u },
        { 1677881505330124707u, 4972153044141962525u },
        { 1677896898492919661u, 13439826948701479323u },
        { 1676051917981160053u, 4973134908025765723u },
        { 1677896898492919661u, 13439825849189851112u },
        { 1676045320911390787u, 13438832990189765804u },
        { 1676049718957903631u, 13438838487747906859u },
        {            eventHash, eventHash             },
        { 1676048619446275420u, 13438843985306047914u },
        { 1676908437539347197u, 13440789021375974723u },
        { 1676899641446321509u, 13439823650166594690u },
        { 1676899641446321509u, 13439823650166594690u },
        { 1676055216516044686u, 13441642242399277234u },
        { 1676055216516044686u, 13441642242399277234u },
        { 13438843985306047914u, 1679871621376808167u },
        { 1676048619446275420u, 13438843985306047914u },
        { 1676055216516044686u, 4973136007537393934u },
        { 1676895243399808665u, 13440791220399231145u },
        { 1676907338027718986u, 13440787921864346512u },
        { 1676056316027672897u, 4973136007537393934u },
        { 1679872720888436378u, 13441636744841136179u },
        { 1679872720888436378u, 13441636744841136179u },
        { 1676050818469531842u, 13438837388236278648u },
        { 1676046420423018998u, 13438842885794419703u },
        { 1675061258004331167u, 13438834089701394015u },
        { 1675057959469446534u, 13438845084817676125u },
        { 1675056859957818323u, 13438840686771163281u },
        { 1675061258004331167u, 4972148646095449681u },
        { 1675056859957818323u, 4972140949514052204u },
        { 1675055760446190112u, 13438839587259535070u },
        { 1679872720888436378u, 13441636744841136179u },
        { 1677880405818496496u, 13439824749678222901u },
        { 1679872720888436378u, 13441636744841136179u },
        { 1677880405818496496u, 13439824749678222901u },
        { 1677880405818496496u, 4973141505095534989u },
        { 1675055760446190112u, 13438839587259535070u },
        { 1675060158492702956u, 13438832990189765804u },
        { 1676898541934693298u, 13439824749678222901u },
        { 1677894699469663239u, 13439829147724735745u },
        { 1679873820400064589u, 13440789021375974723u },
        { 1676894143888180454u, 4972147546583821470u },
        { 1675059058981074745u, 4973140405583906778u },
        { 1676056316027672897u, 13438843985306047914u },
        { 1675062357515959378u, 13439833545771248589u },
        { 1679873820400064589u, 13440789021375974723u },
        { 1676051917981160053u, 4973134908025765723u },
        { 1676050818469531842u, 13438837388236278648u },
        { 1676891944864924032u, 13440791220399231145u },
        { 1677895798981291450u, 13439825849189851112u },
        { 1679873820400064589u, 13440794518934115778u },
        { 1676046420423018998u, 4972146447072193259u },
        { 1676044221399762576u, 13438834089701394015u },
        { 1675065656050844011u, 4972145347560565048u },
        { 1676049718957903631u, 13438842885794419703u },
        { 1677895798981291450u, 13439825849189851112u },
        { 1676045320911390787u, 13438832990189765804u },
        { 1675057959469446534u, 4972142049025680415u },
        { 1677892500446406817u, 13439830247236363956u },
        { 1675060158492702956u, 13438832990189765804u },
        { 1675064556539215800u, 13439831346747992167u },
        { 1676895243399808665u, 13440791220399231145u },
        { 1675063457027587589u, 4973133808514137512u },
        { 1675063457027587589u, 13439833545771248589u },
        { 1675061258004331167u, 4973133808514137512u },
        { 1676055216516044686u, 13441642242399277234u },
        { 1675056859957818323u, 13438840686771163281u },
        { 1675055760446190112u, 13438839587259535070u },
        { 1677889201911522184u, 13439830247236363956u },
        { 1677890301423150395u, 13439831346747992167u },
        { 1677881505330124707u, 13438842885794419703u },
        { 1676891944864924032u, 4973139306072278567u },
        { 1679871621376808167u, 13440795618445743989u },
        { 1676891944864924032u, 13440793419422487567u },
        { 1677895798981291450u, 13440798916980628622u },
        { 1677893599958035028u, 13441641142887649023u },
        { 1675057959469446534u, 13438845084817676125u },
        { 1676896342911436876u, 13439823650166594690u },
        { 1676898541934693298u, 13439828048213107534u },
        { 1675065656050844011u, 13439832446259620378u },
        { 1677891400934778606u, 13441640043376020812u },
        { 1676897442423065087u, 13440790120887602934u },
        { 1675060158492702956u, 13440792319910859356u },
        { 1676898541934693298u, 13439824749678222901u },
        { 1677891400934778606u, 13439830247236363956u },
        { 1675064556539215800u, 13440800016492256833u },
        { 1676896342911436876u, 4973138206560650356u },
        { 1677894699469663239u, 4972151944630334314u },
        { 1677893599958035028u, 13439829147724735745u },
        { 1675064556539215800u, 4972150845118706103u },
        { 1676056316027672897u, 13438843985306047914u },
        { 1676894143888180454u, 13441643341910905445u },
        { 8769170721942624824u, 14477537978666912344u },
        { 16341001078884806474u, 9913932150092391706u },
        { 7854659797556875545u, 5999950843982638879u },
        { 4780541378243794326u, 18345017229883237822u },
        { 2997411918588892139u, 12562706121429926817u },
        { 6589539950519384197u, 3561902408726248099u },
        { 2447364886159768926u, 15632276665898509590u },
        { 7956530560371257544u, 2024757571205803752u },
        { 13563999851587423716u, 502513031628180988u },
        { 4780539179220537904u, 18345015030859981400u },
        { 4780540278732166115u, 18345016130371609611u },
        { 2997411918588892139u, 12562706121429926817u },
        { 16341001078884806474u, 9913932150092391706u },
        { 14284833672245134656u, 7704513452465554544u },
        { 6672704941776910536u, 17951961757311600360u },
        { 13305292637317525948u, 16069264858016261892u },
        { 2447363786648140715u, 15632275566386881379u },
        { 2447364886159768926u, 15632276665898509590u },
        { 4780541378243794326u, 18345017229883237822u },
        { 7854659797556875545u, 5999950843982638879u },
        { 15818376695778914966u, 5701088864462885848u },
        { 7956530560371257544u, 2024757571205803752u },
        { 16341001078884806474u, 9913932150092391706u },
        { 6672704941776910536u, 17951961757311600360u },
        { 4780540278732166115u, 18345016130371609611u },
        { 6589539950519384197u, 3561902408726248099u },
        { 4780540278732166115u, 18345016130371609611u },
        { 7956530560371257544u, 2024757571205803752u },
        { 13563999851587423716u, 502513031628180988u },
        { 6984833918694526192u, 14413583907274219616u },
        { 4780539179220537904u, 18345015030859981400u },
        { 13305292637317525948u, 16069264858016261892u },
        { 342604449375897784u, 8253110425161551320u },
        { 5830741396702654597u, 17953607996949684899u },
        { 13563999851587423716u, 502513031628180988u },
        { 6162140483756004486u, 6162171270081594394u },
        { 11635283243122928556u, 17629394089387610164u },
        { 14284833672245134656u, 7704513452465554544u },
        { 6984833918694526192u, 14413583907274219616u },
        { 4780540278732166115u, 5701094362021026903u },
        { 342604449375897784u, 8253110425161551320u },
        { 5830741396702654597u, 17953607996949684899u },
        { 4780541378243794326u, 18345017229883237822u },
        { 2447363786648140715u, 15632275566386881379u },
        { 6589539950519384197u, 3561902408726248099u },
        { 12738905581603037598u, 5701095461532655114u },
        { 4780539179220537904u, 18345015030859981400u },
        { 11635283243122928556u, 17629394089387610164u },
        { 6672704941776910536u, 17951961757311600360u },
        { 15818376695778914966u, 5701088864462885848u },
        { 13305292637317525948u, 16069264858016261892u },
        { 8769170721942624824u, 14477537978666912344u },
        { 2997411918588892139u, 12562706121429926817u },
        { 7854659797556875545u, 5701093262509398692u },
        { 2447363786648140715u, 15632275566386881379u },
        { 6984833918694526192u, 5701096561044283325u },
        { 6589539950519384197u, 3561902408726248099u },
        { 8769170721942624824u, 14477537978666912344u },
        { 7725829814153603264u, 5701092162997770481u },
        { 4780546875801935381u, 18345022727441378877u },
        { 4665094036540599430u, 11519945754184084270u },
        { 14284833672245134656u, 7704513452465554544u },
        { 7854659797556875545u, 5999950843982638879u },
        { 11635283243122928556u, 17629394089387610164u },
        { 12738905581603037598u, 4426791916416848726u },
        { 6984833918694526192u, 14413583907274219616u },
        { 13305292637317525948u, 16069264858016261892u },
        { 7725829814153603264u, 5701092162997770481u },
        { 6672704941776910536u, 17951961757311600360u },
        { 5830741396702654597u, 17953607996949684899u },
        { 2447364886159768926u, 15632276665898509590u },
        { 342604449375897784u, 8253110425161551320u },
        { 4780546875801935381u, 18345022727441378877u },
        { 11635283243122928556u, 17629394089387610164u },
        { 16341001078884806474u, 9913932150092391706u },
        { 2447364886159768926u, 15632276665898509590u },
        { 2997411918588892139u, 12562706121429926817u },
        { 4780546875801935381u, 18345022727441378877u },
        { 4780539179220537904u, 5701091063486142270u },
        { 12738905581603037598u, 4426791916416848726u },
        { 13563999851587423716u, 502513031628180988u },
        { 14284833672245134656u, 7704513452465554544u },
        { 4780546875801935381u, 18345022727441378877u },
        { 7956530560371257544u, 2024757571205803752u },
        { 16882931869395424672u, 4515385547978135952u },
        { 16882931869395424672u, 4515385547978135952u },
        { 16882931869395424672u, 4515385547978135952u },
        { 16882931869395424672u, 4515385547978135952u },
        { 16882931869395424672u, 4515385547978135952u },
        { 16882931869395424672u, 4515385547978135952u },
        { 538718828553644332u, 10639252279486991937u },
        { 6189149299220963515u, 744948697234498138u },
        { 7520360650147352417u, 3231560995259522968u },
        { 2756478418053350351u, 4769195437400348422u },
        { 5162770839310267307u, 11690997354028679946u },
        { 7520360650147352417u, 3231560995259522968u },
        { 14439216054291849305u, 8284890978883698976u },
        { 4805937820974168436u, 11331443048367529433u },
        { 11147942343095866771u, 1812702195150859522u },
        { 8444690290455066916u, 221992188589330697u },
        { 16299909383459599211u, 4268295780237511370u },
        { 9125837977236588438u, 16150871691787878075u },
        { 4197853775535533550u, 7797506443826343779u },
        { 5955975221769392477u, 14450795946632079964u },
        { 17302261471610567686u, 10041392713565152107u },
        { 2756478418053350351u, 4769195437400348422u },
        { 1108881309583387371u, 2845993206239293002u },
        { 4408860220788168599u, 18001771904838230654u },
        { 8444690290455066916u, 221992188589330697u },
        { 538718828553644332u, 10639252279486991937u },
        { 6189149299220963515u, 744948697234498138u },
        { 5955975221769392477u, 14450795946632079964u },
        { 11147942343095866771u, 1812702195150859522u },
        { 14439216054291849305u, 8284890978883698976u },
        { 6189149299220963515u, 744948697234498138u },
        { 7520357351612467784u, 1345818289025324965u },
        { 1108881309583387371u, 2845993206239293002u },
        { 7520357351612467784u, 1345818289025324965u },
        { 1716759284250366303u, 12829170745926812758u },
        { 16299909383459599211u, 4268295780237511370u },
        { 4197853775535533550u, 7797506443826343779u },
        { 4805937820974168436u, 11331443048367529433u },
        { 5162770839310267307u, 11690997354028679946u },
        { 17302261471610567686u, 10041392713565152107u },
        { 6189149299220963515u, 744948697234498138u },
        { 8444690290455066916u, 221992188589330697u },
        { 9125837977236588438u, 16150871691787878075u },
        { 1716759284250366303u, 12829170745926812758u },
        { 11147942343095866771u, 1812702195150859522u },
        { 7520360650147352417u, 3231560995259522968u },
        { 14439216054291849305u, 8284890978883698976u },
        { 4197853775535533550u, 7797506443826343779u },
        { 6395957127820208723u, 13032247726971474370u },
        { 9125837977236588438u, 16150871691787878075u },
        { 4408860220788168599u, 18001771904838230654u },
        { 1716759284250366303u, 12829170745926812758u },
        { 538718828553644332u, 10639252279486991937u },
        { 2756478418053350351u, 4769195437400348422u },
        { 1108881309583387371u, 2845993206239293002u },
        { 11147942343095866771u, 1812702195150859522u },
        { 538718828553644332u, 10639252279486991937u },
        { 5162770839310267307u, 11690997354028679946u },
        { 7520357351612467784u, 1345818289025324965u },
        { 4408860220788168599u, 18001771904838230654u },
        { 1108881309583387371u, 2845993206239293002u },
        { 8444690290455066916u, 221992188589330697u },
        { 9125837977236588438u, 16150871691787878075u },
        { 7520357351612467784u, 1345818289025324965u },
        { 5955975221769392477u, 14450795946632079964u },
        { 5162770839310267307u, 11690997354028679946u },
        { 11147942343095866771u, 1812702195150859522u },
        { 14439216054291849305u, 8284890978883698976u },
        { 7520360650147352417u, 3231560995259522968u },
        { 16299909383459599211u, 4268295780237511370u },
        { 4805937820974168436u, 11331443048367529433u },
        { 7520357351612467784u, 1345818289025324965u },
        { 4408860220788168599u, 18001771904838230654u },
        { 16299909383459599211u, 4268295780237511370u },
        { 2756478418053350351u, 4769195437400348422u },
        { 4805937820974168436u, 11331443048367529433u },
        { 5955975221769392477u, 14450795946632079964u },
        { 6189149299220963515u, 744948697234498138u },
        { 4197853775535533550u, 7797506443826343779u },
        { 4197853775535533550u, 7797506443826343779u },
        { 538718828553644332u, 10639252279486991937u },
        { 5955975221769392477u, 14450795946632079964u },
        { 16299909383459599211u, 4268295780237511370u },
        { 8444690290455066916u, 221992188589330697u },
        { 16685003352010291762u, 13686551123076485279u },
        { 17302261471610567686u, 10041392713565152107u },
        { 14439216054291849305u, 8284890978883698976u },
        { 1108881309583387371u, 2845993206239293002u },
        { 1716759284250366303u, 12829170745926812758u },
        { 5162770839310267307u, 11690997354028679946u },
        { 4408860220788168599u, 18001771904838230654u },
        { 4805937820974168436u, 11331443048367529433u }
        };
    }
}
