using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using MongoDB.Driver;
using SAIYA.Creatures;
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

        }
    }
}
