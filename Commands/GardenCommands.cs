using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SAIYA.Content.Items;
using SAIYA.Content.Plants;
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
                                if (layer == 0)
                                {
                                    g.DrawImage(plot, new Point(plotX, plotY));
                                    Plant plant = user.Garden.Plants[i].Plant;
                                    if (!user.Garden.Plants[i].Empty && plant != null)
                                    {
                                        Image plantImage = Image.FromFile(plant.PlantTexture);
                                        int defaultHeight = 58;
                                        g.DrawImage(plantImage, new Point(plotX, plotY - 14 + (plantImage.Height - defaultHeight)));
                                    }
                                    //g.DrawString(user.Garden.Plants[i].Name, new Font("Arial", 8), new SolidBrush(Color.Black), new PointF(plotX + 20, plotY + 10));

                                }
                                else if (layer == 1)
                                {
                                    Point barPoint = new Point(plotX + 8, plotY + 38);
                                    int barWidth = 20;
                                    g.DrawImage(bars, barPoint);

                                    int waterBarSize = barWidth;
                                    g.FillRectangle(blueBrush, new Rectangle(barPoint.X + 2, barPoint.Y + 2, waterBarSize, 4));
                                    
                                    int growthBarSize = barWidth;
                                    g.FillRectangle(greenBrush, new Rectangle(barPoint.X + 2, barPoint.Y + 8, waterBarSize, 4));
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
        [SlashCommand("plant", "plant a crop from your inventory")]
        public async Task Plant(InteractionContext ctx,
            [ChoiceProvider(typeof(PlantOption))] [Option("Plant", "Select the crop to plant")] string plantName, 
            [Option("Plot", "Select plot to plant in")] double plotD = 0)
        {
            var user = await User.GetOrCreateUser(ctx.User.Id, ctx.Guild.Id);
            if (plotD % 1 != 0)
            {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder { Description = "That plot doesn't exist" });
                return;
            }
            int plot = (int)plotD;

            if (plot == 0)
            {
                plot = user.Garden.Plants.ToList().FindIndex(x => x.Empty);

                if (plot == -1)
                {
                    await ctx.CreateResponseAsync(new DiscordEmbedBuilder { Description = "Your garden is full!" });
                    return;
                }
            }
            else
            {
                if (plot < 1 || plot > 8)
                {
                    await ctx.CreateResponseAsync(new DiscordEmbedBuilder { Description = "That plot doesn't exist" });
                    return;
                }
                plot--;

                if (!user.Garden.Plants[plot].Empty)
                {
                    await ctx.CreateResponseAsync(new DiscordEmbedBuilder { Description = "That plot is full" });
                    return;
                }
            }

            if (!ItemLoader.plants.Keys.Contains(plantName))
            {
                await ctx.CreateResponseAsync(new DiscordEmbedBuilder { Description = "That plant doesn't exist" });
                return;
            }
            var update = Builders<User>.Update.Set(x => x.Garden.Plants[(int)plot], new DatabasePlant(plantName));
            await Bot.Users.UpdateOneAsync(x => x.UserID == user.UserID && x.GuildID == user.GuildID, update);

            await ctx.CreateResponseAsync(new DiscordEmbedBuilder { Description = "Planted" });
        }

        // this will break if we reach 22+ (?) plants so use PlantProvider
        public class PlantOption : IChoiceProvider
        {
            #pragma warning disable CS1998
            public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
                => ItemLoader.plants.Values.Select(x => new DiscordApplicationCommandOptionChoice(x.Name, x.Name));
            #pragma warning restore CS1998
        }
        /*
        private class PlantProvider : IAutocompleteProvider
        {
            #pragma warning disable CS1998
            public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx) 
                => ItemLoader.plants.Values.Select(x => new DiscordAutoCompleteChoice(x.Name, x.Name));
            #pragma warning restore CS1998
        }
        */
    }
}
