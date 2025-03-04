using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoShardingWithAPI
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoresController : ControllerBase
    {
        private readonly ShardService _shardService;

        public ScoresController(ShardService shardService)
        {
            _shardService = shardService;
        }

        // 🟢 Thêm điểm số
        [HttpPost]
        public async Task<IActionResult> AddScore([FromBody] Score score)
        {
            if (score == null || score.UserId <= 0)
            {
                return BadRequest("Invalid user ID or score data.");
            }

            string connectionString = _shardService.GetShardConnectionString(score.UserId);

            var options = new DbContextOptionsBuilder<ScoreDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            using (var dbContext = new ScoreDbContext(options))
            {
                dbContext.Scores.Add(score);
                await dbContext.SaveChangesAsync();
            }

            return Ok($"Score added for User {score.UserId} in Shard {score.UserId % 3}");
        }

        // 🔵 Xem điểm số theo User ID
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetScores(int userId)
        {
            string connectionString = _shardService.GetShardConnectionString(userId);

            var options = new DbContextOptionsBuilder<ScoreDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            using (var dbContext = new ScoreDbContext(options))
            {
                var scores = await dbContext.Scores
                    .AsNoTracking()
                    .Where(s => s.UserId == userId)
                    .ToListAsync();

                if (scores.Count == 0)
                    return NotFound("No scores found for this user.");

                return Ok(scores);
            }
        }
    }
}
