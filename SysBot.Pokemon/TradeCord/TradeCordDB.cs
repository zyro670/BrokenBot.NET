using System;
using System.Linq;
using System.Collections.Generic;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using static PKHeX.Core.AutoMod.Aesthetics;
using System.Collections.Frozen;

namespace SysBot.Pokemon;

public abstract class TradeCordDatabase<T> : TradeCordBase<T> where T : PKM, new()
{
    protected T RngRoutineSWSH(T pkm, IBattleTemplate template, Shiny shiny)
    {
        if (pkm.Species is (ushort)Species.Alcremie)
        {
            var data = pkm.Data;
            var deco = (uint)Random.Next(7);
            pkm.ChangeFormArgument(deco);
        }

        var laInit = new LegalityAnalysis(pkm);
        var nature = pkm.Nature;
        pkm.Nature = pkm.Species switch
        {
            (ushort)Species.Toxtricity => pkm.Form > 0 ? TradeExtensions<PK8>.LowKey[Random.Next(TradeExtensions<PK8>.LowKey.Length)] : TradeExtensions<PK8>.Amped[Random.Next(TradeExtensions<PK8>.Amped.Length)],
            _ => Random.Next(25),
        };

        var la = new LegalityAnalysis(pkm);
        if (laInit.Valid && !la.Valid)
            pkm.Nature = nature;

        pkm.StatNature = pkm.Nature;
        pkm.Move1_PPUps = pkm.Move2_PPUps = pkm.Move3_PPUps = pkm.Move4_PPUps = 0;
        pkm.SetMaximumPPCurrent(pkm.Moves);
        pkm.ClearHyperTraining();

        var enc = la.Info.EncounterMatch;
        var evoChain = la.Info.EvoChainsAllGens.Gen8.FirstOrDefault(x => x.Species == pkm.Species);
        pkm.CurrentLevel = enc.LevelMin < evoChain.LevelMin ? evoChain.LevelMin : enc.LevelMin;
        while (!new LegalityAnalysis(pkm).Valid)
        {
            pkm.CurrentLevel += 1;
            if (pkm.CurrentLevel >= 100)
                return pkm;
        }

        pkm.SetSuggestedMoves();
        Span<ushort> relearn = stackalloc ushort[4];
        la.GetSuggestedRelearnMoves(relearn, enc);
        pkm.SetRelearnMoves(relearn);
        pkm.HealPP();

        if (!GalarFossils.Contains(pkm.Species) && !pkm.FatefulEncounter)
        {
            if (enc is EncounterSlot8 slot8)
                pkm.SetAbilityIndex(slot8.Ability is AbilityPermission.Any12H ? Random.Next(3) : slot8.Ability is AbilityPermission.Any12 ? Random.Next(2) : slot8.Ability is AbilityPermission.OnlyFirst ? 0 : slot8.Ability is AbilityPermission.OnlySecond ? 1 : 2);
            else if (enc is EncounterStatic8 static8)
                pkm.SetAbilityIndex(static8.Ability is AbilityPermission.Any12H ? Random.Next(3) : static8.Ability is AbilityPermission.Any12 ? Random.Next(2) : static8.Ability is AbilityPermission.OnlyFirst ? 0 : static8.Ability is AbilityPermission.OnlySecond ? 1 : 2);
            else if (enc is EncounterStatic8N static8N)
                pkm.SetAbilityIndex(static8N.Ability is AbilityPermission.Any12H ? Random.Next(3) : static8N.Ability is AbilityPermission.Any12 ? Random.Next(2) : static8N.Ability is AbilityPermission.OnlyFirst ? 0 : static8N.Ability is AbilityPermission.OnlySecond ? 1 : 2);
            else if (enc is EncounterStatic8NC static8NC)
                pkm.SetAbilityIndex(static8NC.Ability is AbilityPermission.Any12H ? Random.Next(3) : static8NC.Ability is AbilityPermission.Any12 ? Random.Next(2) : static8NC.Ability is AbilityPermission.OnlyFirst ? 0 : static8NC.Ability is AbilityPermission.OnlySecond ? 1 : 2);
            else if (enc is EncounterStatic8ND static8ND)
                pkm.SetAbilityIndex(static8ND.Ability is AbilityPermission.Any12H ? Random.Next(3) : static8ND.Ability is AbilityPermission.Any12 ? Random.Next(2) : static8ND.Ability is AbilityPermission.OnlyFirst ? 0 : static8ND.Ability is AbilityPermission.OnlySecond ? 1 : 2);
            else if (enc is EncounterStatic8U static8U)
                pkm.SetAbilityIndex(static8U.Ability is AbilityPermission.Any12H ? Random.Next(3) : static8U.Ability is AbilityPermission.Any12 ? Random.Next(2) : static8U.Ability is AbilityPermission.OnlyFirst ? 0 : static8U.Ability is AbilityPermission.OnlySecond ? 1 : 2);
        }

        bool goMew = pkm.Species is (ushort)Species.Mew && enc.Version is GameVersion.GO && pkm.IsShiny;
        bool goOther = (pkm.Species is (ushort)Species.Victini or (ushort)Species.Jirachi or (ushort)Species.Celebi or (ushort)Species.Genesect) && enc.Version is GameVersion.GO;
        if (enc is EncounterSlot8GO slotGO && !goMew && !goOther)
            pkm.SetRandomIVsGO(slotGO.Type.GetMinIV());
        else if (enc is EncounterStatic8N static8N)
            pkm.SetRandomIVs(static8N.FlawlessIVCount + 1);
        else if (pkm is PK8 pk8 && enc is IOverworldCorrelation8 ow)
        {
            var criteria = EncounterCriteria.GetCriteria(template, pk8.PersonalInfo);
            List<int> IVs = new() { 0, 0, 0, 0, 0, 0 };
            if (enc is EncounterStatic8 static8)
            {
                if (static8.IsOverworldCorrelation)
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        int flawless = static8.FlawlessIVCount + Random.Next(6 - static8.FlawlessIVCount);
                        while (IVs.FindAll(x => x == 31).Count < flawless)
                            IVs[Random.Next(IVs.Count)] = 31;

                        pk8.IVs = new int[] { IVs[0], IVs[1], IVs[2], IVs[3], IVs[4], IVs[5] };
                        var encFlawless = Overworld8Search.GetFlawlessIVCount(enc, pk8.IVs, out uint seed);
                        APILegality.FindWildPIDIV8(pk8, shiny, encFlawless, seed);
                        if (ow.IsOverworldCorrelationCorrect(pk8))
                            break;
                        else IVs = new() { 0, 0, 0, 0, 0, 0 };
                    }
                }
                else pk8.SetRandomIVs(Random.Next(static8.FlawlessIVCount, 7));
            }
            else if (enc is EncounterSlot8 slot8)
            {
                if (ow.GetRequirement(pkm) == OverworldCorrelation8Requirement.MustHave)
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        while (IVs.FindAll(x => x == 31).Count < 4)
                            IVs[Random.Next(IVs.Count)] = 31;

                        pk8.IVs = new int[] { IVs[0], IVs[1], IVs[2], IVs[3], IVs[4], IVs[5] };
                        var encFlawless = Overworld8Search.GetFlawlessIVCount(enc, pk8.IVs, out uint seed);
                        APILegality.FindWildPIDIV8(pk8, shiny, encFlawless, seed);
                        if (ow.IsOverworldCorrelationCorrect(pk8))
                            break;
                        else IVs = new() { 0, 0, 0, 0, 0, 0 };
                    }
                }
                else pk8.SetRandomIVs(4);
            }
        }
        else if (enc.Version is not GameVersion.GO && enc.Generation >= 6)
            pkm.SetRandomIVs(enc.EggEncounter ? Random.Next(7) : 4);

        pkm = (T)TradeExtensions<T>.TrashBytes(pkm);
        pkm.CurrentFriendship = pkm.PersonalInfo.BaseFriendship;
        return pkm;
    }

    protected T RngRoutineBDSP(T pkm, Shiny shiny)
    {
        pkm.Move1_PPUps = pkm.Move2_PPUps = pkm.Move3_PPUps = pkm.Move4_PPUps = 0;
        pkm.SetMaximumPPCurrent(pkm.Moves);
        pkm.ClearHyperTraining();

        var la = new LegalityAnalysis(pkm);
        var enc = la.Info.EncounterMatch;
        var evoChain = la.Info.EvoChainsAllGens.Gen8b.FirstOrDefault(x => x.Species == pkm.Species);
        pkm.CurrentLevel = enc.LevelMin < evoChain.LevelMin ? evoChain.LevelMin : enc.LevelMin;

        while (!new LegalityAnalysis(pkm).Valid)
        {
            pkm.CurrentLevel += 1;
            if (pkm.CurrentLevel >= 100)
                return pkm;
        }

        la = new LegalityAnalysis(pkm);
        if (!la.Valid)
        {
            pkm.SetSuggestedMoves();
            Span<ushort> relearn = stackalloc ushort[4];
            la.GetSuggestedRelearnMoves(relearn, enc);
            pkm.SetRelearnMoves(relearn);
        }
        pkm.HealPP();

        if (enc is not EncounterStatic8b && !pkm.FatefulEncounter)
        {
            pkm.Nature = Random.Next(25);
            pkm.StatNature = pkm.Nature;
            if (enc is EncounterSlot8b slot8)
                pkm.SetAbilityIndex(slot8.Ability is AbilityPermission.Any12H && slot8.CanUseRadar && !slot8.EggEncounter ? Random.Next(3) : slot8.Ability is AbilityPermission.Any12 ? Random.Next(2) : slot8.Ability is AbilityPermission.OnlyFirst ? 0 : slot8.Ability is AbilityPermission.OnlySecond ? 1 : 2);
            else if (!IsLegendaryOrMythical(pkm.Species))
                pkm.SetAbilityIndex(Random.Next(2));

            pkm.SetRandomIVs(Random.Next(3, 7));
            if (shiny is Shiny.AlwaysSquare)
                CommonEdits.SetShiny(pkm, shiny);
        }

        pkm = (T)TradeExtensions<T>.TrashBytes(pkm);
        pkm.CurrentFriendship = pkm.PersonalInfo.BaseFriendship;
        return pkm;
    }

    protected T RngRoutineSV(T pkm, Shiny shiny)
    {
        pkm.Move1_PPUps = pkm.Move2_PPUps = pkm.Move3_PPUps = pkm.Move4_PPUps = 0;
        pkm.SetMaximumPPCurrent(pkm.Moves);
        pkm.ClearHyperTraining();

        var la = new LegalityAnalysis(pkm);
        var enc = la.Info.EncounterMatch;
        var evoChain = la.Info.EvoChainsAllGens.Gen9.FirstOrDefault(x => x.Species == pkm.Species);
        pkm.CurrentLevel = enc.LevelMin < evoChain.LevelMin ? evoChain.LevelMin : enc.LevelMin;

        while (!new LegalityAnalysis(pkm).Valid)
        {
            pkm.CurrentLevel += 1;
            if (pkm.CurrentLevel >= 100)
                return pkm;
        }

        la = new LegalityAnalysis(pkm);
        if (!la.Valid)
        {
            pkm.SetSuggestedMoves();
            Span<ushort> relearn = stackalloc ushort[4];
            la.GetSuggestedRelearnMoves(relearn, enc);
            pkm.SetRelearnMoves(relearn);
        }
        pkm.HealPP();

        if (enc is not EncounterStatic9 && !pkm.FatefulEncounter)
        {
            pkm.Nature = Random.Next(25);
            pkm.StatNature = pkm.Nature;
            if (enc is EncounterSlot9 slot9)
                pkm.SetAbilityIndex(slot9.Ability is AbilityPermission.Any12H && !slot9.EggEncounter ? Random.Next(3) : slot9.Ability is AbilityPermission.Any12 ? Random.Next(2) : slot9.Ability is AbilityPermission.OnlyFirst ? 0 : slot9.Ability is AbilityPermission.OnlySecond ? 1 : 2);
            else if (!IsLegendaryOrMythical(pkm.Species))
                pkm.SetAbilityIndex(Random.Next(2));

            pkm.SetRandomIVs(Random.Next(3, 7));
            if (shiny is Shiny.AlwaysSquare)
                CommonEdits.SetShiny(pkm, shiny);
        }

        pkm = (T)TradeExtensions<T>.TrashBytes(pkm);
        pkm.CurrentFriendship = pkm.PersonalInfo.BaseFriendship;
        return pkm;
    }

    protected T EggRngRoutine(IReadOnlyList<EvoCriteria> evos, int[] balls, int generation, string trainerInfo, Shiny shiny)
    {
        var shinyRng = shiny is Shiny.AlwaysSquare ? "\nShiny: Square" : shiny is Shiny.AlwaysStar ? "\nShiny: Star" : shiny is not Shiny.Never ? "\nShiny: Yes" : "";
        int dittoLoc = DittoSlot(evos[0].Species, evos[1].Species);
        bool random = evos.All(x => x.Species is 132);

        ushort baseSpecies = 0;
        byte formID = 0;
        if (!random)
        {
            for (int i = 0; i < evos.Count; i++)
            {
                if (BaseCanBeEgg(evos[i].Species, evos[i].Form, out _, out baseSpecies) && baseSpecies > 0)
                {
                    formID = evos[i].Form;
                    break;
                }
            }
        }

        var keys = Dex.Keys.ToArray();
        var speciesID = random ? keys[Random.Next(Dex.Count)] : baseSpecies;
        string formName = string.Empty;
        if (random)
        {
            while (true)
            {
                TradeExtensions<T>.FormOutput(speciesID, 0, out string[] formsR);
                formID = (byte)Random.Next(formsR.Length);
                if (BaseCanBeEgg(speciesID, formID, out formID, out baseSpecies) && baseSpecies > 0)
                {
                    formName = TradeExtensions<T>.FormOutput(baseSpecies, formID, out _);
                    speciesID = baseSpecies;
                    break;
                }
                speciesID = keys[Random.Next(Dex.Count)];
            }
        }
        else formName = TradeExtensions<T>.FormOutput(speciesID, formID, out _);

        var speciesName = SpeciesName.GetSpeciesNameGeneration(speciesID, 2, generation);
        if (speciesName.Contains("Nidoran"))
            speciesName = speciesName.Remove(speciesName.Length - 1);

        formName = speciesName switch
        {
            "Nidoran" => _ = !random && dittoLoc is 1 ? (evos[1].Species is 32 ? "-M" : "-F") : !random && dittoLoc is 2 ? (evos[0].Species is 32 ? "-M" : "-F") : (Random.Next(2) is 0 ? "-M" : "-F"),
            "Indeedee" => _ = !random && dittoLoc is 1 ? (evos[1].Species is 876 ? "-M" : "-F") : !random && dittoLoc is 2 ? (evos[0].Species is 876 ? "-M" : "-F") : (Random.Next(2) is 0 ? "-M" : "-F"),
            _ => formName,
        };

        if (speciesID is (ushort)Species.Rotom || FormInfo.IsBattleOnlyForm(speciesID, formID, generation) || !Breeding.CanHatchAsEgg(speciesID, formID, (EntityContext)generation))
            formName = "";

        var set = new ShowdownSet($"Egg({speciesName}{formName}){shinyRng}\n{trainerInfo}");
        var template = AutoLegalityWrapper.GetTemplate(set);
        var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
        var pk = (T)sav.GetLegal(template, out string result);

        var ballRngDC = Random.Next(1, 3);
        pk.Ball = ballRngDC is 1 ? balls[0] : balls[1];
        if (!pk.ValidBall())
            pk.Ball = BallApplicator.ApplyBallLegalRandom(pk);

        TradeExtensions<T>.EggTrade(pk, template);
        pk.SetAbilityIndex(Random.Next(Game is GameVersion.SWSH ? 3 : 2));

        pk.Nature = Random.Next(25);
        pk.StatNature = pk.Nature;
        pk.SetRandomIVs(Random.Next(2, 7));
        return pk;
    }

    private static int DittoSlot(ushort species1, ushort species2)
    {
        if (species1 is 132 && species2 is not 132)
            return 1;
        else if (species2 is 132 && species1 is not 132)
            return 2;
        else return 0;
    }

    protected string GetItemString(int item) => GameInfo.Strings.itemlist[item];

    public static bool ArticleChoice(char letter)
    {
        letter = char.ToLowerInvariant(letter);
        return letter switch
        {
            'a' or 'e' or 'i' or 'o' or 'u' or 'y' => true,
            _ => false,
        };
    }

    public static string TimeOfDayString(int offset, bool icon = true)
    {
        var tod = GetTimeOfDay(offset);
        return tod switch
        {
            TimeOfDay.Dawn => icon ? "https://i.imgur.com/hSQR4MT.png" : "Dawn",
            TimeOfDay.Morning => icon ? "https://i.imgur.com/tZiPlen.png" : "Morning",
            TimeOfDay.Day => icon ? "https://i.imgur.com/tZiPlen.png" : "Day",
            TimeOfDay.Dusk => icon ? "https://i.imgur.com/hSQR4MT.png" : "Dusk",
            _ => icon ? "https://i.imgur.com/ZL7sCqW.png" : "Night",
        };
    }

    private static TimeOfDay GetTimeOfDay(int offset)
    {
        var time = (offset < 0 ? DateTime.UtcNow.Subtract(TimeSpan.FromHours(offset * -1)) : DateTime.UtcNow.AddHours(offset)).Hour;
        if (time < 6 && time >= 5)
            return TimeOfDay.Dawn;
        else if (time >= 6 && time < 12)
            return TimeOfDay.Morning;
        else if (time >= 12 && time < 19)
            return TimeOfDay.Day;
        if (time >= 19 && time < 20)
            return TimeOfDay.Dusk;
        else return TimeOfDay.Night;
    }

    private static uint GetAlcremieDeco(TCItems item)
    {
        return item switch
        {
            TCItems.StrawberrySweet => 0,
            TCItems.BerrySweet => 1,
            TCItems.LoveSweet => 2,
            TCItems.StarSweet => 3,
            TCItems.CloverSweet => 4,
            TCItems.FlowerSweet => 5,
            TCItems.RibbonSweet => 6,
            _ => 0,
        };
    }

    private static T? ShedinjaGenerator(T pk, out string msg)
    {
        T? shedinja = (T)pk.Clone();
        var index = shedinja.PersonalInfo.GetIndexOfAbility(shedinja.Ability);
        shedinja.Species = (int)Species.Shedinja;
        shedinja.SetGender(2);
        shedinja.Ball = 4;
        shedinja.SetAbilityIndex(index);
        shedinja.ClearNickname();
        shedinja.Move1_PPUps = shedinja.Move2_PPUps = shedinja.Move3_PPUps = shedinja.Move4_PPUps = 0;
        shedinja.SetSuggestedMoves();
        shedinja.SetMaximumPPCurrent(shedinja.Moves);
        shedinja.HealPP();

        var la = new LegalityAnalysis(shedinja);
        var enc = la.Info.EncounterMatch;

        Span<ushort> relearn = stackalloc ushort[4];
        la.GetSuggestedRelearnMoves(relearn, enc);
        shedinja.SetRelearnMoves(relearn);

        msg = string.Empty;
        la = new LegalityAnalysis(shedinja);
        if (!la.Valid)
        {
            msg = $"Failed to evolve Nincada: \n{la.Report()}";
            shedinja = null;
        }
        return shedinja;
    }

    protected bool EvolvePK(T pk, TimeOfDay tod, out string msg, out T? shedinja, AlcremieForms alcremie = AlcremieForms.None, RegionalFormArgument arg = RegionalFormArgument.None)
    {
        msg = string.Empty;
        shedinja = null;
        var tree = EvolutionTree.GetEvolutionTree(pk.Context);
        var evos = tree.Forward.GetEvolutions(pk.Species, pk.Form).ToArray();
        bool hasEvo = evos.Length > 0;
        if (!hasEvo)
        {
            msg = "This Pokémon cannot evolve.";
            return false;
        }

        var heldItem = (TCItems)pk.HeldItem;
        byte form = (byte)(arg is not RegionalFormArgument.None && pk.Species is not (ushort)Species.Meowth && (int)arg > 1 ? (int)arg - 1 : (int)arg);
        var evoList = Evolutions.FindAll(x => x.Species == pk.Species && x.Item == (alcremie is not AlcremieForms.None ? TCItems.Sweets : heldItem));
        if (evoList.Count is 0)
        {
            msg = "No evolution results found for this Pokémon or criteria not met.";
            return false;
        }

        var result = EdgeCaseEvolutions(evoList, pk, (int)alcremie, form, (int)heldItem, tod);
        if (result != default && result.DayTime is not TimeOfDay.Any && result.DayTime != tod)
        {
            msg = $"This Pokémon seems to like the {Enum.GetName(typeof(TimeOfDay), result.DayTime)!.ToLower()}.";
            return false;
        }
        else if (result == default)
        {
            msg = "Criteria not met or this Pokémon cannot evolve further.";
            return false;
        }
        else if (pk.CurrentLevel < result.EvolvesAtLevel)
        {
            msg = $"Current level is too low, needs to be at least level {result.EvolvesAtLevel}.";
            return false;
        }
        else if (pk is PK8 pk8 && pk8.CanGigantamax && (pk.Species is (ushort)Species.Meowth || pk.Species is (ushort)Species.Pikachu || pk.Species is (ushort)Species.Eevee))
        {
            msg = $"Gigantamax {SpeciesName.GetSpeciesNameGeneration(pk.Species, 2, 8)} cannot evolve.";
            return false;
        }

        switch (result.EvoType)
        {
            case EvolutionType.Trade:
            case EvolutionType.TradeHeldItem:
            case EvolutionType.TradeShelmetKarrablast:
                {
                    var clone = pk.Clone();
                    clone.OT_Name = "Nishikigoi";
                    var trainer = new PokeTrainerDetails(clone);
                    var encShelm = new LegalityAnalysis(pk).EncounterMatch;
                }; break;
            case EvolutionType.Spin:
                {
                    if ((int)heldItem >= 1109 && (int)heldItem <= 1115)
                        pk.ChangeFormArgument(GetAlcremieDeco(heldItem));
                }; break;
            case EvolutionType.LevelUpFriendship:
            case EvolutionType.LevelUpFriendshipMorning:
            case EvolutionType.LevelUpFriendshipNight:
                {
                    pk.CurrentLevel++;
                    if (pk.CurrentFriendship < 179)
                    {
                        msg = "Your Pokémon isn't friendly enough yet.";
                        return false;
                    }
                }; break;
            case EvolutionType.LevelUpAffection50MoveType:
                {
                    pk.CurrentLevel++;
                    if (pk.CurrentFriendship < 250)
                    {
                        msg = "Your Pokémon isn't affectionate enough yet.";
                        return false;
                    }
                }; break;
            case EvolutionType.UseItemFemale:
            case EvolutionType.LevelUpFemale:
                {
                    if (pk.Gender is not 1)
                    {
                        msg = "Incompatible gender for evolution type.";
                        return false;
                    }

                    if (result.EvoType is EvolutionType.LevelUpFemale)
                        pk.CurrentLevel++;
                }; break;
            case EvolutionType.UseItemMale:
            case EvolutionType.LevelUpMale:
                {
                    if (pk.Gender is not 0)
                    {
                        msg = "Incompatible gender for evolution type.";
                        return false;
                    }

                    if (result.EvoType is EvolutionType.LevelUpMale)
                        pk.CurrentLevel++;
                }; break;
            case EvolutionType.LevelUpKnowMove or EvolutionType.LevelUp: pk.CurrentLevel++; break;
        };

        if (pk.Species is (ushort)Species.Nincada)
        {
            shedinja = ShedinjaGenerator(pk, out msg);
            if (shedinja is null)
                return false;
        }

        bool applyMoves = false;
        bool edgeEvos = (pk.Species is (ushort)Species.Koffing && result.EvolvedForm is 0) || ((pk.Species is (ushort)Species.Exeggcute || pk.Species is (ushort)Species.Pikachu || pk.Species is (ushort)Species.Cubone) && result.EvolvedForm > 0);
        var enc = new LegalityAnalysis(pk).EncounterMatch;
        var sav = new SimpleTrainerInfo() { OT = pk.OT_Name, Gender = pk.OT_Gender, Generation = pk.Version, Language = pk.Language, SID16 = pk.SID16, TID16 = pk.TID16, Context = Game is GameVersion.BDSP ? EntityContext.Gen8b : EntityContext.Gen8 };

        if (typeof(T) == typeof(PK8) && pk.Generation is 8 && edgeEvos)
        {
            applyMoves = true;
            int version = pk.Version;
            pk.Version = (int)GameVersion.UM;
            pk.Met_Location = 78; // Paniola Ranch
            pk.Met_Level = 1;
            pk.SetEggMetData(GameVersion.UM, (GameVersion)version);
            enc = new LegalityAnalysis(pk).EncounterMatch;
            if (pk is PK8 pk8)
            {
                pk8.HeightScalar = 0;
                pk8.WeightScalar = 0;
            }

            if (pk.Ball is (int)Ball.Sport || (pk.WasEgg && pk.Ball is (int)Ball.Master))
                pk.SetSuggestedBall(true);
        }

        var index = pk.PersonalInfo.GetIndexOfAbility(pk.Ability);
        pk.Species = result.EvolvesInto;
        pk.Form = result.EvolvedForm;
        pk.SetAbilityIndex(index);
        pk.Nickname = pk.IsNicknamed ? pk.Nickname : pk.ClearNickname();
        if (pk.Species is (ushort)Species.Runerigus)
            pk.SetSuggestedFormArgument((int)Species.Yamask);

        var la = new LegalityAnalysis(pk);
        if (!la.Valid)
        {
            if (result.EvoType is EvolutionType.LevelUpKnowMove || applyMoves)
                EdgeCaseRelearnMoves(pk, la);
            else if (pk.FatefulEncounter)
            {
                Span<ushort> relearn = stackalloc ushort[4];
                la.GetSuggestedRelearnMoves(relearn, enc);
                pk.SetRelearnMoves(relearn);
            }
        }

        la = new LegalityAnalysis(pk);
        if (!la.Valid)
        {
            msg = $"Failed to evolve! Legality report: \n{la.Report()}\n\nWere all evolution requirements and conditions satisfied?";
            return false;
        }

        if (heldItem > 0)
            pk.HeldItem = 0;

        return true;
    }

    private static void EdgeCaseRelearnMoves(T pk, LegalityAnalysis la)
    {
        if (typeof(T) == typeof(PK8) && (pk.Met_Location is 162 or 244))
            return;

        Span<ushort> relearn = stackalloc ushort[4];
        la.GetSuggestedRelearnMoves(relearn, la.EncounterMatch);
        pk.SetRelearnMoves(relearn);

        Span<ushort> moves = stackalloc ushort[4];
        la.GetMoveSet(moves);
        pk.SetMoves(moves);

        var indexEmpty = pk.RelearnMoves.ToList().IndexOf(0);
        if (indexEmpty is not -1)
        {
            ushort move = pk.Species switch
            {
                (ushort)Species.Tangrowth or (ushort)Species.Yanmega when !pk.RelearnMoves.ToList().Contains(246) => 246, // Ancient Power
                (ushort)Species.Grapploct when !pk.RelearnMoves.ToList().Contains(269) => 269, // Taunt
                (ushort)Species.Lickilicky when !pk.RelearnMoves.ToList().Contains(205) => 205, // Rollout
                (ushort)Species.Ambipom when !pk.RelearnMoves.ToList().Contains(458) => 458, // Double Hit
                _ => 0,
            };

            switch (indexEmpty)
            {
                case 0: pk.RelearnMove1 = move; break;
                case 1: pk.RelearnMove2 = move; break;
                case 2: pk.RelearnMove3 = move; break;
                case 3: pk.RelearnMove4 = move; break;
            };
        }
        pk.HealPP();
    }

    private static EvolutionTemplate EdgeCaseEvolutions(List<EvolutionTemplate> evoList, T pk, int alcremieForm, byte form, int item, TimeOfDay tod)
    {
        EvolutionTemplate? result = pk.Species switch
        {
            (ushort)Species.Tyrogue => pk.Stat_ATK == pk.Stat_DEF ? evoList.Find(x => x.EvoType is EvolutionType.LevelUpAeqD) : pk.Stat_ATK > pk.Stat_DEF ? evoList.Find(x => x.EvoType is EvolutionType.LevelUpATK) : evoList.Find(x => x.EvoType is EvolutionType.LevelUpDEF),
            (ushort)Species.Eevee when item > 0 => evoList.Find(x => x.Item == (TCItems)item),
            (ushort)Species.Eevee when pk.CurrentFriendship >= 250 => evoList.Find(x => x.EvoType is EvolutionType.LevelUpAffection50MoveType),
            (ushort)Species.Eevee when item <= 0 => evoList.Find(x => x.DayTime == tod),
            (ushort)Species.Toxel => TradeExtensions<T>.LowKey.Contains(pk.Nature) ? evoList.Find(x => x.EvolvedForm is 1) : evoList.Find(x => x.EvolvedForm is 0),
            (ushort)Species.Milcery when alcremieForm >= 0 => evoList.Find(x => x.EvolvedForm == alcremieForm),
            (ushort)Species.Cosmoem => pk.Version is 45 ? evoList.Find(x => x.EvolvesInto is (ushort)Species.Lunala) : evoList.Find(x => x.EvolvesInto is (ushort)Species.Solgaleo),
            (ushort)Species.Nincada => evoList.Find(x => x.EvolvesInto is (ushort)Species.Ninjask),
            (ushort)Species.Espurr => evoList.Find(x => x.EvolvedForm == (pk.Gender is (int)Gender.Male ? 0 : 1)),
            (ushort)Species.Combee => evoList.Find(x => x.EvolvesInto == (pk.Gender is (int)Gender.Male ? -1 : (ushort)Species.Vespiquen)),
            (ushort)Species.Koffing or (ushort)Species.Exeggcute or (ushort)Species.Pikachu or (ushort)Species.Cubone => evoList.Find(x => x.EvolvedForm == form),
            (ushort)Species.Meowth when pk.Form is 2 => evoList.Find(x => x.EvolvesInto is (ushort)Species.Perrserker),
            (ushort)Species.Zigzagoon or (ushort)Species.Linoone or (ushort)Species.Yamask or (ushort)Species.Corsola or (ushort)Species.Diglett when pk.Form > 0 => evoList.Find(x => x.BaseForm > 0),
            (ushort)Species.Darumaka => pk.Form is 1 ? evoList.Find(x => x.EvolvedForm is 2 && x.Item == (TCItems)item) : evoList.Find(x => x.EvolvedForm is 0),
            (ushort)Species.Rockruff when pk.Form is 1 => evoList.Find(x => x.EvolvedForm is 2), // Dusk
            (ushort)Species.Rockruff => evoList.Find(x => x.DayTime == tod),
            (ushort)Species.Wurmple => GetWurmpleEvo(pk, evoList),
            _ => evoList.Find(x => x.BaseForm == pk.Form),
        };
        return result!;
    }

    private static EvolutionTemplate GetWurmpleEvo(PKM pkm, List<EvolutionTemplate> list)
    {
        var clone = pkm.Clone();
        clone.Species = (ushort)Species.Silcoon;
        if (WurmpleUtil.IsWurmpleEvoValid(clone))
            return list.Find(x => x.EvolvesInto is (ushort)Species.Silcoon)!;
        else return list.Find(x => x.EvolvesInto is (ushort)Species.Cascoon)!;
    }

    protected string ListNameSanitize(string name)
    {
        if (name == "")
            return name;

        name = name[..1].ToUpper().Trim() + name[1..].ToLower().Trim();
        if (name.Contains('\''))
            name = name.Replace("'", "’");
        else if (name.Contains(" - "))
            name = name.Replace(" - ", "-");

        if (name.Contains('-'))
        {
            var split = name.Split('-');
            bool exceptions = split[1] == "z" || split[1] == "m" || split[1] == "f";
            name = split[0] + "-" + (split[1].Length < 2 && !exceptions ? split[1] : split[1][..1].ToUpper() + split[1][1..].ToLower() + (split.Length > 2 ? "-" + split[2].ToUpper() : ""));
        }

        if (name.Contains(' '))
        {
            var split = name.Split(' ');
            name = split[0] + " " + split[1][..1].ToUpper() + split[1][1..].ToLower();
            if (name.Contains('-'))
                name = name.Split('-')[0] + "-" + name.Split('-')[1][..1].ToUpper() + name.Split('-')[1][1..];
        }
        return name;
    }

    protected Species ParseSpeciesFromSanitizedLabel(string label)
    {
        var speciesWithDash = ((IReadOnlyList<string>)[
                    "Nidoran-M",
            "Nidoran-F",
            "Porygon-Z",
            "Jangmo-o",
            "Hakamo-o",
            "Kommo-o",
            "Wo-Chien",
            "Chien-Pao",
            "Ting-Lu",
            "Chi-Yu"
        ]).ToFrozenSet<string>;
        var speciesWithSpace = ((IReadOnlyList<string>)[
                    "Mr. Mime",
            "Mime Jr.",
            "Tapu Koko",
            "Tapu Lele",
            "Tapu Bulu",
            "Tapu Fini",
            "Mr. Rime",
            "Great Tusk",
            "Scream Tail",
            "Brute Bonnet",
            "Flutter Mane",
            "Slither Wing",
            "Sandy Shocks",
            "Iron Treads",
            "Iron Bundle",
            "Iron Hands",
            "Iron Jugulis",
            "Iron Moth",
            "Iron Thorns",
            "Roaring Moon",
            "Iron Valiant",
            "Walking Wake",
            "Iron Leaves",
            "Gouging Fire",
            "Raging Bolt",
            "Iron Boulder",
            "Iron Crown"
        ]).ToFrozenSet<string>;

        if (label.Contains('’')) // For Farfetch’d and Sirfetch’d
        {
            label = (string)label.Where((char x) => x != '’');
        }
        else if (speciesWithDash.ToString()!.Contains(label) || speciesWithSpace.ToString()!.Contains(label))
        {
            label = (string)label.Where(char.IsLetterOrDigit);
        }

        bool speciesAndForm = label.Contains('-');
        return TradeExtensions<T>.EnumParse<Species>(speciesAndForm ? label.Split('-')[0] : label);
    }

    protected bool CanGenerateEgg(ref TCUser user, out IReadOnlyList<EvoCriteria> criteria, out int[] balls, out bool update)
    {
        update = false;
        criteria = new List<EvoCriteria>();
        balls = new int[2];
        if (user.Daycare.Species1 is 0 || user.Daycare.Species2 is 0)
            return false;

        var pkm1 = GetLookupAsClassObject<T>(user.UserInfo.UserID, "binary_catches", $"and id = {user.Daycare.ID1}");
        if (pkm1.Species is 0)
        {
            if (user.Daycare.Species2 is not 0)
                user.Daycare = new() { Ball2 = user.Daycare.Ball2, Form2 = user.Daycare.Form2, ID2 = user.Daycare.ID2, Shiny2 = user.Daycare.Shiny2, Species2 = user.Daycare.Species2 };
            else user.Daycare = new();
            update = true;
        }

        var pkm2 = GetLookupAsClassObject<T>(user.UserInfo.UserID, "binary_catches", $"and id = {user.Daycare.ID2}");
        if (pkm2.Species is 0)
        {
            if (user.Daycare.Species1 is not 0)
                user.Daycare = new() { Ball1 = user.Daycare.Ball1, Form1 = user.Daycare.Form1, ID1 = user.Daycare.ID1, Shiny1 = user.Daycare.Shiny1, Species1 = user.Daycare.Species1 };
            else user.Daycare = new();
            update = true;
        }

        if (pkm1.IsEgg || pkm2.IsEgg || user.Daycare.Species1 is 0 || user.Daycare.Species2 is 0)
            return false;

        bool sameTree = pkm1.Species is 132 || pkm2.Species is 132 || SameEvoTree(pkm1, pkm2);
        bool breedable = CanHatchTradeCord(pkm1.Species) && CanHatchTradeCord(pkm2.Species);
        if (!sameTree || !breedable)
            return false;

        Random rng = new();

        if (pkm1.Species is 132)
            while (!Breeding.CanHatchAsEgg(pkm1.Species))
            {
                pkm1.Species = (ushort)rng.Next(1021);
            }
        if (pkm2.Species is 132)
            pkm2.Species = pkm1.Species;
        criteria = EggEvoCriteria(pkm1, pkm2);
        if (criteria.Count < 2)
            return false;

        balls = [pkm1.Ball, pkm2.Ball];
        return true;
    }

    private static bool CanHatchTradeCord(ushort species) => Breeding.CanHatchAsEgg(species) || species is (ushort)Species.Ditto;

    private bool SameEvoTree(PKM pkm1, PKM pkm2)
    {
        var evos = EncounterOrigin.GetOriginChain(pkm1, 8);
        var encs = EncounterGenerator.GetGenerator(Game).GetPossible(pkm1, evos, Game, EncounterTypeGroup.Egg).ToArray();
        var base1 = encs.Length > 0 ? encs[^1].Species : -1;

        evos = EncounterOrigin.GetOriginChain(pkm2, 8);
        encs = EncounterGenerator.GetGenerator(Game).GetPossible(pkm2, evos, Game, EncounterTypeGroup.Egg).ToArray();
        var base2 = encs.Length > 0 ? encs[^1].Species : -2;

        return base1 == base2;
    }

    private static List<EvoCriteria> EggEvoCriteria(T pk1, T pk2)
    {
        List<T> list = new() { pk1, pk2 };
        List<EvoCriteria> criteriaList = [];
        for (int i = 0; i < list.Count; i++)
        {
            byte form = list[i].Species switch
            {
                (ushort)Species.Obstagoon or (ushort)Species.Cursola or (ushort)Species.Runerigus or (ushort)Species.Sirfetchd => 1,
                (ushort)Species.Perrserker => 2,
                (ushort)Species.Lycanroc or (ushort)Species.Slowbro or (ushort)Species.Darmanitan when list[i].Form is 2 => 1,
                (ushort)Species.Lycanroc when list[i].Form is 1 => 0,
                (ushort)Species.Sinistea or (ushort)Species.Polteageist or (ushort)Species.Rotom or (ushort)Species.Pikachu or (ushort)Species.Raichu or (ushort)Species.Marowak or (ushort)Species.Exeggutor or (ushort)Species.Weezing or (ushort)Species.Alcremie => 0,
                (ushort)Species.MrMime => list[i].Form == 1 ? (byte)0 : list[i].Form,
                _ => list[i].Form,
            };

            if (list[i].Species is (ushort)Species.Rotom && list[i].Form > 0)
                list[i].Form = 0;

            var tree = EvolutionTree.GetEvolutionTree(list[i].Context); // Obtain the correct evolution tree for the context
            var preEvos = tree.Reverse.GetPreEvolutions(list[i].Species, list[i].Form).Where(x => x.Form == form).ToList(); // Obtain the reverse evolution paths
            var filteredPreEvos = preEvos.ToList();
            if (!filteredPreEvos.Any())
                continue;
            var evo = filteredPreEvos.LastOrDefault();
            criteriaList.Add(new EvoCriteria { Species = evo.Species, Form = evo.Form });
        }
        return criteriaList;
    }

    protected List<string> GetMissingDexEntries(List<ushort> dex)
    {
        List<string> missing = [];
        var keys = Dex.Keys.ToArray();
        for (int s = 0; s < keys.Length; s++)
        {
            if (!dex.Contains(keys[s]))
                missing.Add(SpeciesName.GetSpeciesNameGeneration(keys[s], 2, 8));
        }
        return missing;
    }

    protected void EventHandler(TradeCordSettings settings, out MysteryGift? mg, out byte form)
    {
        string type = string.Empty;
        bool match = false;
        form = 255;
        mg = default!;
        var eventType = $"{settings.PokeEventType}";
        var keys = Dex.Keys.ToArray();

        while (!match)
        {
            Rng.SpeciesRNG = keys[Random.Next(Dex.Count)];
            var formIDs = Dex[Rng.SpeciesRNG].ToArray();
            form = 255;

            if (settings.PokeEventType is PokeEventType.EventPoke)
                mg = MysteryGiftRng(settings);
            if ((int)settings.PokeEventType <= 17)
            {
                for (int i = 0; i < formIDs.Length; i++)
                {
                    var blank = new T { Species = Rng.SpeciesRNG, Form = formIDs[i] };
                    var type1 = GameInfo.Strings.Types[blank.PersonalInfo.Type1];
                    var type2 = GameInfo.Strings.Types[blank.PersonalInfo.Type2];
                    type = type1 == eventType ? type1 : type2 == eventType ? type2 : "";
                    if (type != "")
                    {
                        form = blank.Form;
                        break;
                    }
                }
            }
            else if (settings.PokeEventType is PokeEventType.Halloween)
            {
                if (Rng.SpeciesRNG is (ushort)Species.Corsola or (ushort)Species.Marowak or (ushort)Species.Moltres)
                    form = 1;
            }
            else if (settings.PokeEventType is PokeEventType.Babies)
            {
                BaseCanBeEgg(Rng.SpeciesRNG, 0, out _, out ushort baseSpecies);
                Rng.SpeciesRNG = baseSpecies;
            }
            else if (settings.PokeEventType is PokeEventType.CottonCandy)
                form = formIDs[Random.Next(formIDs.Length)];

            match = settings.PokeEventType switch
            {
                PokeEventType.Legends => IsLegendaryOrMythical(Rng.SpeciesRNG),
                PokeEventType.Babies => Rng.SpeciesRNG is not 0,
                PokeEventType.Halloween => Enum.IsDefined(typeof(Halloween), Rng.SpeciesRNG),
                PokeEventType.CottonCandy => IsCottonCandy(Rng.SpeciesRNG, form),
                PokeEventType.PokePets => Enum.IsDefined(typeof(PokePets), Rng.SpeciesRNG),
                PokeEventType.RodentLite => Enum.IsDefined(typeof(RodentLite), Rng.SpeciesRNG),
                PokeEventType.ClickbaitArticle => Enum.IsDefined(typeof(Clickbait), Rng.SpeciesRNG),
                PokeEventType.HouseParty => Enum.IsDefined(typeof(HouseParty), Rng.SpeciesRNG),
                PokeEventType.LegendOfKoi => Rng.SpeciesRNG is (ushort)Species.Magikarp,
                PokeEventType.Paradox => Enum.IsDefined(typeof(Paradox), Rng.SpeciesRNG),
                PokeEventType.FroggyFrenzy => Enum.IsDefined(typeof(FroggyFrenzy), Rng.SpeciesRNG),
                PokeEventType.Anonymyths => Enum.IsDefined(typeof(Anonymyths), Rng.SpeciesRNG),
                PokeEventType.EventPoke => mg != default,
                _ => type == eventType,
            };
        }
    }

    private bool IsCottonCandy(ushort species, byte form)
    {
        var color = (PersonalColor)(Game is GameVersion.SWSH ? PersonalTable.SWSH.GetFormEntry(species, form).Color : Game is GameVersion.SV ? PersonalTable.SV.GetFormEntry(species, form).Color :
            PersonalTable.BDSP.GetFormEntry(species, form).Color);
        return (ShinyMap[(Species)species] is PersonalColor.Blue or PersonalColor.Red or PersonalColor.Pink or PersonalColor.Purple or PersonalColor.Yellow) &&
            (color is PersonalColor.Blue or PersonalColor.Red or PersonalColor.Pink or PersonalColor.Purple or PersonalColor.Yellow);
    }

    protected MysteryGift? MysteryGiftRng(TradeCordSettings settings)
    {
        var type = typeof(T);
        var forms = Dex[Rng.SpeciesRNG];
        var events = EncounterEvent.GetAllEvents().Where(x => x.Species == Rng.SpeciesRNG);
        var mg = events.Where(x => forms.Contains(x.Form)).ToList();
        mg.RemoveAll(x => x.GetDescription().Count() < 3);

        MysteryGift? mgRng = default;
        if (mg.Count > 0)
        {
            if (Rng.ShinyRNG >= 150 - settings.SquareShinyRate || Rng.ShinyRNG >= 150 - settings.StarShinyRate)
            {
                var mgSh = mg.FindAll(x => x.IsShiny);
                mgRng = mgSh.Count > 0 ? mgSh.ElementAt(Random.Next(mgSh.Count)) : mg.ElementAt(Random.Next(mg.Count));
            }
            else mgRng = mg.ElementAt(Random.Next(mg.Count));
        }

        if (mgRng != default && mgRng.Species is (ushort)Species.Giratina && mgRng.Form > 0)
            mgRng.HeldItem = 112;
        else if (mgRng != default && mgRng.Species is (ushort)Species.Silvally && mgRng.Form > 0)
            mgRng.HeldItem = SilvallyFormMath(mgRng.Form, 0);
        return mgRng;
    }

    protected byte SilvallyFormMath(byte form, int item) => (byte)(item > 0 ? item - 903 : item is 0 && form is 0 ? 0 : form + 903);

    public string ArrayStringify(Array array)
    {
        ushort[] newArray = (ushort[])array;
        var result = "";
        for (int i = 0; i < array.Length; i++)
            result += $"{newArray[i]}{(i + 1 == array.Length ? "" : ",")}";
        return result;
    }

    public static bool SelfBotScanner(ulong id, int cd)
    {
        if (TradeCordHelper<T>.UserCommandTimestamps.TryGetValue(id, out List<DateTime>? timeStamps))
        {
            int[] delta = new int[timeStamps.Count - 1];
            bool[] comp = new bool[delta.Length - 1];

            for (int i = 1; i < timeStamps.Count; i++)
                delta[i - 1] = (int)(timeStamps[i].Subtract(timeStamps[i - 1]).TotalSeconds - cd);

            for (int i = 1; i < delta.Length; i++)
                comp[i - 1] = delta[i] == delta[i - 1] || (delta[i] - delta[i - 1] >= -2 && delta[i] - delta[i - 1] <= 2);

            TradeCordHelper<T>.UserCommandTimestamps[id].Clear();
            if (comp.Any(x => x == false))
                return false;
            else return true;
        }
        return false;
    }

    public string[] TrainerInfoToStringArray(TCTrainerInfo info, GameVersion game)
    {
        var tr = new SimpleTrainerInfo(game) { TID16 = info.TID16, SID16 = info.SID16 };
        return new string[] { $"OT: {info.OTName}\n", $"OTGender: {info.OTGender}\n", $"TID: {tr.GetTrainerTID7()}\n", $"SID: {tr.GetTrainerSID7()}\n", $"Language: {info.Language}\n" };
    }
}