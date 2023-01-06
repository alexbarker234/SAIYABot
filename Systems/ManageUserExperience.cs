using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SAIYA.Models;

namespace SAIYA.Systems
{
    internal class ManageUserExperience
    {
        public static async Task UserXP(MessageCreateEventArgs e, User user)
        {
            int xpCooldown = 60;
            double secondsSinceRoll = DateTime.UtcNow.Subtract(user.LastExperience).TotalSeconds;
            if (secondsSinceRoll > xpCooldown)
            {
                user.Experience += Bot.rand.Next(15, 25);
                user.LastExperience = DateTime.Now;

                List<LevelRole> levelRoles = new()
                {
                   // new LevelRole(1,1059780036451905657),
                   // new LevelRole(2,1059781806297202738),
                   // new LevelRole(3,1059781813612060674)
                };

                if (user.Experience >= user.ExperienceRequired)
                {
                    user.Experience = user.Experience - user.ExperienceRequired;
                    user.Level++;

                    var role = levelRoles.Where(x => x.Level == user.Level).Select(levelRole => e.Guild.Roles.FirstOrDefault(x => x.Key == levelRole.RoleID).Value).FirstOrDefault();
                    if (role != null)
                        await (e.Author as DiscordMember).GrantRoleAsync(role);

                    //await e.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("⬆️"));
                }
            }
        }
        private class LevelRole
        {
            public int Level { get; protected set; }
            public ulong RoleID { get; protected set; }
            public LevelRole(int level, ulong roleID)
            {
                Level = level;
                RoleID = roleID;
            }
        }
    }
}
