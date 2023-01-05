using DSharpPlus;
using DSharpPlus.Entities;
using Emzi0767;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SAIYA
{
    public static class Utilities
    {
        private static readonly ulong[] warehouses = new ulong[] { 907483410258337803, 907492571503292457, 862206016510361600, 917654307648708609 };
        public static DiscordEmoji GetEmojiFromWarehouse(DiscordClient client, string name, string defaultEmoji)
        {
            if (TryGetEmojiFromWarehouse(client, name, out DiscordEmoji emojiOut)) return emojiOut;
            return DiscordEmoji.FromUnicode(defaultEmoji);
        }
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
        public static void WriteLineColor(string text, ConsoleColor color)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = oldColor;
        }
        public static string ScrambleString(string word)
        {
            string output = "";
            int amount = 13;
            for (var i = 0; i < word.Length; i++)
            {
                var c = word[i];
                // uppercase letters
                if (c >= 'A' && c <= 'Z')
                {
                    c = (char)(((c - 'A' + amount) % 26) + 'A');
                }

                // lowercase letters
                else if (c >= 'a' && c <= 'z')
                {
                    c = (char)(((c - 'a' + amount) % 26) + 'a');
                }
                output += c;
            }
            return output;
        }
        private static TimeZoneInfo WATimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Australia Standard Time");
        public static DateTime GetWATime => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, WATimeZone);
    }
}
