using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using SAIYA.Creatures;

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

        [BsonElement("messages")]
        public int Messages { get; set; }

        [BsonElement("creatures")]
        public DatabaseItem[] Creatures { get; set; }

        [BsonElement("eggs")]
        public DatabaseEgg[] Eggs { get; set; }

        // CALCULATED FIELDS
        [BsonIgnore]
        public int ExperienceRequired => 5 * (Level * Level) + (50 * Level) + 100;
        [BsonIgnore]
        public int LevelExperience => (5 / 6) * Level * (2 * Level * Level + 27 * Level + 91);
        [BsonIgnore]
        public int TotalExperience => Experience + LevelExperience;
        /*
        [BsonExtraElements]
        public BsonDocument CatchAll { get; set; }
        */
        public User(ulong userID, ulong guildID)
        {
            id = ObjectId.GenerateNewId();
            UserID = userID;
            GuildID = guildID;
            LastExperience = DateTime.Now;
            Creatures = new DatabaseItem[0];
            Eggs = new DatabaseEgg[0];
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
            var update = Builders<User>.Update.Push(nameof(Eggs), new DatabaseEgg { Name = creature.Name, DateObtained = DateTime.Now });
            await users.UpdateOneAsync(user => user.UserID == UserID && user.GuildID == GuildID, update);
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
        public Creature Creature => CreatureLoader.creatures.FirstOrDefault(creature => creature.Name == Name);
    }
}
