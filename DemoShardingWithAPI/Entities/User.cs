using System.ComponentModel.DataAnnotations;

namespace DemoShardingWithAPI.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
