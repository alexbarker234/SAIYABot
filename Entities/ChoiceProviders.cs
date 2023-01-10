using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using SAIYA.Content.Items;
using SAIYA.Models;
using static SAIYA.Commands.EconomyCommands;
using static SAIYA.Commands.MiscCommands;

namespace SAIYA.Entities
{
#pragma warning disable CS1998

    // this will break if we reach 22+ (?) plants so use PlantProvider
    public class PlantOption : IChoiceProvider
    {
        public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
            => ItemLoader.plants.Values.Select(x => new DiscordApplicationCommandOptionChoice(x.Name, x.Name));
    }
    public class PlantAutocomplete : IAutocompleteProvider
    {
        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            var user = await User.GetOrCreateUser(ctx.User.Id, ctx.Guild.Id);
            return user.Inventory.Where(x => x.Item.Tag == ItemTag.Seed && x.Count > 0).Select(x => new DiscordAutoCompleteChoice(x.Name, x.Name)).ToList();
            //return ItemLoader.plants.Values.Select(x => new DiscordAutoCompleteChoice(x.Name, x.Name));
        }
    }
    public class SellOption : IChoiceProvider
    {
        public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
            => new DiscordApplicationCommandOptionChoice[] {
                    new DiscordApplicationCommandOptionChoice("Fish", (int)SellCategory.Fish),
                };
    }
    public class LeaderboardOption : IChoiceProvider
    {
        public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
            => new DiscordApplicationCommandOptionChoice[] {
                    new DiscordApplicationCommandOptionChoice("Levels", (int)LeaderboardCategory.Levels),
                    new DiscordApplicationCommandOptionChoice("Total Creatures", (int)LeaderboardCategory.CreaturesTotal),
                    new DiscordApplicationCommandOptionChoice("Bestiary Completion", (int)LeaderboardCategory.CreatureCompletion),
                };
    }

#pragma warning restore CS1998
}
