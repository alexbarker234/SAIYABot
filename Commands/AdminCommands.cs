using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MongoDB.Driver;
using SAIYA.Content.Creatures;
using SAIYA.Models;
using SAIYA.Systems;

namespace SAIYA.Commands
{
    public class AdminCommands : BaseCommandModule
    {
        [Command("testcreature")]
        public async Task TestCreature(CommandContext ctx)
        {
            if (ctx.User.Id != 283182274474672128) return;

            Creature creature = CreatureLoader.creatures.Values.ToList()[0];

            using (var fs = new FileStream(creature.CreatureTexture, FileMode.Open, FileAccess.Read))
            {
                DiscordMessageBuilder builder = new DiscordMessageBuilder();
                builder.AddFile($"image.png", fs);
                builder.AddEmbed(new DiscordEmbedBuilder()
                {
                    Title = $"{creature.Name}",
                    Description = $"{creature.Description}",
                    ImageUrl = "attachment://image.png"
                });
                await ctx.Channel.SendMessageAsync(builder);
            }
        }

        [Command("getegg")]
        public async Task GiveEgg(CommandContext ctx)
        {
            if (ctx.User.Id != 283182274474672128) return;
            var cmdUser = await User.GetOrCreateUser(ctx.User.Id, ctx.Guild.Id);

            // give egg that takes 2 mins to hatch
            Creature random = Bot.rand.Next(CreatureLoader.creatures.Values.ToList());
            DatabaseEgg egg = new DatabaseEgg { Name = random.Name, DateObtained = DateTime.Now.AddSeconds(-random.HatchTime + 120) };
            await Bot.Users.UpdateOneAsync(user => user.UserID == cmdUser.UserID && user.GuildID == cmdUser.GuildID, Builders<User>.Update.Push(x => x.Eggs, egg));

            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("👍"));
        }
        [Command("potion")]
        public async Task Potion(CommandContext ctx)
        {
            if (ctx.User.Id != 283182274474672128) return;

            int test = 0;
            DiscordMessage msg = await ctx.Message.Channel.SendMessageAsync("test " + test);
            PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync())
            {
                test++;
                await msg.ModifyAsync("test " + test);
                if (test > 10) return;
            }
            timer.Dispose();
        }
    }
}
