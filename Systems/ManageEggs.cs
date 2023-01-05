using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MongoDB.Driver;
using SAIYA.Creatures;
using SAIYA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIYA.Systems
{
    public static class ManageEggs
    {
        public static async Task EggRoll(DiscordClient client, MessageCreateEventArgs e, User user)
        {

            double secondsSinceRoll = DateTime.Now.Subtract(user.LastEggRoll).TotalSeconds;
            int maxEggs = 3;

            if (secondsSinceRoll > 300 && user.Eggs.Length < maxEggs)
            {
                Utilities.WriteLineColor($"{e.Message.Author.Username} rolled for an egg", ConsoleColor.Yellow);

                var users = Bot.Database.GetCollection<User>("SAIYA_USERS");
                users.UpdateOne(x => x.UserID == user.UserID && x.GuildID == user.GuildID, Builders<User>.Update.Set(x => x.LastEggRoll, DateTime.Now));

                double eggChance = 0.1;

                if (Bot.rand.NextDouble() < eggChance)
                {
                    if (TryChooseEgg(user, out Creature creature))
                    {
                        if (Utilities.TryGetEmojiFromWarehouse(client, creature.Name + "Egg", out DiscordEmoji emoji))
                        {
                            await e.Message.CreateReactionAsync(emoji);
                        }

                        Utilities.WriteLineColor($"{e.Message.Author.Username} obtained a {creature.Name} egg!", ConsoleColor.Green);

                        await user.AddEgg(creature);
                    }
                }
            }
        }
        private static bool TryChooseEgg(User user, out Creature creature)
        {
            var weightSum = 0.0;
            foreach (Creature curCreature in CreatureLoader.creatures)
                weightSum += curCreature.Weight(user);

            var pickPower = Bot.rand.NextDouble() * weightSum;
            foreach (Creature curCreature in CreatureLoader.creatures)
            {
                var weight = curCreature.Weight(user);
                if (pickPower <= weight)
                {
                    creature = curCreature;
                    return true;
                }
                // subtract the weight so the total is only the sum of remaining options
                pickPower -= weight;
            }
            creature = null;
            return false;
        }
    }
}
