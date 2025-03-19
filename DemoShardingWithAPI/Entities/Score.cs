using System.ComponentModel.DataAnnotations;

namespace DemoShardingWithAPI.Entities
{
    public class Score
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ScoreValue { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
