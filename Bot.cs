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
using System.Linq.Expressions;
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
            if (botConfig.SlashCommandGuild != null)
            {
                SlashCommands.RegisterCommands<RemoveDuplicateCommands>(); // remove duplicate thingos
                SlashCommands.RegisterCommands(Assembly.GetExecutingAssembly(), botConfig.SlashCommandGuild);
            }
            else SlashCommands.RegisterCommands(Assembly.GetExecutingAssembly(), null);

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
            Console.WriteLine($"Connected to mongoDB {botConfig.DatabaseName}");

            await InitialiseAllUsers();

            CreatureLoader.Load();
            ItemLoader.Load();
            HelpCommands.LoadTabs();
            TimerManager timer = new TimerManager();
            PingServer.CreateListener();

            await WeatherManager.UpdateWeather();

            await Client.ConnectAsync();
            await Task.Delay(-1);
            await TimerManager.SetActivity();
        }
        private class RemoveDuplicateCommands : ApplicationCommandModule { }
        private ConfigJson LoadConfig()
        {
            var fileContents = string.Empty;

#if DEBUG
            string file = "configDEV.json";
#else
            string file = "config.json";
#endif
            using (var sr = new StreamReader(file, new UTF8Encoding(false)))
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
            user.DiscordStatistics.Messages++;
            await ManageUserExperience.UserXP(e, user);
            await user.Save();

            // MUST BE RUN AFTER SAVE
            await ManageEggs.EggRoll(client, e, user);
        }
        private async Task InitialiseAllUsers()
        {
            await InitialiseUsersField(x => x.Creatures, new DatabaseCreature[0]);
            await InitialiseUsersField(x => x.Eggs, new DatabaseEgg[0]);
            await InitialiseUsersField(x => x.Inventory, new DatabaseInventoryItem[0]);
            await InitialiseUsersField(x => x.Eggs, new DatabaseEgg[0]);
            await InitialiseUsersField(x => x.DiscordStatistics, new UserDiscordStats());
            await InitialiseUsersField(x => x.Garden, new Garden());
            Console.WriteLine("Users initialised");
        }
        private async Task InitialiseUsersField(Expression<Func<User, object>> field, object defaultValue) => await Users.UpdateManyAsync(Builders<User>.Filter.Exists(field, false), Builders<User>.Update.Set(field, defaultValue));
        private Task OnComponentInteract(DiscordClient c, ComponentInteractionCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (HelpCommands.helpTabs.ContainsKey(e.Id))
                {
                    await HelpCommands.GoToHelpTab(e);
                    return;
                }
                switch (e.Id)
                {
                    case "fish":
                        await FishingCommands.OnFish(e);
                        break;
                    default:
                        Console.WriteLine("unknown component reaction: " + e.Id);
                        break;
                }
            });
            return Task.CompletedTask;
        }
    }
}