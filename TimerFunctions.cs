using DSharpPlus;
using DSharpPlus.Entities;
using MongoDB.Driver;
using SAIYA.Content.Plants;
using SAIYA.Models;
using SAIYA.Systems;

namespace SAIYA
{
    public class TimerManager
    {
        private readonly PeriodicTimer timer;
        private readonly Task timerTask;
        private readonly TimeSpan repeatDelay;
        private int secondsElapsed = 0;

        private int activityChangeDelay = (int)TimeSpan.FromHours(1).TotalSeconds;
        public TimerManager()
        {
            repeatDelay = TimeSpan.FromSeconds(10);
            timer = new PeriodicTimer(repeatDelay);
            timerTask = Do();
        }
        private async Task Do()
        {
            while (await timer.WaitForNextTickAsync())
            {
                if (secondsElapsed % activityChangeDelay == 0) await SetActivity();

                secondsElapsed += (int)repeatDelay.TotalSeconds;

                var userList = await Bot.Users.Find(p => true).ToListAsync();
                foreach (User user in userList)
                {
                    await EggHatching(user);
                    if (secondsElapsed % 60 == 0) await ManageGarden(user);
                }
                if (secondsElapsed % 60 == 0)
                {
                    await WeatherManager.UpdateWeather();
                }
            }
        }
        private async Task EggHatching(User user)
        {
            List<DatabaseEgg> toHatch = new();
            foreach (DatabaseEgg egg in user.Eggs)
            {
                if (egg.SecondsUntilHatch <= 0)
                {
                    toHatch.Add(egg);
                    Utilities.WriteLineColor($"{user.UserID}'s {egg.Name} hatched", ConsoleColor.Green);
                }
            }
            foreach (DatabaseEgg egg in toHatch) await user.HatchEgg(egg);
        }
        private async Task ManageGarden(User user)
        {
            for (int i = 0; i < user.Garden.Plants.Length; i++)
            {
                DatabasePlant plantDB = user.Garden.Plants[i];
                if (plantDB.Empty) continue;
                if (plantDB.Plant == null) continue;

                UserStats stats = user.CalculateStats();

                int waterRate = (int)(plantDB.Plant.WaterRate.TotalSeconds * (1 + (1 - stats.gardenWaterRetention)));
                if (DateTime.UtcNow.Subtract(plantDB.LastWatered).TotalSeconds > waterRate)
                {
                    int timeUnwatered = plantDB.GrowthDelay + (int)(DateTime.UtcNow - plantDB.LastUnwateredUpdate).TotalSeconds;

                    UpdateDefinition<User> update = null;
                    if (timeUnwatered > TimeSpan.FromDays(3).TotalSeconds)
                        update = Builders<User>.Update.Set(x => x.Garden.Plants[i], DatabasePlant.None);
                    else
                        update = Builders<User>.Update.Set(x => x.Garden.Plants[i].GrowthDelay, timeUnwatered);
                    await Bot.Users.UpdateOneAsync(x => x.UserID == user.UserID && x.GuildID == user.GuildID, update);
                }
            }
        }
        private List<DiscordActivity> activities = new()
        {
            new DiscordActivity("humans be small", ActivityType.Watching),
            new DiscordActivity("with your mind", ActivityType.Playing),
            new DiscordActivity("you", ActivityType.ListeningTo),
        };
        public async Task SetActivity() => await Bot.Client.UpdateStatusAsync(Bot.rand.Next(activities));
    }
}
