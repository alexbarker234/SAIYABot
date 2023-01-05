using DSharpPlus;
using MongoDB.Driver;
using SAIYA.Models;
using SAIYA.Systems;

namespace SAIYA
{
    public class BackgroundTimerFunctions
    {
        private readonly PeriodicTimer timer;
        private readonly Task timerTask;
        private readonly TimeSpan repeatDelay;
        private int secondsElapsed = 0;
        public BackgroundTimerFunctions()
        {
            repeatDelay = TimeSpan.FromSeconds(10);
            timer = new PeriodicTimer(repeatDelay);
            timerTask = Do();
        }
        private async Task Do()
        {
            while (await timer.WaitForNextTickAsync())
            {
                secondsElapsed += (int)repeatDelay.TotalSeconds;

                var users = Bot.Database.GetCollection<User>("SAIYA_USERS");
                var userList = await users.Find(p => true).ToListAsync();
                foreach (User user in userList)
                {
                    List<DatabaseEgg> toHatch = new();
                    foreach (DatabaseEgg egg in user.Eggs)
                    {
                        if (egg.SecondsUntilHatch <= 0)
                        {
                            toHatch.Add(egg);
                            Console.WriteLine("egg hatch");
                        }
                    }
                    foreach (DatabaseEgg egg in toHatch)
                    {
                        await user.HatchEgg(egg);
                    }
                }
                if (secondsElapsed % 60 == 0)
                {
                    await WeatherManager.UpdateWeather();
                }
            }
        }
    }
}
