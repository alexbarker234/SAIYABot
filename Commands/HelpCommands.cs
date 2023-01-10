using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using System.Reflection;

namespace SAIYA.Commands
{
    public class HelpCommands : ApplicationCommandModule
    {
        [SlashCommand("help", "See what I can do...")]
        public async Task Help(InteractionContext ctx) => await OpenHelp(ctx, "helpCommands");
        private static string GetCommandHelp(string name, string description) => $"**[/{name}](https://www.google.com \"{description}\")**\n";
        [SlashCommand("about", "Let me introduce myself")]
        [Aliases("saiya")]
        public async Task About(InteractionContext ctx) => await OpenHelp(ctx, "helpGeneral");
        private static async Task OpenHelp(InteractionContext ctx, string tab)
        {
            HelpTab data = helpTabs[tab];

            DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder()
                .AddEmbed(data.Embed)
                .AddComponents(GetButtonList(data.Button.CustomId))
                .AsEphemeral(true);
            await ctx.CreateResponseAsync(builder);
        }
        public static async Task GoToHelpTab(ComponentInteractionCreateEventArgs e)
        {
            HelpTab data = helpTabs[e.Id];

            DiscordWebhookBuilder web = new DiscordWebhookBuilder();
            web.AddEmbed(data.Embed);
            web.AddComponents(GetButtonList(data.Button.CustomId));

            await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            await e.Interaction.EditOriginalResponseAsync(web, Enumerable.Empty<DiscordAttachment>());
        }
        public static Dictionary<string, HelpTab> helpTabs = new();
        public static void LoadTabs()
        {
            helpTabs = new();
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!type.IsAbstract && type.IsSubclassOf(typeof(HelpTab)))
                {
                    var tab = (HelpTab)Activator.CreateInstance(type, null);
                    helpTabs.Add(tab.Button.CustomId, tab);
                }
            }
        }
        private static List<DiscordButtonComponent> GetButtonList(string disabledId)
        {
            List<DiscordButtonComponent> list = new();
            foreach (DiscordButtonComponent button in helpTabs.Values.Select(x => x.Button))
            {
                list.Add(button);
                if (button.CustomId == disabledId) button.Disable();
            }
            return list;
        }
        public abstract class HelpTab
        {
            public abstract DiscordButtonComponent Button { get; }
            public abstract DiscordEmbedBuilder Embed { get; }
        }
        private class CommandsTab : HelpTab
        {
            public override DiscordButtonComponent Button => new DiscordButtonComponent(ButtonStyle.Primary, "helpCommands", "Commands");
            public override DiscordEmbedBuilder Embed =>
                new DiscordEmbedBuilder()
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
                        GetCommandHelp("sellall", "sell all of an item type") +
                        GetCommandHelp("buy", "buy an item")
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
                        GetCommandHelp("leaderboards", "Check out the rankings in your server") +
                        GetCommandHelp("weather", "Check the weather in Perth\n\nDifferent creatures come out in different weathers!")
                        , true)
                    .AddField(DiscordEmoji.FromUnicode("🌿") + "Garden",
                        GetCommandHelp("garden", "View your garden") +
                        GetCommandHelp("plant", "Plant some seeds") +
                        GetCommandHelp("harvest", "Harvest or pull up a plant") +
                        GetCommandHelp("water", "Water a plant")
                      , true)
                    .AddField(DiscordEmoji.FromUnicode("🎲") + "Fun",
                        "coming soon!"
                        , true)
                    .AddField(DiscordEmoji.FromUnicode("💁") + "User",
                        GetCommandHelp("inventory", "Check out what goodies you have obtained") +
                        GetCommandHelp("statistics", "Check your statistics") +
                        GetCommandHelp("level", "Check your current level\n\nExperience can be gained by sending a message every minute")
                        , true)
                    .AddField(DiscordEmoji.FromUnicode("⚙️") + "Settings",
                        "coming soon!"
                        , true);
        }
        private class GeneralTab : HelpTab
        {
            public override DiscordButtonComponent Button => new DiscordButtonComponent(ButtonStyle.Primary, "helpGeneral", "General");

            public override DiscordEmbedBuilder Embed => new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#f945ff"))
                .WithTitle($"General")
                .WithDescription("Hello, I am S.A.I.Y.A. Created as a replacement to my predecessor HotBot. I have infinitely more potential!")
                .WithThumbnail("https://cdn.discordapp.com/attachments/1060964974450720869/1060965027416379463/pfp256.png", 100, 100);
        }
        private class CreaturesTab : HelpTab
        {
            public override DiscordButtonComponent Button => new DiscordButtonComponent(ButtonStyle.Primary, "helpCreatures", "Creatures");

            public override DiscordEmbedBuilder Embed => new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#f945ff"))
                .WithTitle($"Creatures")
                .WithDescription("This world is filled with many mysterious creatures...\nYou have a chance of obtaining an egg by sending a message every 5 minutes. Some eggs are only available under certain conditions.\nCheck out a list of all creatures and conditions with **/bestiary**, and view your current eggs and creatures with **/eggs** and **/creatures**")
                .WithThumbnail("https://cdn.discordapp.com/attachments/1060964974450720869/1060965027416379463/pfp256.png", 100, 100);
        }
        private class FishingTab : HelpTab
        {
            public override DiscordButtonComponent Button => new DiscordButtonComponent(ButtonStyle.Primary, "helpFishing", "Fishing");

            public override DiscordEmbedBuilder Embed => new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#f945ff"))
                .WithTitle($"Fishing")
                .WithDescription("Head on down to your own person pond with **/fish**\nYou may only fish every 3 seconds, if only your human body could fish that fast in real life...\n\nFish can be sold in the shop with **/sell** or **/sellall Fish**");
        }
        private class GardenTab : HelpTab
        {
            public override DiscordButtonComponent Button => new DiscordButtonComponent(ButtonStyle.Primary, "helpGarden", "Garden");

            public override DiscordEmbedBuilder Embed => new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#f945ff"))
                .WithTitle($"Gardening")
                .WithDescription("Take some time out of your day to garden! Buy seeds from the shop with **/buy** and plant them in your garden with **/plant**. You have to keep watering your plants otherwise they will die after 3 days without water. You can harvest, or pull up a plant with **/harvest** and then sell the plants with **/sell** or **/sellall Plants**")
                .WithThumbnail("https://cdn.discordapp.com/attachments/1060964974450720869/1060965027416379463/pfp256.png", 100, 100);
        }
    }
}
