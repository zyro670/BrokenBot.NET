using PKHeX.Core;
using System.Collections.Generic;
using System.Threading;

namespace SysBot.Pokemon
{
    public static class SeedSearchUtil
    {
        public static uint GetShinyXor(uint val) => (val >> 16) ^ (val & 0xFFFF);

        public static uint GetShinyType(uint pid, uint tidsid)
        {
            var p = GetShinyXor(pid);
            var t = GetShinyXor(tidsid);
            if (p == t)
                return 2; // square;
            if ((p ^ t) < 0x10)
                return 1; // star
            return 0;
        }

        public static void GetShinyFrames(ulong seed, out int[] frames, out uint[] type, out List<uint[,]> IVs, SeedCheckResults mode)
        {
            int shinyindex = 0;
            frames = new int[3];
            type = new uint[3];
            IVs = new List<uint[,]>();
            bool foundStar = false;
            bool foundSquare = false;

            var rng = new Xoroshiro128Plus(seed);
            for (int i = 0; ; i++)
            {
                uint _ = (uint)rng.NextInt(0xFFFFFFFF); // EC
                uint SIDTID = (uint)rng.NextInt(0xFFFFFFFF);
                uint PID = (uint)rng.NextInt(0xFFFFFFFF);
                var shinytype = GetShinyType(PID, SIDTID);

                // If we found a shiny, record it and return if we got everything we wanted.
                if (shinytype != 0)
                {
                    if (shinytype == 1)
                        foundStar = true;
                    else if (shinytype == 2)
                        foundSquare = true;

                    if (shinyindex == 0 || mode == SeedCheckResults.FirstThree || (foundStar && foundSquare))
                    {
                        frames[shinyindex] = i;
                        type[shinyindex] = shinytype;
                        GetShinyIVs(rng, out uint[,] frameIVs);
                        IVs.Add(frameIVs);

                        shinyindex++;
                    }

                    if (mode == SeedCheckResults.ClosestOnly || (mode == SeedCheckResults.FirstStarAndSquare && foundStar && foundSquare) || shinyindex >= 3)
                        return;
                }

                // Get the next seed, and reset for the next iteration
                rng = new Xoroshiro128Plus(seed);
                seed = rng.Next();
                rng = new Xoroshiro128Plus(seed);
            }
        }

        public static void GetShinyIVs(Xoroshiro128Plus rng, out uint[,] frameIVs)
        {
            frameIVs = new uint[5, 6];
            Xoroshiro128Plus origrng = rng;

            for (int ivcount = 0; ivcount < 5; ivcount++)
            {
                int i = 0;
                int[] ivs = { -1, -1, -1, -1, -1, -1 };

                while (i < ivcount + 1)
                {
                    var stat = (int)rng.NextInt(6);
                    if (ivs[stat] == -1)
                    {
                        ivs[stat] = 31;
                        i++;
                    }
                }

                for (int j = 0; j < 6; j++)
                {
                    if (ivs[j] == -1)
                        ivs[j] = (int)rng.NextInt(32);
                    frameIVs[ivcount, j] = (uint)ivs[j];
                }
                rng = origrng;
            }
        }

        public static bool IsMatch(ulong seed, int[] ivs, int fixed_ivs)
        {
            var rng = new Xoroshiro128Plus(seed);
            rng.NextInt(); // EC
            rng.NextInt(); // TID
            rng.NextInt(); // PID
            int[] check_ivs = { -1, -1, -1, -1, -1, -1 };
            for (int i = 0; i < fixed_ivs; i++)
            {
                uint slot;
                do
                {
                    slot = (uint)rng.NextInt(6);
                } while (check_ivs[slot] != -1);

                if (ivs[slot] != 31)
                    return false;

                check_ivs[slot] = 31;
            }
            for (int i = 0; i < 6; i++)
            {
                if (check_ivs[i] != -1)
                    continue; // already verified?

                uint iv = (uint)rng.NextInt(32);
                if (iv != ivs[i])
                    return false;
            }
            return true;
        }

