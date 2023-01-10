using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using MongoDB.Driver;
using SAIYA.Content.Creatures;
using SAIYA.Content.Items;
using SAIYA.Entities;
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
            [ChoiceProvider(typeof(LeaderboardOption))]
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
                foreach (User user in userList)
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

            GetItemListText(user, ItemTag.Fish, out string fishText, out string fishValues);
            GetItemListText(user, ItemTag.Seed, out string seedText, out string seedValues);
            GetItemListText(user, ItemTag.Plant, out string plantText, out string plantValues);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#c252de"))
                .WithTitle("Inventory");

            if (fishText != "")
            {
                embed.AddField("Fish", fishText, true);
                embed.AddField("Fish Values", fishValues, true);
            }
            if (seedText != "")
                embed.AddField("Seeds", seedText, false);
            if (plantText != "")
                embed.AddField("Plants", plantText, false);


            await ctx.CreateResponseAsync(embed, true);
        }
        private void GetItemListText(User user, ItemTag tag, out string main, out string values) => GetItemListText(user.Inventory.Where(x => x.Item?.Tag == tag && x.Count != 0).ToList(), out main, out values);
        private void GetItemListText(List<DatabaseInventoryItem> list, out string main, out string values)
        {
            main = "";
            values = "";
            foreach (DatabaseInventoryItem item in list)
            {
                if (ItemLoader.items.TryGetValue(item.Name, out var itemData))
                {
                    string emoji = Utilities.TryGetEmojiFromWarehouse(Bot.Client, item.Name.Replace(" ", ""), out var emojiOut) ? emojiOut : "";
                    main += $"{emoji}{item.Name}: ***{item.Count}***\n";
                    values += $"{Bot.CreditEmoji}{itemData.Price}\n";
                }
                else Console.WriteLine("error getting item: " + item.Name);
            }
        }
        [SlashCommand("statistics", "check your statistics")]
        public async Task Statistics(InteractionContext ctx)
        {
            var user = await User.GetOrCreateUser(ctx.User.Id, ctx.Guild.Id);

            string titles =
                $"**Lifetime Credits:**\n" +
                $"**Items Sold:**\n" +
                $"**Items Bought:**\n" +
                $"**Fish Caught:**\n" +
                $"**Times Fished:**\n" +
                $"**Total Messages:**";
            string values = 
                $"{Bot.CreditEmoji}{user.DiscordStatistics.LifetimeCredits}\n" +
                $"{user.DiscordStatistics.ItemsSold}\n" +
                $"{user.DiscordStatistics.ItemsBought}\n" +
                $"{user.DiscordStatistics.FishCaught}\n" +
                $"{user.DiscordStatistics.TimesFished}\n" +
                $"{user.DiscordStatistics.Messages}";
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
            embed.WithTitle($"{ctx.Member.DisplayName}'s Bank Account");
            embed.AddField("Statistic", titles, true);
            embed.AddField("Value", values, true);
            await ctx.CreateResponseAsync(embed, true);
        }
    }
}
