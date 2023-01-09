using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using SAIYA.Content.Creatures;
using SAIYA.Content.Items;
using SAIYA.Content.Plants;

namespace SAIYA.Models
{
    public class User
    {
        [BsonId]
        public ObjectId id { get; protected set; }

        [BsonElement("userID")]
        public ulong UserID { get; protected set; }

        [BsonElement("guildID")]
        public ulong GuildID { get; protected set; }

        [BsonElement("lastExperience")]
        public DateTime LastExperience { get; set; }

        [BsonElement("lastEggRoll")]
        public DateTime LastEggRoll { get; set; }

        [BsonElement("experience")]
        public int Experience { get; set; }

        [BsonElement("level")]
        public int Level { get; set; }

        [BsonElement("credits")]
        public int Credits { get; set; }

        [BsonElement("creatures")]
        public DatabaseCreature[] Creatures { get; set; }

        [BsonElement("eggs")]
        public DatabaseEgg[] Eggs { get; set; }

        [BsonElement("inventory")]
        public DatabaseInventoryItem[] Inventory { get; set; }
        [BsonElement("garden")]
        public Garden Garden { get; set; }

        [BsonElement("statistics")]
        public UserStats Statistics { get; set; }

        // CALCULATED FIELDS
        [BsonIgnore]
        public int ExperienceRequired => 5 * (Level * Level) + (50 * Level) + 100;
        [BsonIgnore]
        public int LevelExperience => (int)((5.0 / 6.0) * Level * (2 * Level * Level + 27 * Level + 91));
        [BsonIgnore]
        public int TotalExperience => Experience + LevelExperience;
        [BsonIgnore]
        public int TotalCreatures => Creatures.Sum(c => c.Count);
        [BsonIgnore]
        public int BestiaryCompletion => Creatures.Where(c => c.Count != 0).ToList().Count;

        [BsonExtraElements]
        public BsonDocument CatchAll { get; set; }

        public User(ulong userID, ulong guildID)
        {
            id = ObjectId.GenerateNewId();
            UserID = userID;
            GuildID = guildID;
            Initialise();
        }
        private bool Initialise()
        {
            bool hasChanged = false;
            if (LastExperience == default) { LastExperience = DateTime.Now; hasChanged = true; }
            if (Creatures == default) { Creatures = new DatabaseCreature[0]; hasChanged = true; }
            if (Inventory == default) { Inventory = new DatabaseInventoryItem[0]; hasChanged = true; }
            if (Eggs == default) { Eggs = new DatabaseEgg[0]; hasChanged = true; }
            if (Statistics == default) { Statistics = new UserStats(); hasChanged = true; }
            if (Garden == default) { Garden = new Garden(); hasChanged = true; }
            if (Garden.Plants == default || Garden.Plants.Length != 8) { Garden.Plants = Enumerable.Repeat(DatabasePlant.None, 8).ToArray(); hasChanged = true; }
            return hasChanged;
        }
        public static async Task<User> GetOrCreateUser(ulong userID, ulong guildID)
        {
            var users = Bot.Users;

            var results = await users.FindAsync(x => x.UserID == userID && x.GuildID == guildID);
            var resultsList = results.ToList();
            User user = null;
            if (resultsList.Count == 0)
            {
                user = new User(userID, guildID);
                await users.InsertOneAsync(user);
            }
            else
            {
                if (resultsList.Count > 1)
                    Console.WriteLine($"DUPLICATE USER FOUND: {resultsList.First().UserID}");
                user = resultsList.First();
                if (user.Initialise())
                    await user.Save();
            }
            return user;
        }
        public async Task Update(string field, object newValue)
        {
            var update = Builders<User>.Update.Set(field, newValue);
            await Bot.Users.UpdateOneAsync(user => user.UserID == UserID && user.GuildID == GuildID, update);
        }

