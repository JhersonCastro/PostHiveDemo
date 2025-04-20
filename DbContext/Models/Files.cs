using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace DbContext.Models
{
    [Table("files")]
    public sealed class Files
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FilesId { get; init; }

        public int? PostId { get; init; }

        public int? CommentId { get; init; }

        [Required]
        [StringLength(50)]
        public string FileType { get; init; }

        [Required]
        [StringLength(50)]
        public string Uri { get; init; }

        [ForeignKey("PostId")]
        public Post Post { get; init; }

        [ForeignKey("CommentId")]
        public Comments Comments { get; init; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

}
