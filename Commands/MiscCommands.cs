using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using MongoDB.Driver;
using SAIYA.Creatures;
using SAIYA.Items;
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
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("pong"));
            await Task.Delay(5000);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("pingi"));
        }
        [SlashCommand("leaderboards", "check the level leaderboards")]
        public async Task Leaderboards(InteractionContext ctx)
        {
            var users = Bot.Database.GetCollection<User>("SAIYA_USERS");

            List<User> userList = await users.Find(p => true).ToListAsync();

            userList.Sort((User x, User y) => -x.TotalExperience.CompareTo(y.TotalExperience));

            string content = $"`{ctx.Guild.Name}'s Level Leaderboard`\n";
            int rank = 1;
            userList.ForEach(user => content += $"**{rank++}:** {user.UserID.ToPing()}: Level {user.Level} {user.Experience}/{user.ExperienceRequired}xp\n");

            await ctx.CreateResponseAsync(embed: new DiscordEmbedBuilder()
            {
                Title = "Leaderboard",
                Description = content,
            }, true);
        }
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

            List<DatabaseInventoryItem> fishList = user.Inventory.Where(x => x.Tag == DatabaseInventoryItem.Tags.Fish && x.Count != 0).ToList();

            var creditEmoji = Utilities.GetEmojiFromWarehouse(ctx.Client, "flarin", "💰");

            string fishText = "";
            string fishValues = "";
            string fishValuesTotal = "";

            foreach (DatabaseInventoryItem curFish in fishList)
            {
                if (FishLoader.fish.TryGetValue(curFish.Name, out var fish))
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
        [SlashCommand("sell", "sell an item")]
        public async Task Sell(InteractionContext ctx, [Option("Item", "Select the item to sell")] string itemName)
        {
            var user = await User.GetOrCreateUser(ctx.User.Id, ctx.Guild.Id);
            DatabaseInventoryItem item = user.Inventory.FirstOrDefault(x => x.Name == itemName);
            if (item == null)
            {
                await ctx.CreateResponseAsync(itemName + " was not found in your inventory", true);
                return;
            }

        }
        [SlashCommand("sellall", "sell all of a type")]
        public async Task SellAll(InteractionContext ctx,
            [ChoiceProvider(typeof(SellOption))]
            [Option("Category", "What to sell")] double category)
        {
            var user = await User.GetOrCreateUser(ctx.User.Id, ctx.Guild.Id);

            if (category == (int)SellCategory.Fish)
            {
                List<DatabaseInventoryItem> items = user.Inventory.Where(x => x.Tag == DatabaseInventoryItem.Tags.Fish && x.Count != 0).ToList();
                if (items.Count == 0)
                {
                    await ctx.CreateResponseAsync("You have no fish to sell!", true);
                    return;
                }
                int totalCredits = 0;
                foreach (DatabaseInventoryItem item in items)
                {
                    Fish fish = FishLoader.fish[item.Name];
                    int amount = await user.RemoveFromInventory(item, item.Count);
                    totalCredits += amount * fish.Price;
                }
                await user.AddCredits(totalCredits);

                var creditEmoji = Utilities.GetEmojiFromWarehouse(ctx.Client, "flarin", "💰");
                await ctx.CreateResponseAsync($"Successfully sold {creditEmoji}{totalCredits} worth of fish", true);
            }
        }
        public enum SellCategory : int
        {
            Fish
        }
        public class SellOption : IChoiceProvider
        {
            #pragma warning disable CS1998
            public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
            {
                return new DiscordApplicationCommandOptionChoice[] {
                    new DiscordApplicationCommandOptionChoice("Fish", (int)SellCategory.Fish),
                };
            }
            #pragma warning restore CS1998
        }
    }
}
