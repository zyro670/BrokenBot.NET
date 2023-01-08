﻿using FluentAssertions;
using SysBot.Pokemon.SV;
using Xunit;

namespace SysBot.Tests
{
    public class BanListTests
    {
        private static readonly string Url = "https://raw.githubusercontent.com/PokemonAutomation/ServerConfigs-PA-SHA/main/PokemonScarletViolet/TeraAutoHost-BanList.json";

        // To-Do: Add a test that compares CFW-name with Levenshtein/log10p result.
        [Fact]
        public async void IsBannedTestCC()
        {
            var result = await BanService.IsRaiderBanned("Fidio", Url, "TestRoutine", true).ConfigureAwait(false);
            result.Item1.Should().BeTrue();

            result = await BanService.IsRaiderBanned("Nishikigoi", Url, "TestRoutine", true).ConfigureAwait(false);
            result.Item1.Should().BeFalse();

            result = await BanService.IsRaiderBanned("Kazuha", Url, "TestRoutine", true).ConfigureAwait(false);
            result.Item1.Should().BeFalse();
        }
    }
}
