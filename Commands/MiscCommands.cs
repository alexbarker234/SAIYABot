using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MongoDB.Driver;
using SAIYA.Creatures;
using SAIYA.Models;
using System.Drawing;
using System.Runtime.Versioning;

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
        [SlashCommand("testcreature", "test creatures")]
        public async Task TestCreature(InteractionContext ctx)
        {
            Creature creature = CreatureLoader.creatures[0];

            using (var fs = new FileStream(creature.CreatureTexture, FileMode.Open, FileAccess.Read))
            {
                DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder();
                builder.AddFile($"image.png", fs);
                builder.AddEmbed(new DiscordEmbedBuilder()
                {
                    Title = $"{creature.Name}",
                    Description = $"{creature.Description}",
                    ImageUrl = "attachment://image.png"
                }).AsEphemeral(true);
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, builder);
            }
        }
        [SlashCommand("eggs", "view your eggs")]
        [SupportedOSPlatform("windows")]
        public async Task Eggs(InteractionContext ctx)
        {
            var user = await User.GetOrCreateUser(ctx.User.Id, ctx.Guild.Id);

            if (user.Eggs.Length == 0)
            {
                await ctx.CreateResponseAsync("You have no eggs", true);
                return;
            }
            using (var ms = new MemoryStream())
            {
                // DRAW EGGS
                int maxHeight = 0;
                int maxWidth = 0;
                for (int i = 0; i < user.Eggs.Length; i++)
                {
                    Image eggimage = Image.FromFile(user.Eggs[i].Creature.EggTexture);
                    if (eggimage.Width > maxWidth) maxWidth = eggimage.Width;
                    if (eggimage.Height > maxHeight) maxHeight = eggimage.Height;
                }
                int eggCanvasWidth = maxWidth + 10; 

                Bitmap b = new Bitmap(eggCanvasWidth * user.Eggs.Length, maxHeight);
                using (Graphics g = Graphics.FromImage(b))
                {
                    int x = eggCanvasWidth / 2; 
                    for (int i = 0; i < user.Eggs.Length; i++)
                    {
                        Image eggimage = Image.FromFile(user.Eggs[i].Creature.EggTexture);
                        g.DrawImage(eggimage, x - eggimage.Width / 2, maxHeight - eggimage.Height, eggimage.Width, eggimage.Height);
                        x += eggCanvasWidth;
                    }
                }
                b.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);
                b.Dispose();

                // ACTUAL MESSAGE
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                  .WithColor(new DiscordColor("#eb98de"))
                  .WithTitle($"Eggs")
                  .WithImageUrl("attachment://image.png");

                for (int i = 0; i < user.Eggs.Length; i++)
                {
                    Creature egg = user.Eggs[i].Creature;
                    if (egg == null) continue;

                    int sinceObtained = (int)(DateTime.UtcNow.Subtract(user.Eggs[i].DateObtained).TotalSeconds);
                    int untilHatchSeconds = egg.HatchTime - sinceObtained;
                    string willHatch = $"<t:{DateTime.UtcNow.ToElapsedSeconds() + untilHatchSeconds}:f>";
                    string untilHatch = Utilities.ToCountdown(untilHatchSeconds);

                    embed.AddField($"Egg {i + 1} ", $"Hatches in {untilHatch} at\n{willHatch}\n", true);
                }
                DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder();
                builder.AddFile($"image.png", ms);
                builder.AddEmbed(embed).AsEphemeral(true);
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, builder);
            }
        }
    }
    public static class Extensions
    {
        public static string ToPing(this ulong obj) => $"<@{obj}>";
        public static int ToElapsedSeconds(this DateTime time) => (int)(time.Subtract(DateTime.UnixEpoch).TotalSeconds);
    }
}
