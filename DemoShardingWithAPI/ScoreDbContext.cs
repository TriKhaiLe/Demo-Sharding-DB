using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DemoShardingWithAPI
{
    public class ScoreDbContext : DbContext
    {
        public ScoreDbContext(DbContextOptions<ScoreDbContext> options) : base(options) { }

        public DbSet<Score> Scores { get; set; }
    }
}
