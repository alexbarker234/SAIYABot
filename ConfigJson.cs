using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIYA
{
    public struct ConfigJson
    {
        [JsonProperty("botToken")]
        public string BotToken { get; private set; }
        [JsonProperty("mongoToken")]
        public string MongoToken { get; private set; }
        [JsonProperty("weatherToken")]
        public string WeatherToken { get; private set; }
        [JsonProperty("databaseName")]
        public string DatabaseName { get; private set; }
        [JsonProperty("slashCommandGuild")]
        public ulong? SlashCommandGuild { get; private set; }
    }
}
