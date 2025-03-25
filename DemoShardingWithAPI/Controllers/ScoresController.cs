using DemoShardingWithAPI.EFCore;
using DemoShardingWithAPI.Entities;
using DemoShardingWithAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoShardingWithAPI.Controllers
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

        // Thêm điểm số
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

        // Xem điểm số theo User ID
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
                    .Where(s => s.UserId == userId)
                    .ToListAsync();

                if (scores.Count == 0)
                    return NotFound("No scores found for this user.");

                return Ok(scores);
            }
        }

        [HttpGet("by-username/{username}")]
        public async Task<IActionResult> GetScoresByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest("Username cannot be empty.");
            }

            int shardCount = _shardService.GetShardCount();
            var allScores = new List<Score>();

            // Tạo danh sách các task để query song song từng shard
            var queryTasks = new List<Task<List<Score>>>();
            for (int shardId = 0; shardId < shardCount; shardId++)
            {
                queryTasks.Add(QueryShardAsync(shardId, username));
            }

            // Chờ tất cả các task hoàn thành và lấy kết quả
            var results = await Task.WhenAll(queryTasks);

            // Tổng hợp kết quả từ tất cả shard
            foreach (var scoresFromShard in results)
            {
                allScores.AddRange(scoresFromShard);
            }

            if (allScores.Count == 0)
            {
                return NotFound($"No scores found for username: {username}");
            }

            return Ok(allScores);
        }

        private async Task<List<Score>> QueryShardAsync(int shardId, string username)
        {
            string connectionString = _shardService.GetShardConnectionString(shardId);
            var options = new DbContextOptionsBuilder<ScoreDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            using (var dbContext = new ScoreDbContext(options))
            {
                return await dbContext.Scores
                    .AsNoTracking() // Thêm AsNoTracking để tối ưu hiệu suất
                    .Join(dbContext.Users,
                        score => score.UserId,
                        user => user.Id,
                        (score, user) => new { Score = score, User = user })
                    .Where(joined => joined.User.Username == username)
                    .Select(joined => joined.Score)
                    .ToListAsync();
            }
        }

        [HttpPost("user")]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Username))
            {
                return BadRequest("Invalid user data.");
            }

            // Sử dụng User.Id để xác định shard (giống với Score)
            string connectionString = _shardService.GetShardConnectionString(user.Id);

            var options = new DbContextOptionsBuilder<ScoreDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            using (var dbContext = new ScoreDbContext(options))
            {
                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();
            }

            return Ok($"User {user.Username} added to Shard {user.Id % 3}");
        }
    }
}
