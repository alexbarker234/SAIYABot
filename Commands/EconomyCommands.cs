using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MongoDB.Driver;
using SAIYA.Content.Items;
using SAIYA.Entities;
using SAIYA.Models;
using SAIYA.Systems;

namespace SAIYA.Commands
{
    public class EconomyCommands : ApplicationCommandModule
    {
        [SlashCommand("sell", "sell an item")]
        public async Task Sell(InteractionContext ctx, [Autocomplete(typeof(SellAutocomplete))][Option("Item", "Select the item to sell")] string itemName, [Option("Amount", "The amount to sell. Enter -1 to sell all")] double amountD = 1)
        {
            int amount = (int)amountD;
            var user = await User.GetOrCreateUser(ctx.User.Id, ctx.Guild.Id);
            DatabaseInventoryItem item = user.Inventory.FirstOrDefault(x => x.Name == itemName);
            if (item == null)
            {
                await ctx.CreateResponseAsync(itemName + " was not found in your inventory", true);
                return;
            }
            if (amount == -1) amount = item.Count;
            amount = await user.RemoveFromInventory(item, amount, true);
            int credits = amount * item.Item.Price;
            await user.AddCredits(credits);

            await ctx.CreateResponseAsync($"Successfully sold {amount} {itemName.Plural()} for {Bot.CreditEmoji}{credits}", true);
        }
        [SlashCommand("buy", "buy an item")]
        public async Task Buy(InteractionContext ctx, [Autocomplete(typeof(BuyAutocomplete))][Option("Item", "Select the item to buy")] string itemName, [Option("Amount", "The amount to buy.")] double amountD = 1)
        {
            int amount = (int)amountD;
            var user = await User.GetOrCreateUser(ctx.User.Id, ctx.Guild.Id);

            int cost = ItemLoader.items[itemName].Price * amount;
            if (user.Credits < cost)
            {
                await ctx.CreateResponseAsync($"You don't have enough {Bot.CreditEmoji} credits", true);
                return;
            }
            var update = Builders<User>.Update.Inc(x => x.Credits, -cost);
            user.AddToInventoryDefinition(itemName, amount, ref update, true);
            await Bot.Users.UpdateOneAsync(x => x.UserID == user.UserID && x.GuildID == user.GuildID, update);
            await ctx.CreateResponseAsync($"Successfully bought {amount} {itemName.Plural()} for {Bot.CreditEmoji}{cost}", true);
        }
        [SlashCommand("sellall", "sell all of a type")]
        public async Task SellAll(InteractionContext ctx,
            [ChoiceProvider(typeof(SellOption))]
            [Option("Category", "What to sell")] double category)
        {
            var user = await User.GetOrCreateUser(ctx.User.Id, ctx.Guild.Id);

            string message = "Category doesn't exist";
            if (category == (int)SellCategory.Fish) message = await TrySellAll(user, ItemTag.Fish, "fish");
            else if (category == (int)SellCategory.Plants) message = await TrySellAll(user, ItemTag.Plant, "plants");

            await ctx.CreateResponseAsync(message, true);
        }
        private async Task<string> TrySellAll(User user, ItemTag tag, string pluralNoun)
        {
            List<DatabaseInventoryItem> items = user.Inventory.Where(x => x.Item.Tag == tag && x.Count != 0).ToList();
            if (items.Count == 0) return $"You have no {pluralNoun} to sell!";
            int totalCredits = 0;
            foreach (DatabaseInventoryItem item in items)
            {
                int amount = await user.RemoveFromInventory(item, item.Count, true);
                totalCredits += amount * item.Item.Price;
            }
            await user.AddCredits(totalCredits);

            return $"Successfully sold {Bot.CreditEmoji}{totalCredits} worth of {pluralNoun}";
        }
        [SlashCommand("balance", "check your balance")]
        public async Task Balance(InteractionContext ctx)
        {
            var user = await User.GetOrCreateUser(ctx.User.Id, ctx.Guild.Id);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = $"{ctx.Member.DisplayName}'s Bank Account",
                Description = $"**Current Credits:** {Bot.CreditEmoji}{user.Credits}\n**Lifetime Credits:** {Bot.CreditEmoji}{user.Statistics.LifetimeCredits}"
            };

            await ctx.CreateResponseAsync(embed, true);
        }
        public enum SellCategory : int
        {
            Fish,
            Plants
        }
    }
}
