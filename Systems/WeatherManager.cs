using Newtonsoft.Json;
using SAIYA.Models;
using System.Net;
using System.Text.RegularExpressions;
using static SAIYA.Models.WeatherModel;

namespace SAIYA.Systems
{
    public static class WeatherManager
    {
        public static string Weather { get; private set; }
        public static string WeatherDescription { get; private set; }
        public static double Humidity { get; private set; }
        public static double Temperature { get; private set; }
        public static double WindSpeedMPS { get; private set; }
        public static double WindSpeedKMH => WindSpeedMPS * 3.6;
        /// <summary> As a percentage </summary>
        public static double Clouds { get; private set; }
        public static MoonPhase CurrentMoonPhase { get; private set; }
        public static string CurrentMoonPhaseString
        {
            get
            {
                string output = "";
                string[] parts = Regex.Split(CurrentMoonPhase.ToString(), @"(?=[A-Z])");
                foreach (string s in parts) output += s + " ";
                return output;
            }
        }
        public static bool IsRaining => Weather == "Rain" || Weather == "Drizzle";
        public static async Task UpdateWeather()
        {
            var url = "http://api.openweathermap.org/data/2.5/weather?q=Perth&appid=" + Bot.botConfig.WeatherToken;

            using var response = await Bot.httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string result = await response.Content.ReadAsStringAsync();

            if (result != null)
            {
                WeatherResponse model = JsonConvert.DeserializeObject<WeatherResponse>(result);
                Weather = model.Weather[0].Main;
                WeatherDescription = model.Weather[0].Description;
                Temperature = model.Main.Temperature - 273.15;
                WindSpeedMPS = model.Wind.Speed;
                Clouds = model.Clouds.All / 100.0;
                Humidity = model.Main.humidity / 100.0;
                CurrentMoonPhase = CalculateMoonPhase();
            }
        }
        private static MoonPhase CalculateMoonPhase()
        {
            int day = DateTime.Now.Day;
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            double c = 0;
            double e = 0;
            double jd = 0;
            double b = 0;

            if (month < 3)
            {
                year--;
                month += 12;
            }

            ++month;
            c = 365.25 * year;
            e = 30.6 * month;
            jd = c + e + day - 694039.09; // jd is total days elapsed
            jd /= 29.5305882; // divide by the moon cycle
            b = Math.Floor(jd); // int(jd) -> b, take integer part of jd
            jd -= b; // subtract integer part to leave fractional part of original jd
            b = Math.Round(jd * 8); // scale fraction from 0-8 and round

            if (b >= 8) b = 0; // 0 and 8 are the same so turn 8 into 0
            return (MoonPhase)b;
        }

        public enum MoonPhase : int
        {
            NewMoon,
            WaxingCrescentMoon,
            QuarterMoon,
            WaxingGibbousMoon,
            FullMoon,
            WaningGibbousMoon,
            LastQuarterMoon,
            WaningCrescentMoon
        }
    }
}
