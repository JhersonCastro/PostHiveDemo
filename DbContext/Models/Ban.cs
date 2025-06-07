using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbContext.Models;
[Table("BAN")]
public class Ban
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BanId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [StringLength(50)]
    public string AdminReason { get; set; }

    [Required]
    public DateTime BanDuration { get; set; }


    [Required]
    public int ReportId { get; set; }

    [ForeignKey("ReportId")]
    public Report Report { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }
}