        public static void SpecificSeedSearch(DenUtil.RaidData raidInfo, out long frames, out ulong seedRes, out ulong threeDay, out string ivSpread, CancellationToken token)
        {
            threeDay = seedRes = 0;
            frames = 0;
            ivSpread = string.Empty;
            ulong seed = raidInfo.Den.Seed;
            var rng = new Xoroshiro128Plus(seed);
            for (long i = 0; i < raidInfo.SearchRange; i++)
            {
                if (token.IsCancellationRequested)
                    return;

                if (i > 0)
                {
                    rng = new Xoroshiro128Plus(seed);
                    seed = rng.Next();
                    rng = new Xoroshiro128Plus(seed);
                }

                uint EC = (uint)rng.NextInt(0xFFFFFFFF);
                uint SIDTID = (uint)rng.NextInt(0xFFFFFFFF);
                uint PID = (uint)rng.NextInt(0xFFFFFFFF);
                uint shinytype = GetShinyType(PID, SIDTID);

                if (shinytype == (uint)raidInfo.Shiny)
                {
                    rng = GetIVs(rng, raidInfo.IVs, raidInfo.GuaranteedIVs, out uint[,] allIVs, out bool IVMatch);
                    if (!IVMatch)
                        continue;

                    GetCharacteristic(EC, allIVs, raidInfo.GuaranteedIVs - 1, out bool charMatch, raidInfo.Characteristic);
                    if (!charMatch)
                        continue;

                    rng = GetAbility(rng, raidInfo.Den.IsEvent ? (uint)raidInfo.RaidDistributionEncounter.Ability : (uint)raidInfo.RaidEncounter.Ability, out uint abilityT);
                    bool abilityMatch = raidInfo.Ability == AbilityType.Any ? abilityT != (uint)raidInfo.Ability : abilityT == (uint)raidInfo.Ability;
                    if (!abilityMatch)
                        continue;

                    rng = GetGender(rng, raidInfo.Ratio, raidInfo.Den.IsEvent ? (uint)raidInfo.RaidDistributionEncounter.Gender : (uint)raidInfo.RaidEncounter.Gender, out uint genderT);
                    bool genderMatch = raidInfo.Gender == GenderType.Any ? genderT != (uint)raidInfo.Gender : genderT == (uint)raidInfo.Gender;
                    if (!genderMatch)
                        continue;

                    GetNature(rng, raidInfo.Den.IsEvent ? raidInfo.RaidDistributionEncounter.Species : raidInfo.RaidEncounter.Species, raidInfo.Den.IsEvent ? raidInfo.RaidDistributionEncounter.AltForm : raidInfo.RaidEncounter.AltForm, out uint natureT);
                    bool natureMatch = raidInfo.Nature == Nature.Random ? natureT != (uint)raidInfo.Nature : natureT == (uint)raidInfo.Nature;
                    if (!natureMatch)
                        continue;

                    ivSpread = DenUtil.IVSpreadByStar(GetIVSpread(allIVs), raidInfo, raidInfo.IVs, seed);
                    frames = i;
                    seedRes = seed;
                    for (int d = 3; d > 0; d--)
                        seed -= 0x82A2B175229D6A5B;
                    threeDay = seed;
                    return;
                }

                if (i >= raidInfo.SearchRange)
                    return;
            }
        }

        public static string GetCurrentFrameInfo(DenUtil.RaidData raidInfo, uint flawlessIVs, ulong seed, out uint shinyType, bool raid = true)
        {
            var rng = new Xoroshiro128Plus(seed);
            _ = (uint)rng.NextInt(0xFFFFFFFF);
            uint SIDTID = (uint)rng.NextInt(0xFFFFFFFF);
            uint PID = (uint)rng.NextInt(0xFFFFFFFF);
            shinyType = GetShinyType(PID, SIDTID);

            var IVs = new uint[] { 255, 255, 255, 255, 255, 255 };
            GetIVs(rng, IVs, flawlessIVs, out uint[,] allIVs, out _);
            uint[] ivs = new uint[6];
            for (int i = 0; i < 6; i++)
                ivs[i] = allIVs[flawlessIVs - 1, i];
            return DenUtil.IVSpreadByStar(GetIVSpread(allIVs), raidInfo, ivs, seed, raid);
        }

