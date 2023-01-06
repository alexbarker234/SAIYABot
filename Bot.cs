using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;
using SAIYA.Commands;
using SAIYA.Creatures;
using SAIYA.Items;
using SAIYA.Models;
using SAIYA.Systems;
using System.Net;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;

namespace SAIYA
{
    public class Bot
    {
        public static DiscordClient Client { get; private set; }
        public static SlashCommandsExtension SlashCommands { get; private set; }
        public static CommandsNextExtension Commands { get; private set; }
        public static IMongoDatabase Database { get; private set; }
        public static Random rand { get; private set; }
        public static ConfigJson botConfig { get; private set; }
        public static HttpClient httpClient { get; private set; }
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
                MinimumLogLevel = LogLevel.Debug,
                Intents = DiscordIntents.All
            };
            Client = new DiscordClient(config);
            Client.Ready += OnClientReady;
            Client.MessageCreated += OnMessageCreated;
            Client.ComponentInteractionCreated += OnComponentInteract;
            

            await WeatherManager.UpdateWeather();

            // slash commands

            SlashCommands = Client.UseSlashCommands();
            SlashCommands.RegisterCommands(Assembly.GetExecutingAssembly());

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
            //Commands.RegisterCommands<AdminCommands>();



            // database
            var mongoClient = new MongoClient(botConfig.MongoToken);
            Database = mongoClient.GetDatabase("SAIYA");

            CreatureLoader.Load();
            ItemLoader.Load();
            BackgroundTimerFunctions timer = new BackgroundTimerFunctions();

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