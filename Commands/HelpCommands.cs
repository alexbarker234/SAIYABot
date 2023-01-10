using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace SAIYA.Commands
{
    public class HelpCommands : ApplicationCommandModule
    {
        [SlashCommand("help", "See what I can do...")]
        public async Task Help(InteractionContext ctx)
        {
            DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder()
                .AddEmbed(helpEmbed)
                .AddComponents(generalButton.Enable(), creaturesButton.Enable(), fishingButton.Enable(), helpButton.Disable())
                .AsEphemeral(true);
            await ctx.CreateResponseAsync(builder);
        }    
        private static string GetCommandHelp(string name, string description) => $"**[/{name}](https://www.google.com \"{description}\")**\n";
        [SlashCommand("about", "Let me introduce myself")]
        [Aliases("saiya")]
        public async Task About(InteractionContext ctx)
        {
            DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder()
                .AddEmbed(aboutGeneralEmbed)
                .AddComponents(generalButton.Disable(), creaturesButton.Enable(), fishingButton.Enable(), helpButton.Enable())
                .AsEphemeral(true);
            await ctx.CreateResponseAsync(builder);
        }
        public static async Task GoAboutGeneral(ComponentInteractionCreateEventArgs e)
        {
            DiscordWebhookBuilder web = new DiscordWebhookBuilder();
            web.AddEmbed(aboutGeneralEmbed);
            web.AddComponents(generalButton.Disable(), creaturesButton.Enable(), fishingButton.Enable(), helpButton.Enable());

            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            await e.Interaction.EditOriginalResponseAsync(web, Enumerable.Empty<DiscordAttachment>());
        }
        public static async Task GoAboutCreatures(ComponentInteractionCreateEventArgs e)
        {
            DiscordWebhookBuilder web = new DiscordWebhookBuilder();
            web.AddEmbed(aboutCreaturesEmbed);
            web.AddComponents(generalButton.Enable(), creaturesButton.Disable(), fishingButton.Enable(), helpButton.Enable());

            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            await e.Interaction.EditOriginalResponseAsync(web, Enumerable.Empty<DiscordAttachment>());
        }
        public static async Task GoAboutFishing(ComponentInteractionCreateEventArgs e)
        {
            DiscordWebhookBuilder web = new DiscordWebhookBuilder();
            web.AddEmbed(aboutFishingEmbed);
            web.AddComponents(generalButton.Enable(), creaturesButton.Enable(), fishingButton.Disable(), helpButton.Enable());

            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            await e.Interaction.EditOriginalResponseAsync(web, Enumerable.Empty<DiscordAttachment>());
        }
        public static async Task GoAboutHelp(ComponentInteractionCreateEventArgs e)
        {
            DiscordWebhookBuilder web = new DiscordWebhookBuilder();
            web.AddEmbed(helpEmbed);
            web.AddComponents(generalButton.Enable(), creaturesButton.Enable(), fishingButton.Enable(), helpButton.Disable());

            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            await e.Interaction.EditOriginalResponseAsync(web, Enumerable.Empty<DiscordAttachment>());
        }
        #region button & embed definitions
        private static DiscordButtonComponent helpButton = new DiscordButtonComponent(ButtonStyle.Primary, "aboutCommands", "Commands");
        private static DiscordButtonComponent generalButton = new DiscordButtonComponent(ButtonStyle.Primary, "aboutGeneral", "General");
        private static DiscordButtonComponent creaturesButton = new DiscordButtonComponent(ButtonStyle.Primary, "aboutCreatures", "Creatures");
        private static DiscordButtonComponent fishingButton = new DiscordButtonComponent(ButtonStyle.Primary, "aboutFishing", "Fishing");

        private static DiscordEmbedBuilder aboutGeneralEmbed = new DiscordEmbedBuilder()
            .WithColor(new DiscordColor("#f945ff"))
            .WithTitle($"General")
            .WithDescription("Hello, I am S.A.I.Y.A. Created as a replacement to my predecessor HotBot. I have infinitely more potential!")
            .WithThumbnail("https://cdn.discordapp.com/attachments/1060964974450720869/1060965027416379463/pfp256.png", 100, 100);
        private static DiscordEmbedBuilder aboutCreaturesEmbed = new DiscordEmbedBuilder()
            .WithColor(new DiscordColor("#f945ff"))
            .WithTitle($"Creatures")
            .WithDescription("This world is filled with many mysterious creatures...\nYou have a chance of obtaining an egg by sending a message every 5 minutes. Some eggs are only available under certain conditions.\nCheck out a list of all creatures and conditions with **/bestiary**, and view your current eggs and creatures with **/eggs** and **/creatures**")
            .WithThumbnail("https://cdn.discordapp.com/attachments/1060964974450720869/1060965027416379463/pfp256.png", 100, 100);
        private static DiscordEmbedBuilder aboutFishingEmbed = new DiscordEmbedBuilder()
            .WithColor(new DiscordColor("#f945ff"))
            .WithTitle($"Fishing")
            .WithDescription("Head on down to your own person pond with **/fish**\nYou may only fish every 3 seconds, if only your human body could fish that fast in real life...\n\nFish can be sold in the shop with **/sell** or **/sellall fish**")
            .WithThumbnail("https://cdn.discordapp.com/attachments/1060964974450720869/1060965027416379463/pfp256.png", 100, 100);
        private static DiscordEmbedBuilder helpEmbed => new DiscordEmbedBuilder()
            .WithColor(new DiscordColor("#f945ff"))
            .WithTitle($"Help")
            .WithDescription("Hello, I am S.A.I.Y.A. Created as a replacement to my predecessor HotBot. I have infinitely more potential!\n\nHover over a command to see its description")
            .WithThumbnail("https://cdn.discordapp.com/attachments/1060964974450720869/1060965027416379463/pfp256.png", 100, 100)
            .AddField(Utilities.GetEmojiFromWarehouse(Bot.Client, "Gorb", "🐷") + "Creatures",
                GetCommandHelp("eggs", "View the eggs you currently have in your hatchery") +
                GetCommandHelp("creatures", "View the creatures you own") +
                GetCommandHelp("bestiary", "View all the creatures in this world\nAlso lists their availability")
                , true)
            .AddField(Bot.CreditEmoji + "Economy",
                GetCommandHelp("sell", "sell an item") +
                GetCommandHelp("sellall", "sell all of an item type")
                , true)
            .AddField(Utilities.GetEmojiFromWarehouse(Bot.Client, "Deepjaw", "🐷") + "Fishing",
                GetCommandHelp("fish", "Open up your personal pond\n\nFishing has a 3 second cooldown")
                , true)
            .AddField(DiscordEmoji.FromUnicode("❓") + "Help",
                GetCommandHelp("help", "Take a look at everything I do!") +
                GetCommandHelp("about", "Let me introduce myself and tell you about the minigames")
                , true)
            .AddField(DiscordEmoji.FromUnicode("🔥") + "Other",
                GetCommandHelp("ping", "Check if I am awake\n\nEven bots have to sleep sometimes sweetie...") +
                GetCommandHelp("inventory", "Check out what goodies you have obtained") +
                GetCommandHelp("level", "Check your current level\n\nExperience can be gained by sending a message every minute") +
                GetCommandHelp("leaderboards", "Check out the rankings in your server") +
                GetCommandHelp("weather", "Check the weather in Perth\n\nDifferent creatures come out in different weathers!")
                , true)
            .AddField(DiscordEmoji.FromUnicode("🌿") + "Garden",
              "coming soon!"
              , true)
            .AddField(DiscordEmoji.FromUnicode("🎲") + "Fun",
              "coming soon!"
              , true)
            .AddField(DiscordEmoji.FromUnicode("💁") + "User",
              "coming soon!"
              , true)
            .AddField(DiscordEmoji.FromUnicode("⚙️") + "Settings",
              "coming soon!"
              , true);
        #endregion
       
    }
}