        public async Task Save() => await Bot.Users.ReplaceOneAsync(user => user.UserID == UserID && user.GuildID == GuildID, this);
        public async Task AddEgg(Creature creature)
        {
            var update = Builders<User>.Update.Push(x => x.Eggs, new DatabaseEgg { Name = creature.Name, DateObtained = DateTime.Now });
            await Bot.Users.UpdateOneAsync(user => user.UserID == UserID && user.GuildID == GuildID, update);
        }
        public async Task HatchEgg(DatabaseEgg egg)
        {
            Creature toHatch = egg.Creature;

            UpdateDefinition<User> update = Builders<User>.Update.Pull(x => x.Eggs, egg);

            int existingIndex = HasItem(Creatures, toHatch.Name);

            if (existingIndex == -1) update = update.Push(x => x.Creatures, new DatabaseCreature(toHatch.Name, 1));
            else update = update.Inc(x => x.Creatures[existingIndex].Count, 1);
            await Bot.Users.UpdateOneAsync(user => user.UserID == UserID && user.GuildID == GuildID, update);
        }
        public async Task AddToInventory(DatabaseInventoryItem item)
        {
            int existingIndex = HasItem(Inventory, item.Name);
            var update = Builders<User>.Update.Push(x => x.Inventory, item);
            if (existingIndex != -1)
                update = Builders<User>.Update.Inc(x => x.Inventory[existingIndex].Count, item.Count);

            await Bot.Users.UpdateOneAsync(user => user.UserID == UserID && user.GuildID == GuildID, update);
        }
        /// <summary> Returns the amount successfully removed </summary>
        public async Task<int> RemoveFromInventory(DatabaseInventoryItem item, int toRemove)
        {
            int index = HasItem(Inventory, item.Name);
            if (index == -1) return 0;

            toRemove = Math.Min(toRemove, Inventory[index].Count);

            var update = Builders<User>.Update.Inc(x => x.Inventory[index].Count, -toRemove);
            await Bot.Users.UpdateOneAsync(user => user.UserID == UserID && user.GuildID == GuildID, update);
            return toRemove;
        }
        public async Task AddCredits(int amount)
        {
            var update = Builders<User>.Update.Inc(x => x.Credits, amount).Inc(x => x.Statistics.LifetimeCredits, amount);
            await Bot.Users.UpdateOneAsync(user => user.UserID == UserID && user.GuildID == GuildID, update);
        }
        private int HasItem<T>(T[] array, string name) where T : DatabaseItem
        {
            for (int i = 0; i < array.Length; i++)
            {
                T creature = array[i];
                if (creature.Name == name) return i;
            }
            return -1;
        }
    }
    public class UserStats
    {
        [BsonElement("messages")]
        public int Messages { get; set; }

        [BsonElement("fishCaught")]
        public int FishCaught { get; set; }

        [BsonElement("timesFished")]
        public int TimesFished { get; set; }
        [BsonElement("lifetimeCredits")]
        public int LifetimeCredits { get; set; }
        [BsonExtraElements]
        public BsonDocument CatchAll { get; set; }
    }
    public class Garden
    {
        [BsonElement("plants")]
        public DatabasePlant[] Plants { get; set; }
    }
    public class DatabasePlant
    {
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("plantedTime")]
        public DateTime? PlantedTime { get; set; }
        [BsonElement("lastWatered")]
        public DateTime? LastWatered { get; set; }
        [BsonElement("timeUnwatered")]
        public int? TimeUnwatered { get; set; }
        [BsonElement("timeUnwateredUpdate")]
        public DateTime? TimeUnwateredUpdate { get; set; }


        [BsonIgnore]
        public bool Empty => Name == "None";

        [BsonIgnore]
        public Plant Plant => ItemLoader.plants.GetValueOrDefault(Name);

        [BsonExtraElements]
        public BsonDocument CatchAll { get; set; }
        public DatabasePlant(string name)
        {
            Name = name;
            PlantedTime = DateTime.Now;
            LastWatered = DateTime.Now;
            TimeUnwateredUpdate = DateTime.Now;
            TimeUnwatered = 0;
        }
        public static DatabasePlant None => new DatabasePlant("None") { PlantedTime = null, LastWatered = null, TimeUnwatered = null, TimeUnwateredUpdate = null };
    }
    public class DatabaseCreature : DatabaseItem
    {
        public DatabaseCreature(string name, int count)
        {
            Name = name;
            Count = count;
        }
    }
    public class DatabaseInventoryItem : DatabaseItem
    {
        [BsonIgnore]
        public Item Item => ItemLoader.items.GetValueOrDefault(Name);
        public DatabaseInventoryItem(string name, int count)
        {
            Name = name;
            Count = count;
        }
    }
    public class DatabaseItem
    {
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("count")]
        public int Count { get; set; }
        [BsonExtraElements]
        public BsonDocument CatchAll { get; set; }
    }
    public class DatabaseEgg
    {
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("obtained")]
        public DateTime DateObtained { get; set; }
        [BsonIgnore]
        public Creature Creature => CreatureLoader.creatures.GetValueOrDefault(Name);

        [BsonIgnore]
        public int SecondsSinceObtained => (int)(DateTime.UtcNow.Subtract(DateObtained).TotalSeconds);
        [BsonIgnore]
        public int SecondsUntilHatch => Creature.HatchTime - SecondsSinceObtained;
        [BsonExtraElements]
        public BsonDocument CatchAll { get; set; }
    }
}
