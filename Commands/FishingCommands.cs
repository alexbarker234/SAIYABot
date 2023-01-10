using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using MongoDB.Driver;
using SAIYA.Content.Items;
using SAIYA.Models;
using SAIYA.Systems;

namespace SAIYA.Commands
{
    public class FishingCommands : ApplicationCommandModule
    {
        [SlashCommand("fish", "start fishing")]
        public async Task Fish(InteractionContext ctx)
        {
            var fishButton = new DiscordButtonComponent(ButtonStyle.Primary, "fish", "Fish", false);

            using (var fs = new FileStream(Directory.GetCurrentDirectory() + "\\Assets\\FishingScene.png", FileMode.Open, FileAccess.Read))
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#1b46e0"),
                    Title = "Fishing!",
                    ImageUrl = "attachment://image.png"
                };
                embed.AddField("Information", "Click the button to go fishing!");

                DiscordInteractionResponseBuilder regularInteraction = new DiscordInteractionResponseBuilder()
                    .AddFile("image.png", fs)
                    .AddEmbed(embed)
                    .AddComponents(fishButton)
                    .AsEphemeral(true);

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, regularInteraction);
            }
        }
        public static async Task OnFish(DiscordClient c, ComponentInteractionCreateEventArgs e)
        {
            await TryFish(e);
        }
        public static Dictionary<ulong, DateTime> fishCooldowns = new();
        private static async Task TryFish(ComponentInteractionCreateEventArgs e)
        {
            bool onCD = false;

            DateTime interactionCreated = e.Interaction.CreationTimestamp.UtcDateTime;

            int milleCD = 3000;
            if (fishCooldowns.ContainsKey(e.User.Id) && (int)interactionCreated.Subtract(fishCooldowns[e.User.Id]).TotalMilliseconds < milleCD) onCD = true;
            else fishCooldowns[e.User.Id] = interactionCreated;

            try
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder(e.Message.Embeds[0]);
                embed.ClearFields();

                if (!onCD)
                {
                    // FISH
                    User user = await User.GetOrCreateUser(e.User.Id, e.Guild.Id);

                    List<Fish> fished = new();
                    if (Bot.rand.NextDouble() < user.CalculateStats().fishChance)
                    {
                        if (TryChooseFish(user, out Fish fish)) fished.Add(fish);
                    }

                    string caughtString = "";
                    foreach (Fish fish in fished)
                    {
                        string emoji = Utilities.TryGetEmojiFromWarehouse(Bot.Client, fish.Name, out var emojiOut) ? emojiOut : "";
                        caughtString += $"{emoji}{fish.Name}\n";

                        await user.AddToInventory(new DatabaseInventoryItem(fish.Name, 1));
                    }
                    if (caughtString == "") caughtString = "Nothing :(";

                    // UPDATE USER IN DB - user object is now old
                    var update = Builders<User>.Update
                        .Inc(x => x.Statistics.FishCaught, fished.Count)
                        .Inc(x => x.Statistics.TimesFished, 1);
                    await Bot.Users.UpdateOneAsync(x => x.UserID == user.UserID && x.GuildID == user.GuildID, update);

                    embed.AddField("You caught...", caughtString);
                    embed.WithColor(new DiscordColor("#1b46e0"));
                }
                else
                {
                    embed.AddField("Cooldown", "You are on cooldown!");
                    embed.WithColor(new DiscordColor("#bf0f24"));
                }
                embed.AddField("Can fish again in: ", $"<t:{fishCooldowns[e.User.Id].AddMilliseconds(milleCD).ToElapsedSeconds()}:R>");

                DiscordWebhookBuilder web = new DiscordWebhookBuilder();
                web.AddEmbed(embed);
                web.AddComponents(e.Message.Components);

                await e.Interaction.EditOriginalResponseAsync(web, Enumerable.Empty<DiscordAttachment>());
            }
            catch (DSharpPlus.Exceptions.BadRequestException error)
            {
                Console.WriteLine(error.JsonMessage);
            }
            catch (DSharpPlus.Exceptions.NotFoundException error)
            {
                Console.WriteLine(error.JsonMessage);
            }

            /* REMOVE ATTACHMENTS
                await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, null);
                await e.Interaction.EditOriginalResponseAsync(web, Enumerable.Empty<DiscordAttachment>());
             */
        }
        public static bool TryChooseFish(User user, out Fish fish)
        {
            var weightSum = 0.0;
            foreach (Fish curFish in ItemLoader.fish.Values)
                weightSum += curFish.Weight(user);

            var pickPower = Bot.rand.NextDouble() * weightSum;
            foreach (Fish curFish in ItemLoader.fish.Values)
            {
                var weight = curFish.Weight(user);
                if (pickPower <= weight)
                {
                    fish = curFish;
                    return true;
                }
                // subtract the weight so the total is only the sum of remaining options
                pickPower -= weight;
            }
            fish = null;
            return false;
        }
    }
}
