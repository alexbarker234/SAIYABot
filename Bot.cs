using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;
using SAIYA.Commands;
using SAIYA.Content.Creatures;
using SAIYA.Content.Items;
using SAIYA.Models;
using SAIYA.Systems;
using System.Reflection;
using System.Text;

namespace SAIYA
{
    public class Bot
    {
        // DISCORD
        public static DiscordClient Client { get; private set; }
        public static SlashCommandsExtension SlashCommands { get; private set; }
        public static CommandsNextExtension Commands { get; private set; }
        // DATABASE
        public static IMongoDatabase Database { get; private set; }
        public static IMongoCollection<User> Users => Database.GetCollection<User>("SAIYA_USERS");
        // OTHER
        public static Random rand { get; private set; }
        public static HttpClient httpClient { get; private set; }
        // CONFIG
        public static ConfigJson botConfig { get; private set; }

        // PROPERTIES
        public static DiscordEmoji CreditEmoji => Utilities.GetEmojiFromWarehouse(Client, "flarin", "💰");

        public Bot() => RunAsync().GetAwaiter().GetResult();
        private async Task RunAsync()
        {
            Console.WriteLine("starting");
            botConfig = LoadConfig();
            rand = new Random();
            httpClient = new HttpClient();

            var config = new DiscordConfiguration
            {
                Token = botConfig.BotToken,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Information,
                Intents = DiscordIntents.All
            };
            Client = new DiscordClient(config);
            Client.Ready += OnClientReady;
            Client.MessageCreated += OnMessageCreated;
            Client.ComponentInteractionCreated += OnComponentInteract;

            // slash commands

            SlashCommands = Client.UseSlashCommands();
            SlashCommands.RegisterCommands(Assembly.GetExecutingAssembly(), botConfig.SlashCommandGuild == null ? null: ulong.Parse(botConfig.SlashCommandGuild));

            // regular commands
            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { "!s " },
                EnableDms = false,
                EnableMentionPrefix = true,
                EnableDefaultHelp = false
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands(Assembly.GetExecutingAssembly());

            // database
            var mongoClient = new MongoClient(botConfig.MongoToken);
            Database = mongoClient.GetDatabase(botConfig.DatabaseName);

            CreatureLoader.Load();
            ItemLoader.Load();
            TimerManager timer = new TimerManager();
            await WeatherManager.UpdateWeather();

            await Client.ConnectAsync();
            await Task.Delay(-1);
            await timer.SetActivity();
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
            user.Statistics.Messages++;
            await ManageUserExperience.UserXP(e, user);
            await user.Save();

            // MUST BE RUN AFTER SAVE
            await ManageEggs.EggRoll(client, e, user);
        }
        private async Task OnComponentInteract(DiscordClient c, ComponentInteractionCreateEventArgs e)
        {
            switch (e.Id)
            {
                case "fish":
                    await FishingCommands.OnFish(c, e);
                    break;
                case "aboutGeneral":
                    await HelpCommands.GoAboutGeneral(e);
                    break;
                case "aboutCreatures":
                    await HelpCommands.GoAboutCreatures(e);
                    break;
                case "aboutFishing":
                    await HelpCommands.GoAboutFishing(e);
                    break;
                case "aboutCommands":
                    await HelpCommands.GoAboutHelp(e);
                    break;
                default:
                    break;
            }
        }
    }
}