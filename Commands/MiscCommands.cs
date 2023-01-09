using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using MongoDB.Driver;
using SAIYA.Content.Creatures;
using SAIYA.Content.Items;
using SAIYA.Models;
using SAIYA.Systems;

namespace SAIYA.Commands
{
    public class MiscCommands : ApplicationCommandModule
    {
        [SlashCommand("ping", "ping bot")]
        public async Task Ping(InteractionContext ctx)
        {
            Console.WriteLine("pinged");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Hello, darling :)").AsEphemeral(true));
        }
        #region leaderboards
        [SlashCommand("leaderboards", "check the level leaderboards")]
        public async Task Leaderboards(InteractionContext ctx,
            [ChoiceProvider(typeof(leaderboardOption))]
            [Option("Category", "Which leaderboard to view")] double category)
        {
            string title = "";
            string users = "";
            string details = "";
            List<User> userList = await Bot.Users.Find(p => p.GuildID == ctx.Guild.Id).ToListAsync();
            if (category == (int)LeaderboardCategory.Levels)
            {
                title = "Experience";
                userList.Sort((User x, User y) => -x.TotalExperience.CompareTo(y.TotalExperience));

                int rank = 1;
                foreach (User user in userList)
                {
                    users += $"**{rank++}:** {user.UserID.ToPing()}:\n";
                    details += $"Level {user.Level} {user.Experience}/{user.ExperienceRequired}xp\n";
                }
            }
            else if (category == (int)LeaderboardCategory.CreaturesTotal)
            {
                title = "Creature";
                userList = userList.Where(x => x.Creatures.Length != 0).ToList();
                userList.Sort((User x, User y) => -x.TotalCreatures.CompareTo(y.TotalCreatures));

                users += $"**Total Creatures:** {userList.Sum(x => x.TotalCreatures)}\n\n";
                details += "---\n\n";

                int rank = 1;
                foreach (User user in userList)
                {
                    users += $"**{rank++}:** {user.UserID.ToPing()}:\n";
                    details += $"{user.TotalCreatures} creatures\n";
                }
            }
            else if (category == (int)LeaderboardCategory.CreatureCompletion)
            {
                title = "Bestiary Completion";
                userList = userList.Where(x => x.Creatures.Length != 0).ToList();
                userList.Sort((User x, User y) => -x.BestiaryCompletion.CompareTo(y.BestiaryCompletion));

                int rank = 1;
                foreach(User user in userList)
                {
                    users += $"**{rank++}:** {user.UserID.ToPing()}:\n";
                    details += $"{user.BestiaryCompletion}/{CreatureLoader.creatures.Count} creatures\n";
                }
            }
            if (users == "") users = "No Results";
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = $"{ctx.Guild.Name}'s {title} Leaderboard", IconUrl = ctx.Guild.IconUrl }
            }
            .AddField("User", users, true)
            .AddField("Details", details, true);

            await ctx.CreateResponseAsync(embed: embed, true);
        }
        public enum LeaderboardCategory : int
        {
            Levels,
            CreaturesTotal,
            CreatureCompletion,
        }
        public class leaderboardOption : IChoiceProvider
        {
            #pragma warning disable CS1998
            public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
            {
                return new DiscordApplicationCommandOptionChoice[] {
                    new DiscordApplicationCommandOptionChoice("Levels", (int)LeaderboardCategory.Levels),
                    new DiscordApplicationCommandOptionChoice("Total Creatures", (int)LeaderboardCategory.CreaturesTotal),
                    new DiscordApplicationCommandOptionChoice("Bestiary Completion", (int)LeaderboardCategory.CreatureCompletion),
                };
            }
            #pragma warning restore CS1998
        }

        #endregion
        [SlashCommand("level", "check your level")]
        public async Task Level(InteractionContext ctx)
        {
            var user = await User.GetOrCreateUser(ctx.User.Id, ctx.Guild.Id);

            await ctx.CreateResponseAsync(embed: new DiscordEmbedBuilder()
            {
                Title = $"{ctx.Member.DisplayName}'s level",
                Description = $"Level {user.Level} {user.Experience}/{user.ExperienceRequired}xp",

            }, true);
        }

        [SlashCommand("weather", "check the weather")]
        public async Task Weather(InteractionContext ctx)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#cce5ed"))
                .WithTitle("Current Weather")
                .AddField("⛅ Weather", $"{WeatherManager.Weather}\n'{WeatherManager.WeatherDescription.ToTitleCase()}'")
                .AddField("🌡 Temperature", WeatherManager.Temperature.ToString("0.##") + "°C")
                .AddField("💨 Wind speed", WeatherManager.WindSpeedMPS.ToString("0.##") + "m/s\n" + WeatherManager.WindSpeedKMH.ToString("0.##") + "km/h")
                .AddField("☁️ Clouds", WeatherManager.Clouds * 100 + "%")
                .AddField("💧 Humidity", WeatherManager.Humidity * 100 + "%")
                .AddField("🌙 Moon Phase", WeatherManager.CurrentMoonPhaseString);

            await ctx.CreateResponseAsync(embed);
        }
        [SlashCommand("inventory", "check your inventory")]
        public async Task Inventory(InteractionContext ctx)
        {
            var user = await User.GetOrCreateUser(ctx.User.Id, ctx.Guild.Id);

            List<DatabaseInventoryItem> fishList = user.Inventory.Where(x => x.Item.Tag == ItemTag.Fish && x.Count != 0).ToList();

            var creditEmoji = Utilities.GetEmojiFromWarehouse(ctx.Client, "flarin", "💰");

            string fishText = "";
            string fishValues = "";
            string fishValuesTotal = "";

            foreach (DatabaseInventoryItem curFish in fishList)
            {
                if (ItemLoader.fish.TryGetValue(curFish.Name, out var fish))
                {
                    string emoji = Utilities.TryGetEmojiFromWarehouse(Bot.Client, curFish.Name, out var emojiOut) ? emojiOut : "";
                    fishText += $"{emoji}{curFish.Name}: ***{curFish.Count}***\n";
                    fishValues += $"{creditEmoji}{fish.Price}\n";
                    fishValuesTotal += $"{creditEmoji}{fish.Price * curFish.Count}\n";
                }
                else Console.WriteLine("error getting fish: " + curFish.Name);
            }
            if (fishText == "") fishText = "Nothing :(";


            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#c252de"))
                .WithTitle("Inventory")
                .AddField("Fish", fishText, true);
            if (fishValues != "")
            {
                embed.AddField("Fish Values", fishValues, true);
                embed.AddField("Total", fishValuesTotal, true);
            }
            await ctx.CreateResponseAsync(embed, true);
        }
    }
}
