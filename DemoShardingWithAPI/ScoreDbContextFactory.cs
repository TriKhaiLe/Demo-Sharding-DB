using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace DemoShardingWithAPI
{
    public class ScoreDbContextFactory : IDesignTimeDbContextFactory<ScoreDbContext>
    {
        public ScoreDbContext CreateDbContext(string[] args)
        {
            // Dùng một shard mặc định (ví dụ: shard 0) cho design-time
            var connectionString = "Host=localhost;Port=5432;Database=leaderboard_shard_0;Username=postgres;Password=example";
            var optionsBuilder = new DbContextOptionsBuilder<ScoreDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            return new ScoreDbContext(optionsBuilder.Options);
        }
    }
}