        public static uint GetCharacteristic(uint ec, uint[,] ivlist, uint guaranteedIVs, out bool charMatch, Characteristics characteristic = Characteristics.Any)
        {
            uint[] statOrder = new uint[6] { 0, 1, 2, 5, 3, 4 };
            uint charStat = ec % 6;
            for (uint i = 0; i < 6; i++)
            {
                uint stat = (charStat + i) % 6;
                if (ivlist[guaranteedIVs, statOrder[stat]] == 31)
                {
                    charMatch = CharMatch(stat, characteristic);
                    return stat;
                }
            }
            charMatch = CharMatch(charStat, characteristic);
            return charStat;
        }

        public static Xoroshiro128Plus GetGender(Xoroshiro128Plus rng, GenderRatio ratio, uint genderIn, out uint gender)
        {
            gender = genderIn switch
            {
                0 => ratio == GenderRatio.Genderless ? 2 : ratio == GenderRatio.Female ? 1 : ratio == GenderRatio.Male ? 0 : ((rng.NextInt(253) + 1) < (uint)ratio ? (uint)GenderType.Female : (uint)GenderType.Male),
                1 => 0,
                2 => 1,
                3 => 2,
                _ => (rng.NextInt(253) + 1) < (uint)ratio ? (uint)GenderType.Female : (uint)GenderType.Male,
            };
            return rng;
        }

        public static Xoroshiro128Plus GetAbility(Xoroshiro128Plus rng, uint nestAbility, out uint ability)
        {
            ability = nestAbility switch
            {
                4 => (uint)rng.NextInt(3),
                3 => (uint)rng.NextInt(2),
                _ => nestAbility,
            };
            return rng;
        }

        public static void GetNature(Xoroshiro128Plus rng, uint species, uint altform, out uint nature)
        {
            nature = species switch
            {
                849 => altform == 0 ? (uint)TradeExtensions<PK8>.Amped[rng.NextInt(13)] : (uint)TradeExtensions<PK8>.LowKey[rng.NextInt(12)],
                _ => (uint)rng.NextInt(25),
            };
        }

        public static Xoroshiro128Plus GetIVs(Xoroshiro128Plus rng, uint[] ivs, uint guaranteedIVs, out uint[,] allIVs, out bool match)
        {
            Xoroshiro128Plus origrng = rng;
            GetShinyIVs(rng, out allIVs);
            rng = origrng;
            uint[] ivRow = new uint[6] { 255, 255, 255, 255, 255, 255 };
            List<bool> ivCheck = new();

            for (uint fixedIV = 0; fixedIV < guaranteedIVs;)
            {
                uint index = (uint)rng.NextInt(6);
                if (ivRow[index] == 255)
                {
                    ivRow[index] = 31;
                    fixedIV++;
                }
            }

            for (int rand = 0; rand < 6; rand++)
            {
                if (ivRow[rand] == 255)
                    ivRow[rand] = (uint)rng.NextInt(32);
            }

            for (int i = 0; i < 6; i++)
                ivCheck.Add(ivs[i] == 255 || ivs[i] == allIVs[guaranteedIVs - 1, i]);

            match = !ivCheck.Contains(false);
            return rng;
        }

        public static string GetIVSpread(uint[,] ivResult)
        {
            string ivlist = "";
            for (int ivcount = 0; ivcount < 5; ivcount++)
            {
                for (int j = 0; j < 6; j++)
                {
                    ivlist += ivResult[ivcount, j];
                    if (j < 5)
                        ivlist += "/";
                    else if (j == 5)
                        ivlist += "\n";
                }
            }
            return ivlist;
        }

        private static bool CharMatch(uint stat, Characteristics characteristic) => characteristic == Characteristics.Any || (uint)characteristic == stat;
    }
}