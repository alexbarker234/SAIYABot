using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SAIYA.Content.Items;
using SAIYA.Content.Plants;
using SAIYA.Entities;
using SAIYA.Models;
using SAIYA.Systems;
using System.Drawing;
using System.Runtime.Versioning;

namespace SAIYA.Commands
{
    public class GardenCommands : ApplicationCommandModule
    {
        [SlashCommand("garden", "view your garden")]
        [SupportedOSPlatform("windows")]
        public async Task Garden(InteractionContext ctx, [Option("Showcase", "Whether or not to broadcast the message")] bool showcase = false)
        {
            try
            {
                var user = await User.GetOrCreateUser(ctx.User.Id, ctx.Guild.Id);
                using (var ms = new MemoryStream())
                {
                    Image gardenBase = Image.FromFile(Directory.GetCurrentDirectory() + "\\Assets\\Garden\\GardenBase.png");
                    Image plot = Image.FromFile(Directory.GetCurrentDirectory() + "\\Assets\\Garden\\Plot.png");
                    Image bars = Image.FromFile(Directory.GetCurrentDirectory() + "\\Assets\\Garden\\Bars.png");

                    SolidBrush greenBrush = new SolidBrush(Color.FromArgb(153, 229, 80));
                    SolidBrush blueBrush = new SolidBrush(Color.FromArgb(99, 155, 255));
                    Bitmap b = new Bitmap(gardenBase);
                    using (Graphics g = Graphics.FromImage(b))
                    {

                        if (user.Garden.Plants.Length < 8) Console.WriteLine(ctx.User.Username + " has an incorrect plant array");
                        for (int layer = 0; layer < 2; layer++)
                        {
                            int plotX = 30;
                            int plotY = 58;
                            for (int i = 0; i < user.Garden.Plants.Length; i++)
                            {
                                Plant plant = user.Garden.Plants[i].Plant;
                                if (layer == 0)
                                {
                                    g.DrawImage(plot, new Point(plotX, plotY));
                                    if (!user.Garden.Plants[i].Empty && plant != null)
                                    {
                                        Image plantImage = Image.FromFile(plant.PlantTexture);
                                        int defaultHeight = 58;
                                        g.DrawImage(plantImage, new Point(plotX, plotY - 14 + (plantImage.Height - defaultHeight)));
                                    }
                                    //g.DrawString(user.Garden.Plants[i].Name, new Font("Arial", 8), new SolidBrush(Color.Black), new PointF(plotX + 20, plotY + 10));

                                }
                                else if (layer == 1 && !showcase)
                                {
                                    if (!user.Garden.Plants[i].Empty && plant != null)
                                    {
                                        Point barPoint = new Point(plotX + 8, plotY + 38);
                                        int barWidth = 20;
                                        g.DrawImage(bars, barPoint);

                                        int waterBarSize = (int)(barWidth * user.Garden.Plants[i].WaterPercent(user));
                                        g.FillRectangle(blueBrush, new Rectangle(barPoint.X + 2, barPoint.Y + 2, waterBarSize, 4));

                                        double p = user.Garden.Plants[i].GrowthPercent(user).Value;
                                        int growthBarSize = (int)(barWidth * user.Garden.Plants[i].GrowthPercent(user));
                                        g.FillRectangle(greenBrush, new Rectangle(barPoint.X + 2, barPoint.Y + 8, growthBarSize, 4));
                                    }
                                }
                                plotX += 36;
                                plotY += 18;
                                if (i == 3)
                                {
                                    plotX = 122;
                                    plotY = 12;
                                }
                            }
                        }
                    }
                    b.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Seek(0, SeekOrigin.Begin);
                    b.Dispose();

                    // ACTUAL MESSAGE
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                      .WithColor(new DiscordColor("#8ced8c"))
                      .WithTitle($"{ctx.Member.DisplayName}'s Garden")
                      .WithImageUrl("attachment://image.png");
                    if (!showcase)
                    {
                        for (int i = 0; i < user.Garden.Plants.Length; i++)
                        {
                            DatabasePlant plantDB = user.Garden.Plants[i];
                            Plant plant = user.Garden.Plants[i].Plant;
                            if (plantDB.Empty) continue;

                            string harvest = $"🌿Harvest {GetTimeString(DateTime.UtcNow.AddSeconds(plantDB.SecondsUntilGrown(user).Value))}";
                            string water = $"💧Water {GetTimeString(DateTime.UtcNow.AddSeconds(plantDB.SecondsUntilWater(user).Value))}";

                            embed.AddField($"Plot {i + 1}", $"{harvest}\n{water}", true);
                        }
                    }
                    DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder();
                    builder.AddFile($"image.png", ms);
                    builder.AddEmbed(embed).AsEphemeral(!showcase);
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, builder);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        private string GetTimeString(DateTime date) => date < DateTime.UtcNow ? "now" : $"<t:{date.ToElapsedSeconds()}:R>";

        [SlashCommand("plant", "plant a crop from your inventory")]
        public async Task Plant(InteractionContext ctx, [Autocomplete(typeof(PlantAutocomplete))][Option("Seed", "Select the seed to plant")] string seedName, [Option("Plot", "Select plot to plant in")] double plotD = 0) =>
            await ctx.CreateResponseAsync(await TryPlant(ctx, seedName, plotD), true);

        /// <returns>An error or success message</returns>
        private async Task<string> TryPlant(InteractionContext ctx, string seedName, double plotD = 0)
        {
            int plot = (int)plotD;
            var user = await User.GetOrCreateUser(ctx.User.Id, ctx.Guild.Id);

            string plantName = seedName.Replace(" Seeds", "");

            // shouldnt occur
            if (!ItemLoader.plants.Keys.Contains(plantName)) return "That plant doesn't exist";
            int seedIndex = user.Inventory.ToList().FindIndex(x => x.Name == seedName && x.Count > 0);
            if (seedIndex == -1) return $"You don't have any {plantName} seeds";

            // plot value not entered - find plot to plant
            if (plotD == 0)
            {
                plotD = user.Garden.Plants.ToList().FindIndex(x => x.Empty);
                if (plotD == -1) return "Your garden is full!";
            }
            // plot value entered
            else
            {
                plot--;
                if (!user.Garden.Plants[plot].Empty) return "That plot is full";
                else if (!IsValidPlot(plot)) return "That plot doesn't exist";
            }

            var update = Builders<User>.Update.Set(x => x.Garden.Plants[plot], new DatabasePlant(plantName)).Inc(x => x.Inventory[seedIndex].Count, -1);
            await Bot.Users.UpdateOneAsync(x => x.UserID == user.UserID && x.GuildID == user.GuildID, update);

            return $"Planted {plantName} in plot {plot + 1}";
        }
        [SlashCommand("harvest", "harvest a crop from your garden")]
        public async Task Harvest(InteractionContext ctx, [Option("Plot", "Select plot to harvest")] double plotD, [Option("Force", "Whether or not to pull up ungrown crops")] bool force = false) =>
            await ctx.CreateResponseAsync(await TryHarvest(ctx, plotD, force), true);


        private async Task<string> TryHarvest(InteractionContext ctx, double plotD = 0, bool force = false)
        {
            int plot = (int)plotD - 1;
            var user = await User.GetOrCreateUser(ctx.User.Id, ctx.Guild.Id);

            DatabasePlant toHarvest = user.Garden.Plants[plot];

            if (!IsValidPlot(plot)) return "That plot doesn't exist";
            else if (toHarvest.Empty) return "That plot is empty";
            else if (!force && toHarvest.GrowthPercent(user) < 1) return "That crop isn't ready to be harvested. Set force to true to destroy this crop";

            var update = Builders<User>.Update.Set(x => x.Garden.Plants[plot], DatabasePlant.None);
            user.AddToInventoryDefinition(new DatabaseInventoryItem(toHarvest.Name, toHarvest.Plant.Yield), ref update);
            await Bot.Users.UpdateOneAsync(x => x.UserID == user.UserID && x.GuildID == user.GuildID, update);
            return (toHarvest.GrowthPercent(user) < 1 ? "Destroyed" : "Harvested") + $" {toHarvest.Name} in plot {plot + 1}";
        }
        private bool IsValidPlot(double plot) => plot % 1 == 0 && plot >= 0 && plot <= 7;
    }
}