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
            if (fishCooldowns.ContainsKey(e.User.Id) && DateTime.Now.Subtract(fishCooldowns[e.User.Id]).TotalSeconds < 3) onCD = true;
            fishCooldowns[e.User.Id] = DateTime.Now;


            var fishButtonOff = new DiscordButtonComponent(ButtonStyle.Danger, "fish", "Fish", true);
            var fishButtonOn = new DiscordButtonComponent(ButtonStyle.Primary, "fish", "Fish", false);


            try
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                // FISH
                if (!onCD)
                {
                    User user = await User.GetOrCreateUser(e.User.Id, e.Guild.Id);

                    List<Fish> fished = new();
                    if (Bot.rand.NextDouble() < 0.33)
                    {
                        if (TryChooseFish(user, out Fish fish)) fished.Add(fish);
                    }

                    // CONSTRUCT MESSAGE

                    string caughtString = "";
                    foreach (Fish fish in fished)
                    {
                        string emoji = Utilities.TryGetEmojiFromWarehouse(Bot.Client, fish.Name, out var emojiOut) ? emojiOut : "";
                        caughtString += $"{emoji}{fish.Name}\n";

                        await user.AddToInventory(new DatabaseInventoryItem(fish.Name, 1, DatabaseInventoryItem.Tags.Fish));
                    }
                    if (caughtString == "") caughtString = "Nothing :(";


                    // UPDATE USER IN DB - user object is now old
                    var users = Bot.Database.GetCollection<User>("SAIYA_USERS");
                    var update = Builders<User>.Update
                        .Inc(x => x.Statistics.FishCaught, fished.Count)
                        .Inc(x => x.Statistics.TimesFished, 1);
                    await users.UpdateOneAsync(x => x.UserID == user.UserID && x.GuildID == user.GuildID, update);


                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder(e.Message.Embeds[0]);
                    embed.ClearFields();
                    embed.AddField("You caught...", caughtString);

                    DiscordWebhookBuilder web = new DiscordWebhookBuilder();
                    web.AddEmbed(embed);
                    web.AddComponents(fishButtonOff);


                    await e.Interaction.EditOriginalResponseAsync(web, Enumerable.Empty<DiscordAttachment>());

                    // REMOVE CD
                    _ = Task.Factory.StartNew(async () =>
                    {
                        Thread.Sleep(3000);
                        web.ClearComponents();
                        web.AddComponents(fishButtonOn);
                        await e.Interaction.EditOriginalResponseAsync(web, Enumerable.Empty<DiscordAttachment>());
                    });
                }
                else
                {
                    await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder { Content = "You are on Cooldown!" }.AsEphemeral(true));
                }
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
            foreach (Fish curFish in FishLoader.fish.Values)
                weightSum += curFish.Weight(user);

            var pickPower = Bot.rand.NextDouble() * weightSum;
            foreach (Fish curFish in FishLoader.fish.Values)
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
