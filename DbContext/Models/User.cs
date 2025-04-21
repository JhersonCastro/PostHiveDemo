using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbContext.Models
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [Table("users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; init; }

        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        [StringLength(50)]
        [Required]
        public string NickName { get; set; }

        [StringLength(128)]
        public string Bio { get; init; } = "";

        [StringLength(50)]
        [Required]
        public string Avatar { get; set; } = "default.png";

        public ICollection<Post?> Posts { get; init; }

        // Relaciones donde este usuario es el iniciador
        public ICollection<Relationship> RelationshipsInitiated { get; init; }

        // Relaciones donde este usuario es el receptor
        public ICollection<Relationship> RelationshipsReceived { get; init; }
        [NotMapped]
        public List<User> Friends { get; set; }
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

}
