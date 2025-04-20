using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbContext.Models
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [Table("CookiesResearch")]
    public class CookiesResearch
    {
        [Key]
        public int Id { get; init; }

        [Required]
        [MaxLength(256)]
        public string CookieCurrentSession { get; init; }

        [Required]
        public int UserId { get; init; }

        [ForeignKey("UserId")]
        public User User { get; init; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

}
