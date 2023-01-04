using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIYA
{
    public static class Utilities
    {
        private static readonly ulong[] warehouses = new ulong[] { 907483410258337803, 907492571503292457, 862206016510361600, 917654307648708609 };
        public static bool TryGetEmojiFromWarehouse(DiscordClient client, string name, out DiscordEmoji emoji)
        {
            foreach (ulong guildID in warehouses)
            {
                if (!client.Guilds.ContainsKey(guildID)) continue;
                DiscordEmoji discordEmoji = client.Guilds[guildID].Emojis.Values.FirstOrDefault((DiscordEmoji emoji) => emoji.Name == name);
                if (discordEmoji != null)
                {
                    emoji = discordEmoji;
                    return true;
                }
            }
            emoji = null;
            return false;
        }
        public static string ToCountdown(int seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);

            return string.Format("{0:D1}d {1:D1}h {2:D1}m {3:D1}s",
                             t.Days,
                             t.Hours,
                             t.Minutes,
                             t.Seconds,
                             t.Milliseconds);
        }
    }
}
