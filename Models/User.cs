using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using SAIYA.Creatures;
using SAIYA.Items;

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

        [BsonElement("statistics")]
        public UserStats Statistics { get; set; }

        // CALCULATED FIELDS
        [BsonIgnore]
        public int ExperienceRequired => 5 * (Level * Level) + (50 * Level) + 100;
        [BsonIgnore]
        public int LevelExperience => (5 / 6) * Level * (2 * Level * Level + 27 * Level + 91);
        [BsonIgnore]
        public int TotalExperience => Experience + LevelExperience;

        [BsonExtraElements]
        public BsonDocument CatchAll { get; set; }

        public User(ulong userID, ulong guildID)
        {
            id = ObjectId.GenerateNewId();
            UserID = userID;
            GuildID = guildID;
            Initialise();
        }
        private void Initialise()
        {
            if (LastExperience == default) LastExperience = DateTime.Now;
            if (Creatures == default) Creatures = new DatabaseCreature[0];
            if (Inventory == default) Inventory = new DatabaseInventoryItem[0];
            if (Eggs == default) Eggs = new DatabaseEgg[0];
            if (Statistics == default) Statistics = new UserStats();
        }
        public static async Task<User> GetOrCreateUser(ulong userID, ulong guildID)
        {
            var users = Bot.Database.GetCollection<User>("SAIYA_USERS");

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
                user.Initialise();
            }
            return user;
        }
        public async Task Update(string field, object newValue)
        {
            var users = Bot.Database.GetCollection<User>("SAIYA_USERS");

            var update = Builders<User>.Update.Set(field, newValue);
            await users.UpdateOneAsync(user => user.UserID == UserID && user.GuildID == GuildID, update);
        }

        public async Task Save()
        {
            var users = Bot.Database.GetCollection<User>("SAIYA_USERS");
            await users.ReplaceOneAsync(user => user.UserID == UserID && user.GuildID == GuildID, this);
        }
        public async Task AddEgg(Creature creature)
        {
            var users = Bot.Database.GetCollection<User>("SAIYA_USERS");
            var update = Builders<User>.Update.Push(x => x.Eggs, new DatabaseEgg { Name = creature.Name, DateObtained = DateTime.Now });
            await users.UpdateOneAsync(user => user.UserID == UserID && user.GuildID == GuildID, update);
        }
        public async Task HatchEgg(DatabaseEgg egg)
        {
            var users = Bot.Database.GetCollection<User>("SAIYA_USERS");
            Creature toHatch = egg.Creature;

            UpdateDefinition<User> update = Builders<User>.Update.Pull(x => x.Eggs, egg);

            int existingIndex = HasItem(Creatures, toHatch.Name);

            if (existingIndex == -1) update = update.Push(x => x.Creatures, new DatabaseCreature(toHatch.Name, 1));
            else update = update.Inc(x => x.Creatures[existingIndex].Count, 1);
            await users.UpdateOneAsync(user => user.UserID == UserID && user.GuildID == GuildID, update);
        }
        public async Task AddToInventory(DatabaseInventoryItem item)
        {
            var users = Bot.Database.GetCollection<User>("SAIYA_USERS");

            int existingIndex = HasItem(Inventory, item.Name);
            var update = Builders<User>.Update.Push(x => x.Inventory, item);
            if (existingIndex != -1)
                update = Builders<User>.Update.Inc(x => x.Inventory[existingIndex].Count, item.Count);

            await users.UpdateOneAsync(user => user.UserID == UserID && user.GuildID == GuildID, update);
        }
        /// <summary> Returns the amount successfully removed </summary>
        public async Task<int> RemoveFromInventory(DatabaseInventoryItem item, int toRemove)
        {
            var users = Bot.Database.GetCollection<User>("SAIYA_USERS");

            int index = HasItem(Inventory, item.Name);
            if (index == -1) return 0;

            toRemove = Math.Min(toRemove, Inventory[index].Count);

            var update = Builders<User>.Update.Inc(x => x.Inventory[index].Count, -toRemove);
            await users.UpdateOneAsync(user => user.UserID == UserID && user.GuildID == GuildID, update);
            return toRemove;
        }
        public async Task AddCredits(int amount)
        {
            var users = Bot.Database.GetCollection<User>("SAIYA_USERS");
            var update = Builders<User>.Update.Inc(x => x.Credits, amount).Inc(x => x.Statistics.LifetimeCredits, amount);
            await users.UpdateOneAsync(user => user.UserID == UserID && user.GuildID == GuildID, update);
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
        [BsonElement("tag")]
        private int _tag { get; set; }
        [BsonIgnore]
        public Tags Tag { get => (Tags)_tag; set => _tag = (int)value; }
        public enum Tags : int
        {
            Fish
        }
        public DatabaseInventoryItem(string name, int count, Tags tag)
        {
            Name = name;
            Count = count;
            Tag = tag;
        }
    }
    public class DatabaseItem
    {
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("count")]
        public int Count { get; set; }
    }
    public class DatabaseEgg
    {
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("obtained")]
        public DateTime DateObtained { get; set; }
        [BsonIgnore]
        public Creature Creature => CreatureLoader.creatures.Values.FirstOrDefault(creature => creature.Name == Name);

        [BsonIgnore]
        public int SecondsSinceObtained => (int)(DateTime.UtcNow.Subtract(DateObtained).TotalSeconds);
        [BsonIgnore]
        public int SecondsUntilHatch => Creature.HatchTime - SecondsSinceObtained;
    }
}
