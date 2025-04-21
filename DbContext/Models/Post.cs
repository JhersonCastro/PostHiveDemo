using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ReSharper disable InconsistentNaming
public enum PostPrivacy
{
    p_public,
    p_private,
    p_only_friends,
    p_unlisted
}
// ReSharper restore InconsistentNaming
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace DbContext.Models
{
    [Table("POST")]
    public sealed class Post
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostId { get; set; }

        [Required]
        public int UserId { get; init; }

        [Required]
        [StringLength(1024)]
        public string Body { get; init; }

        [Required]
        public DateTime CreatedDate { get; init; }

        [Required]
        public PostPrivacy Privacy { get; init; } = PostPrivacy.p_public;


        [ForeignKey("UserId")]
        public User User { get; set; }

        public ICollection<Files> Files { get; init; }

        public ICollection<Comments> Comments { get; init; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

}
