using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbContext.Models
{
    [Table("NOTIFICATION")]
    public sealed class Notifications
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int NotificationId { get; init; }

        [Required]
        public int UserId { get; init; }
        [Required]
        [StringLength(64)]
        public string Title { get; init; } = string.Empty;
        [Required]
        [StringLength(256)]
        public string Reason { get; init; } = string.Empty;
        [Required]
        public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
        [Required]
        public bool IsRead { get; init; } = false;
        [Required]
        [StringLength(1024)]
        public string uri { get; init; } = string.Empty;

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
