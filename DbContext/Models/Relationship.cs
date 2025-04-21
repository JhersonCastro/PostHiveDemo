using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbContext.Models
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public enum RelationshipStatus
    {
        request,
        accept,
        blocked
    }
    [Table("RELATIONSHIP")]
    public sealed class Relationship
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RelationshipKeyId { get; init; }

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; init; }

        [Required]
        [ForeignKey(nameof(RelatedUser))]
        public int RelationshipUserIdA { get; init; }

        [Required]
        [StringLength(25)]
        public RelationshipStatus Status { get; set; } = RelationshipStatus.request;

        [Required]
        public DateTime DateFriendship { get; init; } = DateTime.UtcNow;

        // Relaciones de navegación
        public User User { get; init; }          // Usuario que genera la relación
        public User RelatedUser { get; init; }   // Usuario con quien se tiene la relación
    }


#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

}