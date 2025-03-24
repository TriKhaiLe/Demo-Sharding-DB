using DemoShardingWithAPI.EFCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DemoShardingWithAPI.Services
{
    public class ShardService
    {
        private readonly Dictionary<int, string> _shardConnections = new()
        {
            { 0, "Host=localhost;Port=5432;Database=leaderboard_shard_0;Username=postgres;Password=example" },
            { 1, "Host=localhost;Database=leaderboard_shard_1;Username=postgres;Password=example" },
            { 2, "Host=localhost;Database=leaderboard_shard_2;Username=postgres;Password=example" }
        };

        public ShardService()
        {
            foreach (var shard in _shardConnections)
            {
                ApplyMigrations(shard.Value);
            }
        }

        private void ApplyMigrations(string connectionString)
        {
            var options = new DbContextOptionsBuilder<ScoreDbContext>()
                .UseNpgsql(connectionString)
                .Options;
            using (var dbContext = new ScoreDbContext(options))
            {
                dbContext.Database.Migrate();
            }
        }

        public string GetShardConnectionString(int userId)
        {
            int shardId = userId % 3;
            return _shardConnections[shardId];
        }

        // lấy số shard
        public int GetShardCount()
        {
            return _shardConnections.Count;
        }
    }
}
