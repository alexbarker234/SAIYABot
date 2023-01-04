using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;
using SAIYA.Creatures;
using SAIYA.Models;
using SAIYA.Systems;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;

namespace SAIYA
{
    public class Bot
    {
        public static DiscordClient Client { get; private set; }
        public static CommandsNextExtension Commands { get; private set; }
        public static IMongoDatabase Database { get; private set; }
        public static Random rand { get; private set; }
        public Bot() => RunAsync().GetAwaiter().GetResult();
        private async Task RunAsync()
        {
            rand = new Random();

            var configJson = LoadConfig();

            var config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
                Intents = DiscordIntents.All
            };
            Client = new DiscordClient(config);
            Client.Ready += OnClientReady;
            Client.MessageCreated += OnMessageCreated;

            // slash commands
            var slash = Client.UseSlashCommands();
            slash.RegisterCommands(Assembly.GetExecutingAssembly(), 923496191411507260);

            // database
            var mongoClient = new MongoClient(configJson.MongoConnection);
            Database = mongoClient.GetDatabase("SAIYA");

            CreatureLoader.Load();


            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private ConfigJson LoadConfig()
        {
            var fileContents = string.Empty;
            using (var sr = new StreamReader("config.json", new UTF8Encoding(false)))
                fileContents = sr.ReadToEnd();
            return JsonConvert.DeserializeObject<ConfigJson>(fileContents);
        }

        private Task OnClientReady(DiscordClient client, ReadyEventArgs e)
        {
            Console.WriteLine("hello");
            return Task.CompletedTask;
        }
        private async Task OnMessageCreated(DiscordClient client, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot) return;

            ulong userID = e.Message.Author.Id, guildID = e.Guild.Id;

            User user = await User.GetOrCreateUser(userID, guildID);
            user.Messages++;
            await ManageUserExperience.UserXP(e, user);
            await user.Save();

            // MUST BE RUN AFTER SAVE
            await ManageEggs.EggRoll(client, e, user);
        }    
    }
}
