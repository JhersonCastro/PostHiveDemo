using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbContext.Models
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [Table("credentials")]
    public sealed class Credential
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CredentialId { get; init; }

        [Required]
        [StringLength(100)]
        public string Email { get; init; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        [Required]
        public int UserId;

        [ForeignKey("UserId")]
        public User User { get; init; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

}
