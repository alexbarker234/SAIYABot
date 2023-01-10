using DSharpPlus.SlashCommands;
using MongoDB.Driver;
using SAIYA.Content.Items;
using SAIYA.Entities;
using SAIYA.Models;

namespace SAIYA.Commands
{
    public class EconomyCommands : ApplicationCommandModule
    { 
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
                List<DatabaseInventoryItem> items = user.Inventory.Where(x => x.Item.Tag == ItemTag.Fish && x.Count != 0).ToList();
                if (items.Count == 0)
                {
                    await ctx.CreateResponseAsync("You have no fish to sell!", true);
                    return;
                }
                int totalCredits = 0;
                foreach (DatabaseInventoryItem item in items)
                {
                    Fish fish = ItemLoader.fish[item.Name];
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

    }
}
