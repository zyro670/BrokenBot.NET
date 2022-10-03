using System.Collections.Generic;
using System.ComponentModel;

namespace SysBot.Pokemon
{
    public class EtumrepDumpSettings
    {
        private const string FeatureToggle = nameof(FeatureToggle);
        private const string Server = nameof(Server);
        public override string ToString() => "Etumrep Dump Settings";

        [Category(FeatureToggle), Description("Maximum time to wait for a user to show all Pokémon before quitting a trade.")]
        public int MaxWaitTime { get; set; } = 60;

        [Category(FeatureToggle), Description("List of EtumrepMMO Servers.")]
        public List<EtumrepServer> Servers { get; set; } = new();

        public class EtumrepServer
        {
            public override string ToString() => Name;

            [Category(Server), Description("Friendly name of the server.")]
            public string Name { get; set; } = "EtumrepMMO Server";

            [Category(Server), Description("Server IP address or host name.")]
            public string IP { get; set; } = string.Empty;

            [Category(Server), Description("Server port.")]
            public int Port { get; set; } = 80;

            [Category(Server), Description("Authentication token.")]
            public string Token { get; set; } = string.Empty;

            [Category(Server), Description("Password given by the server host.")]
            public string Password { get; set; } = string.Empty;

            public string LimitInputLength(string input, bool username)
            {
                if (username && input.Length > 37)
                    return input[..37];
                return input.Length > 32 ? input[..32] : input;
            }
        }
    }
}
