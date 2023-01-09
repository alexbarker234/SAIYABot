using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using SAIYA.Content.Creatures;
using SAIYA.Models;
using SAIYA.Systems;
using System.Drawing;
using System.Runtime.Versioning;

namespace SAIYA.Commands
{
    public class CreatureCommands : ApplicationCommandModule
    {
        [SlashCommand("eggs", "View your eggs")]
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

                    int untilHatchSeconds = user.Eggs[i].SecondsUntilHatch;
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
        [SlashCommand("creatures", "View your creatures")]
        public async Task Creatures(InteractionContext ctx)
        {
            var user = await User.GetOrCreateUser(ctx.User.Id, ctx.Guild.Id);

            var creatureText = "";
            var total = 0;
            foreach (DatabaseCreature creatureItem in user.Creatures)
            {
                if (Utilities.TryGetEmojiFromWarehouse(ctx.Client, creatureItem.Name, out DiscordEmoji discordEmoji))
                {
                    creatureText += discordEmoji;
                }
                creatureText += $" {creatureItem.Name}: ***{creatureItem.Count}***\n";
                total += creatureItem.Count;
            }

            if (creatureText == "") creatureText = "lonely...";
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#f0c862"))
                .WithTitle(ctx.Member.DisplayName + "'s creatures")
                .AddField("Creatures", creatureText, true)
                .AddField("Total", total.ToString(), true);
            await ctx.CreateResponseAsync(embed, true);
        }
        [SlashCommand("bestiary", "View all creatures")]
        public async Task Bestiary(InteractionContext ctx)
        {
            var user = await User.GetOrCreateUser(ctx.User.Id, ctx.Guild.Id);
            List<string> creatureText = new() { "" };
            List<string> creaturesGot = new();
            foreach (DatabaseCreature creatureItem in user.Creatures) creaturesGot.Add(creatureItem.Name);

            foreach (Creature creature in CreatureLoader.creatures.Values)
            {
                var emojiName = Utilities.ScrambleString(creature.Name) + "Black";
                var creatureName = "?????";
                if (creaturesGot.Contains(creature.Name)) creatureName = emojiName = creature.Name;

                var emoji = "";
                if (Utilities.TryGetEmojiFromWarehouse(ctx.Client, emojiName, out DiscordEmoji discordEmoji)) emoji += discordEmoji;

                var available = creature.Weight(user) != 0;
                string availableEmoji = available ? Utilities.GetEmojiFromWarehouse(ctx.Client, "check", "✅").ToString() : "❌";
                var line = $"{emoji} {availableEmoji} **{creatureName}**: {creature.Requirements}\n";

                // fields cant be longer than 1024
                if ((creatureText.Last() + line).Length > 1024) creatureText.Add("");
                creatureText[creatureText.Count - 1] += line;
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithColor(new DiscordColor("#f0c862"))
            .WithTitle("Bestiary");

            foreach (string text in creatureText)
                embed.AddField("Creatures", text, true);

            await ctx.CreateResponseAsync(embed, true);
        }
    }
}
