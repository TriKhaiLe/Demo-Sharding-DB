using DemoShardingWithAPI.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DemoShardingWithAPI.EFCore
{
    public class ScoreDbContext : DbContext
    {
        public ScoreDbContext(DbContextOptions<ScoreDbContext> options) : base(options) { }

        public DbSet<Score> Scores { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
