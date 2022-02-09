using System;
using System.Linq;
using System.Collections.Generic;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using static PKHeX.Core.AutoMod.Aesthetics;

namespace SysBot.Pokemon
{
    public abstract class TradeCordDatabase<T> : TradeCordBase<T> where T : PKM, new()
    {
        protected T RngRoutineSWSH(T pkm, IBattleTemplate template, Shiny shiny)
        {
            if (pkm.Species == (int)Species.Alcremie)
            {
                var data = pkm.Data;
                var deco = (uint)Random.Next(7);
                pkm.ChangeFormArgument(deco);
            }

            var laInit = new LegalityAnalysis(pkm);
            var nature = pkm.Nature;
            pkm.Nature = pkm.Species switch
            {
                (int)Species.Toxtricity => pkm.Form > 0 ? TradeExtensions<PK8>.LowKey[Random.Next(TradeExtensions<PK8>.LowKey.Length)] : TradeExtensions<PK8>.Amped[Random.Next(TradeExtensions<PK8>.Amped.Length)],
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
            var evoChain = la.Info.EvoChainsAllGens[pkm.Format].FirstOrDefault(x => x.Species == pkm.Species);
            pkm.CurrentLevel = enc.LevelMin < evoChain.MinLevel ? evoChain.MinLevel : enc.LevelMin;
            while (!new LegalityAnalysis(pkm).Valid)
            {
                pkm.CurrentLevel += 1;
                if (pkm.CurrentLevel >= 100)
                    return pkm;
            }

            pkm.SetSuggestedMoves();
            pkm.SetRelearnMoves(pkm.GetSuggestedRelearnMoves(enc));
            pkm.HealPP();

            if (!GalarFossils.Contains(pkm.Species) && !pkm.FatefulEncounter)
            {
                if (enc is EncounterSlot8 slot8)
                    pkm.SetAbilityIndex(slot8.Ability == AbilityPermission.Any12H ? Random.Next(3) : slot8.Ability == AbilityPermission.Any12 ? Random.Next(2) : slot8.Ability == AbilityPermission.OnlyFirst ? 0 : slot8.Ability == AbilityPermission.OnlySecond ? 1 : 2);
                else if (enc is EncounterStatic8 static8)
                    pkm.SetAbilityIndex(static8.Ability == AbilityPermission.Any12H ? Random.Next(3) : static8.Ability == AbilityPermission.Any12 ? Random.Next(2) : static8.Ability == AbilityPermission.OnlyFirst ? 0 : static8.Ability == AbilityPermission.OnlySecond ? 1 : 2);
                else if (enc is EncounterStatic8N static8N)
                    pkm.SetAbilityIndex(static8N.Ability == AbilityPermission.Any12H ? Random.Next(3) : static8N.Ability == AbilityPermission.Any12 ? Random.Next(2) : static8N.Ability == AbilityPermission.OnlyFirst ? 0 : static8N.Ability == AbilityPermission.OnlySecond ? 1 : 2);
                else if (enc is EncounterStatic8NC static8NC)
                    pkm.SetAbilityIndex(static8NC.Ability == AbilityPermission.Any12H ? Random.Next(3) : static8NC.Ability == AbilityPermission.Any12 ? Random.Next(2) : static8NC.Ability == AbilityPermission.OnlyFirst ? 0 : static8NC.Ability == AbilityPermission.OnlySecond ? 1 : 2);
                else if (enc is EncounterStatic8ND static8ND)
                    pkm.SetAbilityIndex(static8ND.Ability == AbilityPermission.Any12H ? Random.Next(3) : static8ND.Ability == AbilityPermission.Any12 ? Random.Next(2) : static8ND.Ability == AbilityPermission.OnlyFirst ? 0 : static8ND.Ability == AbilityPermission.OnlySecond ? 1 : 2);
                else if (enc is EncounterStatic8U static8U)
                    pkm.SetAbilityIndex(static8U.Ability == AbilityPermission.Any12H ? Random.Next(3) : static8U.Ability == AbilityPermission.Any12 ? Random.Next(2) : static8U.Ability == AbilityPermission.OnlyFirst ? 0 : static8U.Ability == AbilityPermission.OnlySecond ? 1 : 2);
            }

            bool goMew = pkm.Species == (int)Species.Mew && enc.Version == GameVersion.GO && pkm.IsShiny;
            bool goOther = (pkm.Species == (int)Species.Victini || pkm.Species == (int)Species.Jirachi || pkm.Species == (int)Species.Celebi || pkm.Species == (int)Species.Genesect) && enc.Version == GameVersion.GO;
            if (enc is EncounterSlotGO slotGO && !goMew && !goOther)
                pkm.SetRandomIVsGO(slotGO.Type.GetMinIV());
            else if (enc is EncounterStatic8N static8N)
                pkm.SetRandomIVs(static8N.FlawlessIVCount + 1);
            else if (pkm is PK8 pk8 && enc is IOverworldCorrelation8 ow)
            {
                var criteria = EncounterCriteria.GetCriteria(template);
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
            else if (enc.Version != GameVersion.GO && enc.Generation >= 6)
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
            var evoChain = la.Info.EvoChainsAllGens[pkm.Format].FirstOrDefault(x => x.Species == pkm.Species);
            pkm.CurrentLevel = enc.LevelMin < evoChain.MinLevel ? evoChain.MinLevel : enc.LevelMin;
            while (!new LegalityAnalysis(pkm).Valid)
            {
                pkm.CurrentLevel += 1;
                if (pkm.CurrentLevel >= 100)
                    return pkm;
            }

            if (!new LegalityAnalysis(pkm).Valid)
            {
                pkm.SetSuggestedMoves();
                pkm.SetRelearnMoves(pkm.GetSuggestedRelearnMoves(enc));
            }
            pkm.HealPP();
            
            if (enc is not EncounterStatic8b && !pkm.FatefulEncounter)
            {
                pkm.Nature = Random.Next(25);
                pkm.StatNature = pkm.Nature;
                if (enc is EncounterSlot8b slot8)
                    pkm.SetAbilityIndex(slot8.Ability == AbilityPermission.Any12H && slot8.CanUseRadar ? Random.Next(3) : slot8.Ability == AbilityPermission.Any12 ? Random.Next(2) : slot8.Ability == AbilityPermission.OnlyFirst ? 0 : slot8.Ability == AbilityPermission.OnlySecond ? 1 : 2);
                else if (!IsLegendaryOrMythical(pkm.Species))
                    pkm.SetAbilityIndex(Random.Next(2));

                pkm.IVs = pkm.SetRandomIVs(Random.Next(3, 7));
                if (shiny == Shiny.AlwaysSquare)
                    CommonEdits.SetShiny(pkm, shiny);
            }

            pkm = (T)TradeExtensions<T>.TrashBytes(pkm);
            pkm.CurrentFriendship = pkm.PersonalInfo.BaseFriendship;
            return pkm;
        }

        protected T EggRngRoutine(IReadOnlyList<EvoCriteria> evos, int[] balls, int generation, string trainerInfo, Shiny shiny)
        {
            var shinyRng = shiny == Shiny.AlwaysSquare ? "\nShiny: Square" : shiny == Shiny.AlwaysStar ? "\nShiny: Star" : shiny != Shiny.Never ? "\nShiny: Yes" : "";
            int dittoLoc = DittoSlot(evos[0].Species, evos[1].Species);
            bool random = evos.All(x => x.Species == 132);

            int baseSpecies = 0;
            int formID = 0;
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

            int speciesID = random ? Dex[Random.Next(Dex.Length)] : baseSpecies;
            string formName = string.Empty;
            if (random)
            {
                while (true)
                {
                    TradeExtensions<PK8>.FormOutput(speciesID, 0, out string[] formsR);
                    formID = Random.Next(formsR.Length);
                    if (BaseCanBeEgg(speciesID, formID, out formID, out baseSpecies) && baseSpecies > 0)
                    {
                        formName = TradeExtensions<PK8>.FormOutput(baseSpecies, formID, out _);
                        speciesID = baseSpecies;
                        break;
                    }
                    speciesID = Dex[Random.Next(Dex.Length)];
                }
            }
            else formName = TradeExtensions<T>.FormOutput(speciesID, formID, out _);

            var speciesName = SpeciesName.GetSpeciesNameGeneration(speciesID, 2, generation);
            if (speciesName.Contains("Nidoran"))
                speciesName = speciesName.Remove(speciesName.Length - 1);

            formName = speciesName switch
            {
                "Nidoran" => _ = !random && dittoLoc == 1 ? (evos[1].Species == 32 ? "-M" : "-F") : !random && dittoLoc == 2 ? (evos[0].Species == 32 ? "-M" : "-F") : (Random.Next(2) == 0 ? "-M" : "-F"),
                "Indeedee" => _ = !random && dittoLoc == 1 ? (evos[1].Species == 876 ? "-M" : "-F") : !random && dittoLoc == 2 ? (evos[0].Species == 876 ? "-M" : "-F") : (Random.Next(2) == 0 ? "-M" : "-F"),
                _ => formName,
            };

            if (speciesID == (int)Species.Rotom || FormInfo.IsBattleOnlyForm(speciesID, formID, generation) || !Breeding.CanHatchAsEgg(speciesID, formID, generation))
                formName = "";

            var set = new ShowdownSet($"Egg({speciesName}{formName}){shinyRng}\n{trainerInfo}");
            var template = AutoLegalityWrapper.GetTemplate(set);
            var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
            var pk = (T)sav.GetLegal(template, out string result);

            var ballRngDC = Random.Next(1, 3);
            pk.Ball = ballRngDC == 1 ? balls[0] : balls[1];
            if (!pk.ValidBall())
                pk.Ball = BallApplicator.ApplyBallLegalRandom(pk);

            TradeExtensions<T>.EggTrade(pk);
            pk.SetAbilityIndex(Random.Next(Game == GameVersion.SWSH ? 3 : 2));

            pk.Nature = Random.Next(25);
            pk.StatNature = pk.Nature;
            pk.IVs = pk.SetRandomIVs(Random.Next(2, 7));
            return pk;
        }

        private int DittoSlot(int species1, int species2)
        {
            if (species1 == 132 && species2 != 132)
                return 1;
            else if (species2 == 132 && species1 != 132)
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

        private uint GetAlcremieDeco(TCItems item)
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

        private T? ShedinjaGenerator(T pk, out string msg)
        {
            T? shedinja = (T)pk.Clone();
            var index = shedinja.PersonalInfo.GetAbilityIndex(shedinja.Ability);
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
            shedinja.SetRelearnMoves(shedinja.GetSuggestedRelearnMoves(enc));

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
            var tree = EvolutionTree.GetEvolutionTree(pk, 8);
            var evos = tree.GetEvolutions(pk.Species, pk.Form).ToArray();

            bool hasEvo = evos.Length > 0;
            if (!hasEvo)
            {
                msg = "This Pokémon cannot evolve.";
                return false;
            }

            var heldItem = (TCItems)pk.HeldItem;
            var form = arg != RegionalFormArgument.None && pk.Species != (int)Species.Meowth && (int)arg > 1 ? (int)arg - 1 : (int)arg;
            var evoList = Evolutions.FindAll(x => x.Species == pk.Species && x.Item == (alcremie != AlcremieForms.None ? TCItems.Sweets : heldItem));
            if (evoList.Count == 0)
            {
                msg = "No evolution results found for this Pokémon or criteria not met.";
                return false;
            }

            var result = EdgeCaseEvolutions(evoList, pk, (int)alcremie, form, (int)heldItem, tod);
            if (result != default && result.DayTime != TimeOfDay.Any && result.DayTime != tod)
            {
                msg = $"This Pokémon seems to like the {Enum.GetName(typeof(TimeOfDay), result.DayTime).ToLower()}.";
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
            else if (pk is PK8 pk8 && pk8.CanGigantamax && (pk.Species == (int)Species.Meowth || pk.Species == (int)Species.Pikachu || pk.Species == (int)Species.Eevee))
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
                        pk.SetHandlerandMemory(trainer);
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
                        if (pk.Gender != 1)
                        {
                            msg = "Incompatible gender for evolution type.";
                            return false;
                        }

                        if (result.EvoType == EvolutionType.LevelUpFemale)
                            pk.CurrentLevel++;
                    }; break;
                case EvolutionType.UseItemMale:
                case EvolutionType.LevelUpMale:
                    {
                        if (pk.Gender != 0)
                        {
                            msg = "Incompatible gender for evolution type.";
                            return false;
                        }

                        if (result.EvoType == EvolutionType.LevelUpMale)
                            pk.CurrentLevel++;
                    }; break;
                case EvolutionType.LevelUpKnowMove or EvolutionType.LevelUp: pk.CurrentLevel++; break;
            };

            if (pk.Species == (int)Species.Nincada)
            {
                shedinja = ShedinjaGenerator(pk, out msg);
                if (shedinja == null)
                    return false;
            }

            bool applyMoves = false;
            var sav = new SimpleTrainerInfo() { OT = pk.OT_Name, Gender = pk.OT_Gender, Generation = pk.Version, Language = pk.Language, SID = pk.TrainerSID7, TID = pk.TrainerID7 };
            if (typeof(T) == typeof(PK8) && pk.Generation == 8 && ((pk.Species == (int)Species.Koffing && result.EvolvedForm == 0) || ((pk.Species == (int)Species.Exeggcute || pk.Species == (int)Species.Pikachu || pk.Species == (int)Species.Cubone) && result.EvolvedForm > 0)))
            {
                applyMoves = true;
                int version = pk.Version;
                pk.Version = (int)GameVersion.UM;
                pk.Met_Location = 78; // Paniola Ranch
                pk.Met_Level = 1;
                pk.SetEggMetData(GameVersion.UM, (GameVersion)version);
                sav.Generation = version;
                pk.SetHandlerandMemory(sav);
                if (pk is PK8 pk8)
                {
                    pk8.HeightScalar = 0;
                    pk8.WeightScalar = 0;
                }

                if (pk.Ball == (int)Ball.Sport || (pk.WasEgg && pk.Ball == (int)Ball.Master))
                    pk.SetSuggestedBall(true);
            }
            else pk.SetHandlerandMemory(sav);

            var index = pk.PersonalInfo.GetAbilityIndex(pk.Ability);
            pk.Species = result.EvolvesInto;
            pk.Form = result.EvolvedForm;
            pk.SetAbilityIndex(index);
            pk.Nickname = pk.IsNicknamed ? pk.Nickname : pk.ClearNickname();
            if (pk.Species == (int)Species.Runerigus)
                pk.SetSuggestedFormArgument((int)Species.Yamask);

            var la = new LegalityAnalysis(pk);
            if (!la.Valid)
            {
                if (result.EvoType == EvolutionType.LevelUpKnowMove || applyMoves)
                    EdgeCaseRelearnMoves(pk, la);
                else if (pk.FatefulEncounter)
                    pk.RelearnMoves = (int[])la.EncounterMatch.GetSuggestedRelearn(pk);
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

        private void EdgeCaseRelearnMoves(T pk, LegalityAnalysis la)
        {
            if (typeof(T) == typeof(PK8) && (pk.Met_Location == 162 || pk.Met_Location == 244))
                return;

            pk.Moves = la.GetMoveSet();
            pk.RelearnMoves = (int[])pk.GetSuggestedRelearnMoves(la.EncounterMatch);
            var indexEmpty = pk.RelearnMoves.ToList().IndexOf(0);
            if (indexEmpty != -1)
            {
                int move = pk.Species switch
                {
                    (int)Species.Tangrowth or (int)Species.Yanmega when !pk.RelearnMoves.Contains(246) => 246, // Ancient Power
                    (int)Species.Grapploct when !pk.RelearnMoves.Contains(269) => 269, // Taunt
                    (int)Species.Lickilicky when !pk.RelearnMoves.Contains(205) => 205, // Rollout
                    (int)Species.Ambipom when !pk.RelearnMoves.Contains(458) => 458, // Double Hit
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

        private EvolutionTemplate EdgeCaseEvolutions(List<EvolutionTemplate> evoList, T pk, int alcremieForm, int form, int item, TimeOfDay tod)
        {
            EvolutionTemplate result = pk.Species switch
            {
                (int)Species.Tyrogue => pk.Stat_ATK == pk.Stat_DEF ? evoList.Find(x => x.EvoType == EvolutionType.LevelUpAeqD) : pk.Stat_ATK > pk.Stat_DEF ? evoList.Find(x => x.EvoType == EvolutionType.LevelUpATK) : evoList.Find(x => x.EvoType == EvolutionType.LevelUpDEF),
                (int)Species.Eevee when item > 0 => evoList.Find(x => x.Item == (TCItems)item),
                (int)Species.Eevee when pk.CurrentFriendship >= 250 => evoList.Find(x => x.EvoType == EvolutionType.LevelUpAffection50MoveType),
                (int)Species.Eevee when item <= 0 => evoList.Find(x => x.DayTime == tod),
                (int)Species.Toxel => TradeExtensions<T>.LowKey.Contains(pk.Nature) ? evoList.Find(x => x.EvolvedForm == 1) : evoList.Find(x => x.EvolvedForm == 0),
                (int)Species.Milcery when alcremieForm >= 0 => evoList.Find(x => x.EvolvedForm == alcremieForm),
                (int)Species.Cosmoem => pk.Version == 45 ? evoList.Find(x => x.EvolvesInto == (int)Species.Lunala) : evoList.Find(x => x.EvolvesInto == (int)Species.Solgaleo),
                (int)Species.Nincada => evoList.Find(x => x.EvolvesInto == (int)Species.Ninjask),
                (int)Species.Espurr => evoList.Find(x => x.EvolvedForm == (pk.Gender == (int)Gender.Male ? 0 : 1)),
                (int)Species.Combee => evoList.Find(x => x.EvolvesInto == (pk.Gender == (int)Gender.Male ? -1 : (int)Species.Vespiquen)),
                (int)Species.Koffing or (int)Species.Exeggcute or (int)Species.Pikachu or (int)Species.Cubone when form != -1 => evoList.Find(x => x.EvolvedForm == form),
                (int)Species.Meowth when pk.Form == 2 => evoList.Find(x => x.EvolvesInto == (int)Species.Perrserker),
                (int)Species.Zigzagoon or (int)Species.Linoone or (int)Species.Yamask or (int)Species.Corsola or (int)Species.Diglett when pk.Form > 0 => evoList.Find(x => x.BaseForm > 0),
                (int)Species.Darumaka => pk.Form == 1 ? evoList.Find(x => x.EvolvedForm == 2 && x.Item == (TCItems)item) : evoList.Find(x => x.EvolvedForm == 0),
                (int)Species.Rockruff when pk.Form == 1 => evoList.Find(x => x.EvolvedForm == 2), // Dusk
                (int)Species.Rockruff => evoList.Find(x => x.DayTime == tod),
                (int)Species.Wurmple => GetWurmpleEvo(pk, evoList),
                _ => evoList.Find(x => x.BaseForm == pk.Form),
            };
            return result;
        }

        private EvolutionTemplate GetWurmpleEvo(PKM pkm, List<EvolutionTemplate> list)
        {
            var clone = pkm.Clone();
            clone.Species = (int)Species.Silcoon;
            if (WurmpleUtil.IsWurmpleEvoValid(clone))
                return list.Find(x => x.EvolvesInto == (int)Species.Silcoon);
            else return list.Find(x => x.EvolvesInto == (int)Species.Cascoon);
        }

        protected string ListNameSanitize(string name)
        {
            if (name == "")
                return name;

            name = name[..1].ToUpper().Trim() + name[1..].ToLower().Trim();
            if (name.Contains("'"))
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
                if (name.Contains("-"))
                    name = name.Split('-')[0] + "-" + name.Split('-')[1][..1].ToUpper() + name.Split('-')[1][1..];
            }
            return name;
        }

        protected bool CanGenerateEgg(ref TCUser user, out IReadOnlyList<EvoCriteria> criteria, out int[] balls, out bool update)
        {
            update = false;
            criteria = new List<EvoCriteria>();
            balls = new int[2];
            if (user.Daycare.Species1 == 0 || user.Daycare.Species2 == 0)
                return false;

            var pkm1 = GetLookupAsClassObject<T>(user.UserInfo.UserID, "binary_catches", $"and id = {user.Daycare.ID1}");
            if (pkm1.Species == 0)
            {
                if (user.Daycare.Species2 != 0)
                    user.Daycare = new() { Ball2 = user.Daycare.Ball2, Form2 = user.Daycare.Form2, ID2 = user.Daycare.ID2, Shiny2 = user.Daycare.Shiny2, Species2 = user.Daycare.Species2 };
                else user.Daycare = new();
                update = true;
            }

            var pkm2 = GetLookupAsClassObject<T>(user.UserInfo.UserID, "binary_catches", $"and id = {user.Daycare.ID2}");
            if (pkm2.Species == 0)
            {
                if (user.Daycare.Species1 != 0)
                    user.Daycare = new() { Ball1 = user.Daycare.Ball1, Form1 = user.Daycare.Form1, ID1 = user.Daycare.ID1, Shiny1 = user.Daycare.Shiny1, Species1 = user.Daycare.Species1 };
                else user.Daycare = new();
                update = true;
            }

            if (pkm1.IsEgg || pkm2.IsEgg || user.Daycare.Species1 == 0 || user.Daycare.Species2 == 0)
                return false;

            bool sameTree = pkm1.Species == 132 || pkm2.Species == 132 || SameEvoTree(pkm1, pkm2);
            bool breedable = CanHatchTradeCord(pkm1.Species) && CanHatchTradeCord(pkm2.Species);
            if (!sameTree || !breedable)
                return false;

            criteria = EggEvoCriteria(pkm1, pkm2);
            if (criteria.Count < 2)
                return false;

            balls = new[] { pkm1.Ball, pkm2.Ball };
            return true;
        }

        private bool CanHatchTradeCord(int species) => Breeding.CanHatchAsEgg(species) || species == (int)Species.Ditto;

        private bool SameEvoTree(PKM pkm1, PKM pkm2)
        {
            var tree = EvolutionTree.GetEvolutionTree(pkm1, 8);
            var evos = tree.GetValidPreEvolutions(pkm1, 100, 8, true);
            var encs = EncounterEggGenerator.GenerateEggs(pkm1, evos, 8, false).ToArray();
            var base1 = encs.Length > 0 ? encs[^1].Species : -1;

            tree = EvolutionTree.GetEvolutionTree(pkm2, 8);
            evos = tree.GetValidPreEvolutions(pkm2, 100, 8, true);
            encs = EncounterEggGenerator.GenerateEggs(pkm2, evos, 8, false).ToArray();
            var base2 = encs.Length > 0 ? encs[^1].Species : -2;

            return base1 == base2;
        }

        private List<EvoCriteria> EggEvoCriteria(T pk1, T pk2)
        {
            List<T> list = new() { pk1, pk2 };
            List<EvoCriteria> criteriaList = new();
            for (int i = 0; i < list.Count; i++)
            {
                int form = list[i].Species switch
                {
                    (int)Species.Obstagoon or (int)Species.Cursola or (int)Species.Runerigus or (int)Species.Sirfetchd => 1,
                    (int)Species.Perrserker => 2,
                    (int)Species.Lycanroc or (int)Species.Slowbro or (int)Species.Darmanitan when list[i].Form == 2 => 1,
                    (int)Species.Lycanroc when list[i].Form == 1 => 0,
                    (int)Species.Sinistea or (int)Species.Polteageist or (int)Species.Rotom or (int)Species.Pikachu or (int)Species.Raichu or (int)Species.Marowak or (int)Species.Exeggutor or (int)Species.Weezing or (int)Species.Alcremie => 0,
                    (int)Species.MrMime when list[i].Form == 1 => 0,
                    _ => list[i].Form,
                };

                if (list[i].Species == (int)Species.Rotom && list[i].Form > 0)
                    list[i].Form = 0;

                EvoCriteria? evo = default;
                var preEvos = EvolutionTree.GetEvolutionTree(8).GetValidPreEvolutions(list[i], 100, 8, true).FindAll(x => x.MinLevel == 1);
                if (preEvos.Count == 0)
                    continue;
                else evo = preEvos.LastOrDefault(x => x.Form == form);

                if (evo != default)
                    criteriaList.Add(evo);
            }
            return criteriaList;
        }

        protected List<string> GetMissingDexEntries(List<int> dex)
        {
            List<string> missing = new();
            foreach (var entry in Dex)
            {
                if (!dex.Contains(entry))
                    missing.Add(SpeciesName.GetSpeciesNameGeneration(entry, 2, 8));
            }
            return missing;
        }

        protected void EventHandler(TradeCordSettings settings, out MysteryGift? mg, out int form)
        {
            string type = string.Empty;
            var eventType = $"{settings.PokeEventType}";
            mg = default;
            form = -1;
            bool bdsp = Game == GameVersion.BDSP;

            bool match;
            do
            {
                if (settings.PokeEventType == PokeEventType.EventPoke)
                    mg = MysteryGiftRng(settings);
                else if ((int)settings.PokeEventType <= 17)
                {
                    var personal = GameData.GetPersonal(Game).GetFormEntry(Rng.SpeciesRNG, 0);
                    for (int i = 0; i < personal.FormCount; i++)
                    {
                        var blank = new T { Species = Rng.SpeciesRNG, Form = i };
                        var type1 = GameInfo.Strings.Types[blank.PersonalInfo.Type1];
                        var type2 = GameInfo.Strings.Types[blank.PersonalInfo.Type2];
                        type = type1 == eventType ? type1 : type2 == eventType ? type2 : "";
                        form = type != "" ? blank.Form : -1;
                        if (form != -1)
                            break;
                    }
                }
                else if (settings.PokeEventType == PokeEventType.Halloween)
                {
                    if (!bdsp && (Rng.SpeciesRNG == (int)Species.Corsola || Rng.SpeciesRNG == (int)Species.Marowak || Rng.SpeciesRNG == (int)Species.Moltres))
                        form = 1;
                }
                else if (settings.PokeEventType == PokeEventType.Babies)
                {
                    BaseCanBeEgg(Rng.SpeciesRNG, 0, out _, out int baseSpecies);
                    Rng.SpeciesRNG = baseSpecies;
                }
                else if (settings.PokeEventType == PokeEventType.CottonCandy)
                {
                    TradeExtensions<T>.FormOutput(Rng.SpeciesRNG, 0, out string[] forms);
                    form = Random.Next(forms.Length);
                }

                match = settings.PokeEventType switch
                {
                    PokeEventType.Legends => IsLegendaryOrMythical(Rng.SpeciesRNG),
                    PokeEventType.Babies => Rng.SpeciesRNG != -1,
                    PokeEventType.Halloween => Enum.IsDefined(typeof(Halloween), Rng.SpeciesRNG),
                    PokeEventType.CottonCandy => IsCottonCandy(Rng.SpeciesRNG, form),
                    PokeEventType.PokePets => Enum.IsDefined(typeof(PokePets), Rng.SpeciesRNG),
                    PokeEventType.RodentLite => Enum.IsDefined(typeof(RodentLite), Rng.SpeciesRNG),
                    PokeEventType.ClickbaitArticle => Enum.IsDefined(typeof(Clickbait), Rng.SpeciesRNG),
                    PokeEventType.EventPoke => mg != default,
                    _ => type == eventType,
                };

                if (!match)
                {
                    Rng.SpeciesRNG = Dex[Random.Next(Dex.Length)];
                    form = -1;
                }
            } while (!match);
        }

        private bool IsCottonCandy(int species, int form)
        {
            var color = (PersonalColor)(Game == GameVersion.SWSH ? PersonalTable.SWSH.GetFormEntry(species, form).Color : PersonalTable.BDSP.GetFormEntry(species, form).Color);
            return (ShinyMap[(Species)species] is PersonalColor.Blue or PersonalColor.Red or PersonalColor.Pink or PersonalColor.Purple or PersonalColor.Yellow) &&
                (color is PersonalColor.Blue or PersonalColor.Red or PersonalColor.Pink or PersonalColor.Purple or PersonalColor.Yellow);
        }

        protected MysteryGift? MysteryGiftRng(TradeCordSettings settings)
        {
            var mg = EncounterEvent.GetAllEvents().Where(x => x.Species == Rng.SpeciesRNG).ToList();
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

            if (mgRng != default && mgRng.Species == (int)Species.Giratina && mgRng.Form > 0)
                mgRng.HeldItem = 112;
            else if (mgRng != default && mgRng.Species == (int)Species.Silvally && mgRng.Form > 0)
                mgRng.HeldItem = SilvallyFormMath(mgRng.Form, 0);
            return mgRng;
        }

        protected int SilvallyFormMath(int form, int item) => item > 0 ? item - 903 : item == 0 && form == 0 ? 0 : form + 903;

        public string ArrayStringify(Array array)
        {
            int[] newArray = (int[])array;
            var result = "";
            for (int i = 0; i < array.Length; i++)
                result += $"{newArray[i]}{(i + 1 == array.Length ? "" : ",")}";
            return result;
        }

        public static bool SelfBotScanner(ulong id, int cd)
        {
            if (TradeCordHelper<T>.UserCommandTimestamps.TryGetValue(id, out List<DateTime> timeStamps))
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
    }
}
